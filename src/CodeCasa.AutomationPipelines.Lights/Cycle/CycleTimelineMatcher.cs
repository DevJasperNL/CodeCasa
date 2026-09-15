using CodeCasa.Lights;
using CodeCasa.Lights.Timelines.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Occurify;
using System.Reactive.Concurrency;

namespace CodeCasa.AutomationPipelines.Lights.Cycle;

internal static class CycleTimelineMatcher
{
    public static bool LightMatchesTimeline(ILight light, Dictionary<ITimeline, LightParameters> timeline, IServiceProvider serviceProvider)
    {
        var scheduler = serviceProvider.GetRequiredService<IScheduler>();
        var parametersNow = timeline.GetLightParametersAt(scheduler.Now.UtcDateTime);
        return parametersNow != null && LightParametersComparer.Tolerant.Equals(light.GetParameters(), parametersNow);
    }
}
