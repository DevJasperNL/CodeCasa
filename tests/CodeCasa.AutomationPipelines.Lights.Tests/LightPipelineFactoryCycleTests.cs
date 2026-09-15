using CodeCasa.AutomationPipelines.Lights.Pipeline;
using CodeCasa.Lights;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Reactive.Testing;
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
}
