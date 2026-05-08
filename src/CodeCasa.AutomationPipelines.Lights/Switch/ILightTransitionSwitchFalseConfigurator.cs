using CodeCasa.Lights;

namespace CodeCasa.AutomationPipelines.Lights.Switch;

/// <summary>
/// Configures the "false" branch of a switch operation on a light transition pipeline.
/// Call one of the <c>WhenFalse</c> overloads to specify what should happen when the observable emits
/// <see langword="false"/> and complete the switch configuration.
/// </summary>
/// <typeparam name="TLight">The specific type of light being controlled.</typeparam>
public interface ILightTransitionSwitchFalseConfigurator<TLight> where TLight : ILight
{
    /// <summary>
    /// Specifies the <see cref="LightParameters"/> to apply when the observable emits <see langword="false"/>
    /// and returns the parent pipeline configurator for further chaining.
    /// </summary>
    /// <param name="lightParameters">The light parameters to apply.</param>
    /// <returns>The parent pipeline configurator for method chaining.</returns>
    void WhenFalse(LightParameters lightParameters);

    /// <summary>
    /// Specifies the <see cref="LightTransition"/> to apply when the observable emits <see langword="false"/>
    /// and returns the parent pipeline configurator for further chaining.
    /// </summary>
    /// <param name="lightTransition">The light transition to apply.</param>
    /// <returns>The parent pipeline configurator for method chaining.</returns>
    void WhenFalse(LightTransition lightTransition);

    /// <summary>
    /// Specifies a factory that creates the <see cref="LightParameters"/> to apply when the observable emits <see langword="false"/>
    /// and returns the parent pipeline configurator for further chaining.
    /// </summary>
    /// <param name="lightParametersFactory">A factory function that creates the light parameters.</param>
    /// <returns>The parent pipeline configurator for method chaining.</returns>
    void WhenFalse(Func<IServiceProvider, LightParameters> lightParametersFactory);

    /// <summary>
    /// Specifies a factory that creates the <see cref="LightTransition"/> to apply when the observable emits <see langword="false"/>
    /// and returns the parent pipeline configurator for further chaining.
    /// </summary>
    /// <param name="lightTransitionFactory">A factory function that creates the light transition.</param>
    /// <returns>The parent pipeline configurator for method chaining.</returns>
    void WhenFalse(Func<IServiceProvider, LightTransition> lightTransitionFactory);

    /// <summary>
    /// Specifies a factory that creates the pipeline node to apply when the observable emits <see langword="false"/>
    /// and returns the parent pipeline configurator for further chaining.
    /// </summary>
    /// <param name="nodeFactory">A factory function that creates the pipeline node.</param>
    /// <returns>The parent pipeline configurator for method chaining.</returns>
    void WhenFalse(Func<IServiceProvider, IPipelineNode<LightTransition>> nodeFactory);

    /// <summary>
    /// Specifies a pipeline node type to resolve from the service provider and apply when the observable emits <see langword="false"/>
    /// and returns the parent pipeline configurator for further chaining.
    /// </summary>
    /// <typeparam name="TNode">The type of the pipeline node to resolve and apply.</typeparam>
    /// <returns>The parent pipeline configurator for method chaining.</returns>
    void WhenFalse<TNode>() where TNode : IPipelineNode<LightTransition>;
}
