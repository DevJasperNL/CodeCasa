using CodeCasa.AutomationPipelines.Lights.Extensions;
using CodeCasa.Lights;

namespace CodeCasa.AutomationPipelines.Lights.Pipeline;

internal partial class CompositeLightTransitionPipelineConfigurator<TLight>
{
    public ILightTransitionPipelineConfigurator<TLight> UseLightGroup(ILight lightGroup)
    {
        NodeContainers.Values.ForEach(b => b.UseLightGroup(lightGroup));
        return this;
    }

    public ILightTransitionPipelineConfigurator<TLight> UseLightGroup(ILight lightGroup, EqualityComparer<LightTransition> comparer)
    {
        NodeContainers.Values.ForEach(b => b.UseLightGroup(lightGroup, comparer));
        return this;
    }

    public ILightTransitionPipelineConfigurator<TLight> UseLightGroup(ILight lightGroup, TimeSpan timeSpan)
    {
        NodeContainers.Values.ForEach(b => b.UseLightGroup(lightGroup, timeSpan));
        return this;
    }

    public ILightTransitionPipelineConfigurator<TLight> UseLightGroup(ILight lightGroup, TimeSpan timeSpan, EqualityComparer<LightTransition> comparer)
    {
        NodeContainers.Values.ForEach(b => b.UseLightGroup(lightGroup, timeSpan, comparer));
        return this;
    }
}
