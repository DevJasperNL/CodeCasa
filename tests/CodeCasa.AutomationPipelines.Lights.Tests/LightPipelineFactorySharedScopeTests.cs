using CodeCasa.AutomationPipelines.Lights.Pipeline;
using CodeCasa.AutomationPipelines.Lights.ReactiveNode;
using CodeCasa.Lights;
using Microsoft.Extensions.DependencyInjection;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace CodeCasa.AutomationPipelines.Lights.Tests;

[TestClass]
public sealed class LightPipelineFactorySharedScopeTests
{
    [TestMethod]
    public async Task SharedScope_NestedPipeline_TriggeredTwice_BothLightsAppliedTwice()
    {
        var a = new TestLight("a");
        var b = new TestLight("b");
        var group = new TestLight("group", a, b);
        await using var sp = LightPipelineTestSetup.CreateServiceProvider(Scheduler.Immediate);
        var trigger = new Subject<int>();

        var pipelines = sp.GetRequiredService<LightPipelineFactory>().SetupLightPipeline(group, p => p
            .AddReactiveNode(r => r.On(trigger, (ILightTransitionPipelineConfigurator<TestLight> n) => n.AddNode(_ => new BrightnessNode(200)))));

        trigger.OnNext(1);
        Assert.AreEqual(1, a.CountApplied(200));
        Assert.AreEqual(1, b.CountApplied(200));

        trigger.OnNext(2);
        Assert.AreEqual(2, a.CountApplied(200));
        Assert.AreEqual(2, b.CountApplied(200));

        await pipelines.DisposeAsync();
    }

    [TestMethod]
    public async Task SharedScope_NestedReactiveNode_TriggeredTwice_BothLightsAppliedTwice()
    {
        var a = new TestLight("a");
        var b = new TestLight("b");
        var group = new TestLight("group", a, b);
        await using var sp = LightPipelineTestSetup.CreateServiceProvider(Scheduler.Immediate);
        var trigger = new Subject<int>();

        var pipelines = sp.GetRequiredService<LightPipelineFactory>().SetupLightPipeline(group, p => p
            .AddReactiveNode(r => r.On(trigger, (ILightTransitionReactiveNodeConfigurator<TestLight> n) => n.On(Observable.Return(1), new LightParameters { Brightness = 200 }))));

        trigger.OnNext(1);
        Assert.AreEqual(1, a.CountApplied(200));
        Assert.AreEqual(1, b.CountApplied(200));

        trigger.OnNext(2);
        Assert.AreEqual(2, a.CountApplied(200));
        Assert.AreEqual(2, b.CountApplied(200));

        await pipelines.DisposeAsync();
    }

    [TestMethod]
    public async Task SharedScope_PipelineSwitchWhen_BuildsNestedPipelineOnceForAllLights()
    {
        var a = new TestLight("a");
        var b = new TestLight("b");
        var group = new TestLight("group", a, b);
        await using var sp = LightPipelineTestSetup.CreateServiceProvider(Scheduler.Immediate);
        var when = new BehaviorSubject<bool>(false);
        var sw = new BehaviorSubject<bool>(true);
        var trueConfigurations = 0;

        var pipelines = sp.GetRequiredService<LightPipelineFactory>().SetupLightPipeline(group, p => p
            .AddPipelineSwitchWhen(when, sw,
                t =>
                {
                    trueConfigurations++;
                    t.AddNode(_ => new BrightnessNode(200));
                },
                f => f.AddNode(_ => new BrightnessNode(50))));

        when.OnNext(true);

        Assert.AreEqual(1, trueConfigurations, "A shared nested pipeline should be configured once for all lights.");
        Assert.AreEqual(1, a.CountApplied(200));
        Assert.AreEqual(1, b.CountApplied(200));

        sw.OnNext(false);
        Assert.AreEqual(1, a.CountApplied(50));
        Assert.AreEqual(1, b.CountApplied(50));

        when.OnNext(false);
        Assert.AreEqual(0, a.Current.Brightness);

        await pipelines.DisposeAsync();
    }

    [TestMethod]
    public async Task SharedScope_NestedPipeline_DisposesPreviousGenerationOnRetrigger()
    {
        var a = new TestLight("a");
        var b = new TestLight("b");
        var group = new TestLight("group", a, b);
        await using var sp = LightPipelineTestSetup.CreateServiceProvider(Scheduler.Immediate, s => s.AddScoped<TrackedDisposable>());
        var trigger = new Subject<int>();
        var resolved = new List<TrackedDisposable>();

        var pipelines = sp.GetRequiredService<LightPipelineFactory>().SetupLightPipeline(group, p => p
            .AddReactiveNode(r => r.On(trigger, (ILightTransitionPipelineConfigurator<TestLight> n) => n.AddNode(nsp =>
            {
                resolved.Add(nsp.GetRequiredService<TrackedDisposable>());
                return new BrightnessNode(200);
            }))));

        trigger.OnNext(1);
        trigger.OnNext(2);
        Assert.HasCount(4, resolved);
        Assert.IsTrue(resolved.Take(2).All(d => d.IsDisposed), "First generation should be disposed after re-trigger.");
        Assert.IsTrue(resolved.Skip(2).All(d => !d.IsDisposed), "Second generation should still be alive.");

        await pipelines.DisposeAsync();
        Assert.IsTrue(resolved.All(d => d.IsDisposed));
    }
}
