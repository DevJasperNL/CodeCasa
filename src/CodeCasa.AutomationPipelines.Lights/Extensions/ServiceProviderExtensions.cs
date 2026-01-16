using CodeCasa.AutomationPipelines.Lights.Context;
using CodeCasa.Lights;
using Microsoft.Extensions.DependencyInjection;

namespace CodeCasa.AutomationPipelines.Lights.Extensions;

internal static class ServiceProviderExtensions
{
    public static T
        CreateInstanceWithinContext<T, TLight>(this IServiceProvider serviceProvider, TLight light) where TLight : ILight =>
        serviceProvider.CreateInstanceWithinContext<T, ILight>(new LightPipelineContext<ILight>(serviceProvider, light));

    public static T
        CreateInstanceWithinContext<T, TLight>(this IServiceProvider serviceProvider, ILightPipelineContext<TLight> context) where TLight : ILight =>
        (T)serviceProvider.CreateInstanceWithinContext(typeof(T), context);

    public static object CreateInstanceWithinContext<TLight>(this IServiceProvider serviceProvider, Type instanceType,
        ILightPipelineContext<TLight> context) where TLight : ILight
    {
        var contextProvider = serviceProvider.GetRequiredService<LightPipelineContextProvider>();
        contextProvider.SetLightPipelineContext(context);
        var instance = ActivatorUtilities.CreateInstance(serviceProvider, instanceType);
        contextProvider.ResetLight();
        return instance;
    }
}