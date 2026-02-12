using CodeCasa.AutomationPipelines.Lights.Nodes;
using CodeCasa.AutomationPipelines.Lights.ReactiveNode;
using CodeCasa.Lights;
using Microsoft.Extensions.DependencyInjection;
using System.Reactive.Concurrency;
using System.Reactive.Linq;

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
        Func<IServiceProvider, LightParameters> lightParametersFactory) where TObservable : IObservable<bool>
    {
        return When<TObservable>(c => lightParametersFactory(c).AsTransition());
    }

    /// <inheritdoc/>
    public ILightTransitionPipelineConfigurator<TLight> When(IObservable<bool> observable,
        Func<IServiceProvider, LightParameters> lightParametersFactory)
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
        Func<IServiceProvider, LightTransition> lightTransitionFactory) where TObservable : IObservable<bool>
    {
        return When<TObservable>(c => new StaticLightTransitionNode(lightTransitionFactory(c), c.GetRequiredService<IScheduler>()));
    }

    /// <inheritdoc/>
    public ILightTransitionPipelineConfigurator<TLight> When(IObservable<bool> observable,
        Func<IServiceProvider, LightTransition> lightTransitionFactory)
    {
        return When(observable, c => new StaticLightTransitionNode(lightTransitionFactory(c), c.GetRequiredService<IScheduler>()));
    }

    /// <inheritdoc/>
    public ILightTransitionPipelineConfigurator<TLight> When<TObservable>(
        Func<IServiceProvider, IPipelineNode<LightTransition>> nodeFactory) where TObservable : IObservable<bool>
    {
        var observable = ActivatorUtilities.CreateInstance<TObservable>(_serviceProvider);
        return When(observable, nodeFactory);
    }

    /// <inheritdoc/>
    public ILightTransitionPipelineConfigurator<TLight> When(IObservable<bool> observable,
        Func<IServiceProvider, IPipelineNode<LightTransition>> nodeFactory)
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
        var observable = ActivatorUtilities.CreateInstance<TObservable>(_serviceProvider);
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
    public ILightTransitionPipelineConfigurator<TLight> AddReactiveNodeWhen<TObservable>(Action<ILightTransitionReactiveNodeConfigurator<TLight>> configure, InstantiationScope instantiationScope = InstantiationScope.Shared) where TObservable : IObservable<bool>
    {
        var observable = ActivatorUtilities.CreateInstance<TObservable>(_serviceProvider);
        return AddReactiveNodeWhen(observable, configure, instantiationScope);
    }

    /// <inheritdoc/>
    public ILightTransitionPipelineConfigurator<TLight> AddReactiveNodeWhen(IObservable<bool> observable, Action<ILightTransitionReactiveNodeConfigurator<TLight>> configure, InstantiationScope instantiationScope = InstantiationScope.Shared)
    {
        return AddReactiveNode(c => c
            .On(observable.Where(x => x), configure, instantiationScope)
            .PassThroughOn(observable.Where(x => !x)));
    }

    /// <inheritdoc/>
    public ILightTransitionPipelineConfigurator<TLight> AddPipelineWhen<TObservable>(Action<ILightTransitionPipelineConfigurator<TLight>> pipelineConfigurator, InstantiationScope instantiationScope = InstantiationScope.Shared) where TObservable : IObservable<bool>
    {
        if (instantiationScope == InstantiationScope.Shared)
        {
            // Note: even though InstantiationScope is primarily intended for composite pipelines, we try to stay consistent with the lifetime of the inner pipeline to avoid confusion.
            var pipeline = _serviceProvider.GetRequiredService<LightPipelineFactory>().CreateLightPipeline(Light, pipelineConfigurator);
            return When<TObservable>(_ => pipeline);
        }
        return When<TObservable>(c => 
            c.GetRequiredService<LightPipelineFactory>()
                .CreateLightPipeline(c.GetRequiredService<TLight>(), pipelineConfigurator));
    }

    /// <inheritdoc/>
    public ILightTransitionPipelineConfigurator<TLight> AddPipelineWhen(IObservable<bool> observable, Action<ILightTransitionPipelineConfigurator<TLight>> pipelineConfigurator, InstantiationScope instantiationScope = InstantiationScope.Shared)
    {
        if (instantiationScope == InstantiationScope.Shared)
        {
            // Note: even though InstantiationScope is primarily intended for composite pipelines, we try to stay consistent with the lifetime of the inner pipeline to avoid confusion.
            var pipeline = _serviceProvider.GetRequiredService<LightPipelineFactory>().CreateLightPipeline(Light, pipelineConfigurator);
            return When(observable, _ => pipeline);
        }
        return When(observable, c => 
            c.GetRequiredService<LightPipelineFactory>()
                .CreateLightPipeline(c.GetRequiredService<TLight>(), pipelineConfigurator));
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
