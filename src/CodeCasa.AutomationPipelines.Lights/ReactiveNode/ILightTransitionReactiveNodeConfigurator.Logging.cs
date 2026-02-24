namespace CodeCasa.AutomationPipelines.Lights.ReactiveNode;

public partial interface ILightTransitionReactiveNodeConfigurator<TLight>
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
}