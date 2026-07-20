using CodeCasa.Lights;

namespace CodeCasa.AutomationPipelines.Lights.Pipeline;

internal partial class LightTransitionPipelineConfigurator<TLight> : IPipelineHierarchyContext
{
    public Dictionary<ILight, TimeSpan> LightGroups = new();

    public ILightTransitionPipelineConfigurator<TLight> UseLightGroup(ILight lightGroup)
    {
        return UseLightGroup(lightGroup, TimeSpan.FromMilliseconds(10));
    }

    public ILightTransitionPipelineConfigurator<TLight> UseLightGroup(ILight lightGroup, TimeSpan timeSpan)
    {
        LightGroups[lightGroup] = timeSpan;
        return this;
    }
}
