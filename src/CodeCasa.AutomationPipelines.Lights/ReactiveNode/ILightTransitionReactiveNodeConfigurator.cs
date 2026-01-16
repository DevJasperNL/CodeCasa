using CodeCasa.Abstractions;
using CodeCasa.AutomationPipelines.Lights.Context;
using CodeCasa.Lights;

namespace CodeCasa.AutomationPipelines.Lights.ReactiveNode;

/// <summary>
/// Configures a light transition reactive node for controlling light behavior through reactive event handling.
/// Supports adding reactive dimmers, uncoupled dimmers, dynamic node sources, and scoped configurations.
/// </summary>
/// <typeparam name="TLight">The specific type of light being controlled, which must implement <see cref="ILight"/>.</typeparam>
public partial interface ILightTransitionReactiveNodeConfigurator<TLight> where TLight : ILight
{
    /// <summary>
    /// Enables logging for the reactive node configuration.
    /// </summary>
    /// <param name="name">The optional name of the reactive node to include in logs.</param>
    /// <returns>The configurator instance for method chaining.</returns>
    ILightTransitionReactiveNodeConfigurator<TLight> EnableLogging(string? name = null);

    /// <summary>
    /// Disables logging for the reactive node configuration.
    /// </summary>
    /// <returns>The configurator instance for method chaining.</returns>
    ILightTransitionReactiveNodeConfigurator<TLight> DisableLogging();

    /// <summary>
    /// Adds a reactive dimmer control that will be reset when the reactive node activates a new node.
    /// Responds to dimmer events and adjusts light parameters accordingly.
    /// Multiple reactive dimmers can be added and will behave as a group.
    /// </summary>
    /// <param name="dimmer">The dimmer device to add to the reactive node.</param>
    /// <returns>The configurator instance for method chaining.</returns>
    ILightTransitionReactiveNodeConfigurator<TLight> AddReactiveDimmer(IDimmer dimmer);

    /// <summary>
    /// Sets the configuration options for reactive dimmer controls in this node.
    /// </summary>
    /// <param name="dimmerOptions">The dimmer options to configure dimmer behavior.</param>
    /// <returns>The configurator instance for method chaining.</returns>
    ILightTransitionReactiveNodeConfigurator<TLight> SetReactiveDimmerOptions(DimmerOptions dimmerOptions);

    /// <summary>
    /// Adds an uncoupled dimmer control that operates independently without being affected by the reactive node's behavior.
    /// The dimmer will not be reset when the reactive node activates a new node.
    /// </summary>
    /// <param name="dimmer">The dimmer device to add as an uncoupled control.</param>
    /// <returns>The configurator instance for method chaining.</returns>
    ILightTransitionReactiveNodeConfigurator<TLight> AddUncoupledDimmer(IDimmer dimmer);

    /// <summary>
    /// Adds an uncoupled dimmer control with custom configuration options.
    /// The dimmer operates independently without being affected by the reactive node's behavior.
    /// The dimmer will not be reset when the reactive node activates a new node.
    /// </summary>
    /// <param name="dimmer">The dimmer device to add as an uncoupled control.</param>
    /// <param name="dimOptions">An action to configure the dimmer options.</param>
    /// <returns>The configurator instance for method chaining.</returns>
    ILightTransitionReactiveNodeConfigurator<TLight> AddUncoupledDimmer(IDimmer dimmer, Action<DimmerOptions> dimOptions);

    /// <summary>
    /// Adds a dynamic node source that activates a new node in the reactive node each time the observable emits a factory.
    /// The emitted factory is invoked to create and activate the new pipeline node.
    /// </summary>
    /// <param name="nodeFactorySource">An observable that emits factory functions for creating pipeline nodes.</param>
    /// <returns>The configurator instance for method chaining.</returns>
    ILightTransitionReactiveNodeConfigurator<TLight>
        AddNodeSource(
            IObservable<Func<ILightPipelineContext<TLight>, IPipelineNode<LightTransition>?>>
                nodeFactorySource);

    /// <summary>
    /// Creates a scoped reactive node configuration for a specific light identified by its entity ID.
    /// </summary>
    /// <param name="lightId">The entity ID of the light to configure.</param>
    /// <param name="configure">An action to configure the reactive node for this specific light.</param>
    /// <returns>The configurator instance for method chaining.</returns>
    ILightTransitionReactiveNodeConfigurator<TLight> ForLight(string lightId,
        Action<ILightTransitionReactiveNodeConfigurator<TLight>> configure);

    /// <summary>
    /// Creates a scoped reactive node configuration for a specific light.
    /// </summary>
    /// <param name="light">The light to configure.</param>
    /// <param name="configure">An action to configure the reactive node for this specific light.</param>
    /// <returns>The configurator instance for method chaining.</returns>
    ILightTransitionReactiveNodeConfigurator<TLight> ForLight(TLight light,
        Action<ILightTransitionReactiveNodeConfigurator<TLight>> configure);

    /// <summary>
    /// Creates a scoped reactive node configuration for multiple light entities identified by their entity IDs.
    /// </summary>
    /// <param name="lightIds">The entity IDs of the lights to configure.</param>
    /// <param name="configure">An action to configure the reactive node for these lights.</param>
    /// <returns>The configurator instance for method chaining.</returns>
    ILightTransitionReactiveNodeConfigurator<TLight> ForLights(IEnumerable<string> lightIds,
        Action<ILightTransitionReactiveNodeConfigurator<TLight>> configure);

    /// <summary>
    /// Creates a scoped reactive node configuration for multiple light entities.
    /// </summary>
    /// <param name="lights">The light entities to configure.</param>
    /// <param name="configure">An action to configure the reactive node for these lights.</param>
    /// <returns>The configurator instance for method chaining.</returns>
    ILightTransitionReactiveNodeConfigurator<TLight> ForLights(IEnumerable<TLight> lights,
        Action<ILightTransitionReactiveNodeConfigurator<TLight>> configure);
}