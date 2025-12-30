using System.Reactive.Concurrency;
using CodeCasa.AutomationPipelines.Lights.Context;
using CodeCasa.AutomationPipelines.Lights.Extensions;
using CodeCasa.AutomationPipelines.Lights.Nodes;
using CodeCasa.AutomationPipelines.Lights.Pipeline;
using CodeCasa.Lights;
using Microsoft.Extensions.DependencyInjection;

namespace CodeCasa.AutomationPipelines.Lights.ReactiveNode;

public partial class CompositeLightTransitionReactiveNodeConfigurator
{
    /// <inheritdoc/>
    public ILightTransitionReactiveNodeConfigurator On<T>(IObservable<T> triggerObservable, LightParameters lightParameters)
        => On(triggerObservable, lightParameters.AsTransition());

    /// <inheritdoc/>
    public ILightTransitionReactiveNodeConfigurator On<T>(IObservable<T> triggerObservable, Func<ILightPipelineContext, LightParameters> lightParametersFactory)
        => On(triggerObservable, c => lightParametersFactory(c).AsTransition());

    /// <inheritdoc/>
    public ILightTransitionReactiveNodeConfigurator On<T>(IObservable<T> triggerObservable, LightTransition lightTransition)
        => On(triggerObservable, c => new StaticLightTransitionNode(lightTransition, c.ServiceProvider.GetRequiredService<IScheduler>()));

    /// <inheritdoc/>
    public ILightTransitionReactiveNodeConfigurator On<T>(IObservable<T> triggerObservable, Func<ILightPipelineContext, LightTransition> lightTransitionFactory)
        => On(triggerObservable, c => new StaticLightTransitionNode(lightTransitionFactory(c), c.ServiceProvider.GetRequiredService<IScheduler>()));

    /// <inheritdoc/>
    public ILightTransitionReactiveNodeConfigurator On<T, TNode>(IObservable<T> triggerObservable) where TNode : IPipelineNode<LightTransition>
    {
        configurators.Values.ForEach(c => c.On<T, TNode>(triggerObservable));
        return this;
    }

    /// <inheritdoc/>
    public ILightTransitionReactiveNodeConfigurator On<T>(IObservable<T> triggerObservable, Func<ILightPipelineContext, IPipelineNode<LightTransition>> nodeFactory)
    {
        configurators.Values.ForEach(c => c.On(triggerObservable, nodeFactory));
        return this;
    }

    /// <inheritdoc/>
    public ILightTransitionReactiveNodeConfigurator On<T>(IObservable<T> triggerObservable, Action<ILightTransitionPipelineConfigurator> pipelineConfigurator)
    {
        // Note: we create the pipeline in composite context so all configuration is also applied in that context.
        var pipelines = lightPipelineFactory.CreateLightPipelines(configurators.Values.Select(c => c.LightEntity),
            pipelineConfigurator);
        configurators.Values.ForEach(c => c.On(triggerObservable, ctx => pipelines[ctx.LightEntity.Id]));
        return this;
    }

    /// <inheritdoc/>
    public ILightTransitionReactiveNodeConfigurator On<T>(IObservable<T> triggerObservable, Action<ILightTransitionReactiveNodeConfigurator> configure)
    {
        // Note: we create the pipeline in composite context so all configuration is also applied in that context.
        var nodes = reactiveNodeFactory.CreateReactiveNodes(configurators.Values.Select(c => c.LightEntity),
            configure);
        configurators.Values.ForEach(c => c.On(triggerObservable, ctx => nodes[ctx.LightEntity.Id]));
        return this;
    }

    /// <inheritdoc/>
    public ILightTransitionReactiveNodeConfigurator PassThroughOn<T>(IObservable<T> triggerObservable)
    {
        configurators.Values.ForEach(c => c.PassThroughOn(triggerObservable));
        return this;
    }

    /// <inheritdoc/>
    public ILightTransitionReactiveNodeConfigurator TurnOffWhen<T>(IObservable<T> triggerObservable)
    {
        configurators.Values.ForEach(c => c.TurnOffWhen(triggerObservable));
        return this;
    }

    /// <inheritdoc/>
    public ILightTransitionReactiveNodeConfigurator TurnOnWhen<T>(IObservable<T> triggerObservable)
    {
        return On(triggerObservable, LightTransition.On());
    }
}