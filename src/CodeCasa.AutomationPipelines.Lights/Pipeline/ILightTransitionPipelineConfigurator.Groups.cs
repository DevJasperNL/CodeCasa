using CodeCasa.Lights;

namespace CodeCasa.AutomationPipelines.Lights.Pipeline;

public partial interface ILightTransitionPipelineConfigurator<TLight>
{
    /// <summary>
    /// Allows you to provide a group light entity to be used if the same transition is applied to all lights at once within 20 milliseconds.
    /// </summary>
    /// <remarks>
    /// Transitions of every member are held back for the window. A newer transition for the same light within that window
    /// replaces the pending one, which is then never sent to the light.
    /// </remarks>
    /// <param name="lightGroup">The light group entity.</param>
    /// <returns>The configurator instance for method chaining.</returns>
    ILightTransitionPipelineConfigurator<TLight> UseLightGroup(ILight lightGroup);

    /// <summary>
    /// Allows you to provide a group light entity to be used if the same transition is applied to all lights at once within 20 milliseconds, using a custom comparer.
    /// </summary>
    /// <param name="lightGroup">The light group entity.</param>
    /// <param name="comparer">The equality comparer to determine if transitions are the same.</param>
    /// <returns>The configurator instance for method chaining.</returns>
    ILightTransitionPipelineConfigurator<TLight> UseLightGroup(ILight lightGroup, IEqualityComparer<LightTransition> comparer);

    /// <summary>
    /// Allows you to provide a group light entity to be used if the same transition is applied to all lights at once within the specified time span.
    /// </summary>
    /// <param name="lightGroup">The light group entity.</param>
    /// <param name="timeSpan">The window within which all members must receive the same transition for the group entity to be used.</param>
    /// <returns>The configurator instance for method chaining.</returns>
    ILightTransitionPipelineConfigurator<TLight> UseLightGroup(ILight lightGroup, TimeSpan timeSpan);

    /// <summary>
    /// Allows you to provide a group light entity to be used if the same transition is applied to all lights at once within the specified time span, using a custom comparer.
    /// </summary>
    /// <param name="lightGroup">The light group entity.</param>
    /// <param name="timeSpan">The window within which all members must receive the same transition for the group entity to be used.</param>
    /// <param name="comparer">The equality comparer to determine if transitions are the same.</param>
    /// <returns>The configurator instance for method chaining.</returns>
    ILightTransitionPipelineConfigurator<TLight> UseLightGroup(ILight lightGroup, TimeSpan timeSpan, IEqualityComparer<LightTransition> comparer);
}
