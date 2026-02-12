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
        configurators.Values.ForEach(c => c.On<T, TNode>(triggerObservable));
        return this;
    }

    /// <inheritdoc/>
    public ILightTransitionReactiveNodeConfigurator<TLight> On<T>(IObservable<T> triggerObservable, Func<IServiceProvider, IPipelineNode<LightTransition>> nodeFactory)
    {
        configurators.Values.ForEach(c => c.On(triggerObservable, nodeFactory));
        return this;
    }

    /// <inheritdoc/>
    public ILightTransitionReactiveNodeConfigurator<TLight> On<T>(IObservable<T> triggerObservable, Action<ILightTransitionPipelineConfigurator<TLight>> pipelineConfigurator, CompositeAddBehavior addBehavior = CompositeAddBehavior.ImmediatelyInstantiateInCompositeContext)
    {
        if (addBehavior == CompositeAddBehavior.ImmediatelyInstantiateInCompositeContext)
        {
            // Note: we create the pipeline in composite context so all configuration is also applied in that context.
            var pipelines = lightPipelineFactory.CreateLightPipelines(configurators.Values.Select(c => c.Light),
                pipelineConfigurator);
            configurators.Values.ForEach(c => c.On(triggerObservable, _ => pipelines[c.Light.Id]));
            return this;
        }

        configurators.Values.ForEach(c => c.On(triggerObservable, pipelineConfigurator));
        return this;
    }

    /// <inheritdoc/>
    public ILightTransitionReactiveNodeConfigurator<TLight> On<T>(IObservable<T> triggerObservable, Action<ILightTransitionReactiveNodeConfigurator<TLight>> configure, CompositeAddBehavior addBehavior = CompositeAddBehavior.ImmediatelyInstantiateInCompositeContext)
    {
        if (addBehavior == CompositeAddBehavior.ImmediatelyInstantiateInCompositeContext)
        {
            // Note: we create the pipeline in composite context so all configuration is also applied in that context.
            var nodes = reactiveNodeFactory.CreateReactiveNodes(configurators.Values.Select(c => c.Light),
                configure);
            configurators.Values.ForEach(c => c.On(triggerObservable, _ => nodes[c.Light.Id]));
            return this;
        }

        configurators.Values.ForEach(c => c.On(triggerObservable, configure));
        return this;
    }

    /// <inheritdoc/>
    public ILightTransitionReactiveNodeConfigurator<TLight> PassThroughOn<T>(IObservable<T> triggerObservable)
    {
        configurators.Values.ForEach(c => c.PassThroughOn(triggerObservable));
        return this;
    }

    /// <inheritdoc/>
    public ILightTransitionReactiveNodeConfigurator<TLight> TurnOffWhen<T>(IObservable<T> triggerObservable)
    {
        configurators.Values.ForEach(c => c.TurnOffWhen(triggerObservable));
        return this;
    }

    /// <inheritdoc/>
    public ILightTransitionReactiveNodeConfigurator<TLight> TurnOnWhen<T>(IObservable<T> triggerObservable)
    {
        return On(triggerObservable, LightTransition.On());
    }
}
