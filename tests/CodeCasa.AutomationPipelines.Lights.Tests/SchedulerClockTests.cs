using CodeCasa.AutomationPipelines.Lights.Nodes;
using CodeCasa.AutomationPipelines.Lights.Pipeline;
using CodeCasa.Lights;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Reactive.Testing;
using System.Reactive.Subjects;

namespace CodeCasa.AutomationPipelines.Lights.Tests;

[TestClass]
public sealed class SchedulerClockTests
{
    [TestMethod]
    public void LightTransitionNode_PassThroughDuringTransition_ContinuesFromSchedulerTime()
    {
        var scheduler = new TestScheduler();
        var node = new StaticLightTransitionNode(new LightParameters { Brightness = 10, ColorTempKelvin = 3000 }.AsTransition(), scheduler);

        node.Input = new LightParameters { Brightness = 100, ColorTempKelvin = 3000 }.AsTransition();
        node.Input = new LightParameters { Brightness = 200, ColorTempKelvin = 3000 }.AsTransition(TimeSpan.FromSeconds(10));
        scheduler.AdvanceBy(TimeSpan.FromSeconds(5).Ticks);
        node.PassThrough = true;

        var brightness = node.Output?.LightParameters.Brightness;
        Assert.IsNotNull(brightness);
        Assert.IsTrue(brightness is > 140 and < 160, $"Expected the transition to resume halfway, got {brightness}.");
    }

    [TestMethod]
    public async Task Toggle_PressAfterTimeout_StartsOverUsingSchedulerTime()
    {
        var light = new TestLight("a");
        var scheduler = new TestScheduler();
        await using var sp = LightPipelineTestSetup.CreateServiceProvider(scheduler);
        var trigger = new Subject<int>();

        var pipeline = sp.GetRequiredService<LightPipelineFactory>().SetupLightPipeline(light, p => p
            .AddToggle(trigger, new LightParameters { Brightness = 100 }, new LightParameters { Brightness = 200 }));

        trigger.OnNext(1);
        Assert.AreEqual(100, light.Current.Brightness);

        trigger.OnNext(2);
        Assert.AreEqual(200, light.Current.Brightness);

        scheduler.AdvanceBy(TimeSpan.FromSeconds(2).Ticks);
        trigger.OnNext(3);
        Assert.AreEqual(0, light.Current.Brightness, "A press after the toggle timeout should turn the light off.");

        await pipeline.DisposeAsync();
    }
}
