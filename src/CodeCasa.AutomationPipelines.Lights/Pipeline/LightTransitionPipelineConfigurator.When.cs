using CodeCasa.AutomationPipelines.Lights.Context;
using CodeCasa.AutomationPipelines.Lights.Extensions;
using CodeCasa.AutomationPipelines.Lights.Nodes;
using CodeCasa.AutomationPipelines.Lights.ReactiveNode;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using CodeCasa.Lights;
using Microsoft.Extensions.DependencyInjection;

namespace CodeCasa.AutomationPipelines.Lights.Pipeline;

public partial class LightTransitionPipelineConfigurator
{
    public ILightTransitionPipelineConfigurator When<TObservable>(LightParameters lightParameters)
        where TObservable : IObservable<bool>
    {
        return When<TObservable>(lightParameters.AsTransition());
    }

    public ILightTransitionPipelineConfigurator When(IObservable<bool> observable,
        LightParameters lightParameters)
    {
        return When(observable, lightParameters.AsTransition());
    }

    public ILightTransitionPipelineConfigurator When<TObservable>(
        Func<ILightPipelineContext, LightParameters> lightParametersFactory) where TObservable : IObservable<bool>
    {
        return When<TObservable>(c => lightParametersFactory(c).AsTransition());
    }

    public ILightTransitionPipelineConfigurator When(IObservable<bool> observable,
        Func<ILightPipelineContext, LightParameters> lightParametersFactory)
    {
        return When(observable, c => lightParametersFactory(c).AsTransition());
    }

    public ILightTransitionPipelineConfigurator When<TObservable>(LightTransition lightTransition)
        where TObservable : IObservable<bool>
    {
        return When<TObservable>(_ => lightTransition);
    }

    public ILightTransitionPipelineConfigurator When(IObservable<bool> observable,
        LightTransition lightTransition)
    {
        return When(observable, _ => lightTransition);
    }

    public ILightTransitionPipelineConfigurator When<TObservable>(
        Func<ILightPipelineContext, LightTransition> lightTransitionFactory) where TObservable : IObservable<bool>
    {
        return When<TObservable>(c => new StaticLightTransitionNode(lightTransitionFactory(c), c.ServiceProvider.GetRequiredService<IScheduler>()));
    }

    public ILightTransitionPipelineConfigurator When(IObservable<bool> observable,
        Func<ILightPipelineContext, LightTransition> lightTransitionFactory)
    {
        return When(observable, c => new StaticLightTransitionNode(lightTransitionFactory(c), c.ServiceProvider.GetRequiredService<IScheduler>()));
    }

    public ILightTransitionPipelineConfigurator When<TObservable>(
        Func<ILightPipelineContext, IPipelineNode<LightTransition>> nodeFactory) where TObservable : IObservable<bool>
    {
        var observable = serviceProvider.CreateInstanceWithinContext<TObservable>(Light);
        return When(observable, nodeFactory);
    }

    public ILightTransitionPipelineConfigurator When(IObservable<bool> observable,
        Func<ILightPipelineContext, IPipelineNode<LightTransition>> nodeFactory)
    {
        return AddReactiveNode(c => c
            .On(observable.Where(x => x), nodeFactory)
            .PassThroughOn(observable.Where(x => !x)));
    }

    public ILightTransitionPipelineConfigurator When<TObservable, TNode>()
        where TObservable : IObservable<bool>
        where TNode : IPipelineNode<LightTransition>
    {
        var observable = serviceProvider.CreateInstanceWithinContext<TObservable>(Light);
        return When<TNode>(observable);
    }

    public ILightTransitionPipelineConfigurator When<TNode>(IObservable<bool> observable)
        where TNode : IPipelineNode<LightTransition>
    {
        return AddReactiveNode(c => c
            .On<bool, TNode>(observable.Where(x => x))
            .PassThroughOn(observable.Where(x => !x)));
    }

    public ILightTransitionPipelineConfigurator AddReactiveNodeWhen<TObservable>(Action<ILightTransitionReactiveNodeConfigurator> configure) where TObservable : IObservable<bool>
    {
        var observable = serviceProvider.CreateInstanceWithinContext<TObservable>(Light);
        return AddReactiveNodeWhen(observable, configure);
    }

    public ILightTransitionPipelineConfigurator AddReactiveNodeWhen(IObservable<bool> observable, Action<ILightTransitionReactiveNodeConfigurator> configure)
    {
        return AddReactiveNode(c => c
            .On(observable.Where(x => x), configure)
            .PassThroughOn(observable.Where(x => !x)));
    }

    public ILightTransitionPipelineConfigurator AddPipelineWhen<TObservable>(Action<ILightTransitionPipelineConfigurator> pipelineConfigurator) where TObservable : IObservable<bool>
    {
        return When<TObservable>(c => lightPipelineFactory.CreateLightPipeline(c.Light, pipelineConfigurator));
    }

    public ILightTransitionPipelineConfigurator AddPipelineWhen(IObservable<bool> observable, Action<ILightTransitionPipelineConfigurator> pipelineConfigurator)
    {
        return When(observable, c => lightPipelineFactory.CreateLightPipeline(c.Light, pipelineConfigurator));
    }

    public ILightTransitionPipelineConfigurator TurnOffWhen<TObservable>() where TObservable : IObservable<bool>
    {
        return When<TObservable>(LightTransition.Off());
    }

    public ILightTransitionPipelineConfigurator TurnOffWhen(IObservable<bool> observable)
    {
        return When(observable, LightTransition.Off());
    }

    public ILightTransitionPipelineConfigurator TurnOnWhen<TObservable>() where TObservable : IObservable<bool>
    {
        return When<TObservable>(LightTransition.On());
    }

    public ILightTransitionPipelineConfigurator TurnOnWhen(IObservable<bool> observable)
    {
        return When(observable, LightTransition.On());
    }
}