using CodeCasa.AutomationPipelines.Lights.Nodes;
using CodeCasa.Lights;
using Microsoft.Extensions.DependencyInjection;
using System.Reactive.Concurrency;

namespace CodeCasa.AutomationPipelines.Lights.Extensions;

/// <summary>
/// Extension methods for <see cref="IServiceProvider"/> to support light automation pipelines.
/// </summary>
public static class ServiceProviderExtensions
{
    internal static IServiceScope CreateLightContextScope<TLight>(this IServiceProvider serviceProvider, TLight light) where TLight : ILight
    {
        return serviceProvider.CreateScope(cb =>
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
    /// Creates a pipeline node that applies the specified light parameters and automatically transitions 
    /// to an 'Off' state after a specified duration of inactivity.
    /// </summary>
    /// <param name="serviceProvider">The service provider used to resolve the <see cref="IScheduler"/>.</param>
    /// <param name="lightParameters">The light parameters to apply as a transition.</param>
    /// <param name="timeSpan">The duration to wait before turning off.</param>
    /// <returns>A pipeline node that applies the light parameters and manages the turn-off timeout.</returns>
    public static IPipelineNode<LightTransition> CreateAutoOffLightNode(this IServiceProvider serviceProvider,
        LightParameters lightParameters,
        TimeSpan timeSpan)
    {
        var scheduler = serviceProvider.GetRequiredService<IScheduler>();
        var innerNode = new StaticLightTransitionNode(lightParameters.AsTransition(), scheduler);
        return innerNode.TurnOffAfter(timeSpan, scheduler);
    }

    /// <summary>
    /// Creates a pipeline node that applies the specified light parameters and automatically transitions 
    /// to an 'Off' state after a specified duration of inactivity.
    /// </summary>
    /// <param name="serviceProvider">The service provider used to resolve the <see cref="IScheduler"/>.</param>
    /// <param name="lightParameters">The light parameters to apply as a transition.</param>
    /// <param name="timeSpan">The duration to wait before turning off.</param>
    /// <param name="persistObservable">An observable that, when true, prevents the timeout from triggering.</param>
    /// <returns>A pipeline node that applies the light parameters and manages the turn-off timeout.</returns>
    public static IPipelineNode<LightTransition> CreateAutoOffLightNode(this IServiceProvider serviceProvider,
        LightParameters lightParameters,
        TimeSpan timeSpan, IObservable<bool> persistObservable)
    {
        var scheduler = serviceProvider.GetRequiredService<IScheduler>();
        var innerNode = new StaticLightTransitionNode(lightParameters.AsTransition(), scheduler);
        return innerNode.TurnOffAfter(timeSpan, persistObservable, scheduler);
    }
}