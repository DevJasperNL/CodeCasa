using CodeCasa.AutomationPipelines.Lights.Nodes;
using CodeCasa.Lights;

namespace CodeCasa.AutomationPipelines.Lights.Pipeline;

internal partial class LightTransitionPipelineConfigurator<TLight>
{
    /// <inheritdoc/>
    public ILightTransitionPipelineConfigurator<TLight> Transform(Func<LightTransition?, LightTransition?> transform)
    {
        return AddNode(new FactoryNode<LightTransition>(transform) { Name = "Transform Node" });
    }

    /// <inheritdoc/>
    public ILightTransitionPipelineConfigurator<TLight> TransformWhen(IObservable<bool> observable, Func<LightTransition?, LightTransition?> transform)
    {
        return When(observable, _ => new FactoryNode<LightTransition>(transform) { Name = "Transform Node" });
    }
}
