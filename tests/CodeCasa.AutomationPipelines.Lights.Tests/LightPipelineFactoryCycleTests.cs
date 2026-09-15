using CodeCasa.AutomationPipelines.Lights.Pipeline;
using CodeCasa.Lights;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Reactive.Testing;
using Occurify;
using Occurify.Extensions;
using System.Reactive.Subjects;

namespace CodeCasa.AutomationPipelines.Lights.Tests;

[TestClass]
public sealed class LightPipelineFactoryCycleTests
{
    [TestMethod]
    public async Task CompositeCycle_MatcherResolvesLight_UsesTheLightScopedServiceProvider()
    {
        var a = new TestLight("a");
        var b = new TestLight("b");
        var group = new TestLight("group", a, b);
        var scheduler = new TestScheduler();
        await using var sp = LightPipelineTestSetup.CreateServiceProvider(scheduler);
        var trigger = new Subject<int>();

        var pipelines = sp.GetRequiredService<LightPipelineFactory>().SetupLightPipeline(group, p => p
            .AddCycle(trigger, c => c
                .Add(_ => new LightParameters { Brightness = 100 }, lsp => lsp.GetRequiredService<ILight>().GetParameters().Brightness == 100)
                .Add(_ => new LightParameters { Brightness = 200 }, lsp => lsp.GetRequiredService<ILight>().GetParameters().Brightness == 200)));

        trigger.OnNext(1);
        Assert.AreEqual(100, a.Current.Brightness);
        Assert.AreEqual(100, b.Current.Brightness);

        trigger.OnNext(2);
        Assert.AreEqual(200, a.Current.Brightness);
        Assert.AreEqual(200, b.Current.Brightness);

        await pipelines.DisposeAsync();
    }

    [TestMethod]
    public async Task Cycle_LightFollowsTimelineMidRamp_AdvancesToNextEntry()
    {
        await AssertTimelineEntryIsRecognised(Timeline((At(20), 100), (At(22), 200)), expectedTimelineBrightness: 150);
    }

    [TestMethod]
    public async Task Cycle_TimelineEnded_AdvancesToNextEntryWithoutFailing()
    {
        await AssertTimelineEntryIsRecognised(Timeline((At(19), 100), (At(20), 200)), expectedTimelineBrightness: 200);
    }

    private static async Task AssertTimelineEntryIsRecognised(Dictionary<ITimeline, LightParameters> timeline, double expectedTimelineBrightness)
    {
        var light = new TestLight("a");
        var scheduler = new TestScheduler();
        scheduler.AdvanceTo(At(21).Ticks);
        await using var sp = LightPipelineTestSetup.CreateServiceProvider(scheduler);
        var trigger = new Subject<int>();

        var pipeline = sp.GetRequiredService<LightPipelineFactory>().SetupLightPipeline(light, p => p
            .AddCycle(trigger, c => c
                .AddTimeline(timeline)
                .Add(new LightParameters { Brightness = 10 })));

        trigger.OnNext(1);
        Assert.AreEqual(expectedTimelineBrightness, light.Current.Brightness);

        trigger.OnNext(2);
        Assert.AreEqual(10, light.Current.Brightness, "The light follows the timeline, so the cycle should move to the next entry.");

        await pipeline.DisposeAsync();
    }

    private static DateTime At(int hour) => new(2026, 1, 1, hour, 0, 0, DateTimeKind.Utc);

    private static Dictionary<ITimeline, LightParameters> Timeline(params (DateTime Instant, double Brightness)[] points) =>
        points.ToDictionary(p => p.Instant.AsTimeline(), p => new LightParameters { Brightness = p.Brightness });
}
