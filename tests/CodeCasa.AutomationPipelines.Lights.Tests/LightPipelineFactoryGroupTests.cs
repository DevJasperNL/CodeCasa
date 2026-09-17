using CodeCasa.AutomationPipelines.Lights.Extensions;
using CodeCasa.AutomationPipelines.Lights.Pipeline;
using CodeCasa.Lights;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Reactive.Testing;
using System.Reactive.Concurrency;
using System.Reactive.Subjects;

namespace CodeCasa.AutomationPipelines.Lights.Tests;

[TestClass]
public sealed class LightPipelineFactoryGroupTests
{
    [TestMethod]
    public async Task LightGroup_Consensus_UpdatesMemberPipelineOutputs()
    {
        var a = new TestLight("a");
        var b = new TestLight("b");
        var group = new TestLight("group", a, b);
        var scheduler = new TestScheduler();
        await using var sp = LightPipelineTestSetup.CreateServiceProvider(scheduler);
        var triggerA = new Subject<int>();
        var turnOffAll = new Subject<int>();
        var pipelines = new Dictionary<string, IPipeline<LightTransition>>();

        var disposable = sp.GetRequiredService<LightPipelineFactory>().SetupLightPipeline(group, p => p
            .UseLightGroup(group, TimeSpan.FromMilliseconds(20))
            .WithDistinctOutput()
            .ForLight("a", c => c.AddReactiveNode(r => r.On(triggerA, new LightParameters { Brightness = 200 })))
            .AddReactiveNode(r => r.TurnOffWhen(turnOffAll))
            .OnCompleted(e => pipelines[e.Light.Id] = e.Pipeline));

        // Startup: both members default to off, so the group is used once and no member is driven individually.
        Assert.AreEqual(1, group.CountApplied(0));
        Assert.IsEmpty(a.Applied);
        Assert.IsEmpty(b.Applied);
        Assert.AreEqual(LightTransition.Off(), pipelines["a"].Output);
        Assert.AreEqual(LightTransition.Off(), pipelines["b"].Output);

        triggerA.OnNext(1);
        scheduler.AdvanceBy(TimeSpan.FromMilliseconds(25).Ticks);
        Assert.AreEqual(1, a.CountApplied(200));
        Assert.AreEqual(200, pipelines["a"].Output?.LightParameters.Brightness);

        turnOffAll.OnNext(1);
        scheduler.AdvanceBy(TimeSpan.FromMilliseconds(25).Ticks);
        Assert.AreEqual(2, group.CountApplied(0));
        Assert.AreEqual(0, a.CountApplied(0), "Member should not be driven individually when the group was used.");
        Assert.AreEqual(LightTransition.Off(), pipelines["a"].Output);
        Assert.AreEqual(LightTransition.Off(), pipelines["b"].Output);

        triggerA.OnNext(2);
        scheduler.AdvanceBy(TimeSpan.FromMilliseconds(25).Ticks);
        Assert.AreEqual(2, a.CountApplied(200), "Distinct output must compare against the group-applied off state.");

        await disposable.DisposeAsync();
    }

    [TestMethod]
    public async Task LightGroup_WithDistinctOutput_DuplicateConsensusDoesNotDriveTheGroup()
    {
        var a = new TestLight("a");
        var b = new TestLight("b");
        var group = new TestLight("group", a, b);
        await using var sp = LightPipelineTestSetup.CreateServiceProvider(new TestScheduler());
        var turnOffAll = new Subject<int>();

        var pipelines = sp.GetRequiredService<LightPipelineFactory>().SetupLightPipeline(group, p => p
            .UseLightGroup(group)
            .WithDistinctOutput()
            .AddReactiveNode(r => r.TurnOffWhen(turnOffAll)));
        Assert.AreEqual(1, group.CountApplied(0));

        turnOffAll.OnNext(1);

        Assert.AreEqual(1, group.CountApplied(0), "The lights are already off, so the group should not be turned off again.");
        await pipelines.DisposeAsync();
    }

    [TestMethod]
    public async Task LightGroup_NewInputWithinWindow_SupersedesPendingInput()
    {
        var a = new TestLight("a");
        var b = new TestLight("b");
        var group = new TestLight("group", a, b);
        var scheduler = new TestScheduler();
        await using var sp = LightPipelineTestSetup.CreateServiceProvider(scheduler);
        var first = new Subject<int>();
        var second = new Subject<int>();

        var pipelines = sp.GetRequiredService<LightPipelineFactory>().SetupLightPipeline(group, p => p
            .UseLightGroup(group, TimeSpan.FromMilliseconds(20))
            .ForLight("a", l => l
                .AddReactiveNode(r => r.On(first, new LightParameters { Brightness = 100 }))
                .AddReactiveNode(r => r.On(second, new LightParameters { Brightness = 200 }))));

        first.OnNext(1);
        second.OnNext(1);
        scheduler.AdvanceBy(TimeSpan.FromMilliseconds(25).Ticks);

        Assert.AreEqual(0, a.CountApplied(100), "The superseded transition should not be applied.");
        Assert.AreEqual(1, a.CountApplied(200));
        await pipelines.DisposeAsync();
    }

