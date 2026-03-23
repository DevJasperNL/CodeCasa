
namespace CodeCasa.AutomationPipelines.Lights.Extensions
{
    internal static class ObjectExtensions
    {
        public static T SetHierarchyContext<T>(this T obj, string parentName, string name, bool enableLogging)
        {
            if (obj is IPipelineHierarchyContext loggingContext)
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
