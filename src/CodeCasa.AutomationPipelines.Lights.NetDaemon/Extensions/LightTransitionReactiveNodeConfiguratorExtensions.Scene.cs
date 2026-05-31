using CodeCasa.AutomationPipelines.Lights.Cycle;
using CodeCasa.AutomationPipelines.Lights.ReactiveNode;
using CodeCasa.AutomationPipelines.Lights.Toggle;
using CodeCasa.Lights.NetDaemon;
using CodeCasa.Lights.NetDaemon.Scenes;
using NetDaemon.HassModel.Entities;

namespace CodeCasa.AutomationPipelines.Lights.NetDaemon.Extensions;

public static partial class LightTransitionReactiveNodeConfiguratorExtensions
{
    // -------------------------------------------------------------------------
    // On
    // -------------------------------------------------------------------------

    /// <summary>
    /// Registers a trigger that applies light parameters from the given Home Assistant <paramref name="sceneEntity"/>
    /// when the <paramref name="triggerObservable"/> emits a value.
    /// The scene is fetched and cached via <see cref="LightSceneCacheService"/> on first use.
    /// If the current light is not part of the scene, no state is applied.
    /// </summary>
    /// <typeparam name="T">The type of values emitted by the trigger observable.</typeparam>
    /// <param name="configurator">The reactive node configurator.</param>
    /// <param name="triggerObservable">The observable that triggers the scene application.</param>
    /// <param name="sceneEntity">The scene entity whose light parameters will be applied.</param>
    /// <returns>The configurator instance for method chaining.</returns>
    public static ILightTransitionReactiveNodeConfigurator<NetDaemonLight> On<T>(
        this ILightTransitionReactiveNodeConfigurator<NetDaemonLight> configurator,
        IObservable<T> triggerObservable,
        IEntityCore sceneEntity)
    {
        return configurator.On(triggerObservable, sp => SceneExtensionHelpers.GetSceneLightParameters(sp, sceneEntity));
    }

    // -------------------------------------------------------------------------
    // AddToggle (with scene entries)
    // -------------------------------------------------------------------------

    /// <summary>
    /// Adds a time-based toggle trigger that cycles through one or more Home Assistant scenes when triggered by <paramref name="triggerObservable"/>.
    /// Quick consecutive triggers advance through all scenes sequentially. After a timeout period, the next trigger restarts from the beginning.
    /// Scenes are fetched and cached via <see cref="LightSceneCacheService"/> on first use.
    /// If the current light is not part of a scene, no state is applied for that step.
    /// </summary>
    /// <typeparam name="T">The type of values emitted by the trigger observable.</typeparam>
    /// <param name="configurator">The reactive node configurator.</param>
    /// <param name="triggerObservable">The observable that triggers toggling to the next scene.</param>
    /// <param name="sceneEntities">The scene entities to toggle through.</param>
    /// <returns>The configurator instance for method chaining.</returns>
    public static ILightTransitionReactiveNodeConfigurator<NetDaemonLight> AddToggle<T>(
        this ILightTransitionReactiveNodeConfigurator<NetDaemonLight> configurator,
        IObservable<T> triggerObservable,
        params IEntityCore[] sceneEntities)
    {
        return configurator.AddToggle(triggerObservable, (Action<ILightTransitionToggleConfigurator<NetDaemonLight>>)(c =>
        {
            foreach (var scene in sceneEntities)
                c.AddScene(scene);
        }));
    }

    // -------------------------------------------------------------------------
    // AddCycle (with scene entries)
    // -------------------------------------------------------------------------

    /// <summary>
    /// Adds a state-based cycle trigger that rotates through one or more Home Assistant scenes when triggered by <paramref name="triggerObservable"/>.
    /// The cycle advances based on the current light state. If the current state is not recognized, the cycle starts from the beginning.
    /// Scenes are fetched and cached via <see cref="LightSceneCacheService"/> on first use.
    /// If the current light is not part of a scene, no state is applied for that step.
    /// </summary>
    /// <typeparam name="T">The type of values emitted by the trigger observable.</typeparam>
    /// <param name="configurator">The reactive node configurator.</param>
    /// <param name="triggerObservable">The observable that triggers cycling to the next scene.</param>
    /// <param name="sceneEntities">The scene entities to cycle through.</param>
    /// <returns>The configurator instance for method chaining.</returns>
    public static ILightTransitionReactiveNodeConfigurator<NetDaemonLight> AddCycle<T>(
        this ILightTransitionReactiveNodeConfigurator<NetDaemonLight> configurator,
        IObservable<T> triggerObservable,
        params IEntityCore[] sceneEntities)
    {
        return configurator.AddCycle(triggerObservable, (Action<ILightTransitionCycleConfigurator<NetDaemonLight>>)(c =>
        {
            foreach (var scene in sceneEntities)
                c.AddScene(scene);
        }));
    }

    }
