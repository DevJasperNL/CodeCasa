using CodeCasa.AutomationPipelines.Lights.Timeline;
using CodeCasa.Lights;
using Occurify;

namespace CodeCasa.AutomationPipelines.Lights.Switch;

/// <summary>
/// Configures the "true" branch of a switch operation on a light transition pipeline.
/// Call one of the <c>WhenTrue</c> overloads to specify what should happen when the observable emits
/// <see langword="true"/>, then use the returned <see cref="ILightTransitionSwitchFalseConfigurator{TLight}"/>
/// to specify the "false" branch.
/// </summary>
/// <typeparam name="TLight">The specific type of light being controlled.</typeparam>
public interface ILightTransitionSwitchConfigurator<TLight> where TLight : ILight
{
    /// <summary>
    /// Specifies the <see cref="LightParameters"/> to apply when the observable emits <see langword="true"/>.
    /// </summary>
    /// <param name="lightParameters">The light parameters to apply.</param>
    /// <returns>A configurator for specifying the false branch.</returns>
    ILightTransitionSwitchFalseConfigurator<TLight> WhenTrue(LightParameters lightParameters);

    /// <summary>
    /// Specifies the <see cref="LightTransition"/> to apply when the observable emits <see langword="true"/>.
    /// </summary>
    /// <param name="lightTransition">The light transition to apply.</param>
    /// <returns>A configurator for specifying the false branch.</returns>
    ILightTransitionSwitchFalseConfigurator<TLight> WhenTrue(LightTransition lightTransition);

    /// <summary>
    /// Specifies a factory that creates the <see cref="LightParameters"/> to apply when the observable emits <see langword="true"/>.
    /// </summary>
    /// <param name="lightParametersFactory">A factory function that creates the light parameters.</param>
    /// <returns>A configurator for specifying the false branch.</returns>
    ILightTransitionSwitchFalseConfigurator<TLight> WhenTrue(Func<IServiceProvider, LightParameters?> lightParametersFactory);

    /// <summary>
    /// Specifies a factory that creates the <see cref="LightTransition"/> to apply when the observable emits <see langword="true"/>.
    /// </summary>
    /// <param name="lightTransitionFactory">A factory function that creates the light transition.</param>
    /// <returns>A configurator for specifying the false branch.</returns>
    ILightTransitionSwitchFalseConfigurator<TLight> WhenTrue(Func<IServiceProvider, LightTransition?> lightTransitionFactory);

    /// <summary>
    /// Specifies a factory that creates the pipeline node to apply when the observable emits <see langword="true"/>.
    /// </summary>
    /// <param name="nodeFactory">A factory function that creates the pipeline node.</param>
    /// <returns>A configurator for specifying the false branch.</returns>
    ILightTransitionSwitchFalseConfigurator<TLight> WhenTrue(Func<IServiceProvider, IPipelineNode<LightTransition>> nodeFactory);

    /// <summary>
    /// Specifies a pipeline node type to resolve from the service provider and apply when the observable emits <see langword="true"/>.
    /// </summary>
    /// <typeparam name="TNode">The type of the pipeline node to resolve and apply.</typeparam>
    /// <returns>A configurator for specifying the false branch.</returns>
    ILightTransitionSwitchFalseConfigurator<TLight> WhenTrue<TNode>() where TNode : IPipelineNode<LightTransition>;

    /// <summary>
    /// Specifies a timeline node to apply when the observable emits <see langword="true"/>.
    /// The timeline drives the output automatically as time progresses.
    /// </summary>
    /// <param name="timeline">The dictionary mapping timeline points to <see cref="LightParameters"/>.</param>
    /// <param name="transitionTimeForTimelineState">The duration of the initial fade from the current state. Defaults to 400ms if null.</param>
    /// <returns>A configurator for specifying the false branch.</returns>
    ILightTransitionSwitchFalseConfigurator<TLight> WhenTrue(Dictionary<ITimeline, LightParameters> timeline,
        TimeSpan? transitionTimeForTimelineState = null);

    /// <summary>
    /// Specifies a factory that creates a timeline node to apply when the observable emits <see langword="true"/>.
    /// The timeline drives the output automatically as time progresses.
    /// </summary>
    /// <param name="timelineFactory">A factory function that creates the timeline mapping based on the pipeline context.</param>
    /// <param name="transitionTimeForTimelineState">The duration of the initial fade from the current state. Defaults to 400ms if null.</param>
    /// <returns>A configurator for specifying the false branch.</returns>
    ILightTransitionSwitchFalseConfigurator<TLight> WhenTrue(
        Func<IServiceProvider, Dictionary<ITimeline, LightParameters>> timelineFactory,
        TimeSpan? transitionTimeForTimelineState = null);

    /// <summary>
    /// Specifies a timeline node configured by <paramref name="configure"/> to apply when the observable emits <see langword="true"/>.
    /// The timeline drives the output automatically as time progresses.
    /// </summary>
    /// <param name="configure">An action to configure the timeline entries and optional transition time.</param>
    /// <returns>A configurator for specifying the false branch.</returns>
    ILightTransitionSwitchFalseConfigurator<TLight> WhenTrue(Action<ITimelineConfigurator> configure);
}
