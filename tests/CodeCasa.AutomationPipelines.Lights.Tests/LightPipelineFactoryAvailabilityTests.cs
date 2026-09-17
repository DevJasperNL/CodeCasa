using CodeCasa.AutomationPipelines.Lights.Pipeline;
using CodeCasa.Lights;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Reactive.Testing;
using System.Reactive.Subjects;

namespace CodeCasa.AutomationPipelines.Lights.Tests;

[TestClass]
public sealed class LightPipelineFactoryAvailabilityTests
{
    [TestMethod]
    public async Task LightBecomesAvailable_CurrentOutputIsReapplied()
    {
        var light = new TestLight("a");
        await using var sp = LightPipelineTestSetup.CreateServiceProvider(new TestScheduler());
        var trigger = new Subject<int>();

        var pipeline = sp.GetRequiredService<LightPipelineFactory>().SetupLightPipeline(light, p => p
            .WithDistinctOutput()
            .AddReactiveNode(r => r.On(trigger, new LightParameters { Brightness = 100 })));
        trigger.OnNext(1);
        Assert.AreEqual(1, light.CountApplied(100));

        light.SetAvailable(false);
        Assert.AreEqual(1, light.CountApplied(100));

        light.SetAvailable(true);
        Assert.AreEqual(2, light.CountApplied(100));

        await pipeline.DisposeAsync();
    }

    [TestMethod]
    public async Task ReapplyDisabled_LightBecomesAvailable_NothingIsApplied()
    {
        var light = new TestLight("a");
        await using var sp = LightPipelineTestSetup.CreateServiceProvider(new TestScheduler());
        var trigger = new Subject<int>();

        var pipeline = sp.GetRequiredService<LightPipelineFactory>().SetupLightPipeline(light, p => p
            .ReapplyOutputWhenAvailable(false)
            .AddReactiveNode(r => r.On(trigger, new LightParameters { Brightness = 100 })));
        trigger.OnNext(1);

        light.SetAvailable(false);
        light.SetAvailable(true);

        Assert.AreEqual(1, light.CountApplied(100));
        await pipeline.DisposeAsync();
    }

    [TestMethod]
    public async Task ReapplyThrows_LightBecomesAvailableAgain_OutputIsStillReapplied()
    {
        var light = new TestLight("a");
        await using var sp = LightPipelineTestSetup.CreateServiceProvider(new TestScheduler());

        var pipeline = sp.GetRequiredService<LightPipelineFactory>().SetupLightPipeline(light, p => p.AddNode(_ => new BrightnessNode(100)));
        light.ThrowOnNextApply = true;
        light.SetAvailable(true);
        Assert.AreEqual(1, light.CountApplied(100));

        light.SetAvailable(true);

        Assert.AreEqual(2, light.CountApplied(100), "A throwing re-apply must not end the availability subscription.");
        await pipeline.DisposeAsync();
    }

    [TestMethod]
    public async Task PipelineDisposed_LightBecomesAvailable_NothingIsApplied()
    {
        var light = new TestLight("a");
        await using var sp = LightPipelineTestSetup.CreateServiceProvider(new TestScheduler());

        var pipeline = sp.GetRequiredService<LightPipelineFactory>().SetupLightPipeline(light, p => p.AddNode(_ => new BrightnessNode(100)));
        await pipeline.DisposeAsync();

        light.SetAvailable(true);

        Assert.AreEqual(1, light.CountApplied(100));
    }
}
