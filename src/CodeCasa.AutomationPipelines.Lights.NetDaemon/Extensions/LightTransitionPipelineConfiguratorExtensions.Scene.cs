using CodeCasa.AutomationPipelines.Lights.Cycle;
using CodeCasa.AutomationPipelines.Lights.Pipeline;
using CodeCasa.AutomationPipelines.Lights.Toggle;
using CodeCasa.Lights;
using CodeCasa.Lights.NetDaemon;
using CodeCasa.Lights.NetDaemon.Scenes;
using Microsoft.Extensions.DependencyInjection;
using NetDaemon.HassModel.Entities;

namespace CodeCasa.AutomationPipelines.Lights.NetDaemon.Extensions;

public static partial class LightTransitionPipelineConfiguratorExtensions
{
    // -------------------------------------------------------------------------
    // When
    // -------------------------------------------------------------------------

    /// <summary>
    /// Registers a node that applies light parameters from the given Home Assistant <paramref name="sceneEntity"/>
    /// when the observable of type <typeparamref name="TObservable"/> emits <see langword="true"/>.
    /// When the observable emits <see langword="false"/>, inputs are passed through unchanged.
    /// The scene is fetched and cached via <see cref="LightSceneCacheService"/> on first use.
    /// If the current light is not part of the scene, no state is applied.
    /// The observable is resolved from the service provider.
    /// </summary>
    /// <typeparam name="TObservable">The type of the observable to resolve from the service provider.</typeparam>
    /// <param name="configurator">The pipeline configurator.</param>
    /// <param name="sceneEntity">The scene entity whose light parameters will be applied.</param>
    /// <returns>The configurator instance for method chaining.</returns>
    public static ILightTransitionPipelineConfigurator<NetDaemonLight> When<TObservable>(
        this ILightTransitionPipelineConfigurator<NetDaemonLight> configurator,
        IEntityCore sceneEntity)
        where TObservable : IObservable<bool>
    {
        return configurator.When<TObservable>(sp => GetSceneLightParameters(sp, sceneEntity));
    }

    /// <summary>
    /// Registers a node that applies light parameters from the given Home Assistant <paramref name="sceneEntity"/>
    /// when the <paramref name="observable"/> emits <see langword="true"/>.
    /// When the observable emits <see langword="false"/>, inputs are passed through unchanged.
    /// The scene is fetched and cached via <see cref="LightSceneCacheService"/> on first use.
    /// If the current light is not part of the scene, no state is applied.
    /// </summary>
    /// <param name="configurator">The pipeline configurator.</param>
    /// <param name="observable">The observable that determines when to apply the scene.</param>
    /// <param name="sceneEntity">The scene entity whose light parameters will be applied.</param>
    /// <returns>The configurator instance for method chaining.</returns>
    public static ILightTransitionPipelineConfigurator<NetDaemonLight> When(
        this ILightTransitionPipelineConfigurator<NetDaemonLight> configurator,
        IObservable<bool> observable,
        IEntityCore sceneEntity)
    {
        return configurator.When(observable, sp => GetSceneLightParameters(sp, sceneEntity));
    }

    // -------------------------------------------------------------------------
    // Switch
    // -------------------------------------------------------------------------

    /// <summary>
    /// Registers a node that switches between two Home Assistant scenes based on a boolean observable.
    /// When the observable of type <typeparamref name="TObservable"/> emits <see langword="true"/>, the
    /// <paramref name="trueSceneEntity"/> is applied; when it emits <see langword="false"/>, the
    /// <paramref name="falseSceneEntity"/> is applied.
    /// Scenes are fetched and cached via <see cref="LightSceneCacheService"/> on first use.
    /// The observable is resolved from the service provider.
    /// </summary>
    /// <typeparam name="TObservable">The type of the observable to resolve from the service provider.</typeparam>
    /// <param name="configurator">The pipeline configurator.</param>
    /// <param name="trueSceneEntity">The scene entity to apply when the observable emits true.</param>
    /// <param name="falseSceneEntity">The scene entity to apply when the observable emits false.</param>
    /// <returns>The configurator instance for method chaining.</returns>
    public static ILightTransitionPipelineConfigurator<NetDaemonLight> Switch<TObservable>(
        this ILightTransitionPipelineConfigurator<NetDaemonLight> configurator,
        IEntityCore trueSceneEntity,
        IEntityCore falseSceneEntity)
        where TObservable : IObservable<bool>
    {
        return configurator.Switch<TObservable>(
            sp => GetSceneLightParameters(sp, trueSceneEntity),
            sp => GetSceneLightParameters(sp, falseSceneEntity));
    }

