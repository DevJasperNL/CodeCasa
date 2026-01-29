using CodeCasa.Abstractions;
using CodeCasa.AutomationPipelines.Lights.Extensions;
using CodeCasa.AutomationPipelines.Lights.Nodes;
using CodeCasa.Lights;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using Microsoft.Extensions.DependencyInjection;

namespace CodeCasa.AutomationPipelines.Lights.ReactiveNode;

/// <summary>
/// Configures a light transition reactive node for a single light.
/// This configurator allows adding reactive dimmer controls, node sources, and scoped configurations for light automation.
/// </summary>
internal partial class LightTransitionReactiveNodeConfigurator<TLight>
    : ILightTransitionReactiveNodeConfigurator<TLight> where TLight : ILight
{
    private readonly IScheduler _scheduler;

    /// <summary>
    /// The service provider scoped to this light.
    /// </summary>
    public IServiceProvider ServiceProvider { get; }
    /// <summary>
    /// Gets the light associated with this configurator.
    /// </summary>
    public TLight Light { get; }

    public LightTransitionReactiveNodeConfigurator(IServiceProvider serviceProvider, TLight light, IScheduler scheduler)
    {
        ServiceProvider = serviceProvider;
        Light = light;
        _scheduler = scheduler;
    }

    internal string? Name { get; private set; }
    internal bool? Log { get; private set; }
    internal List<IObservable<IPipelineNode<LightTransition>?>> NodeObservables { get; } = new();
    internal List<IDimmer> Dimmers { get; } = new();
    internal DimmerOptions DimmerOptions { get; private set; } = new ();
    
    /// <inheritdoc/>
    public ILightTransitionReactiveNodeConfigurator<TLight> EnableLogging(string? name = null)
    {
        Name = name;
        Log = true;
        return this;
    }

    /// <inheritdoc/>
    public ILightTransitionReactiveNodeConfigurator<TLight> DisableLogging()
    {
        Log = false;
        return this;
    }

    /// <inheritdoc/>
    public ILightTransitionReactiveNodeConfigurator<TLight> AddReactiveDimmer(IDimmer dimmer)
    {
        Dimmers.Add(dimmer);
        return this;
    }

    /// <inheritdoc/>
    public ILightTransitionReactiveNodeConfigurator<TLight> SetReactiveDimmerOptions(DimmerOptions dimmerOptions)
    {
        DimmerOptions = dimmerOptions;
        return this;
    }

    /// <inheritdoc/>
    public ILightTransitionReactiveNodeConfigurator<TLight> AddUncoupledDimmer(IDimmer dimmer)
    {
        return AddUncoupledDimmer(dimmer, _ => { });
    }

    /// <inheritdoc/>
    public ILightTransitionReactiveNodeConfigurator<TLight> AddUncoupledDimmer(IDimmer dimmer, Action<DimmerOptions> dimOptions)
    {
        var options = new DimmerOptions();
        dimOptions(options);
        options.ValidateSingleLight(Light.Id);

        var dimPulses = dimmer.Dimming.ToPulsesWhenTrue(options.TimeBetweenSteps, _scheduler);
        var brightenPulses = dimmer.Brightening.ToPulsesWhenTrue(options.TimeBetweenSteps, _scheduler);

        AddDimPulses(options, [Light], dimPulses, brightenPulses);
        return this;
    }

    internal void AddDimPulses(DimmerOptions options, IEnumerable<ILight> lightsInDimOrder, IObservable<Unit> dimPulses, IObservable<Unit> brightenPulses)
    {
        var dimHelper = new DimHelper(Light, lightsInDimOrder, options.MinBrightness, options.BrightnessStep);
        AddNodeSource(dimPulses
        .Select(_ => dimHelper.DimStep())
        .Where(t => t != null)
        .Select(t => (IPipelineNode<LightTransition>)(t == LightTransition.Off() ? new TurnOffThenPassThroughNode() : new StaticLightTransitionNode(t, _scheduler))));
        AddNodeSource(brightenPulses
        .Select(_ => dimHelper.BrightenStep())
        .Where(t => t != null)
        .Select(t => (IPipelineNode<LightTransition>)(t == LightTransition.Off() ? new TurnOffThenPassThroughNode() : new StaticLightTransitionNode(t, _scheduler))));
    }

    /// <summary>
    /// Adds a node source observable to the reactive node.
    /// </summary>
    /// <param name="nodeSource">An observable that emits pipeline nodes.</param>
    /// <returns>The configurator instance for method chaining.</returns>
    public ILightTransitionReactiveNodeConfigurator<TLight> AddNodeSource(IObservable<IPipelineNode<LightTransition>?> nodeSource)
    {
        NodeObservables.Add(nodeSource);
        return this;
    }

    /// <inheritdoc/>
    public ILightTransitionReactiveNodeConfigurator<TLight> AddNodeSource(IObservable<Func<IServiceProvider, IPipelineNode<LightTransition>?>> nodeFactorySource)
    {
        return AddNodeSource(nodeFactorySource.Select(nodeFactory => 
            nodeFactory.CreateScopedNodeOrNull(ServiceProvider) // Note: This service provider already has the light registered. We scope it further for node lifetime.
            ));
    }

    /// <inheritdoc/>
    public ILightTransitionReactiveNodeConfigurator<TLight> ForLight(string lightId,
        Action<ILightTransitionReactiveNodeConfigurator<TLight>> configure) => ForLights([lightId], configure);

    /// <inheritdoc/>
    public ILightTransitionReactiveNodeConfigurator<TLight> ForLight(TLight light,
        Action<ILightTransitionReactiveNodeConfigurator<TLight>> configure) => ForLights([light], configure);

    /// <inheritdoc/>
    public ILightTransitionReactiveNodeConfigurator<TLight> ForLights(IEnumerable<string> lightIds,
        Action<ILightTransitionReactiveNodeConfigurator<TLight>> configure)
    {
        CompositeHelper.ValidateLightSupported(lightIds, Light.Id);
        return this;
    }

    /// <inheritdoc/>
    public ILightTransitionReactiveNodeConfigurator<TLight> ForLights(IEnumerable<TLight> lights,
        Action<ILightTransitionReactiveNodeConfigurator<TLight>> configure)
    {
        CompositeHelper.ResolveGroupsAndValidateLightSupported(lights, Light.Id);
        return this;
    }
}