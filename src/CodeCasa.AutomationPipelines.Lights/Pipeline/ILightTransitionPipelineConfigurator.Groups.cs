using CodeCasa.Lights;

namespace CodeCasa.AutomationPipelines.Lights.Pipeline;

public partial interface ILightTransitionPipelineConfigurator<TLight>
{
    /// <summary>
    /// Allows you to provide a group light entity to be used if the same transition is applied to all lights at once within 20 milliseconds.
    /// </summary>
    /// <param name="lightGroup">The light group entity.</param>
    /// <returns>The configurator instance for method chaining.</returns>
    ILightTransitionPipelineConfigurator<TLight> UseLightGroup(ILight lightGroup);

    /// <summary>
    /// Allows you to provide a group light entity to be used if the same transition is applied to all lights at once within 20 milliseconds, using a custom comparer.
    /// </summary>
    /// <param name="lightGroup">The light group entity.</param>
    /// <param name="comparer">The equality comparer to determine if transitions are the same.</param>
    /// <returns>The configurator instance for method chaining.</returns>
    ILightTransitionPipelineConfigurator<TLight> UseLightGroup(ILight lightGroup, EqualityComparer<LightTransition> comparer);

    /// <summary>
    /// Allows you to provide a group light entity to be used if the same transition is applied to all lights at once within the specified time span.
    /// </summary>
    /// <param name="lightGroup">The light group entity.</param>
    /// <param name="timeSpan">The time span for the transition.</param>
    /// <returns>The configurator instance for method chaining.</returns>
    ILightTransitionPipelineConfigurator<TLight> UseLightGroup(ILight lightGroup, TimeSpan timeSpan);

    /// <summary>
    /// Allows you to provide a group light entity to be used if the same transition is applied to all lights at once within the specified time span, using a custom comparer.
    /// </summary>
    /// <param name="lightGroup">The light group entity.</param>
    /// <param name="timeSpan">The time span for the transition.</param>
    /// <param name="comparer">The equality comparer to determine if transitions are the same.</param>
    /// <returns>The configurator instance for method chaining.</returns>
    ILightTransitionPipelineConfigurator<TLight> UseLightGroup(ILight lightGroup, TimeSpan timeSpan, EqualityComparer<LightTransition> comparer);
}