
namespace CodeCasa.AutomationPipelines.Lights.Extensions
{
    internal static class ObjectExtensions
    {
        public static T SetLoggingContext<T>(this T obj, string parentName, string name, bool enableLogging)
        {
            if (obj is IInternalLoggingContext loggingContext)
            {
                loggingContext.SetParentName(parentName);
                loggingContext.SetName(name);
                if (enableLogging)
                {
                    loggingContext.EnableLoggingInternal();
                }
            }
            return obj;
        }
    }
}
