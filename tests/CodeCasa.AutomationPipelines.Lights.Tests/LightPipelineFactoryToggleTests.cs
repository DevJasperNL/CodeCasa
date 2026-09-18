using CodeCasa.AutomationPipelines.Lights.Extensions;
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
        // The lights do not report their new state back within the press, as a real light would not either. This passes on
        // the implementation before the shared toggle step as well; it documents that both lights always take the same step.
        var a = new TestLight("a") { ReportsStateWhenApplied = false };
        var b = new TestLight("b") { ReportsStateWhenApplied = false };
        var group = new TestLight("group", a, b);
        await using var sp = LightPipelineTestSetup.CreateServiceProvider(new TestScheduler());
        var trigger = new Subject<int>();

        var pipelines = sp.GetRequiredService<LightPipelineFactory>().SetupLightPipeline(group, p => p
            .AddToggle(trigger, new LightParameters { Brightness = 100 }, new LightParameters { Brightness = 200 }));

        trigger.OnNext(1);
        Assert.AreEqual(100, LastApplied(a));
        Assert.AreEqual(100, LastApplied(b));

        trigger.OnNext(2);
        Assert.AreEqual(200, LastApplied(a));
        Assert.AreEqual(200, LastApplied(b));

        await pipelines.DisposeAsync();
    }

    private static double? LastApplied(TestLight light) => light.Applied[^1].LightParameters.Brightness;

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

    [TestMethod]
    public async Task Toggle_AutoPassThroughNodeTimesOut_LowerLayerTakesOverAndNextPressTurnsOff()
    {
        var timeout = TimeSpan.FromMinutes(10);
        var scheduler = new TestScheduler();
        scheduler.AdvanceTo(new DateTime(2026, 1, 1, 12, 0, 0, DateTimeKind.Utc).Ticks);
        var light = new TestLight("light");
        await using var sp = LightPipelineTestSetup.CreateServiceProvider(scheduler);
        var trigger = new Subject<int>();
        var lowerLayer = new BehaviorSubject<bool>(false);

        var pipelines = sp.GetRequiredService<LightPipelineFactory>().SetupLightPipeline(light, p => p
            .When(lowerLayer, new LightParameters { Brightness = 50 })
            .AddToggle(trigger, c => c.Add(s => s.CreateAutoPassThroughLightNode(new LightParameters { Brightness = 200 }, timeout))));
        var offCountBeforePress = light.CountApplied(0);

        trigger.OnNext(1);
        Assert.AreEqual(200, LastApplied(light));

        lowerLayer.OnNext(true);
        Assert.AreEqual(200, LastApplied(light), "A change of the lower layer must not leak through before the timeout.");

        scheduler.AdvanceBy(timeout.Ticks + 1);
        Assert.AreEqual(50, LastApplied(light));
        Assert.AreEqual(offCountBeforePress, light.CountApplied(0), "The override must hand over to the lower layer without turning the light off.");

        // The lower layer keeps the light on, so the toggle decides from the real light state and turns it off.
        trigger.OnNext(2);
        Assert.AreEqual(0, LastApplied(light));

        // A new activation gets a fresh node with a full timeout.
        scheduler.AdvanceBy(TimeSpan.FromSeconds(5).Ticks);
        trigger.OnNext(3);
        Assert.AreEqual(200, LastApplied(light));
        scheduler.AdvanceBy(timeout.Ticks - 10);
        Assert.AreEqual(200, LastApplied(light));
        scheduler.AdvanceBy(20);
        Assert.AreEqual(50, LastApplied(light));

        await pipelines.DisposeAsync();
    }

    [TestMethod]
    public async Task On_AutoPassThroughNodeTimesOut_WithoutLowerLayer_LightTurnsOff()
    {
        var timeout = TimeSpan.FromMinutes(10);
        var scheduler = new TestScheduler();
        scheduler.AdvanceTo(new DateTime(2026, 1, 1, 12, 0, 0, DateTimeKind.Utc).Ticks);
        var light = new TestLight("light");
        await using var sp = LightPipelineTestSetup.CreateServiceProvider(scheduler);
        var trigger = new Subject<int>();
        var motion = new BehaviorSubject<bool>(true);

        var pipelines = sp.GetRequiredService<LightPipelineFactory>().SetupLightPipeline(light, p => p
            .AddReactiveNode(node => node
                .On(trigger, s => s.CreateAutoPassThroughLightNode(new LightParameters { Brightness = 200 }, timeout, motion))));

        trigger.OnNext(1);
        scheduler.AdvanceBy(timeout.Ticks * 2);
        Assert.AreEqual(200, LastApplied(light), "The timeout is held off while the persist observable is true.");

        motion.OnNext(false);
        scheduler.AdvanceBy(timeout.Ticks + 1);
        Assert.AreEqual(0, LastApplied(light));

        await pipelines.DisposeAsync();
    }
}
