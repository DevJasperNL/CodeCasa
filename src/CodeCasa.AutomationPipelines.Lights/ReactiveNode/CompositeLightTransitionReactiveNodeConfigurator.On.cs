using System.Reactive.Concurrency;
using CodeCasa.AutomationPipelines.Lights.Extensions;
using CodeCasa.AutomationPipelines.Lights.Nodes;
using CodeCasa.AutomationPipelines.Lights.Pipeline;
using CodeCasa.Lights;
using Microsoft.Extensions.DependencyInjection;

namespace CodeCasa.AutomationPipelines.Lights.ReactiveNode;

internal partial class CompositeLightTransitionReactiveNodeConfigurator<TLight>
{
    /// <inheritdoc/>
    public ILightTransitionReactiveNodeConfigurator<TLight> On<T>(IObservable<T> triggerObservable, LightParameters lightParameters)
        => On(triggerObservable, lightParameters.AsTransition());

    /// <inheritdoc/>
    public ILightTransitionReactiveNodeConfigurator<TLight> On<T>(IObservable<T> triggerObservable, Func<IServiceProvider, LightParameters> lightParametersFactory)
        => On(triggerObservable, c => lightParametersFactory(c).AsTransition());

    /// <inheritdoc/>
    public ILightTransitionReactiveNodeConfigurator<TLight> On<T>(IObservable<T> triggerObservable, LightTransition lightTransition)
        => On(triggerObservable, c => new StaticLightTransitionNode(lightTransition, c.GetRequiredService<IScheduler>()));

    /// <inheritdoc/>
    public ILightTransitionReactiveNodeConfigurator<TLight> On<T>(IObservable<T> triggerObservable, Func<IServiceProvider, LightTransition> lightTransitionFactory)
        => On(triggerObservable, c => new StaticLightTransitionNode(lightTransitionFactory(c), c.GetRequiredService<IScheduler>()));

    /// <inheritdoc/>
    public ILightTransitionReactiveNodeConfigurator<TLight> On<T, TNode>(IObservable<T> triggerObservable) where TNode : IPipelineNode<LightTransition>
    {
        var shareableTriggerObservable = _observableSharingStrategy.Apply(triggerObservable);
        configurators.Values.ForEach(c => c.On<T, TNode>(shareableTriggerObservable));
        return this;
    }

    /// <inheritdoc/>
    public ILightTransitionReactiveNodeConfigurator<TLight> On<T>(IObservable<T> triggerObservable, Func<IServiceProvider, IPipelineNode<LightTransition>> nodeFactory)
    {
        var shareableTriggerObservable = _observableSharingStrategy.Apply(triggerObservable);
        configurators.Values.ForEach(c => c.On(shareableTriggerObservable, nodeFactory));
        return this;
    }

    /// <inheritdoc/>
    public ILightTransitionReactiveNodeConfigurator<TLight> On<T>(IObservable<T> triggerObservable, Action<ILightTransitionPipelineConfigurator<TLight>> pipelineConfigurator, InstantiationScope instantiationScope = InstantiationScope.Shared)
    {
        var shareableTriggerObservable = _observableSharingStrategy.Apply(triggerObservable);
        if (instantiationScope == InstantiationScope.Shared)
        {
            var factories = lightPipelineFactory.CreateCompositePipelineFactoryMap(
                pipelineConfigurator
                    .ApplyHierarchySettings(HierarchyPath, LoggingEnabled ?? false)
                    .SetObservableSharingStrategy(_observableSharingStrategy),
                configurators.Values.Select(c => c.Light).ToArray());
            configurators.Values.ForEach(c => c.On(shareableTriggerObservable, factories[c.Light.Id]));
            return this;
        }

        configurators.Values.ForEach(c => c.On(shareableTriggerObservable, pipelineConfigurator
            .ApplyHierarchySettings(HierarchyPath, LoggingEnabled ?? false)
            .SetObservableSharingStrategy(_observableSharingStrategy), instantiationScope));
        return this;
    }

    /// <inheritdoc/>
    public ILightTransitionReactiveNodeConfigurator<TLight> On<T>(IObservable<T> triggerObservable, Action<ILightTransitionReactiveNodeConfigurator<TLight>> configure, InstantiationScope instantiationScope = InstantiationScope.Shared)
    {
        var shareableTriggerObservable = _observableSharingStrategy.Apply(triggerObservable);
        if (instantiationScope == InstantiationScope.Shared)
        {
            var factories = reactiveNodeFactory.CreateCompositeReactiveNodeFactoryMap(
                configure
                    .ApplyHierarchySettings(HierarchyPath, LoggingEnabled ?? false)
                    .SetObservableSharingStrategy(_observableSharingStrategy),
                configurators.Values.Select(c => c.Light).ToArray());
            configurators.Values.ForEach(c => c.On(shareableTriggerObservable, factories[c.Light.Id]));
            return this;
        }

        configurators.Values.ForEach(c => c.On(shareableTriggerObservable, configure
            .ApplyHierarchySettings(HierarchyPath, LoggingEnabled ?? false)
            .SetObservableSharingStrategy(_observableSharingStrategy), instantiationScope));
        return this;
    }

    /// <inheritdoc/>
    public ILightTransitionReactiveNodeConfigurator<TLight> PassThroughOn<T>(IObservable<T> triggerObservable)
    {
        var shareableTriggerObservable = _observableSharingStrategy.Apply(triggerObservable);
        configurators.Values.ForEach(c => c.PassThroughOn(shareableTriggerObservable));
        return this;
    }

    /// <inheritdoc/>
    public ILightTransitionReactiveNodeConfigurator<TLight> TurnOffWhen<T>(IObservable<T> triggerObservable)
    {
        var shareableTriggerObservable = _observableSharingStrategy.Apply(triggerObservable);
        configurators.Values.ForEach(c => c.TurnOffWhen(shareableTriggerObservable));
        return this;
    }

    /// <inheritdoc/>
    public ILightTransitionReactiveNodeConfigurator<TLight> TurnOnWhen<T>(IObservable<T> triggerObservable)
    {
        return On(triggerObservable, LightTransition.On());
    }
}
