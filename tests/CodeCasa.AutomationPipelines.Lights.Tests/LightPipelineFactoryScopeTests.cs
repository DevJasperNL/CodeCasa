using CodeCasa.AutomationPipelines.Lights.Pipeline;
using CodeCasa.Lights;
using Microsoft.Extensions.DependencyInjection;
using System.Reactive.Concurrency;
using System.Reactive.Linq;

namespace CodeCasa.AutomationPipelines.Lights.Tests;

[TestClass]
public sealed class LightPipelineFactoryScopeTests
{
    [TestMethod]
    public async Task ReactiveNode_NoDimmer_ScopedServiceDisposedWithPipeline()
    {
        var light = new TestLight("a");
        await using var sp = LightPipelineTestSetup.CreateServiceProvider(Scheduler.Immediate, s => s.AddScoped<TrackedDisposable>());
        TrackedDisposable? fromReactiveNode = null;
        TrackedDisposable? fromPipeline = null;

        var pipelines = sp.GetRequiredService<LightPipelineFactory>().SetupLightPipeline(light, p => p
            .AddNode(psp =>
            {
                fromPipeline = psp.GetRequiredService<TrackedDisposable>();
                return new BrightnessNode(10);
            })
            .AddReactiveNode(r => r.AddNodeSource(rsp =>
            {
                fromReactiveNode = rsp.GetRequiredService<TrackedDisposable>();
                return Observable.Empty<Func<IServiceProvider, IPipelineNode<LightTransition>?>>();
            })));

        Assert.IsNotNull(fromReactiveNode);
        Assert.IsNotNull(fromPipeline);
        Assert.AreNotSame(fromPipeline, fromReactiveNode, "Reactive node should resolve from its own light context scope.");
        Assert.IsFalse(fromReactiveNode.IsDisposed);

        await pipelines.DisposeAsync();

        Assert.IsTrue(fromReactiveNode.IsDisposed);
        Assert.IsTrue(fromPipeline.IsDisposed);
    }
}
