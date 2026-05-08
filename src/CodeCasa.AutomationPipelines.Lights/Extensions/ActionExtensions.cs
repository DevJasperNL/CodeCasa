
using CodeCasa.AutomationPipelines.Lights.Observables;
using CodeCasa.AutomationPipelines.Lights.Pipeline;
using CodeCasa.AutomationPipelines.Lights.ReactiveNode;
using CodeCasa.Lights;

namespace CodeCasa.AutomationPipelines.Lights.Extensions
{
    internal static class ActionExtensions
    {
        public static Action<T> ApplyHierarchySettings<T>(
            this Action<T> configure, string parentName, bool enableLogging)
        {
            return c =>
            {
                if (c is IPipelineHierarchyContext loggingContext)
                {
                    loggingContext.SetParentName(parentName);
                    if (enableLogging)
                    {
                        loggingContext.EnableLoggingInternal();
                    }
                }
                configure(c);
            };
        }

        public static Action<T> ApplyHierarchySettings<T, TLight>(
            this Action<T> configure, ILightTransitionReactiveNodeConfigurator<TLight> lightTransitionReactiveNodeConfigurator) where TLight : ILight
        {
            // Note: This method is used for convenience. As we use this internally only, we can assume that all implementations of ILightTransitionReactiveNodeConfigurator also implement IInternalLoggingContext.
            var loggingContext = (IPipelineHierarchyContext)lightTransitionReactiveNodeConfigurator;
            return configure.ApplyHierarchySettings(loggingContext.HierarchyPath, loggingContext.LoggingEnabled ?? false);
        }

        public static Action<ILightTransitionPipelineConfigurator<TLight>> SetObservableSharingStrategy<TLight>(
            this Action<ILightTransitionPipelineConfigurator<TLight>> configure, IObservableSharingStrategy observableSharingStrategy) where TLight : ILight
        {
            return c =>
            {
                c.SetObservableSharingStrategy(observableSharingStrategy);
                configure(c);
            };
        }

        public static Action<ILightTransitionReactiveNodeConfigurator<TLight>> SetObservableSharingStrategy<TLight>(
            this Action<ILightTransitionReactiveNodeConfigurator<TLight>> configure, IObservableSharingStrategy observableSharingStrategy) where TLight : ILight
        {
            return c =>
            {
                c.SetObservableSharingStrategy(observableSharingStrategy);
                configure(c);
            };
        }
    }
}
