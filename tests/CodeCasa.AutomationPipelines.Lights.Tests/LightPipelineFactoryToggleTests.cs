using CodeCasa.AutomationPipelines.Lights.Pipeline;
using CodeCasa.Lights;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Reactive.Testing;
using System.Reactive.Subjects;

namespace CodeCasa.AutomationPipelines.Lights.Tests;

[TestClass]
public sealed class LightPipelineFactoryToggleTests
{
    [TestMethod]
    public async Task CompositeToggle_ConsecutivePresses_AllLightsGetTheSameValue()
    {
        var a = new TestLight("a");
        var b = new TestLight("b");
        var group = new TestLight("group", a, b);
        await using var sp = LightPipelineTestSetup.CreateServiceProvider(new TestScheduler());
        var trigger = new Subject<int>();

        var pipelines = sp.GetRequiredService<LightPipelineFactory>().SetupLightPipeline(group, p => p
            .AddToggle(trigger, new LightParameters { Brightness = 100 }, new LightParameters { Brightness = 200 }));

        trigger.OnNext(1);
        Assert.AreEqual(100, a.Current.Brightness);
        Assert.AreEqual(100, b.Current.Brightness);

        trigger.OnNext(2);
        Assert.AreEqual(200, a.Current.Brightness);
        Assert.AreEqual(200, b.Current.Brightness);

        await pipelines.DisposeAsync();
    }

    [TestMethod]
    public async Task CompositeToggle_OneLightChangedWithinGracePeriod_AllLightsTakeTheSameStep()
    {
        var scheduler = new TestScheduler();
        scheduler.AdvanceTo(new DateTime(2026, 1, 1, 12, 0, 0, DateTimeKind.Utc).Ticks);
        var a = new TestLight("a") { LastChangedUtc = scheduler.Now.UtcDateTime - TimeSpan.FromMilliseconds(500) };
        var b = new TestLight("b");
        var group = new TestLight("group", a, b);
        await using var sp = LightPipelineTestSetup.CreateServiceProvider(scheduler);
        var trigger = new Subject<int>();

        var pipelines = sp.GetRequiredService<LightPipelineFactory>().SetupLightPipeline(group, p => p
            .AddToggle(trigger, new LightParameters { Brightness = 100 }, new LightParameters { Brightness = 200 }));

        trigger.OnNext(1);

        Assert.AreEqual(a.Current.Brightness, b.Current.Brightness);

        await pipelines.DisposeAsync();
    }
}
