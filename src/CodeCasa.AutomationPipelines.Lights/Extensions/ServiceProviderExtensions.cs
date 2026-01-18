using CodeCasa.AutomationPipelines.Lights.Context;
using CodeCasa.DependencyInjection.Extensions;
using CodeCasa.Lights;
using Microsoft.Extensions.DependencyInjection;

namespace CodeCasa.AutomationPipelines.Lights.Extensions;

internal static class ServiceProviderExtensions
{
    // public static T
    //     CreateInstanceWithinContext<T, TLight>(this IServiceProvider serviceProvider, TLight light) where TLight : ILight =>
    //     serviceProvider.CreateInstanceWithinContext<T, ILight>(new LightPipelineContext<ILight>(serviceProvider, light));
    //
    // public static T
    //     CreateInstanceWithinContext<T, TLight>(this IServiceProvider serviceProvider, ILightPipelineContext<TLight> context) where TLight : ILight =>
    //     (T)serviceProvider.CreateInstanceWithinContext(typeof(T), context);
    //
    // private static object CreateInstanceWithinContext<TLight>(this IServiceProvider serviceProvider, Type instanceType,
    //     ILightPipelineContext<TLight> context) where TLight : ILight
    // {
    //     serviceProvider.CreateContextScope(cb =>
    //     {
    //         cb.AddTransient(context);
    //     });
    //     var contextProvider = serviceProvider.GetRequiredService<LightPipelineContextProvider>();
    //     contextProvider.SetLightPipelineContext(context);
    //     var instance = ActivatorUtilities.CreateInstance(serviceProvider, instanceType);
    //     contextProvider.ResetLight();
    //     return instance;
    // }

    // public static T
    //     CreateInstanceWithinContext<T, TLight>(this IServiceProvider serviceProvider, ILightPipelineContext<TLight> context) where TLight : ILight =>
    //     (T)serviceProvider.CreateInstanceWithinContext(typeof(T), context);
    //
    // public static object CreateInstanceWithinContext<TLight>(this IServiceProvider serviceProvider, Type instanceType,
    //     ILightPipelineContext<TLight> context) where TLight : ILight
    // {
    //     var contextProvider = serviceProvider.GetRequiredService<LightPipelineContextProvider>();
    //     contextProvider.SetLightPipelineContext(context);
    //     var instance = ActivatorUtilities.CreateInstance(serviceProvider, instanceType);
    //     contextProvider.ResetLight();
    //     return instance;
    // }

    public static IServiceScope CreateLightContextScope<TLight>(this IServiceProvider serviceProvider, TLight light) where TLight : ILight
    {
        return serviceProvider.CreateContextScope(cb =>
        {
            cb.AddTransient(typeof(ILight), _ => light);

            if (
                typeof(TLight) != typeof(ILight) && // Only add the second registration if TLight isn't already ILight
                typeof(TLight).IsClass || typeof(TLight).IsInterface) // Check at runtime if TLight is a reference type
            {
                cb.AddTransient(typeof(TLight), _ => light);
            }
        });
    }
}