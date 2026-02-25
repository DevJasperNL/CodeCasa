
using CodeCasa.AutomationPipelines.Lights.ReactiveNode;
using CodeCasa.Lights;

namespace CodeCasa.AutomationPipelines.Lights.Extensions
{
    internal static class ActionExtensions
    {
        public static Action<T> SetLoggingContext<T>(
            this Action<T> configure, string parentName, bool enableLogging)
        {
            return c =>
            {
                if (c is IInternalLoggingContext loggingContext)
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

        public static Action<T> SetLoggingContext<T, TLight>(
            this Action<T> configure, ILightTransitionReactiveNodeConfigurator<TLight> lightTransitionReactiveNodeConfigurator) where TLight : ILight
        {
            // Note: This method is used for convenience. As we use this internally only, we can assume that all implementations of ILightTransitionReactiveNodeConfigurator also implement IInternalLoggingContext.
            var loggingContext = (IInternalLoggingContext)lightTransitionReactiveNodeConfigurator;
            return configure.SetLoggingContext(loggingContext.LogName, loggingContext.LoggingEnabled ?? false);
        }
    }
}
