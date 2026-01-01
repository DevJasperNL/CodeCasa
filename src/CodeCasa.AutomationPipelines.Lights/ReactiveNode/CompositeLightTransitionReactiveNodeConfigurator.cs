using System.Reactive.Concurrency;
using CodeCasa.Abstractions;
using CodeCasa.AutomationPipelines.Lights.Context;
using CodeCasa.AutomationPipelines.Lights.Extensions;
using CodeCasa.AutomationPipelines.Lights.Pipeline;
using CodeCasa.Lights;

namespace CodeCasa.AutomationPipelines.Lights.ReactiveNode;

/// <summary>
/// Configures light transition reactive nodes for multiple light entities as a composite.
/// This configurator applies configurations across all included lights and allows for selective scoping to subsets of lights.
/// </summary>
internal partial class CompositeLightTransitionReactiveNodeConfigurator(
    IServiceProvider serviceProvider,
    LightPipelineFactory lightPipelineFactory,
    ReactiveNodeFactory reactiveNodeFactory,
    Dictionary<string, LightTransitionReactiveNodeConfigurator> configurators,
    IScheduler scheduler)
    : ILightTransitionReactiveNodeConfigurator
{
    /// <inheritdoc/>
    public ILightTransitionReactiveNodeConfigurator SetName(string name)
    {
        configurators.Values.ForEach(c => c.SetName(name));
        return this;
    }

    /// <inheritdoc/>
    public ILightTransitionReactiveNodeConfigurator AddReactiveDimmer(IDimmer dimmer)
    {
        foreach (var configurator in configurators)
        {
            configurator.Value.AddReactiveDimmer(dimmer);
        }
        return this;
    }

    /// <inheritdoc/>
    public ILightTransitionReactiveNodeConfigurator SetReactiveDimmerOptions(DimmerOptions dimmerOptions)
    {
        foreach (var configurator in configurators)
        {
            configurator.Value.SetReactiveDimmerOptions(dimmerOptions);
        }
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

        var dimPulses = dimmer.Dimming.ToPulsesWhenTrue(options.TimeBetweenSteps, scheduler);
        var brightenPulses = dimmer.Brightening.ToPulsesWhenTrue(options.TimeBetweenSteps, scheduler);

        var configuratorsInOrder = options.ValidateAndOrderMultipleLightTypes(configurators).Select(kvp => kvp.Value).ToArray();
        foreach (var configurator in configuratorsInOrder)
        {
            var lightsInDimOrder = configuratorsInOrder.Select(c => c.Light);
            configurator.AddDimPulses(options, lightsInDimOrder, dimPulses, brightenPulses);
        }
        return this;

    }

    /// <inheritdoc/>
    public ILightTransitionReactiveNodeConfigurator AddNodeSource(IObservable<Func<ILightPipelineContext, IPipelineNode<LightTransition>?>> nodeFactorySource)
    {
        configurators.Values.ForEach(c => c.AddNodeSource(nodeFactorySource));
        return this;
    }

    /// <inheritdoc/>
    public ILightTransitionReactiveNodeConfigurator ForLight(string lightId, Action<ILightTransitionReactiveNodeConfigurator> configure) => ForLights([lightId], configure);

    /// <inheritdoc/>
    public ILightTransitionReactiveNodeConfigurator ForLight(ILight light, Action<ILightTransitionReactiveNodeConfigurator> configure) => ForLights([light], configure);

    /// <inheritdoc/>
    public ILightTransitionReactiveNodeConfigurator ForLights(IEnumerable<string> lightIds, Action<ILightTransitionReactiveNodeConfigurator> configure)
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

        configure(new CompositeLightTransitionReactiveNodeConfigurator(
            serviceProvider, 
            lightPipelineFactory,
            reactiveNodeFactory,
            configurators
            .Where(kvp => lightIdsArray.Contains(kvp.Key)).ToDictionary(kvp => kvp.Key, kvp => kvp.Value), scheduler));
        return this;
    }

    /// <inheritdoc/>
    public ILightTransitionReactiveNodeConfigurator ForLights(IEnumerable<ILight> lights, Action<ILightTransitionReactiveNodeConfigurator> configure)
    {
        var lightIds = CompositeHelper.ResolveGroupsAndValidateLightsSupported(lights, configurators.Keys);
        return ForLights(lightIds, configure);
    }
}