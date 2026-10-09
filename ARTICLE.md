<!--
PUBLISHING NOTES (strip this block before publishing)

Title:      Supercharging Home Assistant Automations: Layered Light Pipelines for NetDaemon
Alternates: I Dropped the Hue Bridge for Home Assistant and Had to Rebuild Everything It Did
            Stop Checking the Alarm in Every Automation: Layered Light Pipelines in C#
Tags:       homeassistant, csharp, dotnet, smarthome
Series:     part 2, follows "Initial States and Boolean Logic for NetDaemon Rx"
Canonical:  publish on jasperlammers.com first, set that URL as canonical_url on dev.to
Cover:      TODO

Every HTML comment starting with REVIEW needs a decision or your real code.
-->

## 📖 Table of Contents
- [Introduction](#introduction)
- [What You Lose Without the Bridge](#what-you-lose-without-the-bridge)
- [Rebuilding It in Automations](#rebuilding-it-in-automations)
- [Thinking in Layers](#thinking-in-layers)
- [Building a Room, One Line at a Time](#building-a-room-one-line-at-a-time)
    - [The Hallway](#the-hallway)
    - [The Office](#the-office)
    - [The Living Room](#the-living-room)
- [From Alarm to Notifications](#from-alarm-to-notifications)
- [Natural Light](#natural-light)
- [Seeing Which Layer Won](#seeing-which-layer-won)
- [Cheat Sheet](#cheat-sheet)
- [When Not to Use This](#when-not-to-use-this)
- [Conclusion](#conclusion)
- [Libraries](#libraries)

> 🔗 You can find the full source code of the library on [GitHub](https://github.com/DevJasperNL/CodeCasa). Every room in this article is taken from my [showcase repository](https://github.com/DevJasperNL/CodeCasa.Showcase), a trimmed-down copy of my own home automation.

## Introduction

My smart home started the way many do: with a Philips Hue starter kit. I screwed in the bulbs, stuck a dimmer switch to the wall and everything just worked. The switch cycled through scenes, holding a button dimmed the lights, and the motion sensor in the hallway knew the difference between day and night.

Then I discovered Home Assistant, and with it the freedom to connect my lights to everything else in the house. At some point I took the next step and retired the Hue Bridge altogether, pairing the bulbs, switches and sensors directly to my own Zigbee network through Zigbee2MQTT.

That is when I found out how much the bridge had been doing for me. All the behaviour I took for granted was gone, and rebuilding it turned out to be a lot more work than I expected. This article is about that work, about why it gets messy so quickly, and about the library I built to keep it manageable: **CodeCasa.AutomationPipelines.Lights**.

This is the second article in a series about patterns for [NetDaemon](https://netdaemon.xyz/). The [first one](https://dev.to/devjaspernl/supercharging-home-assistant-automations-initial-states-and-boolean-logic-for-netdaemon-rx-3bd5) introduced stateful boolean observables. They are the input for everything below, but you can follow along without having read it.

## What You Lose Without the Bridge

Once your Hue devices talk to Home Assistant directly, a dimmer switch is no longer a dimmer switch. It is a device that emits button events. A motion sensor is a binary sensor. What you do with them is entirely up to you, which sounds great until you write down what the bridge used to do:

- Pressing **on** repeatedly cycles through scenes, starting from the scene the lights are actually in.
- **Holding** a button dims or brightens smoothly until you let go.
- The motion sensor uses different scenes for **day and night**, and does nothing when the room is already in use.
- **Natural light** shifts colour temperature and brightness slowly over the day.
- A whole room changes **at the same moment**, not bulb by bulb.
- Turning a light off by hand **sticks**. The motion sensor does not turn it right back on.

> 💡 **Note:** If you keep the Hue Bridge and use Home Assistant's Hue integration, none of this applies. The bridge keeps running its own logic. This article is about the situation where Home Assistant is the only brain left.

## Rebuilding It in Automations

None of these features is hard to build on its own. Here is a motion automation with a day and night scene:

```yaml
- alias: Living room motion
  triggers:
    - trigger: state
      entity_id: binary_sensor.living_room_motion
      to: "on"
  actions:
    - choose:
        - conditions:
            - condition: state
              entity_id: sun.sun
              state: below_horizon
          sequence:
            - action: scene.turn_on
              target:
                entity_id: scene.living_room_nightlight
      default:
        - action: scene.turn_on
          target:
            entity_id: scene.living_room_bright
```

Add a second automation that turns the lights off two minutes after the last motion and you have a working room. Then you add the dimmer switch, and the first problem shows up. Someone picks the *Relax* scene, sits down to read, and two minutes later the motion automation turns the lights off. The fix is an `input_boolean.living_room_manual` helper that the switch automation sets and both motion automations check.

Now imagine you want the lights to turn red when the security alarm is triggered. Turning them red is one more automation. The real work is in everything else:

```yaml
  conditions:
    - condition: state
      entity_id: input_boolean.living_room_manual
      state: "off"
    - condition: not
      conditions:
        - condition: state
          entity_id: alarm_control_panel.home
          state: triggered
```

This block has to go into the motion-on automation, the motion-off automation and every button of the switch automation, because any of them would happily overwrite the red light. And when the alarm ends, what should the room look like? Whatever it *would* have looked like without the alarm, which no single automation knows.

There are workarounds. You can snapshot the lights with `scene.create` and restore them afterwards, but then you restore a moment from the past: if the sun set or everybody left during the alarm, the room comes back wrong. You can disable the other automations with `automation.turn_off`, but then the triggers that fired in the meantime are lost. Both work until they don't.

The underlying issue is that every automation writes to the light directly, and nothing decides who wins. Home Assistant gives you triggers, conditions and actions, but it has no built-in notion of priority between automations. So every cross-cutting concern ends up as a condition in all of them. What is missing is not another trigger. It is a model.

## Thinking in Layers

The model I ended up with is a pipeline per light. A pipeline is an ordered list of nodes, and each node represents one concern: motion, the switch, the alarm. A node receives the output of the node before it and then does one of two things. It either sets its own output, or it **passes through** what it received, which is its way of saying "I have no opinion right now".

```
  motion / night  ──►  night light, bright, or nothing
        │
  dimmer switch   ──►  a scene, or pass through
        │
  alarm           ──►  red, or pass through
        │
        ▼
      light            the only place the light is driven
```

<!-- REVIEW: replace the ASCII diagram with an image, or add a GIF of the room responding, if you have one. -->

Later nodes win over earlier ones, so the order of the list *is* the priority. The light is only ever driven from the end of the pipeline. No node knows about any other node. The motion layer keeps tracking motion while the alarm is active, so when the alarm passes through again, the room is immediately what it should be at that moment. Nothing needs to be restored.

The pipeline itself is a small generic library, `CodeCasa.AutomationPipelines`. On top of it, `CodeCasa.AutomationPipelines.Lights` adds a fluent API for lights, and a NetDaemon package adds overloads for Home Assistant entities and scenes.

If you have not come across NetDaemon before: it lets you write Home Assistant automations as C# applications, with entities exposed as strongly typed objects and state changes as observables. Setting up the pipelines takes one line in your startup code:

```csharp
services.AddLightPipelines();
```

## Building a Room, One Line at a Time

Everything in this chapter is taken from my [showcase repository](https://github.com/DevJasperNL/CodeCasa.Showcase), which runs a trimmed-down copy of my own house. Three rooms are enough to rebuild the list from the start of this article: the hallway, the office and the living room.

A word on the inputs first. Without the bridge, a Hue dimmer switch is a Zigbee2MQTT device that publishes `on_press`, `down_hold` and so on, and a wall module publishes `left_press`. In the showcase these are wrapped in small classes that expose observables: `HallwayWallSwitch` is an `IObservable<Unit>` that emits on every press, and `OfficeDimmerSwitch` exposes `OnOffPressed`, `ScenePressed`, `Dimming` and `Brightening`. Lights, scenes and sensors are the usual NetDaemon generated entities.

### The Hallway

#### Motion

The hallway has a Hue motion sensor, which Zigbee2MQTT exposes as an occupancy binary sensor and an illuminance sensor. `MotionSensor` from `CodeCasa.NetDaemon.Sensors.Composite` combines the two:

```csharp
public class HallwayMotionSensor(
    IScheduler scheduler,
    BinarySensorEntities binarySensorEntities,
    SensorEntities sensorEntities)
    : MotionSensor(scheduler, binarySensorEntities.HallwayMotionSensorOccupancy,
        sensorEntities.HallwayMotionSensorIlluminance);
```

The result is a stateful `IObservable<bool>`: it becomes `true` when there is movement and the room is dark, and stays `true` until a minute after the last movement. That one observable replaces the motion-on automation, the motion-off automation and the illuminance condition.

#### Day and Night

The light itself is one `SwitchWhen`. It takes the observable that activates the node and a second one that chooses between two outcomes:

```csharp
var night = PeriodTimeline.Between(AstroInstants.LocalSunsets, AstroInstants.LocalSunrises).ToBooleanObservable(scheduler);

lightPipelineFactory.SetupLightPipeline(lightEntities.HallwayLight, pipeline => pipeline
    .SwitchWhen(hallwayMotionSensor, night, LightParameters.NightLight, LightParameters.Bright)
```

`night` comes from [Occurify](https://dev.to/devjaspernl/occurify-fluent-time-modeling-for-net-developers-3e4n), which turns the period between sunset and sunrise into a boolean observable. While the motion sensor is `false` the node passes through, and since there is nothing before it the pipeline falls back to its default, which is off. Because both observables are stateful, this works at startup and at the moment the sun sets while somebody is in the hallway. The `choose` block from the YAML version is gone. Without the day and night distinction it would simply be `When(hallwayMotionSensor, LightParameters.Bright)`.

#### Bright for Five Minutes

The hallway also has a wall switch. Sometimes you want proper light for a while, to find something in the cupboard, and then you want the hallway to go back to doing its own thing. The switch gets its own node, placed after the motion node, so it wins:

```csharp
    .AddInteractionNode(node => node
        .AddToggle(hallwayWallSwitch, sp => sp.CreateAutoPassThroughLightNode(LightParameters.Bright, TimeSpan.FromMinutes(5)))));
```

`AddToggle` decides from the actual state of the light. If it is off, a press activates the node: bright, for five minutes. After that the node does not turn the light off. It starts passing through. If there is still motion, the hallway falls back to the motion scene. If not, it goes dark. If the light is already on when the switch is pressed, from motion or from an earlier press, it turns off and stays off until the motion layer changes. There is also a `CreateAutoOffLightNode` for the cases where time up has to mean off.

`AddInteractionNode` does one more thing: it watches the light itself. When somebody turns it off from outside the pipeline, with the Home Assistant app for example, the node holds it off until something earlier in the pipeline changes. Turning a light off by hand sticks, and there is no `input_boolean.hallway_manual`.

That is the whole hallway, in [one file](https://github.com/DevJasperNL/CodeCasa.Showcase/blob/main/src/CodeCasa.Automations/Apps/Lights/Hallway/HallwayLightsApp.cs).

### The Office

#### The Wall Switch and the Dimmer

The office has a wall switch and a Hue dimmer switch, and no motion sensor; I sit still for long enough that motion would be wrong more often than right. The complete pipeline:

```csharp
lightPipelineFactory.SetupLightPipeline(lightEntities.OfficeLights, pipeline => pipeline
    .UseLightGroup(lightEntities.OfficeLightsZ2m)
    .AddInteractionNode(node => node
        .AddToggle(officeWallSwitch, LightParameters.Bright)
        .AddToggle(officeDimmerSwitch.OnOffPressed, LightParameters.Bright)
        .AddCycle(officeDimmerSwitch.ScenePressed, LightParameters.Bright, LightParameters.Concentrate, LightParameters.Relax)
        .AddReactiveDimmer(officeDimmerSwitch)
        .TurnOffWhenLastPersonToAsleepOrAway()));
```

There is a lot of Hue behaviour in these few lines:

- The two `AddToggle` calls make the wall switch and the **on** button of the dimmer toggle between off and bright.
- `AddCycle` is the scene button. On every press it looks at the **actual state** of the lights to decide which scene comes next, with some tolerance, because a bulb asked for 255 will happily report 254. If the state is not recognised, the cycle starts at the first scene. `LightParameters.Bright` and friends are built-in parameter sets; the living room below uses Home Assistant scenes instead.
- `AddReactiveDimmer` implements hold to dim. It steps the brightness every 400 ms while a button is held, and resets when a new scene is activated. The dimmer itself is a tiny interface with two observables, `Dimming` and `Brightening`, which the switch wrapper implements.
- `TurnOffWhenLastPersonToAsleepOrAway` is not from the library. It is a [ten-line extension method](https://github.com/DevJasperNL/CodeCasa.Showcase/blob/main/src/CodeCasa.CustomEntities.Automation/Extensions/LightTransitionReactiveNodeConfiguratorExtensions.cs) in the showcase, built on `AddNodeSource`, the primitive that all of the above is sugar for. Your own concerns slot in next to the built-in ones.

#### One Command for the Whole Room

One Hue feature is left. `lightEntities.OfficeLights` is a Home Assistant group of three bulbs. The factory flattens it and builds one pipeline per bulb, so a scene change results in three separate Zigbee commands, and you can see the bulbs change one after another. The bridge avoided this by addressing the Zigbee group. `UseLightGroup`, the first line of the pipeline, does the same with the Zigbee2MQTT group entity of the office.

Each bulb's output is held back for a very short window, 20 ms by default. If all members of the group ask for the same thing within that window, a single command goes to the group. If they differ, for instance because a scene gives every bulb its own colour, each bulb is driven individually. You get the synchronised look when it is possible and the per-bulb control when it is needed.

### The Living Room

#### Home Assistant Scenes

The living room scenes were made in the Home Assistant scene editor, with a different colour per bulb, and I wanted to keep them. The NetDaemon package accepts scene entities everywhere light parameters are accepted:

```csharp
lightPipelineFactory.SetupLightPipeline(lightEntities.LivingRoomLights, pipeline => pipeline
    .UseLightGroup(lightEntities.LivingRoomLightsZ2m)
    .AddInteractionNode(node => node
        .AddToggle(livingRoomWallSwitch,
            sceneEntities.LivingRoomAmbiance,
            sceneEntities.LivingRoomRelax,
            sceneEntities.LivingRoomBright)
        .TurnOffWhenLastPersonToAsleepOrAway())
    .AddNotifications());
```

A toggle with several entries is the Hue **on** button: the first press gives the first scene, quick consecutive presses walk through the others, and a press after a pause turns the lights off. Each scene is read from Home Assistant once, and each bulb's pipeline picks its own part of it.

The last line, `AddNotifications`, is where the alarm from the YAML chapter ends up.

## From Alarm to Notifications

The YAML chapter ended with the alarm: one more automation to turn the lights red, and a condition in every other automation to keep them red. In a pipeline the alarm would be one last node, `When(alarmTriggered, red)`, that wins over everything and knows nothing about motion, switches or timeouts.

But an alarm is just one reason to take over the lights for a while. The doorbell, the washing machine and the smoke detector are others, and none of them belong in the living room code. For this there is a separate package, `CodeCasa.Notifications.Lights`. Register it next to the pipelines:

```csharp
services.AddLightPipelines();
services.AddLightNotifications();
```

Every pipeline that should show notifications opts in with the line you saw in the living room:

```csharp
    .AddNotifications();
```

Any other application can now raise a notification, without knowing which lights or pipelines exist. In the showcase the doorbell plays that role:

```csharp
[NetDaemonApp]
internal class DoorbellLightNotifications
{
    public DoorbellLightNotifications(
        FrontDoorDoorbell frontDoorDoorbell,
        LightNotificationContext lightNotifications,
        IScheduler scheduler)
    {
        const string notificationId = nameof(DoorbellLightNotifications);

        frontDoorDoorbell.BellPressed
            .PersistTrueFor(TimeSpan.FromSeconds(30), scheduler)
            .SubscribeTrueFalse(
                () => lightNotifications.Notify(notificationId, sp => new BlinkNode(scheduler, Color.Blue, Color.White), priority: 10),
                () => lightNotifications.RemoveNotification(notificationId));
    }
}
```

`BellPressed` is a binary sensor that is only on for a moment; `PersistTrueFor` stretches it to thirty seconds. A notification is a node factory, so it is not limited to a fixed colour. The alarm would be `sp => new LightParameters { Brightness = 255, RgbColor = Color.Red }.AsPipelineNode(scheduler)` with a higher priority. `BlinkNode` is a custom node, and writing one is not much more work:

```csharp
public class BlinkNode : LightTransitionNode
{
    private readonly IDisposable _timer;

    public BlinkNode(IScheduler scheduler, Color firstColor, Color secondColor, TimeSpan? interval = null) : base(scheduler)
    {
        Name = "Blink";
        LightTransition[] states =
        [
            new LightParameters { Brightness = 255, RgbColor = firstColor }.AsTransition(),
            new LightParameters { Brightness = 255, RgbColor = secondColor }.AsTransition()
        ];

        Output = states[0];
        _timer = Observable.Interval(interval ?? TimeSpan.FromSeconds(1), scheduler)
            .Subscribe(tick => Output = states[(int)((tick + 1) % 2)]);
    }

    public override ValueTask DisposeAsync()
    {
        _timer.Dispose();
        return base.DisposeAsync();
    }
}
```

A node sets `Output`, or calls `PassInputThrough()` when it has no opinion. The base class serialises those changes, so setting the output from a timer is safe. When several notifications are active, the one with the highest priority is shown. When the last one is removed, every pipeline disposes its copy of the node, the notification layer passes through, and the room shows whatever the layers below have worked out in the meantime. If everybody left while the doorbell was blinking, the lights go off. If somebody picked a scene, that scene appears.

## Natural Light

Hue's natural light scene can be rebuilt with [Occurify](https://dev.to/devjaspernl/occurify-fluent-time-modeling-for-net-developers-3e4n), my library for working with time. A timeline maps moments of the day to light parameters, and the pipeline interpolates between them continuously. This is the attic in the showcase, where the wall switch toggles a timeline instead of a fixed scene:

```csharp
Action<ITimelineConfigurator> naturalLight = tl => tl
    .Add(AstroInstants.LocalSunrises, LightParameters.Bright)
    .Add(AstroInstants.LocalSunsets.OffsetHours(-2), LightParameters.Bright)
    .Add(AstroInstants.LocalSunsets, LightParameters.Dimmed)
    .Add(TimeZoneInstants.DailyAt(5), LightParameters.Dimmed);

lightPipelineFactory.SetupLightPipeline(lightEntities.AtticLights, pipeline => pipeline
    .UseLightGroup(lightEntities.AtticLightsZ2m)
    .AddInteractionNode(node => node
        .AddToggle(atticWallSwitch, naturalLight)
```

From sunrise until two hours before sunset the attic is bright. Over the last two hours of daylight it drifts to dimmed, slowly enough that nobody notices, stays dimmed until five in the morning, and ramps back up to bright at sunrise. The timeline is a continuous ramp, which is why every value appears twice: adding the same parameters at two moments is how you hold them in between. Timelines are accepted almost everywhere parameters are: `When(motion, naturalLight)` for a motion layer, or as an entry in a toggle or cycle.

## Seeing Which Layer Won

With several layers, "why is this light on?" becomes a fair question. Every pipeline can log each hop between nodes:

```csharp
    .EnableLogging("Hallway")
```

The hallway pipeline above does this. The log shows the light, the path of the node and the value it passed on, so you can read which layer produced the final result. For something more visual, there is a package that publishes the same telemetry over MQTT.

## Cheat Sheet

| Hue behaviour | Pipeline equivalent |
|---|---|
| Button cycles through scenes | `AddCycle(trigger, scenes...)` |
| Button toggles on and off | `AddToggle(trigger, ...)` |
| Hold to dim or brighten | `AddReactiveDimmer(dimmer)`, `AddDimmer(dimmer)` |
| Motion with day and night scenes | `SwitchWhen(motion, isNight, night, day)` |
| Natural light | `AddToggle(trigger, timeline)`, `When(observable, timeline)` |
| Whole room changes at once | `UseLightGroup(group)` |
| Manual off sticks | `AddInteractionNode()` |
| Temporary override | `CreateAutoPassThroughLightNode`, `CreateAutoOffLightNode` |
| Doorbell, alarm, anything that takes the lights over | `AddNotifications()` + `LightNotificationContext.Notify(...)` |
| Your own concern (everyone asleep, nobody home) | `AddNodeSource(...)` in an extension method |
| Cap brightness at night | `LimitBrightnessWhen(isNight, 100)` |
| Bulb was unreachable for a moment | re-applied automatically when it comes back |

## When Not to Use This

This approach is not for everyone, and I would rather say so here than in the comments.

- You need to be comfortable with C# and willing to run NetDaemon next to Home Assistant. If YAML or the automation editor covers your needs, stay there.
- If you still have a Hue Bridge and are happy with it, keep it. It does all of this out of the box.
- Scenes are read from Home Assistant once and cached. After editing a scene, you restart your apps.
- `UseLightGroup` requires the pipeline's lights to be exactly the members of the group.
- The library works with an `IScheduler` that you register yourself. It makes everything testable with virtual time, but it is one more thing to set up.

## Conclusion

Moving away from the Hue Bridge showed me how much behaviour sits in a product that "just works". Rebuilding that behaviour was not the hard part. Keeping the pieces from getting in each other's way was.

Separating concerns into layers changed how I think about my home. Adding the alarm was one line, and removing it again would be one line too. Every new idea since has been a question of where in the pipeline it belongs, not which existing automations need another condition.

## Libraries

All libraries in this article are open source under the MIT license and available on NuGet:

- [CodeCasa.AutomationPipelines.Lights](https://www.nuget.org/packages/CodeCasa.AutomationPipelines.Lights): the light pipelines
- [CodeCasa.AutomationPipelines.Lights.NetDaemon](https://www.nuget.org/packages/CodeCasa.AutomationPipelines.Lights.NetDaemon): overloads for NetDaemon entities and scenes
- [CodeCasa.Notifications.Lights](https://www.nuget.org/packages/CodeCasa.Notifications.Lights): notifications as a pipeline layer
- [CodeCasa.NetDaemon.Extensions.Observables](https://www.nuget.org/packages/CodeCasa.NetDaemon.Extensions.Observables): the boolean observables from part one
- [CodeCasa.NetDaemon.Sensors.Composite](https://www.nuget.org/packages/CodeCasa.NetDaemon.Sensors.Composite): the motion sensor that combines occupancy and illuminance

You can explore the full source code on [GitHub](https://github.com/DevJasperNL/CodeCasa). The rooms in this article live in the [CodeCasa.Showcase](https://github.com/DevJasperNL/CodeCasa.Showcase) repository, together with the dashboard and the other automations of my house.

If you find CodeCasa helpful or interesting, a ⭐️ on the repo would mean a lot!
