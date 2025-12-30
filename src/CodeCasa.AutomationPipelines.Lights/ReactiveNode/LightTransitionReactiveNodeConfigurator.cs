using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using CodeCasa.Abstractions;
using CodeCasa.AutomationPipelines.Lights.Context;
using CodeCasa.AutomationPipelines.Lights.Extensions;
using CodeCasa.AutomationPipelines.Lights.Nodes;
using CodeCasa.AutomationPipelines.Lights.Pipeline;
using CodeCasa.Lights;

namespace CodeCasa.AutomationPipelines.Lights.ReactiveNode;

/// <summary>
/// Configures a light transition reactive node for a single light entity.
/// This configurator allows adding reactive dimmer controls, node sources, and scoped configurations for light automation.
/// </summary>
public partial class LightTransitionReactiveNodeConfigurator(
    IServiceProvider serviceProvider,
    LightPipelineFactory lightPipelineFactory,
    ReactiveNodeFactory reactiveNodeFactory,
    ILight lightEntity, 
    IScheduler scheduler) : ILightTransitionReactiveNodeConfigurator
{
    /// <summary>
    /// Gets the light entity associated with this configurator.
    /// </summary>
    public ILight LightEntity { get; } = lightEntity;

    internal string? Name { get; private set; }
    internal List<IObservable<IPipelineNode<LightTransition>?>> NodeObservables { get; } = new();
    internal List<IDimmer> Dimmers { get; } = new();
    internal DimmerOptions DimmerOptions { get; private set; } = new ();
    
    /// <inheritdoc/>
    public ILightTransitionReactiveNodeConfigurator SetName(string name)
    {
        Name = name;
        return this;
    }

    /// <inheritdoc/>
    public ILightTransitionReactiveNodeConfigurator AddReactiveDimmer(IDimmer dimmer)
    {
        Dimmers.Add(dimmer);
        return this;
    }

    /// <inheritdoc/>
    public ILightTransitionReactiveNodeConfigurator SetReactiveDimmerOptions(DimmerOptions dimmerOptions)
    {
        DimmerOptions = dimmerOptions;
        return this;
    }

    /// <inheritdoc/>
    public ILightTransitionReactiveNodeConfigurator AddUncoupledDimmer(IDimmer dimmer)
    {
        return AddUncoupledDimmer(dimmer, _ => { });
    }

    /// <inheritdoc/>
    public ILightTransitionReactiveNodeConfigurator AddUncoupledDimmer(IDimmer dimmer, Action<DimmerOptions> dimOptions)
    {
        var options = new DimmerOptions();
        dimOptions(options);
        options.ValidateSingleLightEntity(LightEntity.Id);

        var dimPulses = dimmer.Dimming.ToPulsesWhenTrue(options.TimeBetweenSteps, scheduler);
        var brightenPulses = dimmer.Brightening.ToPulsesWhenTrue(options.TimeBetweenSteps, scheduler);

        AddDimPulses(options, [LightEntity], dimPulses, brightenPulses);
        return this;
    }

    internal void AddDimPulses(DimmerOptions options, IEnumerable<ILight> lightsInDimOrder, IObservable<Unit> dimPulses, IObservable<Unit> brightenPulses)
    {
        var dimHelper = new DimHelper(LightEntity, lightsInDimOrder, options.MinBrightness, options.BrightnessStep);
        AddNodeSource(dimPulses
            .Select(_ => dimHelper.DimStep())
            .Where(t => t != null)
            .Select(t => (IPipelineNode<LightTransition>)(t == LightTransition.Off() ? new TurnOffThenPassThroughNode() : new StaticLightTransitionNode(t, scheduler))));
        AddNodeSource(brightenPulses
            .Select(_ => dimHelper.BrightenStep())
            .Where(t => t != null)
            .Select(t => (IPipelineNode<LightTransition>)(t == LightTransition.Off() ? new TurnOffThenPassThroughNode() : new StaticLightTransitionNode(t, scheduler))));
    }

    /// <summary>
    /// Adds a node source observable to the reactive node.
    /// </summary>
    /// <param name="nodeSource">An observable that emits pipeline nodes.</param>
    /// <returns>The configurator instance for method chaining.</returns>
    public ILightTransitionReactiveNodeConfigurator AddNodeSource(IObservable<IPipelineNode<LightTransition>?> nodeSource)
    {
        NodeObservables.Add(nodeSource);
        return this;
    }

    /// <inheritdoc/>
    public ILightTransitionReactiveNodeConfigurator AddNodeSource(IObservable<Func<ILightPipelineContext, IPipelineNode<LightTransition>?>> nodeFactorySource)
    {
        return AddNodeSource(nodeFactorySource.Select(f => f(new LightPipelineContext(serviceProvider, LightEntity))));
    }

    /// <inheritdoc/>
    public ILightTransitionReactiveNodeConfigurator ForLight(string lightEntityId,
        Action<ILightTransitionReactiveNodeConfigurator> configure) => ForLights([lightEntityId], configure);

    /// <inheritdoc/>
    public ILightTransitionReactiveNodeConfigurator ForLight(ILight lightEntity,
        Action<ILightTransitionReactiveNodeConfigurator> configure) => ForLights([lightEntity], configure);

    /// <inheritdoc/>
    public ILightTransitionReactiveNodeConfigurator ForLights(IEnumerable<string> lightEntityIds,
        Action<ILightTransitionReactiveNodeConfigurator> configure)
    {
        CompositeHelper.ValidateLightSupported(lightEntityIds, LightEntity.Id);
        return this;
    }

    /// <inheritdoc/>
    public ILightTransitionReactiveNodeConfigurator ForLights(IEnumerable<ILight> lightEntities,
        Action<ILightTransitionReactiveNodeConfigurator> configure)
    {
        CompositeHelper.ResolveGroupsAndValidateLightSupported(lightEntities, LightEntity.Id);
        return this;
    }
}