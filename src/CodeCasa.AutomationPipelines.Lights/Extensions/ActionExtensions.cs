
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

        public static Action<T> SetLoggingContext<T>(
            this Action<T> configure, IInternalLoggingContext parentLoggingContext)
        {
            return configure.SetLoggingContext(parentLoggingContext.LogName, parentLoggingContext.LoggingEnabled ?? false);
        }
    }
}
