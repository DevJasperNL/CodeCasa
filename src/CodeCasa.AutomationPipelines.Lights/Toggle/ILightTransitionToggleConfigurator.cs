using CodeCasa.AutomationPipelines.Lights.Context;
using CodeCasa.Lights;

namespace CodeCasa.AutomationPipelines.Lights.Toggle
{
    /// <summary>
    /// Configurator for time-based toggle behavior. Quick consecutive triggers advance through all states sequentially.
    /// After a timeout period, the next trigger restarts from the beginning.
    /// If the light is currently on, the first trigger will turn it off.
    /// </summary>
    /// <typeparam name="TLight">The specific type of light being controlled, which must implement <see cref="ILight"/>.</typeparam>
    public interface ILightTransitionToggleConfigurator<TLight> where TLight : ILight
    {
        /// <summary>
        /// Sets the timeout duration after which the toggle cycle restarts from the beginning.
        /// </summary>
        /// <param name="timeout">The timeout duration between triggers that determines when to restart the cycle.</param>
        /// <returns>The configurator instance for method chaining.</returns>
        ILightTransitionToggleConfigurator<TLight> SetToggleTimeout(TimeSpan timeout);

        /// <summary>
        /// Includes the "off" state in the toggle cycle. When the light is off and toggled, it will advance to the first state in the cycle.
        /// </summary>
        /// <returns>The configurator instance for method chaining.</returns>
        ILightTransitionToggleConfigurator<TLight> IncludeOffInToggleCycle();

        /// <summary>
        /// Excludes the "off" state from the toggle cycle. When the light is off and toggled, it will turn on to the first state but "off" won't be part of the sequential cycle.
        /// </summary>
        /// <returns>The configurator instance for method chaining.</returns>
        ILightTransitionToggleConfigurator<TLight> ExcludeOffFromToggleCycle();

        /// <summary>
        /// Adds an "off" state to the toggle sequence.
        /// </summary>
        /// <returns>The configurator instance for method chaining.</returns>
        ILightTransitionToggleConfigurator<TLight> AddOff();

        /// <summary>
        /// Adds an "on" state to the toggle sequence.
        /// </summary>
        /// <returns>The configurator instance for method chaining.</returns>
        ILightTransitionToggleConfigurator<TLight> AddOn();

        /// <summary>
        /// Adds light parameters to the toggle sequence. Quick consecutive triggers will advance through all added parameter sets.
        /// </summary>
        /// <param name="lightParameters">The light parameters to add to the toggle sequence.</param>
        /// <returns>The configurator instance for method chaining.</returns>
        ILightTransitionToggleConfigurator<TLight> Add(LightParameters lightParameters);

        /// <summary>
        /// Adds light parameters created by a factory to the toggle sequence.
        /// </summary>
        /// <param name="lightParametersFactory">A factory function that creates light parameters based on the pipeline context.</param>
        /// <returns>The configurator instance for method chaining.</returns>
        ILightTransitionToggleConfigurator<TLight> Add(Func<ILightPipelineContext<TLight>, LightParameters?> lightParametersFactory);

        /// <summary>
        /// Adds light parameters created by a factory to the toggle sequence.
        /// The factory receives both the pipeline context and the current light transition.
        /// </summary>
        /// <param name="lightParametersFactory">A factory function that creates light parameters based on the pipeline context and current transition.</param>
        /// <returns>The configurator instance for method chaining.</returns>
        ILightTransitionToggleConfigurator<TLight> Add(Func<ILightPipelineContext<TLight>, LightTransition?, LightParameters?> lightParametersFactory);

        /// <summary>
        /// Adds a light transition to the toggle sequence. Quick consecutive triggers will advance through all added transitions.
        /// </summary>
        /// <param name="lightTransition">The light transition to add to the toggle sequence.</param>
        /// <returns>The configurator instance for method chaining.</returns>
        ILightTransitionToggleConfigurator<TLight> Add(LightTransition lightTransition);

        /// <summary>
        /// Adds a light transition created by a factory to the toggle sequence.
        /// </summary>
        /// <param name="lightTransitionFactory">A factory function that creates a light transition based on the pipeline context.</param>
        /// <returns>The configurator instance for method chaining.</returns>
        ILightTransitionToggleConfigurator<TLight> Add(Func<ILightPipelineContext<TLight>, LightTransition?> lightTransitionFactory);

