using CodeCasa.AutomationPipelines.Lights.Context;
using CodeCasa.Lights;

namespace CodeCasa.AutomationPipelines.Lights.Cycle
{
    /// <summary>
    /// Configurator for state-based cycle behavior. Cycles advance based on the current light state:
    /// if the light matches a state in the cycle, it advances to the next state.
    /// If the current state is not recognized, the cycle starts from the beginning.
    /// </summary>
    public interface ILightTransitionCycleConfigurator
    {
        /// <summary>
        /// Adds an "off" state to the cycle.
        /// </summary>
        /// <returns>The configurator instance for method chaining.</returns>
        ILightTransitionCycleConfigurator AddOff();

        /// <summary>
        /// Adds an "on" state to the cycle.
        /// </summary>
        /// <returns>The configurator instance for method chaining.</returns>
        ILightTransitionCycleConfigurator AddOn();

        /// <summary>
        /// Adds light parameters to the cycle. The cycle will advance to these parameters when the current state matches the previous entry in the cycle.
        /// </summary>
        /// <param name="lightParameters">The light parameters to add to the cycle.</param>
        /// <param name="comparer">An optional equality comparer for determining if light parameters match. If null, the default equality comparison is used.</param>
        /// <returns>The configurator instance for method chaining.</returns>
        ILightTransitionCycleConfigurator Add(LightParameters lightParameters, IEqualityComparer<LightParameters>? comparer = null);

        /// <summary>
        /// Adds light parameters created by a factory to the cycle, with a custom state matching function.
        /// The <paramref name="matchesNodeState"/> function determines if the current light state matches this cycle entry.
        /// </summary>
        /// <param name="lightParametersFactory">A factory function that creates light parameters based on the pipeline context.</param>
        /// <param name="matchesNodeState">A function that determines if the current state matches this cycle entry.</param>
        /// <returns>The configurator instance for method chaining.</returns>
        ILightTransitionCycleConfigurator Add(Func<ILightPipelineContext, LightParameters?> lightParametersFactory, Func<ILightPipelineContext, bool> matchesNodeState);

        /// <summary>
        /// Adds light parameters created by a factory to the cycle, with a custom state matching function.
        /// The factory receives both the pipeline context and the current light transition.
        /// The <paramref name="matchesNodeState"/> function determines if the current light state matches this cycle entry.
        /// </summary>
        /// <param name="lightParametersFactory">A factory function that creates light parameters based on the pipeline context and current transition.</param>
        /// <param name="matchesNodeState">A function that determines if the current state matches this cycle entry.</param>
        /// <returns>The configurator instance for method chaining.</returns>
        ILightTransitionCycleConfigurator Add(Func<ILightPipelineContext, LightTransition?, LightParameters?> lightParametersFactory, Func<ILightPipelineContext, bool> matchesNodeState);

        /// <summary>
        /// Adds a light transition to the cycle. The cycle will advance to this transition when the current state matches the previous entry in the cycle.
        /// </summary>
        /// <param name="lightTransition">The light transition to add to the cycle.</param>
        /// <param name="comparer">An optional equality comparer for determining if light parameters match. If null, the default equality comparison is used.</param>
        /// <returns>The configurator instance for method chaining.</returns>
        ILightTransitionCycleConfigurator Add(LightTransition lightTransition, IEqualityComparer<LightParameters>? comparer = null);

        /// <summary>
        /// Adds a light transition created by a factory to the cycle, with a custom state matching function.
        /// The <paramref name="matchesNodeState"/> function determines if the current light state matches this cycle entry.
        /// </summary>
        /// <param name="lightTransitionFactory">A factory function that creates a light transition based on the pipeline context.</param>
        /// <param name="matchesNodeState">A function that determines if the current state matches this cycle entry.</param>
        /// <returns>The configurator instance for method chaining.</returns>
        ILightTransitionCycleConfigurator Add(Func<ILightPipelineContext, LightTransition?> lightTransitionFactory, Func<ILightPipelineContext, bool> matchesNodeState);

        /// <summary>
        /// Adds a light transition created by a factory to the cycle, with a custom state matching function.
        /// The factory receives both the pipeline context and the current light transition.
        /// The <paramref name="matchesNodeState"/> function determines if the current light state matches this cycle entry.
        /// </summary>
        /// <param name="lightTransitionFactory">A factory function that creates a light transition based on the pipeline context and current transition.</param>
        /// <param name="matchesNodeState">A function that determines if the current state matches this cycle entry.</param>
        /// <returns>The configurator instance for method chaining.</returns>
        ILightTransitionCycleConfigurator Add(Func<ILightPipelineContext, LightTransition?, LightTransition?> lightTransitionFactory, Func<ILightPipelineContext, bool> matchesNodeState);

