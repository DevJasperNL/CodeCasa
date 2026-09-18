using CodeCasa.AutomationPipelines.Lights.Nodes;
using CodeCasa.AutomationPipelines.Lights.Pipeline;
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
    /// Returns true when <paramref name="serviceProvider"/> does not yet carry a <see cref="LightPipelineContext"/> for
    /// <paramref name="light"/>, i.e. a pipeline created from it is the root pipeline for that light.
    /// </summary>
    internal static bool OwnsLightPipelineContext<TLight>(this IServiceProvider serviceProvider, TLight light) where TLight : ILight
    {
        var existingContext = serviceProvider.GetService<LightPipelineContext>();
        return existingContext == null || existingContext.Light.Id != light.Id;
    }

    internal static IServiceScope CreateLightPipelineContextScope<TLight>(this IServiceProvider serviceProvider, TLight light) where TLight : ILight
    {
        var ownsContext = serviceProvider.OwnsLightPipelineContext(light);
        return serviceProvider.CreateScope(cb =>
        {
            cb.AddTransient(typeof(ILight), _ => light);
            if (
                typeof(TLight) != typeof(ILight) && // Only add the second registration if TLight isn't already ILight
                typeof(TLight).IsClass || typeof(TLight).IsInterface) // Check at runtime if TLight is a reference type
            {
                cb.AddTransient(typeof(TLight), _ => light);
            }

            // Nested pipelines share the root pipeline's context: it must describe what was actually sent to the light.
            if (ownsContext)
            {
                cb.AddSingleton(new LightPipelineContext(light));
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
    /// <seealso cref="CreateAutoPassThroughLightNode(IServiceProvider, LightParameters, TimeSpan)"/>
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
    /// <seealso cref="CreateAutoPassThroughLightNode(IServiceProvider, LightParameters, TimeSpan, IObservable{bool})"/>
    public static IPipelineNode<LightTransition> CreateAutoOffLightNode(this IServiceProvider serviceProvider,
        LightParameters lightParameters,
        TimeSpan timeSpan, IObservable<bool> persistObservable)
    {
        var scheduler = serviceProvider.GetRequiredService<IScheduler>();
        var innerNode = new StaticLightTransitionNode(lightParameters.AsTransition(), scheduler);
        return innerNode.TurnOffAfter(timeSpan, persistObservable, scheduler);
    }

    /// <summary>
    /// Creates a pipeline node that applies the specified light parameters and, after a specified duration of inactivity,
    /// passes its input through, handing control back to the nodes before it.
    /// </summary>
    /// <remarks>
    /// Use this for an override that should expire, for example a toggle that sets a bright scene on top of a motion or
    /// night-time layer: when the time is up, whatever the earlier nodes output takes effect at once, and the light only
    /// turns off when they output nothing. Use
    /// <see cref="CreateAutoOffLightNode(IServiceProvider, LightParameters, TimeSpan)"/> when the light has to be off once the
    /// time is up, regardless of the earlier nodes. After the timeout the node never applies its light parameters again; a
    /// new activation, such as the next toggle press, creates a new node.
    /// </remarks>
    /// <param name="serviceProvider">The service provider used to resolve the <see cref="IScheduler"/>.</param>
    /// <param name="lightParameters">The light parameters to apply as a transition.</param>
    /// <param name="timeSpan">The duration to wait before passing the input through.</param>
    /// <returns>A pipeline node that applies the light parameters and manages the pass-through timeout.</returns>
    public static IPipelineNode<LightTransition> CreateAutoPassThroughLightNode(this IServiceProvider serviceProvider,
        LightParameters lightParameters,
        TimeSpan timeSpan)
    {
        var scheduler = serviceProvider.GetRequiredService<IScheduler>();
        var innerNode = new StaticLightTransitionNode(lightParameters.AsTransition(), scheduler);
        return innerNode.PassThroughAfter(timeSpan, scheduler);
    }

    /// <summary>
    /// Creates a pipeline node that applies the specified light parameters and, after a specified duration of inactivity,
    /// passes its input through, handing control back to the nodes before it.
    /// </summary>
    /// <remarks>
    /// Use this for an override that should expire, for example a toggle that sets a bright scene on top of a motion or
    /// night-time layer: when the time is up, whatever the earlier nodes output takes effect at once, and the light only
    /// turns off when they output nothing. Use
    /// <see cref="CreateAutoOffLightNode(IServiceProvider, LightParameters, TimeSpan, IObservable{bool})"/> when the light has
    /// to be off once the time is up, regardless of the earlier nodes. After the timeout the node ignores
    /// <paramref name="persistObservable"/> and never applies its light parameters again; a new activation, such as the next
    /// toggle press, creates a new node.
    /// </remarks>
    /// <param name="serviceProvider">The service provider used to resolve the <see cref="IScheduler"/>.</param>
    /// <param name="lightParameters">The light parameters to apply as a transition.</param>
    /// <param name="timeSpan">The duration to wait before passing the input through.</param>
    /// <param name="persistObservable">An observable that, when true, prevents the timeout from triggering. The timeout restarts when it becomes false.</param>
    /// <returns>A pipeline node that applies the light parameters and manages the pass-through timeout.</returns>
    public static IPipelineNode<LightTransition> CreateAutoPassThroughLightNode(this IServiceProvider serviceProvider,
        LightParameters lightParameters,
        TimeSpan timeSpan, IObservable<bool> persistObservable)
    {
        var scheduler = serviceProvider.GetRequiredService<IScheduler>();
        var innerNode = new StaticLightTransitionNode(lightParameters.AsTransition(), scheduler);
        return innerNode.PassThroughAfter(timeSpan, persistObservable, scheduler);
    }
}