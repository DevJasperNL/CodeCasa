# CodeCasa

[![GitHub license](https://img.shields.io/github/license/DevJasperNL/CodeCasa?label=License)](https://github.com/DevJasperNL/CodeCasa?tab=MIT-1-ov-file)
[![GitHub release](https://img.shields.io/github/v/release/DevJasperNL/CodeCasa?label=Release)](https://github.com/DevJasperNL/CodeCasa/releases/latest)
[![Build Status](https://github.com/DevJasperNL/CodeCasa/actions/workflows/ci-build-and-test.yml/badge.svg)](https://github.com/DevJasperNL/CodeCasa/actions/workflows/ci-build-and-test.yml)

A collection of .NET libraries providing [NetDaemon](https://github.com/net-daemon/netdaemon) extensions alongside general purpose smart home automation utilities.

## 📖 Table of Contents

- [Available Libraries](#available-libraries)
- [CodeCasa.Lights](#codecasalights)
  - [Overview](#overview)
  - [Usage](#usage)
    - [Working with Light Parameters](#working-with-light-parameters)
    - [Light Transitions](#light-transitions)
    - [Blending Parameters](#blending-parameters)
    - [Using Light Scenes](#using-light-scenes)
    - [Inspecting Light State](#inspecting-light-state)
    - [Working with Light Groups](#working-with-light-groups)
- [CodeCasa.AutomationPipelines](#codecasaautomationpipelines)
  - [Why use AutomationPipelines?](#why-use-automationpipelines)
  - [Real-World Usage](#real-world-usage)
  - [Example](#example)
- [CodeCasa.AutomationPipelines.Lights](#codecasaautomationpipelineslights)
  - [Overview](#overview-1)
  - [Usage](#usage-1)
    - [Basic Light Pipeline Setup](#basic-light-pipeline-setup)
    - [Controlling with Dimmers](#controlling-with-dimmers)
    - [Working with Light Groups](#working-with-light-groups)
    - [NetDaemon Integration](#netdaemon-integration)
- [CodeCasa.Lights.NetDaemon](#codecasalightsnetdaemon)
  - [Overview](#overview-2)
  - [Usage](#usage-2)
    - [Applying Transitions to NetDaemon Lights](#applying-transitions-to-netdaemon-lights)
    - [Automatic Color Mode Detection](#automatic-color-mode-detection)
    - [Getting Current Light State](#getting-current-light-state)
    - [Working with Light Groups](#working-with-light-groups-1)
- [CodeCasa.NetDaemon.Notifications.Phone](#codecasanetdaemonnotificationsphone)
  - [Usage](#usage-3)
- [CodeCasa.NetDaemon.Notifications.InputSelect](#codecasanetdaemonnotificationsinputselect)
  - [Usage](#usage-4)
- [CodeCasa.NetDaemon.RuntimeState](#codecasanetdaemonruntimestate)
  - [Runtime States](#runtime-states)
  - [Usage](#usage-5)
- [CodeCasa.NetDaemon.TypedEntities](#codecasanetdaemontypedentities)
  - [Usage](#usage-6)
    - [Base Class (optional)](#base-class-optional)
    - [Concrete Wrapper Example](#concrete-wrapper-example)
    - [Another Example: PersonEntity](#another-example-personentity)
- [CodeCasa.NetDaemon.Extensions.Observables](#codecasanetdaemonextensionsobservables)
  - [Example Usage](#example-usage)
  - [Boolean Observables](#boolean-observables)
  - [Scheduling](#scheduling)
    - [OnForAtLeast](#onforatleast)
    - [PersistOnFor](#persistonfor)
    - [WhenOnFor](#whenonfor)
    - [LimitOnDuration](#limitonduration)
  - [Unavailability of entities](#unavailability-of-entities)

## Available Libraries

Package | Description
--- |---
[CodeCasa.Lights](#codecasalights) | Core light management library providing advanced color handling, simpler state representation, and generic scenes—platform agnostic.
[CodeCasa.AutomationPipelines](#codecasaautomationpipelines) | Composable, reactive, and layered logic pipelines for automation.

#### NetDaemon
Package | Description
--- |---
[CodeCasa.Lights.NetDaemon](#codecasalightsnetdaemon) | NetDaemon integration for light entities, enabling Home Assistant light control with `CodeCasa.Lights` abstractions.
[CodeCasa.NetDaemon.Notifications.Phone](#codecasanetdaemonnotificationsphone) | This library provides the `PhoneNotificationEntity` class, making it easy to create, update, and manage phone notifications in Home Assistant.
[CodeCasa.NetDaemon.Notifications.InputSelect](#codecasanetdaemonnotificationsinputselect) | This library helps you turn a Home Assistant Dropdown/InputSelect helper entity into a dynamic notifications list.
[CodeCasa.NetDaemon.RuntimeState](#codecasanetdaemonruntimestate) | This library provides the `NetDaemonRuntimeStateService`, which allows you to check and subscribe to the runtime state of `NetDaemon`.
[CodeCasa.NetDaemon.TypedEntities](#codecasanetdaemontypedentities) | Strongly-typed wrappers for Home Assistant entities in `NetDaemon`.
[CodeCasa.NetDaemon.Extensions.Observables](#codecasanetdaemonextensionsobservables) | Collection of extension methods meant to enhance NetDaemon entities with boolean observables allowing for a more intuitive coding experience.

## CodeCasa.Lights

Core library providing platform-agnostic light management abstractions.

**Features include:**

- **ILight Interface** – Platform-agnostic abstraction for light entities, enabling code that works with any light implementation.
- **LightParameters** – A simple, state-focused representation of light configuration (brightness, RGB color, color temperature).
- **LightTransition** – Combines `LightParameters` with optional transition duration for smooth color or brightness changes.
- **Light Scene Templates** – Pre-built scenes (Relax, NightLight, Concentrate, Bright, Dimmed) that adapt to any light's capabilities.
- **Light Utilities** – Extension methods for inspecting and manipulating light state, including flattening light hierarchies.

### Overview

The main goal of this library is to abstract away the complexity of Home Assistant's light entity API. Instead of dealing with device-specific capabilities and conditional logic in your automations, you work with `LightParameters` that describe *what* state you want—and the library handles *how* to achieve it.

For example, you can apply the same "Relax" scene to any light—whether it supports color temperature, RGB colors, or only brightness—and get appropriate results for each device. Similarly, you can create a transition from one set of parameters to another without worrying about whether the light supports color modes or requires fallback brightness handling.

### Usage

#### Working with Light Parameters

Create light parameters to represent desired states:

```cs
var brightParams = new LightParameters { Brightness = 255 };
var warmDimmed = new LightParameters { ColorTemp = 366, Brightness = 100 };
var colorful = new LightParameters { RgbColor = Color.Red, Brightness = 200 };
```

Use predefined states for off and on:

```cs
var off = LightParameters.Off();
var on = LightParameters.On();   // Useful for binary lights
```

#### Light Transitions

Convert parameters to transitions and apply them:

```cs
var transition = brightParams.AsTransition();
kitchenLight.TurnOn(transition);

// With a custom transition duration:
var smoothTransition = warmDimmed.AsTransition(TimeSpan.FromSeconds(3));
bedroomLight.TurnOn(smoothTransition);
```

#### Blending Parameters

Blend two sets of parameters together for smooth transitions or animations:

```cs
var morning = new LightParameters { ColorTemp = 366, Brightness = 100 };
var evening = new LightParameters { RgbColor = Color.Red, Brightness = 200 }; // Blending is possible between different parameter types.

var transitionPercent = 0.7; // 70% towards evening
var blended = morning.Interpolate(evening, transitionPercent);
livingRoomLight.TurnOn(blended.AsTransition());
```

#### Using Light Scenes

Apply pre-built scene templates that adapt to each light's capabilities:

```cs
officeLight.TurnOn(LightSceneTemplates.Bright);
bedroomLight.TurnOn(LightSceneTemplates.NightLight);
livingRoomLight.TurnOn(LightSceneTemplates.Relax);
```

Apply a scene to all lights in a collection:

```cs
foreach (var light in lightEntities.EnumerateAll())
{
    light.TurnOn(LightSceneTemplates.Concentrate);
}
```

#### Inspecting Light State

Get the current parameters of a light:

```cs
var currentState = kitchenLight.GetLightParameters();
Console.WriteLine($"Brightness: {currentState.Brightness}, Color Temp: {currentState.ColorTemp}");

var brightness = bedroomLight.GetBrightness();
```

#### Working with Light Groups

Flatten light groups to get individual entities:

```cs
var groupLight = lightEntities.AllLights; // A light group entity
var individualLights = groupLight.Flatten();

// Now apply transitions to each light separately
foreach (var light in individualLights)
{
    light.ExecuteLightTransition(warmDimmed.AsTransition());
}
```

**Available Scenes:**
- **Relax** – Warm color temperature with medium brightness for relaxing environments
- **NightLight** – Very warm color temperature with very low brightness for bedtime
- **Concentrate** – Cool/neutral color temperature with high brightness for focus
- **Bright** – Warm color temperature with maximum brightness for general illumination
- **Dimmed** – Warm color temperature with low brightness for ambient lighting

## CodeCasa.AutomationPipelines

Composable, reactive, and layered logic pipelines for automation.

In complex systems, multiple conditions often need to influence a single behavior. Instead of funneling all logic into a single block, **AutomationPipelines** encourages clean separation of concerns using a flexible, node-based pipeline model.

At its core is the `Pipeline<TState>` class, which coordinates a chain of self-contained logic units (`IPipelineNode<TState>`). Each node can independently process input, produce output, or opt out entirely—while maintaining full support for dependency injection.

### Why use AutomationPipelines?

* **Modular Logic**: Each pipeline node is a focused class with a single responsibility.
* **Dependency Injection Support**: Nodes can depend on external services, entities, or sensors—perfect for reactive systems like NetDaemon or Home Assistant.
* **Prioritized Overrides**: Later nodes in the pipeline can override the output of earlier ones, enabling intuitive layering and override mechanisms.
* **Reactive and Self-Contained**: Nodes are notified when input changes and can independently decide their output—or disable themselves to pass values through untouched.

This approach leads to cleaner, more maintainable automation logic—especially in event-driven or rules-based environments.

### Real-World Usage

This library was designed with automation platforms like [NetDaemon](https://netdaemon.xyz/) in mind but is suitable for any scenario where behavior is built from layered rules and inputs.

Check out [CodeCasa on GitHub](https://github.com/DevJasperNL/CodeCasa) for real-world examples and usage patterns.

### Example

```csharp
[NetDaemonApp]
internal class PipelineTest
{
    public PipelineTest(IPipeline<string> messagesPipeline)
    {
        messagesPipeline
            .RegisterNode<DefaultMessageNode>()
            .RegisterNode<OfficeLightsMessageNode>()
            .RegisterNode<CherryOnTopMessageNode>()
            .SetOutputHandler(Console.WriteLine);
    }
}

// Provides a default message when no other conditions are met
public class DefaultMessageNode : PipelineNode<string>
{
    public DefaultMessageNode()
    {
        Output = "Default message";
    }
}

// Overrides message if the office lights are turned on
public class OfficeLightsMessageNode : PipelineNode<string>
{
    public OfficeLightsMessageNode(LightEntities lightEntities)
    {
        lightEntities.OfficeLights.SubscribeOnOff(
            () => Output = "ON",
            DisableNode
        );
    }
}

// Modifies the incoming message by appending a "cherry on top"
public class CherryOnTopMessageNode : PipelineNode<string>
{
    protected override void InputReceived(string? input)
    {
        Output = string.IsNullOrEmpty(input)
            ? "Just a cherry"
            : $"{input} with a cherry on top";
    }
}
```

In this example:

* The `DefaultMessageNode` sets an initial message.
* The `OfficeLightsMessageNode` conditionally overrides it based on light state.
* The `CherryOnTopMessageNode` decorates the result with additional context.

The output handler is called whenever the final result changes, keeping logic reactive and centralized.

## CodeCasa.AutomationPipelines.Lights

Light automation pipeline extensions applying the **AutomationPipelines** pattern specifically to light control logic.

**Features include:**

- **Composable Light Pipelines** – Chain light control nodes together to build sophisticated automation logic.
- **Reactive Nodes** – Automatically respond to observable state changes (e.g., motion detection, time-based conditions).
- **Conditional Logic** – Use `When()` and `Switch()` operators to apply light parameters based on boolean observables.
- **Dimmer Integration** – Control brightness progression through dimmer devices within the pipeline.
- **Light Groups** – Scope pipeline nodes to specific lights or light groups.
- **Nested Pipelines** – Compose pipelines hierarchically for modular, maintainable light automation.

### Overview

This library extends the core `CodeCasa.AutomationPipelines` with light-specific pipeline nodes and configurators. Instead of writing conditional logic for individual lights, you can build declarative pipelines that automatically respond to changes in your environment.

For example, a single pipeline can orchestrate multiple conditions—motion detection, time of day, presence—and translate them into appropriate light settings, all while remaining easy to read and modify.

### Usage

#### Basic Light Pipeline Setup

Register the light pipeline services:

```cs
serviceCollection.AddLightPipelines();
```

Set up a light pipeline for a single light:

```cs
[NetDaemonApp]
internal class LightAutomation
{
    public LightAutomation(LightPipelineFactory lightPipelineFactory, LightEntities lightEntities)
    {
        lightPipelineFactory.SetupLightPipeline(lightEntities.LivingRoomLight, pipeline =>
        {
            pipeline
                .SetName("Living Room Automation")
                .When(motionDetected, new LightParameters { Brightness = 255 })
                .When(timeIsNight, LightSceneTemplates.NightLight)
                .TurnOffWhen(nobodyHome);
        });
    }
}
```

#### Controlling with Dimmers

Add dimmer control to modulate brightness within the pipeline:

```cs
lightPipelineFactory.SetupLightPipeline(lightEntities.KitchenLight, pipeline =>
{
    pipeline
        .When(morningTime, LightSceneTemplates.Bright)
        .AddDimmer(kitchenDimmer, dimOptions =>
        {
            dimOptions.MinBrightness = 50;
            dimOptions.MaxBrightness = 255;
        });
});
```

#### Working with Light Groups

Configure pipelines for multiple lights or light groups:

```cs
lightPipelineFactory.SetupLightPipeline(lightEntities.AllBedroomLights, pipeline =>
{
    pipeline
        .ForLights(new[] { "light.bedroom_main", "light.bedroom_accent" }, subPipeline =>
        {
            subPipeline
                .When(bedtimeRoutine, LightSceneTemplates.NightLight)
                .AddDimmer(bedroomDimmer);
        })
        .TurnOffWhen(allAsleep);
});
```

### NetDaemon Integration

For NetDaemon-based automations, use the `SetupLightPipeline` extension method:

```cs
lightPipelineFactory.SetupLightPipeline(lightEntity, pipeline =>
{
    // Configure pipeline for NetDaemon light entity
});
```

This adapts NetDaemon light entities to the pipeline framework automatically.

## CodeCasa.NetDaemon.Notifications.Phone

This library provides the `PhoneNotificationEntity` class, making it easy to create, update, and manage phone notifications in Home Assistant.

**Features include:**

- Easy notification creation and updating.
- Support for actionable notifications with customizable buttons.
- Support for Android and iOS.

### Usage

Define an entity for a specific phone:
```cs
public class JasperPhoneNotifications(NotifyServices notificationServices, IHaContext haContext)
    : PhoneNotificationEntity(haContext, notificationServices.MobileAppPixel7);
```

Register the entity as a service:
```cs
serviceCollection.AddTransient<JasperPhoneNotifications>();
```

Use it to send notifications (with optional actions):
```cs
[NetDaemonApp]
internal class Example
{
    public Example(
        LightEntities lightEntities,
        JasperPhoneNotifications jasperPhoneNotifications)
    {
        var notificationId = $"{nameof(Example)}_Notification"; // Note: Using an ID that is consistent between runs also ensures that old notifications are removed/replaced on phones when the app is reloaded.
        lightEntities.OfficeLights.SubscribeOnOff(
            () =>
            {
                jasperPhoneNotifications.Notify(new AndroidNotificationConfig
                {
                    Message = "Hey Jasper, the office lights are on!",
                    StatusBarIcon = "mdi:lightbulb",
                    Actions =
                    [
                        new(() => lightEntities.OfficeLights.TurnOff(), "Click here to turn them off.")
                    ]
                }, notificationId);
            },
            () => jasperPhoneNotifications.RemoveNotification(notificationId));
    }
}
```

This automation sends a notification to Jasper’s phone whenever the office lights are turned on. The notification includes a button that allows him to turn off the lights directly from the notification:

![Screenshot of notification shown on phone](img/phone_notification.png "Phone Notification")

> For a more advanced demo like the example below, check out: https://github.com/DevJasperNL/CodeCasa ![Gif demonstrating phone notifications](img/phone_notification_demo.gif "Phone Notifications")


## CodeCasa.Lights.NetDaemon

NetDaemon integration for light entities, enabling Home Assistant light control with `CodeCasa.Lights` abstractions.

**Features include:**

- **NetDaemonLight** – Adapter implementing `ILight` for NetDaemon light entities.
- **LightEntityExtensions** – Extension methods for Home Assistant light entities enabling parameter-based control.
- **ColorMode Handling** – Automatic translation of `LightParameters` to appropriate Home Assistant light API calls.
- **Transition Support** – Smooth transitions between light states with optional duration.

> Please note that this library is primarily designed for my own use. It may contain untested edge cases and might not work with all types of light devices.

### Overview

This library bridges `CodeCasa.Lights` abstractions with NetDaemon's Home Assistant light entities. It handles the complexity of Home Assistant's color modes and provides a simpler interface for controlling lights in NetDaemon automations.

### Usage

#### Applying Transitions to NetDaemon Lights

Use extension methods to apply transitions directly:

```cs
var transition = new LightParameters 
{ 
    ColorTemp = 366, 
    Brightness = 100 
}.AsTransition(TimeSpan.FromSeconds(2));

lightEntity.TurnOn(transition);
```

#### Automatic Color Mode Detection

The library automatically selects the appropriate Home Assistant API based on light capabilities:

```cs
var params = new LightParameters { RgbColor = Color.Red, Brightness = 200 };
lightEntity.TurnOn(params);

// For RGB lights, uses color mode. For temperature-only lights, falls back to color_temp.
```

#### Getting Current Light State

Retrieve the current parameters of a light:

```cs
var currentState = lightEntity.GetLightParameters();
Console.WriteLine($"Brightness: {currentState.Brightness}");
Console.WriteLine($"Color Temp: {currentState.ColorTemp}");
```

#### Working with Light Groups

Flatten and process light groups:

```cs
var flattened = lightEntity.Flatten();
foreach (var light in flattened)
{
    light.ApplyTransition(LightSceneTemplates.Bright.AsTransition());
}

## CodeCasa.NetDaemon.Notifications.InputSelect

This library helps you turn a Home Assistant Dropdown/InputSelect helper entity into a dynamic notifications list.

I always liked Android’s dynamic notifications and wished for something similar in Home Assistant. While HA has a built‑in notification panel, it’s limited and somewhat hidden away. I wanted to display rich, actionable notifications directly on my own dashboard—without too much hassle.

Originally built as a quick solution, this implementation worked so well that I decided to share it. It stores notifications as JSON entries in an `input_select` entity, including fields like message, icon, color, and actions. The library handles adding, updating, removing, and even executing actions for you.

One of the biggest upsides of this approach is that it keeps everything inside Home Assistant itself: your notifications persist in the state of the `input_select entity`, surviving reboots or refreshes.

You can render these notifications in a custom UI (like my [Blazor dashboard](https://github.com/DevJasperNL/CodeCasa)) or even with [native Home Assistant templating](docs/home-assistant-dashboard-setup.md).
By default, the library uses a standard JSON format for entries, but you can define your own if needed.

### Usage

First, update your `appsettings.json` with an `InputSelectNotificationEntities` array containing the input select entities you want to use as notification lists.
Optionally, you can add an `InputNumberEntityId` to store the current notification count (handy for templating in dashboards):

```json
"InputSelectNotificationEntities": [
  {
    "InputSelectEntityId": "input_select.living_room_panel_notifications"
  },
  {
    "InputSelectEntityId": "input_select.jasper_notifications",
    "InputNumberEntityId": "input_number.jasper_notification_count"
  }
]
```

Register the service in your DI container:

```cs
serviceCollection.AddInputSelectNotifications(configuration);
```

> Note: As an alternative, you can also provide configuration directly in code when calling `AddInputSelectNotifications`.

Inject the corresponding `IInputSelectNotificationEntity` service using the input select entity ID as the key, and use it to create, update, or remove notifications:

```cs
[NetDaemonApp]
internal class Example
{
    public Example([FromKeyedServices("input_select.living_room_panel_notifications")] IInputSelectNotificationEntity livingRoomPanelDashboardNotifications)
    {
        var notificationId = $"{nameof(Example)}_Notification";
        livingRoomPanelDashboardNotifications.Notify(new InputSelectDashboardNotificationConfig
        {
            Message = "This is a notification.",
            SecondaryMessage = "Click me to delete me!.",
            Icon = "mdi:lightbulb",
            IconColor = Color.Yellow,
            Action = () => livingRoomPanelDashboardNotifications.RemoveNotification(notificationId)
        }, notificationId);
    }
}
```

Alternatively, you can wrap it in the provided `InputSelectNotificationEntity` class and register that instead:

```cs
public class LivingRoomPanelDashboardNotifications(
    [FromKeyedServices("input_select.living_room_panel_notifications")] IInputSelectNotificationEntity inputSelectNotifications)
    : InputSelectNotificationEntity(inputSelectNotifications);
```

```cs
serviceCollection.AddTransient<LivingRoomPanelDashboardNotifications>();
```

Then inject and use it just like any other service:

```cs
[NetDaemonApp]
internal class Example
{
    public Example(LivingRoomPanelDashboardNotifications livingRoomPanelDashboardNotifications)
    {
        ...
    }
}
```

Finally, to trigger an action, make sure to raise the following event:

```cs
HaContext.SendEvent("notification_clicked", new { notificationEntity = InputSelectEntities.LivingRoomPanelNotifications.EntityId, notificationIndex = index });
```

> To display the notification using native Home Assistant templating, please follow [this setup guide](docs/home-assistant-dashboard-setup.md). Result: ![Gif of home assistant dashboard notifications](img/hass_notification_demo.gif "Home Assistant Dashboard Notifications")

> Example implementations (including visualisation in Blazor): https://github.com/DevJasperNL/CodeCasa

## CodeCasa.NetDaemon.RuntimeState

This library provides the `NetDaemonRuntimeStateService`, which allows you to check and subscribe to the runtime state of `NetDaemon`.

While this service isn’t necessary when using `NetDaemonApp` classes for automations, it can be useful in contexts where you access NetDaemon entities outside of that scope — for example, in a (Blazor) UI or background service. In such cases, `NetDaemonRuntimeStateService` helps determine whether the NetDaemon runtime is initialized and connected.

### Runtime States
The service exposes three possible states:
- **Initializing** – NetDaemon is still initializing and building its state cache. Entity access is not yet available.
- **Connected** – NetDaemon is connected to Home Assistant.
- **Disconnected** – NetDaemon is disconnected from Home Assistant. Cached entity states remain available, but no updates will be received until reconnected.

### Usage

Register the service:
```cs
builder.Services.AddNetDaemonRuntimeStateService();
```

Then use it, for example, in a Blazor component:

```cs
@inject NetDaemonRuntimeStateService NetDaemonRuntimeStateService

@code {
    private bool _netDaemonInitialized;
    private bool _netDaemonConnected;

    protected override void OnInitialized()
    {
        NetDaemonRuntimeStateService.ConnectedChangesWithCurrent().Subscribe(state =>
        {
            _netDaemonInitialized = state != NetDaemonStates.Initializing;
            _netDaemonConnected = state == NetDaemonStates.Connected;
            InvokeAsync(StateHasChanged);
        });
    }
}
```

or in a `BackgroundService`:
```cs
internal class ExampleService : BackgroundService
{
    private readonly NetDaemonRuntimeStateService _netDaemonRuntimeStateService;

    public ExampleService(NetDaemonRuntimeStateService netDaemonRuntimeStateService)
    {
        _netDaemonRuntimeStateService = netDaemonRuntimeStateService;
    }

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        // Wait for the NetDaemon runtime to be initialized.
        await _netDaemonRuntimeStateService.WaitForInitializationAsync(cancellationToken);
        if (cancellationToken.IsCancellationRequested)
        {
            return;
        }

        // Implement code that works with NetDaemon entities here.
    }
}
```

> Example Blazor/BackgroundServices implementations: https://github.com/DevJasperNL/CodeCasa

## CodeCasa.NetDaemon.TypedEntities

Strongly-typed wrappers for Home Assistant entities in **NetDaemon**.

This library provides `EnumEntity`, which enables entities with enum-based states.  
You can configure it by:

- Supplying only the enum type (defaults to enum names as state values)
- Providing an enum-to-string dictionary
- Supplying custom `Func<TEnum, string>` and `Func<string, TEnum?>` conversion functions

### Usage

With auto-generated NetDaemon entities, you can create type-safe variants.  
For example, here’s a type-safe `input_select` entity:

#### Base Class (optional)
```cs
public record TypeSafeInputSelectEntity<T> : EnumEntity<T, TypeSafeInputSelectEntity<T>, EnumEntityState<T, InputSelectAttributes>, InputSelectAttributes>, IInputSelectEntityCore 
    where T : struct, Enum
{
    public TypeSafeInputSelectEntity(IHaContext haContext, string entityId) : base(haContext, entityId)
    {
    }

    public TypeSafeInputSelectEntity(IEntityCore entity) : base(entity)
    {
    }
}
```

#### Concrete Wrapper Example
```cs
public record PersonStateEntity : TypeSafeInputSelectEntity<PersonStates>
{
    public PersonStateEntity(InputSelectEntity inputSelectEntity)
        : base(inputSelectEntity)
    {
    }
}

public enum PersonStates
{
    Awake,
    Asleep,
    Away
}
```

Now, `PersonStateEntity` can be used as a wrapper for any `InputSelectEntity` that has the states `Awake`, `Asleep` and `Away`.

### Another Example: PersonEntity

The person entity can be in states like `home`, `not_home`, or a zone name.
If you don’t want to sync all zone names, you can use the `Func` constructor overload to fall back to `NotHome`:

```cs
public record TypeSafePersonEntity : EnumEntity<PersonEntityStates, TypeSafePersonEntity, EnumEntityState<PersonEntityStates, PersonAttributes>, PersonAttributes>, IPersonEntityCore
{
    private static readonly Dictionary<PersonEntityStates, string> PeopleEntityStatesToStateValues = new()
    {
        { PersonEntityStates.Home , "home" },
        { PersonEntityStates.NotHome , "not_home" }
        { PersonEntityStates.Store , "store" }
    };
    public static readonly Dictionary<string, PersonEntityStates> StateValuesToPeopleEntityStates = PeopleEntityStatesToStateValues.Inverse(StringComparer.OrdinalIgnoreCase);

    public TypeSafePersonEntity(IHaContext haContext, string entityId) 
        : base(haContext, entityId, 
            e => PeopleEntityStatesToStateValues[e], 
            v => StateValuesToPeopleEntityStates.GetValueOrDefault(v, PersonEntityStates.NotHome)) // By using GetValueOrDefault, any zone outside home will result in state NotHome.
    {
    }

    public TypeSafePersonEntity(IEntityCore entity)
        : base(entity,
            e => PeopleEntityStatesToStateValues[e],
            v => StateValuesToPeopleEntityStates.GetValueOrDefault(v, PersonEntityStates.NotHome))
    {
    }
}

public enum PersonEntityStates
{
    Home,
    NotHome,
    Store
}
```

> 🔗 For more implementation examples, check out: [CodeCasa on GitHub](https://github.com/DevJasperNL/CodeCasa).

## CodeCasa.NetDaemon.Extensions.Observables

Collection of extension methods meant to enhance NetDaemon entities with boolean observables allowing for a more intuitive coding experience.

- For more information on NetDaemon, click [here](https://netdaemon.xyz/).
- Article containing more examples and explanation about this library: [Article with examples](https://dev.to/devjaspernl/supercharging-home-assistant-automations-initial-states-and-boolean-logic-for-netdaemon-rx-3bd5).

### Example Usage

```cs
public Example(
    SunEntities sunEntities, 
    CoverEntities coverEntities)
{
    const int curtainCloseSunElevation = 1;
    sunEntities.Sun
            .ToBooleanObservable(e => e.Attributes?.Elevation <= curtainCloseSunElevation);
    
    sunIsDown.SubscribeTrueFalse(
        () =>
        {
            coverEntities.LivingRoomFrontWindowCurtain.CloseCover();
            coverEntities.LivingRoomBackWindowCurtain.CloseCover();
        },
        () =>
        {
            coverEntities.LivingRoomFrontWindowCurtain.OpenCover();
            coverEntities.LivingRoomBackWindowCurtain.OpenCover();
        });
}
```

**Breakdown:**

```cs
sunEntities.Sun.ToBooleanObservable(e => e.Attributes?.Elevation <= curtainCloseSunElevation);
```
Create a stateful `IObservable<bool>` from the sun entity `Sun` which emits `true` when `Elevation` is smaller than `curtainCloseSunElevation`, `false` otherwise. Stateful in this context means that the observable will immediately emit the current value returned from the predicate when an observer is subscribed. (internally, the NetDaemon extension methods `StateChangesWithCurrent` and `StateAllChangesWithCurrent` are used for this.)

```cs
sunIsDown.SubscribeTrueFalse(
        () =>
        {
            coverEntities.LivingRoomFrontWindowCurtain.CloseCover();
            coverEntities.LivingRoomBackWindowCurtain.CloseCover();
        },
        () =>
        {
            coverEntities.LivingRoomFrontWindowCurtain.OpenCover();
            coverEntities.LivingRoomBackWindowCurtain.OpenCover();
        });
```

`SubscribeTrueFalse` is an extension method on `IObservable<bool>` that assigns an action for when `true` is emitted as well as when `false` is emitted.

This implementation will immediately open or close the covers depending on the initial result of the predicate as well as update whenever the result of the predicate changes.

### Boolean Observables

This library contains a set of extension methods to convert entities to implementations of `IObservable<bool>`. If no predicate is provided, the `On` state is mapped to true, the `Off` state is mapped to `false`.

The usage of `ToBooleanObservable` (or one of the aliases `ToOnOffObservable` or `ToOpenClosedObservable`) on an entity will use `StateChanges` (or `StateAllChanges` when a predicate is provided) to result in a stateful `IObservable<bool>`.

In case a stateful observable is not desired, the method `ToChangesOnlyBooleanObservable` can be used.

The use of stateful `IObservable<bool>` implementations enables the use of logic operators and scheduling extensions provided by the `Reactive.Boolean` library. This allows for improved readability of implementations.

For a more detailed documentation on extension methods for `IObservable<bool>`, check out the documentation for `Reactive.Boolean` [here](https://github.com/DevJasperNL/Reactive.Boolean).

**Example**

The following example shows the usage of boolean (`Not`, `Or` and `And`) and scheduling (`LimitTrueDuration`) logic to write a complex automation that is still easy to understand and will trigger on startup.
```cs
var noOneAsleep = inputBooleanEntities.JasperAsleep.ToBooleanObservable()
    .Or(inputBooleanEntities.AnonAsleep.ToBooleanObservable()).Not();

var closetDoorOpenShorterThanOneMin = binarySensorEntities.BedroomClosetDoorSensorContact
    .ToOpenClosedObservable().LimitTrueDuration(TimeSpan.FromMinutes(5), scheduler);

noOneAsleep.And(closetDoorOpenShorterThanOneMin).BindToOnOff(lightEntities.ClosetLight);
```

Note that even though scheduling is mostly handled by the `Reactive.Boolean` library, knowledge of the `Entity` does improve some scheduling methods. In the cases of `WhenTrueFor` and `LimitTrueDuration` both `LastChanged` and the passing of time are used to evaluate whether a true is emitted.

### Scheduling

A breakdown of all scheduling extension methods this library enables for `Entity` and `IObservable<bool>`:

#### OnForAtLeast

Returns an observable that won't emit `false` for at least the provided timespan after an initial `on` (`true`) is emitted by the `entity`.
If a `false` is emitted during the provided timespan, it will be emitted immediately after the timer is completed.

**Example Use Case**

Turn on a light for at least 3 seconds after a button was pressed. If 3 seconds are passed, only keep it on if the button is still being pressed, but immediately turn if off if not.
```cs
// buttonPressed is a IObservable<bool>
var buttonPressed = buttonEntity.ToBooleanObservable(s => s.State == "pressed");
buttonPressed
    .TrueForAtLeast(TimeSpan.FromSeconds(3), scheduler)
    .BindToOnOff(lightEntity);
```

> Aliases: `OpenForAtLeast`/`TrueForAtLeast`.

#### PersistOnFor

Returns an observable that delays the first `off` (`false`) that is emitted after an `on` (`true`) by the `entity` for a duration of the provided timespan.

**Example Use Case**

Keep a light on for 3 more seconds after last motion was detected.
```cs
// motionDetected is a IObservable<bool>
var motionDetected = motionSensorEntity.ToBooleanObservable(s => s.State == "motion");
motionDetected
    .PersistTrueFor(TimeSpan.FromSeconds(3), scheduler)
    .BindToOnOff(lightEntity);
```

> Aliases: `PersistOpenFor`/`PersistTrueFor`.

#### WhenOnFor

Returns an observable that emits `true` once `entity` does not emit `off` (`false`) for a minimum of the provided timespan.

When called on an `Entity`, this method takes into account `EntityState.LastChanged`, meaning the returned observable can emit `true` even if the time did not pass during runtime.

**Example Use Case**

Send notification when washing machine power has been 0 for at least 1 minute.
```cs
// washingMachineCurrentIsZero is a IObservable<bool>
var washingMachineCurrentIsZero = washingMachineCurrentEntity.ToBooleanObservable(s => s.State == 0);
washingMachineCurrentIsZero
    .WhenTrueFor(TimeSpan.FromMinutes(1), scheduler)
    .SubscribeTrue(() => notificationEntity.Send("Washing machine is done!"));
```

> Aliases: `WhenOpenFor`/`WhenTrueFor`.

#### LimitOnDuration

Returns an observable that will automatically emit `false` if the `entity` does not emit an `off` (`false`) itself within the provided timespan after emitting `on` (`true`).

When called on an `Entity`, this method takes into account `EntityState.LastChanged`, meaning the returned observable can emit `false` even if the time did not pass during runtime.

**Example Use Case**

Keep closet lights on for a maximum amount of time.
```cs
// closetDoorOpen is a IObservable<bool>
var closetDoorOpen = closetDoorEntity.ToBooleanObservable(s => s.State == "open");
closetDoorOpen
    .LimitTrueDuration(TimeSpan.FromMinutes(2), scheduler)
    .BindToOnOff(closetLightEntity);
```

> Aliases: `LimitOpenDuration`/`LimitCloseDuration`.

### Unavailability of entities

This library also implements the utility method `RepeatWhenEntitiesBecomeAvailable<T>`. This method can be called on a `IObservable<T>` to repeat a value when one of the provided entities become available.

**Example**

The following example will re-apply the correct state as soon as the plug becomes available (is plugged in).
```cs
const int nightTimeSunElevation = 0;
var nightTime = sunEntities.Sun
    .ToBooleanObservable(e => e.Attributes?.Elevation <= nightTimeSunElevation);

nightTime
    .RepeatWhenEntitiesBecomeAvailable(switchEntities.LivingRoomChristmasTreeLights)
    .BindToOnOff(switchEntities.LivingRoomChristmasTreeLights);
