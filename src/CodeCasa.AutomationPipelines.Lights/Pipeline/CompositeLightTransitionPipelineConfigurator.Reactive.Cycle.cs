using CodeCasa.AutomationPipelines.Lights.Cycle;
using CodeCasa.AutomationPipelines.Lights.Extensions;
using CodeCasa.Lights;

namespace CodeCasa.AutomationPipelines.Lights.Pipeline;

internal partial class CompositeLightTransitionPipelineConfigurator<TLight>
{
    public ILightTransitionPipelineConfigurator<TLight> AddCycle<T>(IObservable<T> triggerObservable, IEnumerable<LightParameters> lightParameters)
    {
        return AddReactiveNode(c => c
            .SetLoggingContext(LogName, "Cycle", LoggingEnabled ?? false)
            .AddCycle(triggerObservable, lightParameters));
    }

    public ILightTransitionPipelineConfigurator<TLight> AddCycle<T>(IObservable<T> triggerObservable,
        params LightParameters[] lightParameters)
    {
        return AddReactiveNode(c => c
            .SetLoggingContext(LogName, "Cycle", LoggingEnabled ?? false)
            .AddCycle(triggerObservable, lightParameters));
    }

    public ILightTransitionPipelineConfigurator<TLight> AddCycle<T>(IObservable<T> triggerObservable, IEnumerable<LightTransition> lightTransitions)
    {
        return AddReactiveNode(c => c
            .SetLoggingContext(LogName, "Cycle", LoggingEnabled ?? false)
            .AddCycle(triggerObservable, lightTransitions));
    }

    public ILightTransitionPipelineConfigurator<TLight> AddCycle<T>(IObservable<T> triggerObservable,
        params LightTransition[] lightTransitions)
    {
        return AddReactiveNode(c => c
            .SetLoggingContext(LogName, "Cycle", LoggingEnabled ?? false)
            .AddCycle(triggerObservable, lightTransitions));
    }

    public ILightTransitionPipelineConfigurator<TLight> AddCycle<T>(IObservable<T> triggerObservable, Action<ILightTransitionCycleConfigurator<TLight>> configure)
    {
        return AddReactiveNode(c => c
            .SetLoggingContext(LogName, "Cycle", LoggingEnabled ?? false)
            .AddCycle(triggerObservable, configure));
    }
}