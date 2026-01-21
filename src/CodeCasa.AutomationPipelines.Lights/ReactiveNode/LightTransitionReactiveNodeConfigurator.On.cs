using CodeCasa.AutomationPipelines.Lights.Nodes;
using CodeCasa.AutomationPipelines.Lights.Pipeline;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using CodeCasa.Lights;
using Microsoft.Extensions.DependencyInjection;

namespace CodeCasa.AutomationPipelines.Lights.ReactiveNode;

internal partial class LightTransitionReactiveNodeConfigurator<TLight>
{
    /// <inheritdoc/>
    public ILightTransitionReactiveNodeConfigurator<TLight> On<T>(IObservable<T> triggerObservable,
        LightParameters lightParameters) => On(triggerObservable, lightParameters.AsTransition());

    /// <inheritdoc/>
    public ILightTransitionReactiveNodeConfigurator<TLight> On<T>(IObservable<T> triggerObservable,
        Func<IServiceProvider, LightParameters> lightParametersFactory)
        => On(triggerObservable, c => lightParametersFactory(c).AsTransition());

    /// <inheritdoc/>
    public ILightTransitionReactiveNodeConfigurator<TLight> On<T>(IObservable<T> triggerObservable,
        LightTransition lightTransition) =>
        On(triggerObservable, c => new StaticLightTransitionNode(lightTransition, c.GetRequiredService<IScheduler>()));

    /// <inheritdoc/>
    public ILightTransitionReactiveNodeConfigurator<TLight> On<T>(IObservable<T> triggerObservable,
        Func<IServiceProvider, LightTransition> lightTransitionFactory)
        => On(triggerObservable, c => new StaticLightTransitionNode(lightTransitionFactory(c), c.GetRequiredService<IScheduler>()));

    /// <inheritdoc/>
    public ILightTransitionReactiveNodeConfigurator<TLight> On<T, TNode>(IObservable<T> triggerObservable)
        where TNode : IPipelineNode<LightTransition> =>
        AddNodeSource(triggerObservable.Select(_ =>
            new Func<IServiceProvider, IPipelineNode<LightTransition>?>(c =>
                ActivatorUtilities.CreateInstance<TNode>(c))));

    /// <inheritdoc/>
    public ILightTransitionReactiveNodeConfigurator<TLight> On<T>(IObservable<T> triggerObservable,
        Func<IServiceProvider, IPipelineNode<LightTransition>> nodeFactory) =>
        AddNodeSource(triggerObservable.Select(_ => nodeFactory));

    /// <inheritdoc/>
    public ILightTransitionReactiveNodeConfigurator<TLight> On<T>(IObservable<T> triggerObservable, Action<ILightTransitionPipelineConfigurator<TLight>> pipelineConfigurator) => On(triggerObservable, s => s.GetRequiredService<LightPipelineFactory>().CreateLightPipeline(light, pipelineConfigurator));

    /// <inheritdoc/>
    public ILightTransitionReactiveNodeConfigurator<TLight> On<T>(IObservable<T> triggerObservable, Action<ILightTransitionReactiveNodeConfigurator<TLight>> configure) => On(triggerObservable, s => s.GetRequiredService<ReactiveNodeFactory>().CreateReactiveNode(light, configure));

    /// <inheritdoc/>
    public ILightTransitionReactiveNodeConfigurator<TLight> PassThroughOn<T>(IObservable<T> triggerObservable)
    {
        AddNodeSource(triggerObservable.Select(_ => new PassThroughNode<LightTransition>()));
        return this;
    }

    /// <inheritdoc/>
    public ILightTransitionReactiveNodeConfigurator<TLight> TurnOffWhen<T>(IObservable<T> triggerObservable)
    {
        AddNodeSource(triggerObservable.Select(_ => new TurnOffThenPassThroughNode()));
        return this;
    }

    /// <inheritdoc/>
    public ILightTransitionReactiveNodeConfigurator<TLight> TurnOnWhen<T>(IObservable<T> triggerObservable)
    {
        return On(triggerObservable, LightTransition.On());
    }
}
