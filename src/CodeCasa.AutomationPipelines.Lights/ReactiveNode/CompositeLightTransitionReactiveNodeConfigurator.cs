using CodeCasa.Abstractions;
using CodeCasa.AutomationPipelines.Lights.Extensions;
using CodeCasa.AutomationPipelines.Lights.Pipeline;
using CodeCasa.Lights;
using System.Reactive.Concurrency;

namespace CodeCasa.AutomationPipelines.Lights.ReactiveNode;

/// <summary>
/// Configures light transition reactive nodes for multiple light entities as a composite.
/// This configurator applies configurations across all included lights and allows for selective scoping to subsets of lights.
/// </summary>
internal partial class CompositeLightTransitionReactiveNodeConfigurator<TLight>(
    IServiceProvider serviceProvider,
    LightPipelineFactory lightPipelineFactory,
    ReactiveNodeFactory reactiveNodeFactory,
    Dictionary<string, LightTransitionReactiveNodeConfigurator<TLight>> configurators,
    IScheduler scheduler)
    : ILightTransitionReactiveNodeConfigurator<TLight>
    where TLight : ILight
{
    /// <inheritdoc/>
    public ILightTransitionReactiveNodeConfigurator<TLight> AddReactiveDimmer(IDimmer dimmer)
    {
        foreach (var configurator in configurators)
        {
            configurator.Value.AddReactiveDimmer(dimmer);
        }
        return this;
    }

    /// <inheritdoc/>
    public ILightTransitionReactiveNodeConfigurator<TLight> SetReactiveDimmerOptions(DimmerOptions dimmerOptions)
    {
        foreach (var configurator in configurators)
        {
            configurator.Value.SetReactiveDimmerOptions(dimmerOptions);
        }
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

        var dimPulses = dimmer.Dimming.ToPulsesWhenTrue(options.TimeBetweenSteps, scheduler);
        var brightenPulses = dimmer.Brightening.ToPulsesWhenTrue(options.TimeBetweenSteps, scheduler);

        var configuratorsInOrder = options.ValidateAndOrderMultipleLightTypes(configurators).Select(kvp => kvp.Value).ToArray();
        foreach (var configurator in configuratorsInOrder)
        {
            var lightsInDimOrder = configuratorsInOrder.Select(c => (ILight)c.Light);
            configurator.AddDimPulses(options, lightsInDimOrder, dimPulses, brightenPulses);
        }
        return this;


    }

    /// <inheritdoc/>
    public ILightTransitionReactiveNodeConfigurator<TLight> AddNodeSource<TNodeSource>()
        where TNodeSource : IObservable<Func<IServiceProvider, IPipelineNode<LightTransition>?>>
    {
        configurators.Values.ForEach(c => c.AddNodeSource<TNodeSource>());
        return this;
    }

    /// <inheritdoc/>
    public ILightTransitionReactiveNodeConfigurator<TLight> AddNodeSource(
        Func<IServiceProvider, IObservable<Func<IServiceProvider, IPipelineNode<LightTransition>?>>> nodeFactorySourceFactory)
    {
        configurators.Values.ForEach(c => c.AddNodeSource(nodeFactorySourceFactory));
        return this;
    }

    /// <inheritdoc/>
    public ILightTransitionReactiveNodeConfigurator<TLight> AddNodeSource(IObservable<Func<IServiceProvider, IPipelineNode<LightTransition>?>> nodeFactorySource)
    {
        configurators.Values.ForEach(c => c.AddNodeSource(nodeFactorySource));
        return this;
    }

    /// <inheritdoc/>
    public ILightTransitionReactiveNodeConfigurator<TLight> ForLight(string lightId, Action<ILightTransitionReactiveNodeConfigurator<TLight>> configure) => ForLights([lightId], configure);

    /// <inheritdoc/>
    public ILightTransitionReactiveNodeConfigurator<TLight> ForLight(TLight light, Action<ILightTransitionReactiveNodeConfigurator<TLight>> configure) => ForLights([light], configure);

    /// <inheritdoc/>
    public ILightTransitionReactiveNodeConfigurator<TLight> ForLights(IEnumerable<string> lightIds, Action<ILightTransitionReactiveNodeConfigurator<TLight>> configure)
    {
        var lightIdsArray =
            CompositeHelper.ValidateLightsSupported(lightIds, configurators.Keys);

        if (lightIdsArray.Length == configurators.Count)
        {
            configure(this);
            return this;
        }
        if (lightIdsArray.Length == 1)
        {
            configure(configurators[lightIdsArray.First()]);
            return this;
        }

        configure(new CompositeLightTransitionReactiveNodeConfigurator<TLight>(
            serviceProvider, 
            lightPipelineFactory,
            reactiveNodeFactory,
            configurators
            .Where(kvp => lightIdsArray.Contains(kvp.Key)).ToDictionary(kvp => kvp.Key, kvp => kvp.Value), scheduler));
        return this;
    }

    /// <inheritdoc/>
    public ILightTransitionReactiveNodeConfigurator<TLight> ForLights(IEnumerable<TLight> lights, Action<ILightTransitionReactiveNodeConfigurator<TLight>> configure)
    {
        var lightIds = CompositeHelper.ResolveGroupsAndValidateLightsSupported(lights, configurators.Keys);
        return ForLights(lightIds, configure);
    }
}