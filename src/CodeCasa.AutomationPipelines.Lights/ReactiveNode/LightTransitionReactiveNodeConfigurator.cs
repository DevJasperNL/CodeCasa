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
/// Configures a light transition reactive node for a single light.
/// This configurator allows adding reactive dimmer controls, node sources, and scoped configurations for light automation.
/// </summary>
internal partial class LightTransitionReactiveNodeConfigurator(
    IServiceProvider serviceProvider,
    LightPipelineFactory lightPipelineFactory,
    ReactiveNodeFactory reactiveNodeFactory,
    ILight light, 
    IScheduler scheduler) : ILightTransitionReactiveNodeConfigurator
{
    /// <summary>
    /// Gets the light associated with this configurator.
    /// </summary>
    public ILight Light { get; } = light;

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
        options.ValidateSingleLight(Light.Id);

        var dimPulses = dimmer.Dimming.ToPulsesWhenTrue(options.TimeBetweenSteps, scheduler);
        var brightenPulses = dimmer.Brightening.ToPulsesWhenTrue(options.TimeBetweenSteps, scheduler);

        AddDimPulses(options, [Light], dimPulses, brightenPulses);
        return this;
    }

    internal void AddDimPulses(DimmerOptions options, IEnumerable<ILight> lightsInDimOrder, IObservable<Unit> dimPulses, IObservable<Unit> brightenPulses)
    {
        var dimHelper = new DimHelper(Light, lightsInDimOrder, options.MinBrightness, options.BrightnessStep);
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
        return AddNodeSource(nodeFactorySource.Select(f => f(new LightPipelineContext(serviceProvider, Light))));
    }

    /// <inheritdoc/>
    public ILightTransitionReactiveNodeConfigurator ForLight(string lightId,
        Action<ILightTransitionReactiveNodeConfigurator> configure) => ForLights([lightId], configure);

    /// <inheritdoc/>
    public ILightTransitionReactiveNodeConfigurator ForLight(ILight light,
        Action<ILightTransitionReactiveNodeConfigurator> configure) => ForLights([light], configure);

    /// <inheritdoc/>
    public ILightTransitionReactiveNodeConfigurator ForLights(IEnumerable<string> lightIds,
        Action<ILightTransitionReactiveNodeConfigurator> configure)
    {
        CompositeHelper.ValidateLightSupported(lightIds, Light.Id);
        return this;
    }

    /// <inheritdoc/>
    public ILightTransitionReactiveNodeConfigurator ForLights(IEnumerable<ILight> lightEntities,
        Action<ILightTransitionReactiveNodeConfigurator> configure)
    {
        CompositeHelper.ResolveGroupsAndValidateLightSupported(lightEntities, Light.Id);
        return this;
    }
}