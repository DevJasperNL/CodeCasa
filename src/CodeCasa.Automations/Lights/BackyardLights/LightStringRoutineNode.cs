using System.Reactive.Concurrency;
using CodeCasa.Pipeline;
using Occurify;
using Occurify.Astro;
using Occurify.Extensions;
using Occurify.Reactive.Extensions;
using Occurify.TimeZones;
using Reactive.Boolean;

namespace CodeCasa.Automations.Lights.BackyardLights;

public class LightStringRoutineNode<TState> : PipelineNode<TState>
{
    public LightStringRoutineNode(IScheduler scheduler, TState onState, TimeSpan turnOnOffset)
    {
        var eveningAndNight = PeriodTimeline.Between(AstroInstants.LocalSunsets.Offset(turnOnOffset), TimeZoneInstants.DailyAt(2)).ToBooleanObservable(scheduler);
        var morning = TimeZonePeriods.DailyBetween(TimeZoneInstants.DailyAt(6).Offset(turnOnOffset), AstroInstants.LocalSunrises).ToBooleanObservable(scheduler);

        eveningAndNight.Or(morning).SubscribeTrueFalse(() => Output = onState, DisableNode);
    }
}