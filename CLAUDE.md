# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## What this repo is

A set of independently published NuGet libraries (`src/*`) for .NET smart-home automation, most of them extensions for [NetDaemon](https://netdaemon.xyz/) / Home Assistant. All projects target `net10.0` (SDK 10.0.x), `Nullable` + `ImplicitUsings` enabled, and use C# 14 features (`field` keyword, `Lock`). One solution: `CodeCasa.sln`. The README is the user-facing docs for each package; its samples occasionally lag the code (e.g. it still shows `LightSceneTemplates.Bright`, which is now `LightParameters.Bright`).

## Commands

```bash
dotnet build CodeCasa.sln -p:TreatWarningsAsErrors=true   # what CI does; XML doc warnings (GenerateDocumentationFile=true) are errors
dotnet test tests/<Project>.Tests                          # one test project
dotnet test tests/<Project>.Tests --filter "FullyQualifiedName~ClassName"        # one class
dotnet test tests/<Project>.Tests --filter "FullyQualifiedName~ClassName.Method" # one test
for p in tests/*/*.csproj; do dotnet test "$p"; done       # all tests (CI runs them per project, not via the .sln)
dotnet pack CodeCasa.sln -c Release -p:PackageVersion=X -p:Version=X   # release packaging (done by CI on GitHub release)
```

Tests are MSTest + Moq + `Microsoft.Reactive.Testing` (`TestScheduler` for anything time-based — always inject `IScheduler` rather than using real time). Every test assembly has `[assembly: Parallelize(Scope = ExecutionScope.MethodLevel)]`, so tests must not share mutable static state. `tests/Directory.Build.props` marks test projects non-packable.

## Repository process

- History is squash-merged PRs (`Title (#NNN)`), branches named `feature/...` / `bugfix/...`.
- The `check-pr-labels` workflow **fails any PR without a label starting with `pr:`** (`pr: new-feature`, `pr: enhancement`, `pr: bugfix`, `pr: breaking change`, `pr: documentation`, `pr: dependency-update`). Release Drafter builds release notes from those labels; NuGet publish runs on GitHub release, version taken from the git tag.
- Every `src` project carries full NuGet metadata and packs `README.md`, `LICENSE` and an icon from `img/`; a new library must copy that block. `InternalsVisibleTo` is the mechanism for cross-project internals (tests, `Notifications.Lights`, `Lights.NetDaemon.Scenes`).

## Architecture

### Dependency layers

```
CodeCasa.Abstractions (IDimmer, StateChange<TEntity,TState>)
  └─ CodeCasa.Lights            platform-agnostic: ILight, LightParameters, LightTransition, Interpolate, Flatten
       ├─ CodeCasa.Lights.Timelines        Occurify ITimeline -> IObservable<LightTransition>
       ├─ CodeCasa.AutomationPipelines     generic Pipeline<TState> / PipelineNode<TState> (no light knowledge)
       │    └─ CodeCasa.AutomationPipelines.Lights   LightPipelineFactory + configurator DSL (the big one, ~100 files)
       │         ├─ CodeCasa.Notifications.Lights     priority-arbitrated light notifications as a reactive node
       │         ├─ CodeCasa.AutomationPipelines.Lights.Mqtt   publishes pipeline telemetry JSON over MQTT
       │         └─ CodeCasa.AutomationPipelines.Lights.NetDaemon  NetDaemon entity/scene overloads of the DSL
       └─ CodeCasa.Lights.NetDaemon        NetDaemonLight : ILight adapter (+ .Scenes: HA scene -> LightParameters)
CodeCasa.NetDaemon.Extensions.Observables   entity -> stateful IObservable<bool> (+ scheduling ops)
  └─ CodeCasa.NetDaemon.Sensors.Composite   MotionSensor (occupancy + illuminance latch)
CodeCasa.NetDaemon.{RuntimeState,TypedEntities}, CodeCasa.Notifications.{Phone,InputSelect}.NetDaemon   standalone
```

Rule of thumb: anything named `*.NetDaemon` is a thin adapter; keep logic in the platform-agnostic project below it and add only entity-typed overloads in the NetDaemon one.

### `Generated/` folders are vendored NetDaemon codegen

`Lights.NetDaemon`, `Lights.NetDaemon.Scenes` and `Sensors.Composite` each contain a `Generated/` folder with **hand-copied, `internal`** subsets of NetDaemon's generated entity classes (`LightEntity`, `LightAttributes`, `LightTurnOnParameters`, `SceneEntity`, ...). Public APIs only accept NetDaemon's core interfaces (`ILightEntityCore`, `IEntityCore`, ...) so the packages never depend on a consumer's generated code. Keep it that way; don't make those types public.

### Core pipeline model (`CodeCasa.AutomationPipelines`)

`Pipeline<TState>` is itself a `PipelineNode<TState>`; `RegisterNode` wires `previous.OnNewOutput -> next.Input`, and the last node's output becomes the pipeline output, passed to the `SetOutputHandler` action. Later nodes override earlier ones. A node either sets `Output` or sets `PassThrough = true` (input flows through untouched); `ChangeOutputAndTurnOnPassThroughOnNextInput` is the "influence once" primitive (switch press, motion trigger). `RegisterNode` deliberately sets output twice (subscription + explicit read) because nodes may emit synchronously on input. Output is **not** distinct by default; pass an `IEqualityComparer` (or `WithDistinctOutput()` in the light DSL) to suppress duplicates. `ServiceProviderPipeline<T>` (registered by `AddAutomationPipelines()`) resolves nodes via DI inside its own scope. `Telemetry` emits every hop, nested pipelines re-emit with `NestingPath`.

### Light pipelines (`CodeCasa.AutomationPipelines.Lights`)

- **Entry**: `AddLightPipelines()` registers `LightPipelineFactory` and `ReactiveNodeFactory`. `SetupLightPipeline(light, configure)` flattens groups and builds **one `Pipeline<LightTransition>` per leaf light**, default state `LightTransition.Off()`, output handler `light.ApplyTransition` — that handler is the only place a light is driven. Everything is wrapped in `ManagedPipeline` (lifetime: subscriptions -> pipeline -> DI scope) and returned as one `CompositeAsyncDisposable`.
- **Per-light DI scope**: `ServiceProviderExtensions.CreateLightPipelineContextScope` uses the `DependencyInjection.Composite` package to create a child scope where `ILight`/`TLight`/`LightPipelineContext` resolve to *that* light. Factories are constructed with the root provider, so internal overloads thread an explicit `compositeServiceProvider` through — don't resolve the factory from a scope and expect the scoped provider.
- **Configurator trios**: for Pipeline, ReactiveNode, Toggle, Cycle and Switch there are `I…Configurator` (public), `…Configurator` (internal leaf: one light, one node list) and `Composite…Configurator` (internal fan-out over `Dictionary<lightId, leaf>`), each split into partial files per feature (`.When`, `.Switch`, `.SwitchWhen`, `.Groups`, `.Reactive.Toggle`, `.Reactive.Cycle`, `.Subscribe`, `.Logging`). The Composite's extra responsibility is **observable sharing** (`IObservableSharingStrategy`, default `Replay(1).RefCount()`) so N lights share one subscription; the leaf's `SetObservableSharingStrategy` is a no-op. Adding a DSL method means touching all three plus, usually, a NetDaemon overload in `AutomationPipelines.Lights.NetDaemon`.
- **`ForLights(ids, configure)`** re-scopes the composite to a subset (single light -> the leaf configurator; the leaf's own `ForLights` only validates). **`ExcludedLightBehaviours.PassThrough`** keeps toggle/cycle indices aligned by pushing pass-through entries for lights outside the subset.
- **`UseLightGroup`** (unrelated to `ForLights`) is the Zigbee-group optimisation: a `GroupNode` is appended last to each pipeline and `GroupNodeContext` buffers transitions for a window (default 20 ms); if every member gets an identical transition, one `ApplyTransition` goes to the group entity instead of N unicasts.
- **Reactive nodes** (`ReactiveNode/`): a `ReactiveNode` swaps its active child based on an observable of **node factories** (`Func<IServiceProvider, IPipelineNode<LightTransition>>`), each activation getting its own DI scope; `null` = deactivate/pass through. `On`, `TurnOffWhen`, `AddToggle`, `AddCycle`, `AddInteractionNode` and `AddNotifications` are all sugar over this. `InstantiationScope.Shared` builds nested nodes once for all lights via the composite factory maps (lazy, locked, `ScopedPipelineNode` disposal clears the map); `PerChild` builds per light per activation. Dimmers (`AddReactiveDimmer`) wrap the reactive node in `ReactiveDimmerPipeline`; for a dimmer shared across lights the first configurator's options win.
- **Interaction node**: `AddInteractionNode` watches `light.StateChanges()` and, when brightness hits 0 while `LightPipelineContext` says the pipeline didn't output that off, inserts a `TurnOffThenPassThroughNode` — physical switch wins until the next upstream change.
- **Threading**: `ReactiveNode` serialises all state mutation through a `Subject<Action>` + `Synchronize()` queue (replaced a `Lock` that deadlocked — see commit `0b4571e`). Don't reintroduce locks held across callbacks into nodes. `Lock` remains in `GroupNodeContext`, `RegistrationManager` and the composite factory maps.
- **Hierarchy/logging**: all configurators implement internal `IPipelineHierarchyContext`; `ActionExtensions.ApplyHierarchySettings` propagates names/logging into nested configurators before user code runs. `PipelineLogger` logs at Trace, prefixed `[light.id] Path->To->Node`.

### Light model conventions (`CodeCasa.Lights`, `Lights.NetDaemon`)

- `LightParameters` = `Brightness` (0–255, **0 or null means off** — there is no separate on/off flag), `RgbColor`, `ColorTempKelvin`. `ApplyTransition` routes brightness 0 to `turn_off`.
- **RGB wins over colour temperature** on every write path: `ToLightTurnOnParameters` drops `color_temp_kelvin` when an RGB colour is set (HA rejects both), and `Interpolate` collapses both sides to RGB if either has one (gamma-corrected blend, gamma 2.8 tuned against Hue bulbs).
- Default transition is **400 ms** (`DimmerOptions.TimeBetweenSteps`, `TimelineNode`, `SchedulerExtensions.ScheduleInterpolatedLightTransition`); shorter transitions are sent without a `transition` value.
- `NetDaemonLight.GetChildren()` reads the HA `entity_id` attribute; `Flatten()` uses a visited set and self-referencing groups are handled.
- `Lights.NetDaemon.Scenes`: scene contents come from the HA **REST** API (`config/scene/config/{id}`), hence async; `LightSceneCacheService` caches in a **static** dictionary for the process lifetime (editing a scene in HA needs a restart). `SceneExtensionHelpers` calls it sync-over-async during pipeline configuration.
- `LightParameters.Relax/NightLight/Concentrate/Bright/Dimmed` are mutable `public static` fields.

### Observables (`CodeCasa.NetDaemon.Extensions.Observables`)

"Stateful" = emits the current value on subscribe (`StateChangesWithCurrent`), which is what makes the `Reactive.Boolean` operators (`And`/`Or`/`Not`/`WhenTrueFor`/...) usable. `ToBooleanObservable` maps **anything other than `on` — including unavailable/unknown/removed — to `false`** and is `DistinctUntilChanged`. Scheduling operators on `Entity` use `LastChanged` so timers can fire correctly after a restart. `Constants.EntityUnavailableStates` is the hardcoded `["unknown","unavailable"]`.

### Notifications

- `Notifications.InputSelect.NetDaemon` stores notifications as compact JSON options (single-letter keys, `MessageShortener`) in an `input_select` — HA caps an option at **255 chars**. Config section `InputSelectNotificationEntities` -> one keyed singleton `IInputSelectNotificationEntity` per entity id (also injectable as `IEnumerable<>`); a `BackgroundService` waits for `NetDaemonRuntimeStateService.WaitForInitializationAsync()` before touching HA; clicks arrive via the HA event `notification_clicked`.
- `Notifications.Phone.NetDaemon` dispatches button presses from the `mobile_app_notification_action` event by action index.
- `Notifications.Lights`: `LightNotificationManager` (singleton) picks the highest-priority active notification; `AddNotifications()` exposes it as a reactive node whose factory returns `null` for pipelines whose `TLight` type doesn't match.

## Code conventions

- Lambda parameter for an `IServiceProvider` is `sp`; configurator lambdas are `configure` / `trueConfigure` / `falseConfigure`.
- Prefer node **factories** (`Func<IServiceProvider, IPipelineNode<LightTransition>>`) over node instances in the DSL so scope/lifetime can be injected at activation time.
- Public surface is interfaces + extension methods; implementations stay `internal`. Every public member needs XML docs (warnings are errors in CI).
- Dispose helpers: `ObjectExtensions.DisposeOrDisposeAsync`, `CompositeAsyncDisposable`. New nodes must handle `DisposeAsync` and cancel any scheduled work on new input/output.