    [TestMethod]
    public async Task LightGroup_ApplyTransitionWaitsForInputFromAnotherThread_DoesNotDeadlock()
    {
        var a = new TestLight("a");
        var b = new TestLight("b");
        var pipelineLights = new TestLight("lights", a, b);
        var feedback = new Subject<int>();
        var feedbackCompleted = false;
        var group = new CallbackLight("group", [a, b], transition =>
        {
            if (transition.LightParameters.Brightness == 100)
            {
                feedbackCompleted = Task.Run(() => feedback.OnNext(1)).Wait(TimeSpan.FromSeconds(5));
            }
        });
        await using var sp = LightPipelineTestSetup.CreateServiceProvider(new TestScheduler());
        var allOn = new Subject<int>();

        var pipelines = sp.GetRequiredService<LightPipelineFactory>().SetupLightPipeline(pipelineLights, p => p
            .UseLightGroup(group)
            .AddReactiveNode(r => r.On(allOn, new LightParameters { Brightness = 100 }))
            .ForLight("a", l => l.AddReactiveNode(r => r.On(feedback, new LightParameters { Brightness = 50 }))));

        allOn.OnNext(1);

        Assert.IsTrue(feedbackCompleted, "Input from another thread was blocked while the group entity was being driven.");
        await pipelines.DisposeAsync();
    }

    private sealed class CallbackLight(string id, ILight[] children, Action<LightTransition> onApply) : ILight
    {
        public string Id => id;
        public LightParameters GetParameters() => LightParameters.Off();
        public void ApplyTransition(LightTransition transition) => onApply(transition);
        public ILight[] GetChildren() => children;
        public IObservable<CodeCasa.Abstractions.StateChange<ILight, LightParameters>> StateChanges() =>
            System.Reactive.Linq.Observable.Never<CodeCasa.Abstractions.StateChange<ILight, LightParameters>>();
        public IObservable<CodeCasa.Abstractions.StateChange<ILight, LightParameters>> StateChangesWithCurrent() => StateChanges();
        public DateTime? LastChangedUtc => null;
        public DateTime? LastUpdatedUtc => null;
    }

    [TestMethod]
    public async Task LightGroup_ApplyingToTheGroupFails_MembersAreDrivenIndividually()
    {
        var a = new TestLight("a");
        var b = new TestLight("b");
        var lights = new TestLight("lights", a, b);
        var failures = 0;
        var group = new CallbackLight("group", [a, b], _ =>
        {
            failures++;
            throw new InvalidOperationException("home assistant call failed");
        });
        await using var sp = LightPipelineTestSetup.CreateServiceProvider(new TestScheduler());
        var trigger = new Subject<int>();
        var pipelines = new Dictionary<string, IPipeline<LightTransition>>();

        var disposable = sp.GetRequiredService<LightPipelineFactory>().SetupLightPipeline(lights, p => p
            .UseLightGroup(group)
            .WithDistinctOutput()
            .AddReactiveNode(r => r.On(trigger, new LightParameters { Brightness = 123 }))
            .OnCompleted(e => pipelines[e.Light.Id] = e.Pipeline));

        trigger.OnNext(1);

        Assert.AreEqual(1, a.CountApplied(123), "The group call failed, so the member must be driven individually.");
        Assert.AreEqual(1, b.CountApplied(123));
        Assert.AreEqual(123, pipelines["a"].Output?.LightParameters.Brightness);
        Assert.AreEqual(123, pipelines["b"].Output?.LightParameters.Brightness);
        Assert.AreEqual(2, failures, "Both the start-up off transition and the brightness transition are sent to the group.");

        await disposable.DisposeAsync();
    }