    /// <summary>
    /// Registers a node that switches between two Home Assistant scenes based on the <paramref name="observable"/>.
    /// When the observable emits <see langword="true"/>, the <paramref name="trueSceneEntity"/> is applied;
    /// when it emits <see langword="false"/>, the <paramref name="falseSceneEntity"/> is applied.
    /// Scenes are fetched and cached via <see cref="LightSceneCacheService"/> on first use.
    /// </summary>
    /// <param name="configurator">The pipeline configurator.</param>
    /// <param name="observable">The observable that determines which scene to apply.</param>
    /// <param name="trueSceneEntity">The scene entity to apply when the observable emits true.</param>
    /// <param name="falseSceneEntity">The scene entity to apply when the observable emits false.</param>
    /// <returns>The configurator instance for method chaining.</returns>
    public static ILightTransitionPipelineConfigurator<NetDaemonLight> Switch(
        this ILightTransitionPipelineConfigurator<NetDaemonLight> configurator,
        IObservable<bool> observable,
        IEntityCore trueSceneEntity,
        IEntityCore falseSceneEntity)
    {
        return configurator.Switch(
            observable,
            sp => GetSceneLightParameters(sp, trueSceneEntity),
            sp => GetSceneLightParameters(sp, falseSceneEntity));
    }

    // -------------------------------------------------------------------------
    // SwitchWhen
    // -------------------------------------------------------------------------

    /// <summary>
    /// Registers a node that applies a scene switch between two Home Assistant scenes when the
    /// <paramref name="whenObservable"/> emits <see langword="true"/>, and passes inputs through unchanged when it emits <see langword="false"/>.
    /// The <paramref name="switchObservable"/> determines which scene to apply.
    /// Scenes are fetched and cached via <see cref="LightSceneCacheService"/> on first use.
    /// </summary>
    /// <param name="configurator">The pipeline configurator.</param>
    /// <param name="whenObservable">The observable that gates the switch; when false, inputs pass through unchanged.</param>
    /// <param name="switchObservable">The observable that selects which scene to apply.</param>
    /// <param name="trueSceneEntity">The scene entity to apply when the switch observable emits true.</param>
    /// <param name="falseSceneEntity">The scene entity to apply when the switch observable emits false.</param>
    /// <returns>The configurator instance for method chaining.</returns>
    public static ILightTransitionPipelineConfigurator<NetDaemonLight> SwitchWhen(
        this ILightTransitionPipelineConfigurator<NetDaemonLight> configurator,
        IObservable<bool> whenObservable,
        IObservable<bool> switchObservable,
        IEntityCore trueSceneEntity,
        IEntityCore falseSceneEntity)
    {
        return configurator.SwitchWhen(
            whenObservable,
            switchObservable,
            sp => GetSceneLightParameters(sp, trueSceneEntity),
            sp => GetSceneLightParameters(sp, falseSceneEntity));
    }

    /// <summary>
    /// Registers a node that applies a scene switch between two Home Assistant scenes when the observable of type
    /// <typeparamref name="TWhenObservable"/> emits <see langword="true"/>, and passes inputs through unchanged when it emits <see langword="false"/>.
    /// Both observables are resolved from the service provider.
    /// Scenes are fetched and cached via <see cref="LightSceneCacheService"/> on first use.
    /// </summary>
    /// <typeparam name="TWhenObservable">The type of the gating observable to resolve from the service provider.</typeparam>
    /// <typeparam name="TSwitchObservable">The type of the branch-selecting observable to resolve from the service provider.</typeparam>
    /// <param name="configurator">The pipeline configurator.</param>
    /// <param name="trueSceneEntity">The scene entity to apply when the switch observable emits true.</param>
    /// <param name="falseSceneEntity">The scene entity to apply when the switch observable emits false.</param>
    /// <returns>The configurator instance for method chaining.</returns>
    public static ILightTransitionPipelineConfigurator<NetDaemonLight> SwitchWhen<TWhenObservable, TSwitchObservable>(
        this ILightTransitionPipelineConfigurator<NetDaemonLight> configurator,
        IEntityCore trueSceneEntity,
        IEntityCore falseSceneEntity)
        where TWhenObservable : IObservable<bool>
        where TSwitchObservable : IObservable<bool>
    {
        return configurator.SwitchWhen<TWhenObservable, TSwitchObservable>(
            sp => GetSceneLightParameters(sp, trueSceneEntity),
            sp => GetSceneLightParameters(sp, falseSceneEntity));
    }

