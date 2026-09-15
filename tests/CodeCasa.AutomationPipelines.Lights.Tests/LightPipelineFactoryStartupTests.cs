using CodeCasa.AutomationPipelines.Lights.Pipeline;
using CodeCasa.Lights;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Reactive.Testing;
using System.Reactive.Subjects;

namespace CodeCasa.AutomationPipelines.Lights.Tests;

[TestClass]
public sealed class LightPipelineFactoryStartupTests
{
    [TestMethod]
    public async Task TurnOff_Default_TurnsLightOffAtStartup()
    {
        var light = new TestLight("a") { Current = new LightParameters { Brightness = 80 } };
        await using var sp = LightPipelineTestSetup.CreateServiceProvider(new TestScheduler());

        var pipeline = sp.GetRequiredService<LightPipelineFactory>().SetupLightPipeline(light, _ => { });

        Assert.AreEqual(1, light.CountApplied(0));
        await pipeline.DisposeAsync();
    }

    [TestMethod]
    public async Task SkipInitialOutput_LeavesLightAloneUntilOutputChanges()
    {
        var light = new TestLight("a") { Current = new LightParameters { Brightness = 80 } };
        await using var sp = LightPipelineTestSetup.CreateServiceProvider(new TestScheduler());
        var trigger = new Subject<int>();

        var pipeline = sp.GetRequiredService<LightPipelineFactory>().SetupLightPipeline(light, p => p
            .SetStartupBehaviour(PipelineStartupBehaviour.SkipInitialOutput)
            .AddNode(_ => new BrightnessNode(100))
            .AddReactiveNode(r => r.On(trigger, new LightParameters { Brightness = 200 })));

        Assert.IsEmpty(light.Applied);

        trigger.OnNext(1);
        Assert.AreEqual(200, light.Current.Brightness);
        await pipeline.DisposeAsync();
    }

    [TestMethod]
    public async Task StartFromCurrentLightState_PassesCurrentStateThroughWithoutApplyingIt()
    {
        var light = new TestLight("a") { Current = new LightParameters { Brightness = 80 } };
        await using var sp = LightPipelineTestSetup.CreateServiceProvider(new TestScheduler());
        var trigger = new Subject<int>();
        IPipeline<LightTransition>? created = null;

        var pipeline = sp.GetRequiredService<LightPipelineFactory>().SetupLightPipeline(light, p => p
            .SetStartupBehaviour(PipelineStartupBehaviour.StartFromCurrentLightState)
            .AddReactiveNode(r => r.On(trigger, new LightParameters { Brightness = 200 }))
            .OnCompleted(e => created = e.Pipeline));

        Assert.IsEmpty(light.Applied);
        Assert.AreEqual(80, created?.Output?.LightParameters.Brightness);

        trigger.OnNext(1);
        Assert.AreEqual(200, light.Current.Brightness);
        await pipeline.DisposeAsync();
    }

    [TestMethod]
    public async Task SkipInitialOutput_WithLightGroup_NeitherGroupNorMembersAreDrivenAtStartup()
    {
        var a = new TestLight("a");
        var b = new TestLight("b");
        var group = new TestLight("group", a, b);
        var scheduler = new TestScheduler();
        await using var sp = LightPipelineTestSetup.CreateServiceProvider(scheduler);
        var trigger = new Subject<int>();

        var pipelines = sp.GetRequiredService<LightPipelineFactory>().SetupLightPipeline(group, p => p
            .SetStartupBehaviour(PipelineStartupBehaviour.SkipInitialOutput)
            .UseLightGroup(group)
            .ForLight("a", l => l.AddNode(_ => new BrightnessNode(100)))
            .AddReactiveNode(r => r.On(trigger, new LightParameters { Brightness = 200 })));
        scheduler.AdvanceBy(TimeSpan.FromSeconds(1).Ticks);

        Assert.IsEmpty(group.Applied);
        Assert.IsEmpty(a.Applied);
        Assert.IsEmpty(b.Applied);

        trigger.OnNext(1);
        Assert.AreEqual(1, group.CountApplied(200));
        await pipelines.DisposeAsync();
    }
}
