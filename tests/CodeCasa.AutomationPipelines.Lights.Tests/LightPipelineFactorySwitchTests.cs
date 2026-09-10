using CodeCasa.AutomationPipelines.Lights.Pipeline;
using CodeCasa.Lights;
using Microsoft.Extensions.DependencyInjection;
using System.Reactive.Concurrency;

namespace CodeCasa.AutomationPipelines.Lights.Tests;

[TestClass]
public sealed class LightPipelineFactorySwitchTests
{
    [TestMethod]
    public async Task Switch_ParametersFactoryOverload_SingleLight_TrueBranchApplied()
    {
        var light = new TestLight("a");
        await using var sp = LightPipelineTestSetup.CreateServiceProvider(Scheduler.Immediate);

        var pipelines = sp.GetRequiredService<LightPipelineFactory>().SetupLightPipeline(light, p => p
            .Switch<TrueObservable>(_ => new LightParameters { Brightness = 200 }, _ => new LightParameters { Brightness = 50 }));

        Assert.AreEqual(200, light.Current.Brightness);
        await pipelines.DisposeAsync();
    }

    [TestMethod]
    public async Task Switch_ParametersOverload_SingleLight_TrueBranchApplied()
    {
        var light = new TestLight("a");
        await using var sp = LightPipelineTestSetup.CreateServiceProvider(Scheduler.Immediate);

        var pipelines = sp.GetRequiredService<LightPipelineFactory>().SetupLightPipeline(light, p => p
            .Switch<TrueObservable>(new LightParameters { Brightness = 200 }, new LightParameters { Brightness = 50 }));

        Assert.AreEqual(200, light.Current.Brightness);
        await pipelines.DisposeAsync();
    }

    [TestMethod]
    public async Task Switch_TransitionFactoryOverload_SingleLight_TrueBranchApplied()
    {
        var light = new TestLight("a");
        await using var sp = LightPipelineTestSetup.CreateServiceProvider(Scheduler.Immediate);

        var pipelines = sp.GetRequiredService<LightPipelineFactory>().SetupLightPipeline(light, p => p
            .Switch<TrueObservable>(_ => new LightParameters { Brightness = 200 }.AsTransition(), _ => new LightParameters { Brightness = 50 }.AsTransition()));

        Assert.AreEqual(200, light.Current.Brightness);
        await pipelines.DisposeAsync();
    }

    [TestMethod]
    public async Task Switch_TransitionOverload_SingleLight_TrueBranchApplied()
    {
        var light = new TestLight("a");
        await using var sp = LightPipelineTestSetup.CreateServiceProvider(Scheduler.Immediate);

        var pipelines = sp.GetRequiredService<LightPipelineFactory>().SetupLightPipeline(light, p => p
            .Switch<TrueObservable>(new LightParameters { Brightness = 200 }.AsTransition(), new LightParameters { Brightness = 50 }.AsTransition()));

        Assert.AreEqual(200, light.Current.Brightness);
        await pipelines.DisposeAsync();
    }
}