    /// <summary>
    /// Registers a node that applies a scene switch between two Home Assistant scenes when the observable of type
    /// <typeparamref name="TWhenObservable"/> emits <see langword="true"/>, and passes inputs through unchanged when it emits <see langword="false"/>.
    /// The <paramref name="switchObservable"/> determines which scene to apply.
    /// The when observable is resolved from the service provider.
    /// Scenes are fetched and cached via <see cref="LightSceneCacheService"/> on first use.
    /// </summary>
    /// <typeparam name="TWhenObservable">The type of the gating observable to resolve from the service provider.</typeparam>
    /// <param name="configurator">The pipeline configurator.</param>
    /// <param name="switchObservable">The observable that selects which scene to apply.</param>
    /// <param name="trueSceneEntity">The scene entity to apply when the switch observable emits true.</param>
    /// <param name="falseSceneEntity">The scene entity to apply when the switch observable emits false.</param>
    /// <returns>The configurator instance for method chaining.</returns>
    public static ILightTransitionPipelineConfigurator<NetDaemonLight> SwitchWhen<TWhenObservable>(
        this ILightTransitionPipelineConfigurator<NetDaemonLight> configurator,
        IObservable<bool> switchObservable,
        IEntityCore trueSceneEntity,
        IEntityCore falseSceneEntity)
        where TWhenObservable : IObservable<bool>
    {
        return configurator.SwitchWhen<TWhenObservable>(
            switchObservable,
            sp => GetSceneLightParameters(sp, trueSceneEntity),
            sp => GetSceneLightParameters(sp, falseSceneEntity));
    }

    // -------------------------------------------------------------------------
    // AddToggle (with scene entries)
    // -------------------------------------------------------------------------

    /// <summary>
    /// Adds a toggle node that cycles through one or more Home Assistant scenes when triggered by <paramref name="triggerObservable"/>.
    /// Each trigger toggles to the next scene in order, wrapping back to the first after the last.
    /// Scenes are fetched and cached via <see cref="LightSceneCacheService"/> on first use.
    /// If the current light is not part of a scene, no state is applied for that step.
    /// </summary>
    /// <typeparam name="T">The type of values emitted by the trigger observable.</typeparam>
    /// <param name="configurator">The pipeline configurator.</param>
    /// <param name="triggerObservable">The observable that triggers toggling to the next scene.</param>
    /// <param name="sceneEntities">The scene entities to toggle through.</param>
    /// <returns>The configurator instance for method chaining.</returns>
    public static ILightTransitionPipelineConfigurator<NetDaemonLight> AddToggle<T>(
        this ILightTransitionPipelineConfigurator<NetDaemonLight> configurator,
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
    /// Adds a cycle node that rotates through one or more Home Assistant scenes when triggered by <paramref name="triggerObservable"/>.
    /// Each trigger cycles to the next scene in order.
    /// Scenes are fetched and cached via <see cref="LightSceneCacheService"/> on first use.
    /// If the current light is not part of a scene, no state is applied for that step.
    /// </summary>
    /// <typeparam name="T">The type of values emitted by the trigger observable.</typeparam>
    /// <param name="configurator">The pipeline configurator.</param>
    /// <param name="triggerObservable">The observable that triggers cycling to the next scene.</param>
    /// <param name="sceneEntities">The scene entities to cycle through.</param>
    /// <returns>The configurator instance for method chaining.</returns>
    public static ILightTransitionPipelineConfigurator<NetDaemonLight> AddCycle<T>(
        this ILightTransitionPipelineConfigurator<NetDaemonLight> configurator,
        IObservable<T> triggerObservable,
        params IEntityCore[] sceneEntities)
    {
        return configurator.AddCycle(triggerObservable, (Action<ILightTransitionCycleConfigurator<NetDaemonLight>>)(c =>
        {
            foreach (var scene in sceneEntities)
                c.AddScene(scene);
        }));
    }

    // -------------------------------------------------------------------------
    // Shared helper
    // -------------------------------------------------------------------------

    private static LightParameters? GetSceneLightParameters(IServiceProvider sp, IEntityCore sceneEntity)
    {
        var cacheService = sp.GetRequiredService<LightSceneCacheService>();
        var sceneLights = cacheService.GetLightSceneAsync(sceneEntity.EntityId).GetAwaiter().GetResult();
        var light = sp.GetRequiredService<ILight>();
        return sceneLights.GetValueOrDefault(light.Id);
    }
}
