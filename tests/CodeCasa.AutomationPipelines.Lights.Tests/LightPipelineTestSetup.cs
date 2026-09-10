using CodeCasa.AutomationPipelines.Lights.Extensions;
using CodeCasa.Lights;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System.Reactive.Concurrency;
using System.Reactive.Linq;

namespace CodeCasa.AutomationPipelines.Lights.Tests;

internal static class LightPipelineTestSetup
{
    public static ServiceProvider CreateServiceProvider(IScheduler scheduler, Action<IServiceCollection>? configure = null)
    {
        var services = new ServiceCollection();
        services.AddLightPipelines();
        services.AddSingleton(typeof(ILogger<>), typeof(NullLogger<>));
        services.AddSingleton(scheduler);
        configure?.Invoke(services);
        return services.BuildServiceProvider();
    }
}

internal sealed class BrightnessNode : PipelineNode<LightTransition>
{
    public BrightnessNode(int brightness)
    {
        Output = new LightParameters { Brightness = brightness }.AsTransition();
    }
}

internal sealed class TrueObservable : IObservable<bool>
{
    public IDisposable Subscribe(IObserver<bool> observer) => Observable.Return(true).Subscribe(observer);
}

internal sealed class TrackedDisposable : IDisposable
{
    public bool IsDisposed { get; private set; }
    public void Dispose() => IsDisposed = true;
}
