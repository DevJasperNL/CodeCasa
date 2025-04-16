﻿using System.Reactive.Concurrency;
using CodeCasa.AutoGenerated;
using CodeCasa.NetDaemon.Utilities;
using CodeCasa.Pipeline;
using NetDaemon.Extensions.Observables;
using Occurify;
using Occurify.Reactive.Extensions;
using Occurify.TimeZones;
using Reactive.Boolean;

namespace CodeCasa.Automations.Lights.BackyardLights;

public class BackyardLightsEnergySavingNode<TState> : PipelineNode<TState>
{
    public BackyardLightsEnergySavingNode(CoverEntities coverEntities, TState energySavingState, IScheduler scheduler)
    {
        var timeWindow = PeriodTimeline.Between(TimeZoneInstants.DailyAt(23), TimeZoneInstants.DailyAt(6)).ToBooleanObservable(scheduler);

        var livingRoomCurtainsClosed = coverEntities.LivingRoomBackWindowCurtain.ToOpenClosedObservable().Not();
        var livingRoomRollerShutterClosed = coverEntities.LivingRoomRollerShutter.ToOpenClosedObservable().Not();
        var anyLivingRoomShutterClosed = livingRoomCurtainsClosed.Or(livingRoomRollerShutterClosed);

        var kitchenRollerShutterClosed = coverEntities.KitchenRollerShutter.ToOpenClosedObservable().Not();

        timeWindow
            .And(anyLivingRoomShutterClosed)
            .And(kitchenRollerShutterClosed)
            .SubscribeTrueFalse(() => Output = energySavingState, DisableNode);
    }
}

public class BackyardStringLightsEnergySavingNode(CoverEntities coverEntities, IScheduler scheduler)
    : BackyardLightsEnergySavingNode<bool>(coverEntities, false, scheduler);

public class BackyardCoachLightsEnergySavingNode(CoverEntities coverEntities, IScheduler scheduler)
    : BackyardLightsEnergySavingNode<LightParameters>(coverEntities, LightParameters.Off(), scheduler);