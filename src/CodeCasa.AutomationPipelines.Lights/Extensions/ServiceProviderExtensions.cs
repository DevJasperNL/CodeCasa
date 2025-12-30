using CodeCasa.AutomationPipelines.Lights.Context;
using CodeCasa.Lights;
using Microsoft.Extensions.DependencyInjection;

namespace CodeCasa.AutomationPipelines.Lights.Extensions;

internal static class ServiceProviderExtensions
{
    public static T
        CreateInstanceWithinContext<T>(this IServiceProvider serviceProvider, ILight light) =>
        serviceProvider.CreateInstanceWithinContext<T>(new LightPipelineContext(serviceProvider, light));

    public static T
        CreateInstanceWithinContext<T>(this IServiceProvider serviceProvider, ILightPipelineContext context) =>
        (T)serviceProvider.CreateInstanceWithinContext(typeof(T), context);

    public static object CreateInstanceWithinContext(this IServiceProvider serviceProvider, Type instanceType,
        ILightPipelineContext context)
    {
        var contextProvider = serviceProvider.GetRequiredService<LightPipelineContextProvider>();
        contextProvider.SetLightPipelineContext(context);
        var instance = ActivatorUtilities.CreateInstance(serviceProvider, instanceType);
        contextProvider.ResetLight();
        return instance;
    }
}