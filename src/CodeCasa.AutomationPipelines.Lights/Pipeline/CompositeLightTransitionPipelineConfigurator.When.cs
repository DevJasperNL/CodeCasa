using CodeCasa.AutomationPipelines.Lights.Extensions;
using CodeCasa.AutomationPipelines.Lights.ReactiveNode;
using Microsoft.Extensions.DependencyInjection;


using System.Reactive.Linq;
using CodeCasa.Lights;

namespace CodeCasa.AutomationPipelines.Lights.Pipeline;

internal partial class CompositeLightTransitionPipelineConfigurator<TLight>
{
    /// <inheritdoc />
    public ILightTransitionPipelineConfigurator<TLight> When<TObservable>(LightParameters lightParameters)
        where TObservable : IObservable<bool>
    {
        NodeContainers.Values.ForEach(b => b.When<TObservable>(lightParameters));
        return this;
    }

    /// <inheritdoc />
    public ILightTransitionPipelineConfigurator<TLight> When(IObservable<bool> observable,
        LightParameters lightParameters)
    {
        NodeContainers.Values.ForEach(b => b.When(observable, lightParameters));
        return this;
    }

    /// <inheritdoc />
    public ILightTransitionPipelineConfigurator<TLight> When<TObservable>(
        Func<IServiceProvider, LightParameters> lightParametersFactory) where TObservable : IObservable<bool>
    {
        NodeContainers.Values.ForEach(b => b.When<TObservable>(lightParametersFactory));
        return this;
    }

    /// <inheritdoc />
    public ILightTransitionPipelineConfigurator<TLight> When(IObservable<bool> observable,
        Func<IServiceProvider, LightParameters> lightParametersFactory)
    {
        NodeContainers.Values.ForEach(b => b.When(observable, lightParametersFactory));
        return this;
    }

    /// <inheritdoc />
    public ILightTransitionPipelineConfigurator<TLight> When<TObservable>(LightTransition lightTransition)
        where TObservable : IObservable<bool>
    {
        NodeContainers.Values.ForEach(b => b.When<TObservable>(lightTransition));
        return this;
    }

    /// <inheritdoc />
    public ILightTransitionPipelineConfigurator<TLight> When(IObservable<bool> observable,
        LightTransition lightTransition)
    {
        NodeContainers.Values.ForEach(b => b.When(observable, lightTransition));
        return this;
    }

    /// <inheritdoc />
    public ILightTransitionPipelineConfigurator<TLight> When<TObservable>(
        Func<IServiceProvider, LightTransition> lightTransitionFactory) where TObservable : IObservable<bool>
    {
        NodeContainers.Values.ForEach(b => b.When<TObservable>(lightTransitionFactory));
        return this;
    }

    /// <inheritdoc />
    public ILightTransitionPipelineConfigurator<TLight> When(IObservable<bool> observable,
        Func<IServiceProvider, LightTransition> lightTransitionFactory)
    {
        NodeContainers.Values.ForEach(b => b.When(observable, lightTransitionFactory));
        return this;
    }

    /// <inheritdoc />
    public ILightTransitionPipelineConfigurator<TLight> When<TObservable>(
        Func<IServiceProvider, IPipelineNode<LightTransition>> nodeFactory) where TObservable : IObservable<bool>
    {
        NodeContainers.Values.ForEach(b => b.When<TObservable>(nodeFactory));
        return this;
    }

    /// <inheritdoc />
    public ILightTransitionPipelineConfigurator<TLight> When(IObservable<bool> observable,
        Func<IServiceProvider, IPipelineNode<LightTransition>> nodeFactory)
    {
        NodeContainers.Values.ForEach(b => b.When(observable, nodeFactory));
        return this;
    }

    /// <inheritdoc />
    public ILightTransitionPipelineConfigurator<TLight> When<TObservable, TNode>()
        where TObservable : IObservable<bool> where TNode : IPipelineNode<LightTransition>
    {
        NodeContainers.Values.ForEach(b => b.When<TObservable, TNode>());
        return this;
    }

    /// <inheritdoc />
    public ILightTransitionPipelineConfigurator<TLight> When<TNode>(IObservable<bool> observable)
        where TNode : IPipelineNode<LightTransition>
    {
        NodeContainers.Values.ForEach(b => b.When<TNode>(observable));
        return this;
    }

    /// <inheritdoc />
    public ILightTransitionPipelineConfigurator<TLight> AddReactiveNodeWhen<TObservable>(Action<ILightTransitionReactiveNodeConfigurator<TLight>> configure, InstantiationScope instantiationScope = InstantiationScope.Shared) where TObservable : IObservable<bool>
    {
        var observable = ActivatorUtilities.CreateInstance<TObservable>(serviceProvider);
        return AddReactiveNodeWhen(observable, configure, instantiationScope);
    }

    /// <inheritdoc />
    public ILightTransitionPipelineConfigurator<TLight> AddReactiveNodeWhen(IObservable<bool> observable, Action<ILightTransitionReactiveNodeConfigurator<TLight>> configure, InstantiationScope instantiationScope = InstantiationScope.Shared)
    {
        return AddReactiveNode(c => c
            .SetLoggingContext(LogName, "Conditional", LoggingEnabled ?? false)
            .On(observable.Where(x => x), configure.SetLoggingContext(c), instantiationScope)
            .PassThroughOn(observable.Where(x => !x)));
    }

    /// <inheritdoc />
    public ILightTransitionPipelineConfigurator<TLight> AddPipelineWhen<TObservable>(Action<ILightTransitionPipelineConfigurator<TLight>> configure, InstantiationScope instantiationScope = InstantiationScope.Shared) where TObservable : IObservable<bool>
    {
        var observable = ActivatorUtilities.CreateInstance<TObservable>(serviceProvider);
        return AddPipelineWhen(observable, configure, instantiationScope);
    }

    /// <inheritdoc />
    public ILightTransitionPipelineConfigurator<TLight> AddPipelineWhen(IObservable<bool> observable, Action<ILightTransitionPipelineConfigurator<TLight>> configure, InstantiationScope instantiationScope = InstantiationScope.Shared)
    {
        return AddReactiveNode(c => c
            .SetLoggingContext(LogName, "Conditional", LoggingEnabled ?? false)
            .On(observable.Where(x => x), configure.SetLoggingContext(c), instantiationScope)
            .PassThroughOn(observable.Where(x => !x)));
    }

    /// <inheritdoc />
    public ILightTransitionPipelineConfigurator<TLight> TurnOffWhen<TObservable>() where TObservable : IObservable<bool>
    {
        NodeContainers.Values.ForEach(b => b.TurnOffWhen<TObservable>());
        return this;
    }

    /// <inheritdoc />
    public ILightTransitionPipelineConfigurator<TLight> TurnOffWhen(IObservable<bool> observable)
    {
        NodeContainers.Values.ForEach(b => b.TurnOffWhen(observable));
        return this;
    }

    /// <inheritdoc />
    public ILightTransitionPipelineConfigurator<TLight> TurnOnWhen<TObservable>() where TObservable : IObservable<bool>
    {
        return When<TObservable>(LightTransition.On());
    }

    /// <inheritdoc />
    public ILightTransitionPipelineConfigurator<TLight> TurnOnWhen(IObservable<bool> observable)
    {
        return When(observable, LightTransition.On());
    }
}