        /// <summary>
        /// Adds a light transition created by a factory to the toggle sequence.
        /// The factory receives both the pipeline context and the current light transition.
        /// </summary>
        /// <param name="lightTransitionFactory">A factory function that creates a light transition based on the pipeline context and current transition.</param>
        /// <returns>The configurator instance for method chaining.</returns>
        ILightTransitionToggleConfigurator<TLight> Add(Func<ILightPipelineContext<TLight>, LightTransition?, LightTransition?> lightTransitionFactory);

        /// <summary>
        /// Adds a pipeline node of type <typeparamref name="TNode"/> to the toggle sequence.
        /// The node is resolved from the service provider. Quick consecutive triggers will advance through all added nodes.
        /// </summary>
        /// <typeparam name="TNode">The type of the pipeline node to add to the toggle sequence.</typeparam>
        /// <returns>The configurator instance for method chaining.</returns>
        ILightTransitionToggleConfigurator<TLight> Add<TNode>() where TNode : IPipelineNode<LightTransition>;

        /// <summary>
        /// Adds a pipeline node created by a factory to the toggle sequence.
        /// Quick consecutive triggers will advance through all added nodes.
        /// </summary>
        /// <param name="nodeFactory">A factory function that creates a pipeline node based on the pipeline context.</param>
        /// <returns>The configurator instance for method chaining.</returns>
        ILightTransitionToggleConfigurator<TLight> Add(Func<ILightPipelineContext<TLight>, IPipelineNode<LightTransition>> nodeFactory);

        /// <summary>
        /// Adds a pass-through state to the toggle sequence that maintains the current light state.
        /// </summary>
        /// <returns>The configurator instance for method chaining.</returns>
        ILightTransitionToggleConfigurator<TLight> AddPassThrough();

        /// <summary>
        /// Creates a scoped toggle configuration for a specific light identified by its entity ID.
        /// </summary>
        /// <param name="lightId">The entity ID of the light to configure.</param>
        /// <param name="configure">An action to configure the toggle for this specific light.</param>
        /// <param name="excludedLightBehaviour">Specifies the behavior for lights not included in this scoped configuration. Defaults to <see cref="ExcludedLightBehaviours.None"/>.</param>
        /// <returns>The configurator instance for method chaining.</returns>
        ILightTransitionToggleConfigurator<TLight> ForLight(string lightId, Action<ILightTransitionToggleConfigurator<TLight>> configure, ExcludedLightBehaviours excludedLightBehaviour = ExcludedLightBehaviours.None);

        /// <summary>
        /// Creates a scoped toggle configuration for a specific light.
        /// </summary>
        /// <param name="light">The light to configure.</param>
        /// <param name="configure">An action to configure the toggle for this specific light.</param>
        /// <param name="excludedLightBehaviour">Specifies the behavior for lights not included in this scoped configuration. Defaults to <see cref="ExcludedLightBehaviours.None"/>.</param>
        /// <returns>The configurator instance for method chaining.</returns>
        ILightTransitionToggleConfigurator<TLight> ForLight(TLight light, Action<ILightTransitionToggleConfigurator<TLight>> configure, ExcludedLightBehaviours excludedLightBehaviour = ExcludedLightBehaviours.None);

        /// <summary>
        /// Creates a scoped toggle configuration for multiple light entities identified by their entity IDs.
        /// </summary>
        /// <param name="lightIds">The entity IDs of the lights to configure.</param>
        /// <param name="configure">An action to configure the toggle for these lights.</param>
        /// <param name="excludedLightBehaviour">Specifies the behavior for lights not included in this scoped configuration. Defaults to <see cref="ExcludedLightBehaviours.None"/>.</param>
        /// <returns>The configurator instance for method chaining.</returns>
        ILightTransitionToggleConfigurator<TLight> ForLights(IEnumerable<string> lightIds, Action<ILightTransitionToggleConfigurator<TLight>> configure, ExcludedLightBehaviours excludedLightBehaviour = ExcludedLightBehaviours.None);

        /// <summary>
        /// Creates a scoped toggle configuration for multiple light entities.
        /// </summary>
        /// <param name="lights">The light entities to configure.</param>
        /// <param name="configure">An action to configure the toggle for these lights.</param>
        /// <param name="excludedLightBehaviour">Specifies the behavior for lights not included in this scoped configuration. Defaults to <see cref="ExcludedLightBehaviours.None"/>.</param>
        /// <returns>The configurator instance for method chaining.</returns>
        ILightTransitionToggleConfigurator<TLight> ForLights(IEnumerable<TLight> lights, Action<ILightTransitionToggleConfigurator<TLight>> configure, ExcludedLightBehaviours excludedLightBehaviour = ExcludedLightBehaviours.None);
    }
}
