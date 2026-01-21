using CodeCasa.AutomationPipelines.Lights.Nodes;
using CodeCasa.DependencyInjection.Extensions;
using CodeCasa.Lights;
using Microsoft.Extensions.DependencyInjection;
using System.Reactive.Concurrency;

namespace CodeCasa.AutomationPipelines.Lights.Extensions;

public static class ServiceProviderExtensions
{
    internal static IServiceScope CreateLightContextScope<TLight>(this IServiceProvider serviceProvider, TLight light) where TLight : ILight
    {
        return serviceProvider.CreateContextScope(cb =>
        {
            cb.AddTransient(typeof(ILight), _ => light);

            if (
                typeof(TLight) != typeof(ILight) && // Only add the second registration if TLight isn't already ILight
                typeof(TLight).IsClass || typeof(TLight).IsInterface) // Check at runtime if TLight is a reference type
            {
                cb.AddTransient(typeof(TLight), _ => light);
            }
        });
    }

    /// <summary>
    /// Creates a pipeline node that applies the specified light parameters and automatically turns off the light after a specified duration.
    /// The timeout is not reset by any external events.
    /// </summary>
    /// <param name="serviceProvider">The service provider.</param>
    /// <param name="lightParameters">The light parameters to apply as a transition.</param>
    /// <param name="timeSpan">The duration after which the light should turn off.</param>
    /// <returns>A pipeline node that applies the light parameters and handles the turn-off behavior.</returns>
    public static IPipelineNode<LightTransition> CreateAutoOffLightNode(this IServiceProvider serviceProvider,
        LightParameters lightParameters,
        TimeSpan timeSpan)
    {
        var scheduler = serviceProvider.GetRequiredService<IScheduler>();
        var innerNode = new StaticLightTransitionNode(lightParameters.AsTransition(), scheduler);
        return innerNode.TurnOffAfter(timeSpan, scheduler);
    }

    /// <summary>
    /// Creates a pipeline node that applies the specified light parameters and automatically turns off the light after a specified duration.
    /// The timeout can be reset when the observable emits a value.
    /// </summary>
    /// <typeparam name="T">The type of elements emitted by the reset timer observable.</typeparam>
    /// <param name="serviceProvider">The service provider.</param>
    /// <param name="lightParameters">The light parameters to apply as a transition.</param>
    /// <param name="timeSpan">The duration after which the light should turn off.</param>
    /// <param name="resetTimerObservable">An observable that resets the turn-off timer when it emits.</param>
    /// <returns>A pipeline node that applies the light parameters and handles the turn-off behavior.</returns>
    public static IPipelineNode<LightTransition> CreateAutoOffLightNode<T>(this IServiceProvider serviceProvider,
        LightParameters lightParameters,
        TimeSpan timeSpan, IObservable<T> resetTimerObservable)
    {
        var scheduler = serviceProvider.GetRequiredService<IScheduler>();
        var innerNode = new StaticLightTransitionNode(lightParameters.AsTransition(), scheduler);
        return innerNode.TurnOffAfter(timeSpan, resetTimerObservable, scheduler);
    }
}