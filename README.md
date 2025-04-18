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

### Automation Pipelines

Sometimes, multiple factors influence a single entity’s behavior. Instead of handling all logic in one monolithic automation, it's often cleaner to break responsibilities into separate classes. To support this, I introduced the Pipeline<TState> class.

Rather than a basic pipeline, this pipeline implementation allows nodes (`IPipelineNode<TState>`) to be constructed using dependency injection and have their own agency. That way they basically function as individual apps.

The order in which nodes are registered determines their priority. Later nodes can override the output of earlier ones, making layering of logic intuitive and maintainable.

Nodes get notified when their input changes (by initial pipeline state or by a previous node in the pipeline). They can also independently change their output or even disable themselves (pass input through unmodified).

An example implementation can be found in the `BackyardStringLightsPipeline` app:

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

### 🛠️ NetDaemon Utilities (`CodeCasa.NetDaemon.Utilities`)

A collection of utility classes for working with NetDaemon entities. These utilities are tailored to the use-cases of this project but may be useful for similar implementations in other projects.

### 🧩 Pipeline (`CodeCasa.Pipeline`)

Contains the pipeline implementation used for building modular automation flows. Each pipeline node has its own agency, allowing for clean separation of responsibilities. While currently project-specific, this may be extracted into a standalone library in the future.