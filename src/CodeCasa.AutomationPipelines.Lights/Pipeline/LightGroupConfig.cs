using CodeCasa.Lights;

namespace CodeCasa.AutomationPipelines.Lights.Pipeline;

internal record LightGroupConfig(TimeSpan TimeSpan, IEqualityComparer<LightTransition> Comparer);
