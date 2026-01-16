using CodeCasa.AutomationPipelines.Lights.Context;
using CodeCasa.AutomationPipelines.Lights.Extensions;
using CodeCasa.AutomationPipelines.Lights.Nodes;
using CodeCasa.AutomationPipelines.Lights.ReactiveNode;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using CodeCasa.Lights;
using Microsoft.Extensions.DependencyInjection;

namespace CodeCasa.AutomationPipelines.Lights.Pipeline;

internal partial class LightTransitionPipelineConfigurator<TLight>
{
    /// <inheritdoc/>
    public ILightTransitionPipelineConfigurator<TLight> When<TObservable>(LightParameters lightParameters)
        where TObservable : IObservable<bool>
    {
        return When<TObservable>(lightParameters.AsTransition());
    }

    /// <inheritdoc/>
    public ILightTransitionPipelineConfigurator<TLight> When(IObservable<bool> observable,
        LightParameters lightParameters)
    {
        return When(observable, lightParameters.AsTransition());
    }

    /// <inheritdoc/>
    public ILightTransitionPipelineConfigurator<TLight> When<TObservable>(
        Func<ILightPipelineContext<TLight>, LightParameters> lightParametersFactory) where TObservable : IObservable<bool>
    {
        return When<TObservable>(c => lightParametersFactory(c).AsTransition());
    }

    /// <inheritdoc/>
    public ILightTransitionPipelineConfigurator<TLight> When(IObservable<bool> observable,
        Func<ILightPipelineContext<TLight>, LightParameters> lightParametersFactory)
    {
        return When(observable, c => lightParametersFactory(c).AsTransition());
    }

    /// <inheritdoc/>
    public ILightTransitionPipelineConfigurator<TLight> When<TObservable>(LightTransition lightTransition)
        where TObservable : IObservable<bool>
    {
        return When<TObservable>(_ => lightTransition);
    }

    /// <inheritdoc/>
    public ILightTransitionPipelineConfigurator<TLight> When(IObservable<bool> observable,
        LightTransition lightTransition)
    {
        return When(observable, _ => lightTransition);
    }

    /// <inheritdoc/>
    public ILightTransitionPipelineConfigurator<TLight> When<TObservable>(
        Func<ILightPipelineContext<TLight>, LightTransition> lightTransitionFactory) where TObservable : IObservable<bool>
    {
        return When<TObservable>(c => new StaticLightTransitionNode(lightTransitionFactory(c), c.ServiceProvider.GetRequiredService<IScheduler>()));
    }

    /// <inheritdoc/>
    public ILightTransitionPipelineConfigurator<TLight> When(IObservable<bool> observable,
        Func<ILightPipelineContext<TLight>, LightTransition> lightTransitionFactory)
    {
        return When(observable, c => new StaticLightTransitionNode(lightTransitionFactory(c), c.ServiceProvider.GetRequiredService<IScheduler>()));
    }

    /// <inheritdoc/>
    public ILightTransitionPipelineConfigurator<TLight> When<TObservable>(
        Func<ILightPipelineContext<TLight>, IPipelineNode<LightTransition>> nodeFactory) where TObservable : IObservable<bool>
    {
        var observable = serviceProvider.CreateInstanceWithinContext<TObservable, TLight>(Light);
        return When(observable, nodeFactory);
    }

    /// <inheritdoc/>
    public ILightTransitionPipelineConfigurator<TLight> When(IObservable<bool> observable,
        Func<ILightPipelineContext<TLight>, IPipelineNode<LightTransition>> nodeFactory)
    {
        return AddReactiveNode(c => c
            .On(observable.Where(x => x), nodeFactory)
            .PassThroughOn(observable.Where(x => !x)));
    }

    /// <inheritdoc/>
    public ILightTransitionPipelineConfigurator<TLight> When<TObservable, TNode>()
        where TObservable : IObservable<bool>
        where TNode : IPipelineNode<LightTransition>
    {
        var observable = serviceProvider.CreateInstanceWithinContext<TObservable, TLight>(Light);
        return When<TNode>(observable);
    }

    /// <inheritdoc/>
    public ILightTransitionPipelineConfigurator<TLight> When<TNode>(IObservable<bool> observable)
        where TNode : IPipelineNode<LightTransition>
    {
        return AddReactiveNode(c => c
            .On<bool, TNode>(observable.Where(x => x))
            .PassThroughOn(observable.Where(x => !x)));
    }

    /// <inheritdoc/>
    public ILightTransitionPipelineConfigurator<TLight> AddReactiveNodeWhen<TObservable>(Action<ILightTransitionReactiveNodeConfigurator<TLight>> configure) where TObservable : IObservable<bool>
    {
        var observable = serviceProvider.CreateInstanceWithinContext<TObservable, TLight>(Light);
        return AddReactiveNodeWhen(observable, configure);
    }

    /// <inheritdoc/>
    public ILightTransitionPipelineConfigurator<TLight> AddReactiveNodeWhen(IObservable<bool> observable, Action<ILightTransitionReactiveNodeConfigurator<TLight>> configure)
    {
        return AddReactiveNode(c => c
            .On(observable.Where(x => x), configure)
            .PassThroughOn(observable.Where(x => !x)));
    }

    /// <inheritdoc/>
    public ILightTransitionPipelineConfigurator<TLight> AddPipelineWhen<TObservable>(Action<ILightTransitionPipelineConfigurator<TLight>> pipelineConfigurator) where TObservable : IObservable<bool>
    {
        return When<TObservable>(c => lightPipelineFactory.CreateLightPipeline(c.Light, pipelineConfigurator));
    }

    /// <inheritdoc/>
    public ILightTransitionPipelineConfigurator<TLight> AddPipelineWhen(IObservable<bool> observable, Action<ILightTransitionPipelineConfigurator<TLight>> pipelineConfigurator)
    {
        return When(observable, c => lightPipelineFactory.CreateLightPipeline(c.Light, pipelineConfigurator));
    }

    /// <inheritdoc/>
    public ILightTransitionPipelineConfigurator<TLight> TurnOffWhen<TObservable>() where TObservable : IObservable<bool>
    {
        return When<TObservable>(LightTransition.Off());
    }

    /// <inheritdoc/>
    public ILightTransitionPipelineConfigurator<TLight> TurnOffWhen(IObservable<bool> observable)
    {
        return When(observable, LightTransition.Off());
    }

    /// <inheritdoc/>
    public ILightTransitionPipelineConfigurator<TLight> TurnOnWhen<TObservable>() where TObservable : IObservable<bool>
    {
        return When<TObservable>(LightTransition.On());
    }

    /// <inheritdoc/>
    public ILightTransitionPipelineConfigurator<TLight> TurnOnWhen(IObservable<bool> observable)
    {
        return When(observable, LightTransition.On());
    }
}
