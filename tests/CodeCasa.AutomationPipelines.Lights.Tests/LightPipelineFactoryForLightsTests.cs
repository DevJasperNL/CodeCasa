using CodeCasa.AutomationPipelines.Lights.Pipeline;
using Microsoft.Extensions.DependencyInjection;
using System.Reactive.Concurrency;

namespace CodeCasa.AutomationPipelines.Lights.Tests;

[TestClass]
public sealed class LightPipelineFactoryForLightsTests
{
    [TestMethod]
    public async Task ForLights_DuplicateIds_OnlyConfiguresThatLight()
    {
        var a = new TestLight("a");
        var b = new TestLight("b");
        var group = new TestLight("group", a, b);
        await using var sp = LightPipelineTestSetup.CreateServiceProvider(Scheduler.Immediate);

        var pipelines = sp.GetRequiredService<LightPipelineFactory>().SetupLightPipeline(group, p => p
            .ForLights(["a", "a"], c => c.AddNode(_ => new BrightnessNode(200))));

        Assert.AreEqual(200, a.Current.Brightness);
        Assert.AreEqual(0, b.Current.Brightness);
        await pipelines.DisposeAsync();
    }
}