    [TestMethod]
    public async Task LightGroup_GroupAndMemberCallFail_OtherMembersAreStillDrivenAndFailingMemberRecovers()
    {
        var failNextApply = true;
        var appliedToA = new List<LightTransition>();
        var a = new CallbackLight("a", [], transition =>
        {
            if (failNextApply)
            {
                failNextApply = false;
                throw new InvalidOperationException("home assistant call failed");
            }
            appliedToA.Add(transition);
        });
        var appliedToB = new List<LightTransition>();
        var b = new CallbackLight("b", [], appliedToB.Add);
        var lights = new CallbackLight("lights", [a, b], _ => { });
        var group = new CallbackLight("group", [a, b], transition =>
        {
            if (transition.LightParameters.Brightness == 123)
            {
                throw new InvalidOperationException("home assistant call failed");
            }
        });
        var scheduler = new TestScheduler();
        await using var sp = LightPipelineTestSetup.CreateServiceProvider(scheduler);
        var allOn = new Subject<int>();
        var aOn = new Subject<int>();

        var disposable = sp.GetRequiredService<LightPipelineFactory>().SetupLightPipeline(lights, p => p
            .UseLightGroup(group, TimeSpan.FromMilliseconds(20))
            .WithDistinctOutput()
            .AddReactiveNode(r => r.On(allOn, new LightParameters { Brightness = 123 }))
            .ForLight("a", l => l.AddReactiveNode(r => r.On(aOn, new LightParameters { Brightness = 50 }))));

        allOn.OnNext(1);

        Assert.AreEqual(1, appliedToB.Count(t => t.LightParameters.Brightness == 123), "A failing member must not keep the other members from being driven.");

        aOn.OnNext(1);
        scheduler.AdvanceBy(TimeSpan.FromMilliseconds(25).Ticks);

        Assert.AreEqual(1, appliedToA.Count(t => t.LightParameters.Brightness == 50), "The pipeline of the failing member must keep handling outputs.");

        await disposable.DisposeAsync();
    }

    [TestMethod]
    public async Task LightGroup_StateReportedBackDuringGroupCall_IsNotTreatedAsExternalChange()
    {
        var a = new TestLight("a");
        var b = new TestLight("b");
        var lights = new TestLight("lights", a, b);
        var groupCalls = new List<LightTransition>();
        // The members report the new state while the group call is still in progress, like Home Assistant reporting back
        // before the pipelines were updated.
        var group = new CallbackLight("group", [a, b], transition =>
        {
            groupCalls.Add(transition);
            a.ReportExternalState(transition.LightParameters);
            b.ReportExternalState(transition.LightParameters);
        });
        var scheduler = new TestScheduler();
        await using var sp = LightPipelineTestSetup.CreateServiceProvider(scheduler);
        var allOn = new Subject<int>();
        var turnOff = new Subject<int>();

        var disposable = sp.GetRequiredService<LightPipelineFactory>().SetupLightPipeline(lights, p => p
            .UseLightGroup(group)
            .AddReactiveNode(r => r.On(allOn, new LightParameters { Brightness = 100 }))
            .AddReactiveNode(r => r.TurnOffWhen(turnOff))
            .AddInteractionNode());

        allOn.OnNext(1);
        turnOff.OnNext(1);
        scheduler.AdvanceBy(TimeSpan.FromMilliseconds(25).Ticks);

        Assert.AreEqual(2, groupCalls.Count(t => t.LightParameters.Brightness == 0), "Start-up and the turn-off are the only group turn-offs; a turn-off sent through the group must not be mistaken for an external one.");
        Assert.AreEqual(1, groupCalls.Count(t => t.LightParameters.Brightness == 100));
        Assert.IsEmpty(a.Applied);
        Assert.IsEmpty(b.Applied);

        await disposable.DisposeAsync();
    }

    [TestMethod]
    public async Task LightGroup_MemberPipelineDisposed_RemainingMembersAreDrivenIndividually()
    {
        var a = new TestLight("a");
        var b = new TestLight("b");
        var group = new TestLight("group", a, b);
        var scheduler = new TestScheduler();
        await using var sp = LightPipelineTestSetup.CreateServiceProvider(scheduler);
        var bOn = new Subject<int>();
        var pipelines = new Dictionary<string, IPipeline<LightTransition>>();

        var disposable = sp.GetRequiredService<LightPipelineFactory>().SetupLightPipeline(group, p => p
            .UseLightGroup(group)
            .ForLight("b", l => l.AddReactiveNode(r => r.On(bOn, new LightParameters { Brightness = 100 })))
            .OnCompleted(e => pipelines[e.Light.Id] = e.Pipeline));

        await pipelines["a"].DisposeAsync();
        bOn.OnNext(1);
        scheduler.AdvanceBy(TimeSpan.FromMilliseconds(25).Ticks);

        Assert.AreEqual(0, group.CountApplied(100), "The group entity still contains the disposed light, so it must not be driven.");
        Assert.AreEqual(1, b.CountApplied(100));

        await disposable.DisposeAsync();
    }

