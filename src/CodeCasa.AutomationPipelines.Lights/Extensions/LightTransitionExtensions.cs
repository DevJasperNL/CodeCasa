using System.Reactive.Concurrency;
using CodeCasa.AutomationPipelines.Lights.Nodes;
using CodeCasa.Lights;

namespace CodeCasa.AutomationPipelines.Lights.Extensions;

/// <summary>
/// Provides extension methods for converting <see cref="LightTransition"/> and <see cref="LightParameters"/>
/// instances into pipeline nodes.
/// </summary>
public static class LightTransitionExtensions
{
    /// <summary>
    /// Converts a <see cref="LightTransition"/> into a static pipeline node that always outputs this transition.
    /// </summary>
    /// <param name="lightTransition">The light transition to use as the static output.</param>
    /// <param name="scheduler">The scheduler used for the pipeline node.</param>
    /// <returns>A pipeline node that always outputs the specified <see cref="LightTransition"/>.</returns>
    public static IPipelineNode<LightTransition> AsPipelineNode(this LightTransition lightTransition, IScheduler scheduler)
    {
        return new StaticLightTransitionNode(lightTransition, scheduler);
    }

    /// <summary>
    /// Converts a <see cref="LightParameters"/> instance into a static pipeline node that always outputs
    /// the corresponding <see cref="LightTransition"/> with no transition time.
    /// </summary>
    /// <param name="lightParameters">The light parameters to use as the static output.</param>
    /// <param name="scheduler">The scheduler used for the pipeline node.</param>
    /// <returns>A pipeline node that always outputs the specified <see cref="LightParameters"/> as a <see cref="LightTransition"/>.</returns>
    public static IPipelineNode<LightTransition> AsPipelineNode(this LightParameters lightParameters, IScheduler scheduler)
    {
        return new StaticLightTransitionNode(lightParameters.AsTransition(), scheduler);
    }

    /// <summary>
    /// Converts a <see cref="LightParameters"/> instance into a static pipeline node that always outputs
    /// the corresponding <see cref="LightTransition"/> with the specified transition time.
    /// </summary>
    /// <param name="lightParameters">The light parameters to use as the static output.</param>
    /// <param name="transitionTime">The duration of the transition.</param>
    /// <param name="scheduler">The scheduler used for the pipeline node.</param>
    /// <returns>A pipeline node that always outputs the specified <see cref="LightParameters"/> as a <see cref="LightTransition"/> with the given transition time.</returns>
    public static IPipelineNode<LightTransition> AsPipelineNode(this LightParameters lightParameters, TimeSpan transitionTime, IScheduler scheduler)
    {
        return new StaticLightTransitionNode(lightParameters.AsTransition(transitionTime), scheduler);
    }
}
