using CodeCasa.AutomationPipelines.Lights.Extensions;
using CodeCasa.Lights;

namespace CodeCasa.AutomationPipelines.Lights.Pipeline;

internal partial class CompositeLightTransitionPipelineConfigurator<TLight>
{
    /// <inheritdoc/>
    public ILightTransitionPipelineConfigurator<TLight> Transform(Func<LightTransition?, LightTransition?> transform)
    {
        NodeContainers.Values.ForEach(b => b.Transform(transform));
        return this;
    }

    /// <inheritdoc/>
    public ILightTransitionPipelineConfigurator<TLight> TransformWhen(IObservable<bool> observable, Func<LightTransition?, LightTransition?> transform)
    {
        var shareableObservable = _observableSharingStrategy.Apply(observable);
        NodeContainers.Values.ForEach(b => b.TransformWhen(shareableObservable, transform));
        return this;
    }
}
