# Jasper's Code Casa 🏡

[![Build Status](https://github.com/DevJasperNL/CodeCasa/actions/workflows/ci-build-and-test.yml/badge.svg)](https://github.com/DevJasperNL/CodeCasa/actions/workflows/ci-build-and-test.yml)

A smart home implementation example using C# and NetDaemon.

This repository explores creative and powerful ways to use a rich programming language like C# for home automation. From custom logic to seamless integrations, you'll find practical examples and unique ideas to elevate your smart home setup. Stay tuned for ongoing updates and new features!

## 📖 Table of Contents  
- [Architectures & Implementations](#🛠️-architectures--implementations)
    - [Automation Pipelines](#automation-pipelines)
- [Projects Overview](#🔧-projects-overview)

## 🛠️ Architectures & Implementations

One of the great advantages of using a general-purpose programming language like C# for home automations is the ability to introduce your own architectural patterns. This chapter highlights some of the patterns used in this example project.

### People

This demo shows how to use a custom compound entity to group related properties and services for a person.

In this example, the `Jasper` class provides Jasper’s name and location and making it easy to trigger context-aware notifications to his phone:

```cs
[NetDaemonApp]
internal class OfficeLightsNotifications
{
    public OfficeLightsNotifications(
        LightEntities lightEntities,
        Jasper jasper)
    {
        var notificationId = $"{nameof(OfficeLightsNotifications)}_Notification"; // Note: Using an ID that is consistent between runs also ensures that old notifications are removed/replaced on phones when the app is reloaded.

        var officeLights = lightEntities.OfficeLights.ToOnOffObservable();
        var jasperHome = jasper.CreateHomeObservable();

        // Only notify Jasper if he is at home and the lights are on.
        jasperHome.And(officeLights).SubscribeOnOff(
            () =>
            {
                jasper.Phone.Notify(new AndroidNotificationConfig
                {
                    Message = $"Hey {jasper.Name}, the office lights are on!",
                    StatusBarIcon = "mdi:lightbulb",
                    Actions =
                    [
                        new(() => lightEntities.OfficeLights.TurnOff(), "Click here to turn them off.")
                    ]
                }, notificationId);
            },
            () => jasper.Phone.RemoveNotification(notificationId));
    }
}
```

- The code from this `NetDaemonApp`: [OfficeLightsNotifications.cs](src\CodeCasa.Automations\Apps\Notifications\DemoNotifications.cs)
- The custom Jasper entity: [Jasper.cs](src\CodeCasa.CustomEntities\People\Jasper.cs)

### Input Select Notifications

This project showcases **rich notifications** using an input select entity, built with the `CodeCasa.NetDaemon.Notifications.InputSelect` library from [`NetDaemon.Utils`](https://github.com/DevJasperNL/NetDaemon.Utils).

Here’s a preview of the notifications in action:

![Gif demonstrating dashboard notifications](img/blazor_dashboard_notification_demso.gif "Dashboard Notifications")

For detailed usage and setup instructions, see the [`CodeCasa.NetDaemon.Notifications.InputSelect` documentation](https://github.com/DevJasperNL/NetDaemon.Utils?tab=readme-ov-file#codecasanetdaemonnotificationsinputselect).

- The `NetDaemonApp` demo code: [DemoNotifications.cs](src\CodeCasa.Automations\Apps\Notifications\DemoNotifications.cs)
- The Blazor component: [Notifications.razor](src\CodeCasa.Dashboard\Components\Dashboard\Notifications.razor)

### Automation Pipelines

This automation uses the [`AutomationPipelines`](https://github.com/DevJasperNL/CodeCasa.Libraries) library to handle complex logic in a modular, layered way.

Rather than implementing behavior directly in a single class, logic is split into small, independent pipeline nodes. Each node can contribute to or override the final outcome based on its own conditions. This makes the automation easier to reason about, test, and extend.

Below is the setup used in the `BackyardStringLightsPipeline` app:

```cs
backyardPorchStringLightsPipeline
    .SetDefault(false)
    .RegisterNode(new LightStringRoutineNode<bool>(scheduler, true, TimeSpan.Zero))
    .RegisterNode<BackyardStringLightsEnergySavingNode>()
    .SetOutputHandler(b => UpdateLightState(lightEntities.BackyardPorchStringLights, b));
```

In this example:
- The pipeline starts with a default state of false (lights off).
- The first node (LightStringRoutineNode) schedules the lights to turn on during morning and evening hours.
- The second node (`BackyardStringLightsEnergySavingNode`) can turn them off again if all curtains are closed.
- Finally, `SetOutputHandler` applies the resulting output to the actual light entity.

## 🔧 Projects Overview

### 🤖 Automations (`CodeCasa.Automations`)

This project contains the NetDaemon automations. It runs as a console application and can be hosted as a container.

### 📊 Blazor Dashboard (`CodeCasa.Dashboard`)

A Blazor-based web dashboard that demonstrates how to integrate with Home Assistant. This project showcases how to build responsive, interactive UIs that control and reflect your smart home’s state in real-time.

### 🧬 Auto-Generated Code (`CodeCasa.AutoGenerated`)

NetDaemon can auto-generate strongly-typed classes based on the entities in your Home Assistant configuration. For this demo, a curated selection of generated code is included to illustrate how this feature simplifies development and enhances type safety.

### 🧩 Custom Entities (`CodeCasa.CustomEntities`)

This project combines existing entities into compound entities or creates entirely new entities tailored to specific automation scenarios. It also includes helper constants to simplify and standardize usage across automations and dashboards.

### 🛠️ NetDaemon Utilities (`CodeCasa.NetDaemon.Utilities`)

A collection of utility classes for working with NetDaemon entities. These utilities are tailored to the use-cases of this project but may be useful for similar implementations in other projects.