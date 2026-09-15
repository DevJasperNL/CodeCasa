using CodeCasa.AutomationPipelines.Lights.Extensions;
using CodeCasa.AutomationPipelines.Lights.Pipeline;
using CodeCasa.Lights;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Reactive.Testing;
using System.Reactive.Subjects;

namespace CodeCasa.AutomationPipelines.Lights.Tests;

[TestClass]
public sealed class LightPipelineFactoryTransformTests
{
    [TestMethod]
    public async Task Transform_ModifiesOutputOfEarlierNodes()
    {
        var light = new TestLight("a");
        await using var sp = LightPipelineTestSetup.CreateServiceProvider(new TestScheduler());
        var trigger = new Subject<int>();

        var pipeline = sp.GetRequiredService<LightPipelineFactory>().SetupLightPipeline(light, p => p
            .AddReactiveNode(r => r.On(trigger, new LightParameters { Brightness = 100 }))
            .Transform(t => t == null ? null : t with { LightParameters = t.LightParameters with { ColorTempKelvin = 2200 } }));

        trigger.OnNext(1);

        Assert.AreEqual(100, light.Current.Brightness);
        Assert.AreEqual(2200, light.Current.ColorTempKelvin);
        await pipeline.DisposeAsync();
    }

    [TestMethod]
    public async Task LimitBrightnessWhen_OnlyCapsWhileObservableIsTrue()
    {
        var a = new TestLight("a");
        var b = new TestLight("b");
        var group = new TestLight("group", a, b);
        await using var sp = LightPipelineTestSetup.CreateServiceProvider(new TestScheduler());
        var night = new BehaviorSubject<bool>(false);

        var pipelines = sp.GetRequiredService<LightPipelineFactory>().SetupLightPipeline(group, p => p
            .AddNode(_ => new BrightnessNode(200))
            .LimitBrightnessWhen(night, 50));
        Assert.AreEqual(200, a.Current.Brightness);

        night.OnNext(true);
        Assert.AreEqual(50, a.Current.Brightness);
        Assert.AreEqual(50, b.Current.Brightness);

        night.OnNext(false);
        Assert.AreEqual(200, a.Current.Brightness);
        await pipelines.DisposeAsync();
    }

    [TestMethod]
    public async Task LimitBrightness_KeepsOffAndLowerBrightness()
    {
        var light = new TestLight("a");
        await using var sp = LightPipelineTestSetup.CreateServiceProvider(new TestScheduler());
        var trigger = new Subject<int>();

        var pipeline = sp.GetRequiredService<LightPipelineFactory>().SetupLightPipeline(light, p => p
            .AddReactiveNode(r => r.On(trigger, new LightParameters { Brightness = 20 }))
            .LimitBrightness(50));
        Assert.AreEqual(0, light.Current.Brightness);

        trigger.OnNext(1);
        Assert.AreEqual(20, light.Current.Brightness);
        await pipeline.DisposeAsync();
    }

    [TestMethod]
    public async Task LimitBrightness_OutOfRange_Throws()
    {
        var light = new TestLight("a");
        await using var sp = LightPipelineTestSetup.CreateServiceProvider(new TestScheduler());

        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() =>
            sp.GetRequiredService<LightPipelineFactory>().SetupLightPipeline(light, p => p.LimitBrightness(300)));
    }
}
