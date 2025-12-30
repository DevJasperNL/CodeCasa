using CodeCasa.AutomationPipelines.Lights.Context;
using CodeCasa.AutomationPipelines.Lights.Extensions;
using CodeCasa.AutomationPipelines.Lights.ReactiveNode;
using Microsoft.Extensions.DependencyInjection;


using System.Reactive.Linq;
using CodeCasa.Lights;

namespace CodeCasa.AutomationPipelines.Lights.Pipeline;

public partial class CompositeLightTransitionPipelineConfigurator
{
    /// <inheritdoc />
    public ILightTransitionPipelineConfigurator When<TObservable>(LightParameters lightParameters)
        where TObservable : IObservable<bool>
    {
        NodeContainers.Values.ForEach(b => b.When<TObservable>(lightParameters));
        return this;
    }

    /// <inheritdoc />
    public ILightTransitionPipelineConfigurator When(IObservable<bool> observable,
        LightParameters lightParameters)
    {
        NodeContainers.Values.ForEach(b => b.When(observable, lightParameters));
        return this;
    }

    /// <inheritdoc />
    public ILightTransitionPipelineConfigurator When<TObservable>(
        Func<ILightPipelineContext, LightParameters> lightParametersFactory) where TObservable : IObservable<bool>
    {
        NodeContainers.Values.ForEach(b => b.When<TObservable>(lightParametersFactory));
        return this;
    }

    /// <inheritdoc />
    public ILightTransitionPipelineConfigurator When(IObservable<bool> observable,
        Func<ILightPipelineContext, LightParameters> lightParametersFactory)
    {
        NodeContainers.Values.ForEach(b => b.When(observable, lightParametersFactory));
        return this;
    }

    /// <inheritdoc />
    public ILightTransitionPipelineConfigurator When<TObservable>(LightTransition lightTransition)
        where TObservable : IObservable<bool>
    {
        NodeContainers.Values.ForEach(b => b.When<TObservable>(lightTransition));
        return this;
    }

    /// <inheritdoc />
    public ILightTransitionPipelineConfigurator When(IObservable<bool> observable,
        LightTransition lightTransition)
    {
        NodeContainers.Values.ForEach(b => b.When(observable, lightTransition));
        return this;
    }

    /// <inheritdoc />
    public ILightTransitionPipelineConfigurator When<TObservable>(
        Func<ILightPipelineContext, LightTransition> lightTransitionFactory) where TObservable : IObservable<bool>
    {
        NodeContainers.Values.ForEach(b => b.When<TObservable>(lightTransitionFactory));
        return this;
    }

    /// <inheritdoc />
    public ILightTransitionPipelineConfigurator When(IObservable<bool> observable,
        Func<ILightPipelineContext, LightTransition> lightTransitionFactory)
    {
        NodeContainers.Values.ForEach(b => b.When(observable, lightTransitionFactory));
        return this;
    }

    /// <inheritdoc />
    public ILightTransitionPipelineConfigurator When<TObservable>(
        Func<ILightPipelineContext, IPipelineNode<LightTransition>> nodeFactory) where TObservable : IObservable<bool>
    {
        NodeContainers.Values.ForEach(b => b.When<TObservable>(nodeFactory));
        return this;
    }

    /// <inheritdoc />
    public ILightTransitionPipelineConfigurator When(IObservable<bool> observable,
        Func<ILightPipelineContext, IPipelineNode<LightTransition>> nodeFactory)
    {
        NodeContainers.Values.ForEach(b => b.When(observable, nodeFactory));
        return this;
    }

    /// <inheritdoc />
    public ILightTransitionPipelineConfigurator When<TObservable, TNode>()
        where TObservable : IObservable<bool> where TNode : IPipelineNode<LightTransition>
    {
        NodeContainers.Values.ForEach(b => b.When<TObservable, TNode>());
        return this;
    }

    /// <inheritdoc />
    public ILightTransitionPipelineConfigurator When<TNode>(IObservable<bool> observable)
        where TNode : IPipelineNode<LightTransition>
    {
        NodeContainers.Values.ForEach(b => b.When<TNode>(observable));
        return this;
    }

    /// <inheritdoc />
    public ILightTransitionPipelineConfigurator AddReactiveNodeWhen<TObservable>(Action<ILightTransitionReactiveNodeConfigurator> configure) where TObservable : IObservable<bool>
    {
        /*
         * For this implementation we can either instantiate the TObservable for each container and pass configure to them individual, breaking composite dimming behavior.
         * Or we can create a single TObservable without light context.
         * I decided to go with the latter to preserve composite dimming behavior.
         */
        var observable = ActivatorUtilities.CreateInstance<TObservable>(serviceProvider);
        return AddReactiveNodeWhen(observable, configure);
    }

    /// <inheritdoc />
    public ILightTransitionPipelineConfigurator AddReactiveNodeWhen(IObservable<bool> observable, Action<ILightTransitionReactiveNodeConfigurator> configure)
    {
        // Note: we use CompositeLightTransitionPipelineConfigurator.AddReactiveNode so configure is also applied on the composite context.
        return AddReactiveNode(c => c
            .On(observable.Where(x => x), configure)
            .PassThroughOn(observable.Where(x => !x)));
    }

    /// <inheritdoc />
    public ILightTransitionPipelineConfigurator AddPipelineWhen<TObservable>(Action<ILightTransitionPipelineConfigurator> configure) where TObservable : IObservable<bool>
    {
        var observable = ActivatorUtilities.CreateInstance<TObservable>(serviceProvider);
        return AddPipelineWhen(observable, configure);
    }

    /// <inheritdoc />
    public ILightTransitionPipelineConfigurator AddPipelineWhen(IObservable<bool> observable, Action<ILightTransitionPipelineConfigurator> configure)
    {
        // Note: we use CompositeLightTransitionPipelineConfigurator.AddReactiveNode so configure is also applied on the composite context.
        return AddReactiveNode(c => c
            .On(observable.Where(x => x), configure)
            .PassThroughOn(observable.Where(x => !x)));
    }

    /// <inheritdoc />
    public ILightTransitionPipelineConfigurator TurnOffWhen<TObservable>() where TObservable : IObservable<bool>
    {
        NodeContainers.Values.ForEach(b => b.TurnOffWhen<TObservable>());
        return this;
    }

    /// <inheritdoc />
    public ILightTransitionPipelineConfigurator TurnOffWhen(IObservable<bool> observable)
    {
        NodeContainers.Values.ForEach(b => b.TurnOffWhen(observable));
        return this;
    }

    /// <inheritdoc />
    public ILightTransitionPipelineConfigurator TurnOnWhen<TObservable>() where TObservable : IObservable<bool>
    {
        return When<TObservable>(LightTransition.On());
    }

    /// <inheritdoc />
    public ILightTransitionPipelineConfigurator TurnOnWhen(IObservable<bool> observable)
    {
        return When(observable, LightTransition.On());
    }
}