        /// <summary>
        /// Adds a pipeline node of type <typeparamref name="TNode"/> to the cycle, with a custom state matching function.
        /// The node is resolved from the service provider.
        /// The <paramref name="matchesNodeState"/> function determines if the current light state matches this cycle entry.
        /// </summary>
        /// <typeparam name="TNode">The type of the pipeline node to add to the cycle.</typeparam>
        /// <param name="matchesNodeState">A function that determines if the current state matches this cycle entry.</param>
        /// <returns>The configurator instance for method chaining.</returns>
        ILightTransitionCycleConfigurator Add<TNode>(Func<ILightPipelineContext, bool> matchesNodeState) where TNode : IPipelineNode<LightTransition>;

        /// <summary>
        /// Adds a pipeline node created by a factory to the cycle, with a custom state matching function.
        /// The <paramref name="matchesNodeState"/> function determines if the current light state matches this cycle entry.
        /// </summary>
        /// <param name="nodeFactory">A factory function that creates a pipeline node based on the pipeline context.</param>
        /// <param name="matchesNodeState">A function that determines if the current state matches this cycle entry.</param>
        /// <returns>The configurator instance for method chaining.</returns>
        ILightTransitionCycleConfigurator Add(Func<ILightPipelineContext, IPipelineNode<LightTransition>> nodeFactory, Func<ILightPipelineContext, bool> matchesNodeState);

        /// <summary>
        /// Adds a pass-through state to the cycle that maintains the current light state.
        /// The <paramref name="matchesNodeState"/> function determines if the current light state matches this cycle entry.
        /// </summary>
        /// <param name="matchesNodeState">A function that determines if the current state matches this cycle entry.</param>
        /// <returns>The configurator instance for method chaining.</returns>
        ILightTransitionCycleConfigurator AddPassThrough(Func<ILightPipelineContext, bool> matchesNodeState);

        /// <summary>
        /// Creates a scoped cycle configuration for a specific light identified by its entity ID.
        /// </summary>
        /// <param name="lightId">The entity ID of the light to configure.</param>
        /// <param name="configure">An action to configure the cycle for this specific light.</param>
        /// <param name="excludedLightBehaviour">Specifies the behavior for lights not included in this scoped configuration. Defaults to <see cref="ExcludedLightBehaviours.None"/>.</param>
        /// <returns>The configurator instance for method chaining.</returns>
        ILightTransitionCycleConfigurator ForLight(string lightId, Action<ILightTransitionCycleConfigurator> configure, ExcludedLightBehaviours excludedLightBehaviour = ExcludedLightBehaviours.None);

        /// <summary>
        /// Creates a scoped cycle configuration for a specific light.
        /// </summary>
        /// <param name="light">The light to configure.</param>
        /// <param name="configure">An action to configure the cycle for this specific light.</param>
        /// <param name="excludedLightBehaviour">Specifies the behavior for lights not included in this scoped configuration. Defaults to <see cref="ExcludedLightBehaviours.None"/>.</param>
        /// <returns>The configurator instance for method chaining.</returns>
        ILightTransitionCycleConfigurator ForLight(ILight light, Action<ILightTransitionCycleConfigurator> configure, ExcludedLightBehaviours excludedLightBehaviour = ExcludedLightBehaviours.None);

        /// <summary>
        /// Creates a scoped cycle configuration for multiple light entities identified by their entity IDs.
        /// </summary>
        /// <param name="lightIds">The entity IDs of the lights to configure.</param>
        /// <param name="configure">An action to configure the cycle for these lights.</param>
        /// <param name="excludedLightBehaviour">Specifies the behavior for lights not included in this scoped configuration. Defaults to <see cref="ExcludedLightBehaviours.None"/>.</param>
        /// <returns>The configurator instance for method chaining.</returns>
        ILightTransitionCycleConfigurator ForLights(IEnumerable<string> lightIds, Action<ILightTransitionCycleConfigurator> configure, ExcludedLightBehaviours excludedLightBehaviour = ExcludedLightBehaviours.None);

        /// <summary>
        /// Creates a scoped cycle configuration for multiple light entities.
        /// </summary>
        /// <param name="lightEntities">The light entities to configure.</param>
        /// <param name="configure">An action to configure the cycle for these lights.</param>
        /// <param name="excludedLightBehaviour">Specifies the behavior for lights not included in this scoped configuration. Defaults to <see cref="ExcludedLightBehaviours.None"/>.</param>
        /// <returns>The configurator instance for method chaining.</returns>
        ILightTransitionCycleConfigurator ForLights(IEnumerable<ILight> lightEntities, Action<ILightTransitionCycleConfigurator> configure, ExcludedLightBehaviours excludedLightBehaviour = ExcludedLightBehaviours.None);
    }
}