    [TestMethod]
    public async Task LightGroup_OnNestedPipeline_Throws()
    {
        var a = new TestLight("a");
        var b = new TestLight("b");
        var group = new TestLight("group", a, b);
        await using var sp = LightPipelineTestSetup.CreateServiceProvider(new TestScheduler());

        Assert.ThrowsExactly<InvalidOperationException>(() => sp.GetRequiredService<LightPipelineFactory>().SetupLightPipeline(group, p => p
            .AddPipeline(n => n.UseLightGroup(group))));
        Assert.AreEqual(0, group.Applied.Count);
    }

    [TestMethod]
    public async Task LightGroup_UsedForSubsetOfItsMembers_Throws()
    {
        var a = new TestLight("a");
        var b = new TestLight("b");
        var c = new TestLight("c");
        var group = new TestLight("group", a, b, c);
        await using var sp = LightPipelineTestSetup.CreateServiceProvider(new TestScheduler());

        var exception = Assert.ThrowsExactly<InvalidOperationException>(() => sp.GetRequiredService<LightPipelineFactory>().SetupLightPipeline(group, p => p
            .ForLights(["a", "b"], l => l.UseLightGroup(group))));
        StringAssert.Contains(exception.Message, "c");
    }

    [TestMethod]
    public async Task LightGroup_WithUnknownMembership_IsAllowed()
    {
        var a = new TestLight("a");
        var b = new TestLight("b");
        var pipelineLights = new TestLight("lights", a, b);
        var groupEntity = new TestLight("zigbee_group");
        await using var sp = LightPipelineTestSetup.CreateServiceProvider(new TestScheduler());

        var pipelines = sp.GetRequiredService<LightPipelineFactory>().SetupLightPipeline(pipelineLights, p => p.UseLightGroup(groupEntity));

        Assert.AreEqual(1, groupEntity.CountApplied(0));
        await pipelines.DisposeAsync();
    }

    [TestMethod]
    public async Task LightGroup_SeparateInstancesForSameGroupId_AreTreatedAsOneGroup()
    {
        var a = new TestLight("a");
        var b = new TestLight("b");
        var pipelineLights = new TestLight("lights", a, b);
        var groupForA = new TestLight("group", a, b);
        var groupForB = new TestLight("group", a, b);
        await using var sp = LightPipelineTestSetup.CreateServiceProvider(new TestScheduler());

        var pipelines = sp.GetRequiredService<LightPipelineFactory>().SetupLightPipeline(pipelineLights, p => p
            .ForLight("a", l => l.UseLightGroup(groupForA))
            .ForLight("b", l => l.UseLightGroup(groupForB)));

        Assert.AreEqual(1, groupForA.Applied.Count + groupForB.Applied.Count);
        await pipelines.DisposeAsync();
    }

    [TestMethod]
    public async Task LightGroup_ConflictingTimeSpans_Throws()
    {
        var a = new TestLight("a");
        var b = new TestLight("b");
        var group = new TestLight("group", a, b);
        await using var sp = LightPipelineTestSetup.CreateServiceProvider(new TestScheduler());

        Assert.ThrowsExactly<InvalidOperationException>(() => sp.GetRequiredService<LightPipelineFactory>().SetupLightPipeline(group, p => p
            .ForLight("a", l => l.UseLightGroup(group, TimeSpan.FromMilliseconds(20)))
            .ForLight("b", l => l.UseLightGroup(group, TimeSpan.FromMilliseconds(50)))));
    }

    [TestMethod]
    public async Task NestedPipeline_SharesLightPipelineContextWithRoot()
    {
        var light = new TestLight("a");
        await using var sp = LightPipelineTestSetup.CreateServiceProvider(Scheduler.Immediate);
        var trigger = new Subject<int>();
        LightPipelineContext? rootContext = null;
        LightPipelineContext? nestedContext = null;

        var pipelines = sp.GetRequiredService<LightPipelineFactory>().SetupLightPipeline(light, p => p
            .AddNode(psp =>
            {
                rootContext = psp.GetRequiredService<LightPipelineContext>();
                return new BrightnessNode(10);
            })
            .AddPipeline(n => n.AddNode(nsp =>
            {
                nestedContext = nsp.GetRequiredService<LightPipelineContext>();
                return new BrightnessNode(20);
            }))
            .AddReactiveNode(r => r.On(trigger, new LightParameters { Brightness = 30 })));

        Assert.IsNotNull(rootContext);
        Assert.AreSame(rootContext, nestedContext);
        Assert.AreEqual(20, rootContext.State?.Output?.LightParameters.Brightness);

        trigger.OnNext(1);
        Assert.AreEqual(30, light.Current.Brightness);
        Assert.AreEqual(30, rootContext.State?.Output?.LightParameters.Brightness, "Context must reflect what was sent to the light, not the nested pipeline output.");

        await pipelines.DisposeAsync();
    }
}
