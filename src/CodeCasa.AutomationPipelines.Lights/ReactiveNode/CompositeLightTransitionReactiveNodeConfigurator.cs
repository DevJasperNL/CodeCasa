using System.Reactive.Concurrency;
using CodeCasa.Abstractions;
using CodeCasa.AutomationPipelines.Lights.Context;
using CodeCasa.AutomationPipelines.Lights.Extensions;
using CodeCasa.AutomationPipelines.Lights.Pipeline;
using CodeCasa.Lights;

namespace CodeCasa.AutomationPipelines.Lights.ReactiveNode;

public partial class CompositeLightTransitionReactiveNodeConfigurator(
    IServiceProvider serviceProvider,
    LightPipelineFactory lightPipelineFactory,
    ReactiveNodeFactory reactiveNodeFactory,
    Dictionary<string, LightTransitionReactiveNodeConfigurator> configurators,
    IScheduler scheduler)
    : ILightTransitionReactiveNodeConfigurator
{
    public ILightTransitionReactiveNodeConfigurator SetName(string name)
    {
        configurators.Values.ForEach(c => c.SetName(name));
        return this;
    }

    public ILightTransitionReactiveNodeConfigurator AddReactiveDimmer(IDimmer dimmer)
    {
        foreach (var configurator in configurators)
        {
            configurator.Value.AddReactiveDimmer(dimmer);
        }
        return this;
    }

    public ILightTransitionReactiveNodeConfigurator SetReactiveDimmerOptions(DimmerOptions dimmerOptions)
    {
        foreach (var configurator in configurators)
        {
            configurator.Value.SetReactiveDimmerOptions(dimmerOptions);
        }
        return this;
    }

    public ILightTransitionReactiveNodeConfigurator AddUncoupledDimmer(IDimmer dimmer)
    {
        return AddUncoupledDimmer(dimmer, _ => { });
    }

    public ILightTransitionReactiveNodeConfigurator AddUncoupledDimmer(IDimmer dimmer, Action<DimmerOptions> dimOptions)
    {
        throw new NotSupportedException();
    }

    public ILightTransitionReactiveNodeConfigurator AddNodeSource(IObservable<Func<ILightPipelineContext, IPipelineNode<LightTransition>?>> nodeFactorySource)
    {
        configurators.Values.ForEach(c => c.AddNodeSource(nodeFactorySource));
        return this;
    }

    public ILightTransitionReactiveNodeConfigurator ForLight(string lightEntityId, Action<ILightTransitionReactiveNodeConfigurator> configure) => ForLights([lightEntityId], configure);

    public ILightTransitionReactiveNodeConfigurator ForLight(ILight lightEntity, Action<ILightTransitionReactiveNodeConfigurator> configure) => ForLights([lightEntity], configure);

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

    public ILightTransitionReactiveNodeConfigurator ForLights(IEnumerable<ILight> lightEntities, Action<ILightTransitionReactiveNodeConfigurator> configure)
    {
        var lightIds = CompositeHelper.ResolveGroupsAndValidateLightsSupported(lightEntities, configurators.Keys);
        return ForLights(lightIds, configure);
    }
}