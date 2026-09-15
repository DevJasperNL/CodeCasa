using CodeCasa.AutomationPipelines.Lights.Pipeline;
using CodeCasa.AutomationPipelines.Lights.ReactiveNode;
using CodeCasa.Lights;

namespace CodeCasa.AutomationPipelines.Lights.Extensions;

/// <summary>
/// Extension methods for <see cref="ILightTransitionPipelineConfigurator{TLight}"/>.
/// </summary>
public static class LightTransitionPipelineConfiguratorExtensions
{
    /// <summary>
    /// Adds a reactive node to the pipeline that handles external light state changes,
    /// with additional configuration applied via <paramref name="configure"/>.
    /// </summary>
    /// <typeparam name="TLight">The specific type of light being controlled.</typeparam>
    /// <param name="configurator">The pipeline configurator.</param>
    /// <param name="configure">An optional action to further configure the reactive node.</param>
    /// <returns>The configurator instance for method chaining.</returns>
    public static ILightTransitionPipelineConfigurator<TLight> AddInteractionNode<TLight>(
        this ILightTransitionPipelineConfigurator<TLight> configurator, Action<ILightTransitionReactiveNodeConfigurator<TLight>>? configure = null) where TLight : ILight
    {
        return configurator.AddInteractionNode(new InteractionNodeOptions(), configure);
    }

    /// <summary>
    /// Registers a node that caps the brightness of every input at <paramref name="maxBrightness"/>. Colour and transition time
    /// are kept, and off stays off.
    /// </summary>
    /// <typeparam name="TLight">The specific type of light being controlled.</typeparam>
    /// <param name="configurator">The pipeline configurator.</param>
    /// <param name="maxBrightness">The maximum brightness, from 0 to 255.</param>
    /// <returns>The configurator instance for method chaining.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="maxBrightness"/> is outside 0 to 255.</exception>
    public static ILightTransitionPipelineConfigurator<TLight> LimitBrightness<TLight>(
        this ILightTransitionPipelineConfigurator<TLight> configurator, double maxBrightness) where TLight : ILight
    {
        ValidateBrightness(maxBrightness);
        return configurator.Transform(transition => LimitBrightness(transition, maxBrightness));
    }

    /// <summary>
    /// Registers a node that caps the brightness of every input at <paramref name="maxBrightness"/> while the
    /// <paramref name="observable"/> emits <see langword="true"/>, for example at night. Colour and transition time are kept,
    /// and off stays off.
    /// </summary>
    /// <typeparam name="TLight">The specific type of light being controlled.</typeparam>
    /// <param name="configurator">The pipeline configurator.</param>
    /// <param name="observable">The observable that determines when the brightness is capped.</param>
    /// <param name="maxBrightness">The maximum brightness, from 0 to 255.</param>
    /// <returns>The configurator instance for method chaining.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="maxBrightness"/> is outside 0 to 255.</exception>
    public static ILightTransitionPipelineConfigurator<TLight> LimitBrightnessWhen<TLight>(
        this ILightTransitionPipelineConfigurator<TLight> configurator, IObservable<bool> observable, double maxBrightness) where TLight : ILight
    {
        ValidateBrightness(maxBrightness);
        return configurator.TransformWhen(observable, transition => LimitBrightness(transition, maxBrightness));
    }

    private static void ValidateBrightness(double maxBrightness)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(maxBrightness);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(maxBrightness, byte.MaxValue);
    }

    private static LightTransition? LimitBrightness(LightTransition? transition, double maxBrightness)
    {
        if (transition?.LightParameters.Brightness is not { } brightness || brightness <= maxBrightness)
        {
            return transition;
        }
        return transition with { LightParameters = transition.LightParameters with { Brightness = maxBrightness } };
    }

    /// <summary>
    /// Adds a reactive node to the pipeline that handles external light state changes as described by <paramref name="options"/>,
    /// with additional configuration applied via <paramref name="configure"/>.
    /// </summary>
    /// <remarks>
    /// Turning the light off externally is always handled: the light stays off until the pipeline output upstream of this node
    /// changes. With <see cref="InteractionNodeOptions.HoldExternalChanges"/> other external changes are held as well.
    /// </remarks>
    /// <typeparam name="TLight">The specific type of light being controlled.</typeparam>
    /// <param name="configurator">The pipeline configurator.</param>
    /// <param name="options">The options controlling which external changes are handled and for how long.</param>
    /// <param name="configure">An optional action to further configure the reactive node.</param>
    /// <returns>The configurator instance for method chaining.</returns>
    public static ILightTransitionPipelineConfigurator<TLight> AddInteractionNode<TLight>(
        this ILightTransitionPipelineConfigurator<TLight> configurator, InteractionNodeOptions options,
        Action<ILightTransitionReactiveNodeConfigurator<TLight>>? configure = null) where TLight : ILight
    {
        return configurator.AddReactiveNode(c =>
        {
            configure?.Invoke(c);
            c.HandleExternalLightStateChanges(options);
        });
    }
}
