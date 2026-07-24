using CodeCasa.Lights;

namespace CodeCasa.AutomationPipelines.Lights.Pipeline;

internal partial class LightTransitionPipelineConfigurator<TLight>
{
    public Dictionary<ILight, LightGroupConfig> LightGroups = new();

    public ILightTransitionPipelineConfigurator<TLight> UseLightGroup(ILight lightGroup)
    {
        return UseLightGroup(lightGroup, TimeSpan.FromMilliseconds(20));
    }

    public ILightTransitionPipelineConfigurator<TLight> UseLightGroup(ILight lightGroup, EqualityComparer<LightTransition> comparer)
    {
        return UseLightGroup(lightGroup, TimeSpan.FromMilliseconds(20), comparer);
    }

    public ILightTransitionPipelineConfigurator<TLight> UseLightGroup(ILight lightGroup, TimeSpan timeSpan)
    {
        return UseLightGroup(lightGroup, timeSpan, EqualityComparer<LightTransition>.Default);
    }

    public ILightTransitionPipelineConfigurator<TLight> UseLightGroup(ILight lightGroup, TimeSpan timeSpan, EqualityComparer<LightTransition> comparer)
    {
        LightGroups[lightGroup] = new LightGroupConfig(timeSpan, comparer);
        return this;
    }
}
