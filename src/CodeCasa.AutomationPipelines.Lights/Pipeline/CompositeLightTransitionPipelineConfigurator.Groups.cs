using CodeCasa.AutomationPipelines.Lights.Extensions;
using CodeCasa.Lights;

namespace CodeCasa.AutomationPipelines.Lights.Pipeline;

internal partial class CompositeLightTransitionPipelineConfigurator<TLight> : IPipelineHierarchyContext
{
    public ILightTransitionPipelineConfigurator<TLight> UseLightGroup(ILight lightGroup)
    {
        NodeContainers.Values.ForEach(b => b.UseLightGroup(lightGroup));
        return this;
    }

    public ILightTransitionPipelineConfigurator<TLight> UseLightGroup(ILight lightGroup, TimeSpan timeSpan)
    {
        NodeContainers.Values.ForEach(b => b.UseLightGroup(lightGroup, timeSpan));
        return this;
    }
}
