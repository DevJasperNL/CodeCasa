using CodeCasa.Lights;

namespace CodeCasa.AutomationPipelines.Lights.Pipeline;

public partial interface ILightTransitionPipelineConfigurator<TLight> where TLight : ILight
{
    /// <summary>
    /// Registers a node that modifies every input with <paramref name="transform"/> instead of replacing it,
    /// for example to cap the brightness or force a colour temperature on whatever the earlier nodes produce.
    /// </summary>
    /// <param name="transform">A function that receives the input of the node and returns its output.</param>
    /// <returns>The configurator instance for method chaining.</returns>
    ILightTransitionPipelineConfigurator<TLight> Transform(Func<LightTransition?, LightTransition?> transform);

    /// <summary>
    /// Registers a node that modifies every input with <paramref name="transform"/> while the <paramref name="observable"/>
    /// emits <see langword="true"/>. When the observable emits <see langword="false"/>, inputs are passed through unchanged.
    /// </summary>
    /// <param name="observable">The observable that determines when the transform is applied.</param>
    /// <param name="transform">A function that receives the input of the node and returns its output.</param>
    /// <returns>The configurator instance for method chaining.</returns>
    ILightTransitionPipelineConfigurator<TLight> TransformWhen(IObservable<bool> observable, Func<LightTransition?, LightTransition?> transform);
}
