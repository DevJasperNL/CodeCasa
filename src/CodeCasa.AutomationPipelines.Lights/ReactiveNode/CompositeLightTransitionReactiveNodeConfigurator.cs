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
public partial class CompositeLightTransitionReactiveNodeConfigurator(
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
        // todo: support.
        throw new NotSupportedException();
    }

    /// <inheritdoc/>
    public ILightTransitionReactiveNodeConfigurator AddNodeSource(IObservable<Func<ILightPipelineContext, IPipelineNode<LightTransition>?>> nodeFactorySource)
    {
        configurators.Values.ForEach(c => c.AddNodeSource(nodeFactorySource));
        return this;
    }

    /// <inheritdoc/>
    public ILightTransitionReactiveNodeConfigurator ForLight(string lightEntityId, Action<ILightTransitionReactiveNodeConfigurator> configure) => ForLights([lightEntityId], configure);

    /// <inheritdoc/>
    public ILightTransitionReactiveNodeConfigurator ForLight(ILight lightEntity, Action<ILightTransitionReactiveNodeConfigurator> configure) => ForLights([lightEntity], configure);

    /// <inheritdoc/>
    public ILightTransitionReactiveNodeConfigurator ForLights(IEnumerable<string> lightEntityIds, Action<ILightTransitionReactiveNodeConfigurator> configure)
    {
        var lightEntityIdsArray =
            CompositeHelper.ValidateLightsSupported(lightEntityIds, configurators.Keys);

        if (lightEntityIdsArray.Length == configurators.Count)
        {
            configure(this);
            return this;
        }
        if (lightEntityIdsArray.Length == 1)
        {
            configure(configurators[lightEntityIdsArray.First()]);
            return this;
        }

        configure(new CompositeLightTransitionReactiveNodeConfigurator(
            serviceProvider, 
            lightPipelineFactory,
            reactiveNodeFactory,
            configurators
            .Where(kvp => lightEntityIdsArray.Contains(kvp.Key)).ToDictionary(kvp => kvp.Key, kvp => kvp.Value), scheduler));
        return this;
    }

    /// <inheritdoc/>
    public ILightTransitionReactiveNodeConfigurator ForLights(IEnumerable<ILight> lightEntities, Action<ILightTransitionReactiveNodeConfigurator> configure)
    {
        var lightIds = CompositeHelper.ResolveGroupsAndValidateLightsSupported(lightEntities, configurators.Keys);
        return ForLights(lightIds, configure);
    }
}