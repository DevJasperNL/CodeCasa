using CodeCasa.Lights;

namespace CodeCasa.AutomationPipelines.Lights.Pipeline;

internal record LightGroupConfig(TimeSpan TimeSpan, EqualityComparer<LightTransition> Comparer);