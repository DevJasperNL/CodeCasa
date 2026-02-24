
namespace CodeCasa.AutomationPipelines.Lights.Extensions
{
    internal static class ObjectExtensions
    {
        public static void SetLoggingContext(this object obj, string parentName, string name, bool enableLogging)
        {
            if (obj is IInternalLoggingContext loggingContext)
            {
                loggingContext.SetParentName(parentName);
                loggingContext.SetNameInternal(name);
                if (enableLogging)
                {
                    loggingContext.EnableLoggingInternal();
                }
            }
        }
    }
}
