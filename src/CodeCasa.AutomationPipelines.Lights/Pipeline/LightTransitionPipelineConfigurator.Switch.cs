using CodeCasa.AutomationPipelines.Lights.Context;
using CodeCasa.AutomationPipelines.Lights.Extensions;
using CodeCasa.AutomationPipelines.Lights.Nodes;
using CodeCasa.AutomationPipelines.Lights.ReactiveNode;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using CodeCasa.Lights;
using Microsoft.Extensions.DependencyInjection;

namespace CodeCasa.AutomationPipelines.Lights.Pipeline;

internal partial class LightTransitionPipelineConfigurator
{
    /// <inheritdoc/>
    public ILightTransitionPipelineConfigurator Switch<TObservable>(LightParameters trueLightParameters,
        LightParameters falseLightParameters) where TObservable : IObservable<bool>
    {
        return Switch<TObservable>(trueLightParameters.AsTransition(), falseLightParameters.AsTransition());
    }

    /// <inheritdoc/>
    public ILightTransitionPipelineConfigurator Switch(IObservable<bool> observable, LightParameters trueLightParameters,
        LightParameters falseLightParameters)
    {
        return Switch(observable, trueLightParameters.AsTransition(), falseLightParameters.AsTransition());
    }

    /// <inheritdoc/>
    public ILightTransitionPipelineConfigurator Switch<TObservable>(Func<ILightPipelineContext, LightParameters> trueLightParametersFactory,
        Func<ILightPipelineContext, LightParameters> falseLightParametersFactory) where TObservable : IObservable<bool>
    {
        return Switch<TObservable>(c => falseLightParametersFactory(c).AsTransition(), c => trueLightParametersFactory(c).AsTransition());
    }

    /// <inheritdoc/>
    public ILightTransitionPipelineConfigurator Switch(IObservable<bool> observable, Func<ILightPipelineContext, LightParameters> trueLightParametersFactory,
        Func<ILightPipelineContext, LightParameters> falseLightParametersFactory)
    {
        return Switch(observable, c => trueLightParametersFactory(c).AsTransition(), c => falseLightParametersFactory(c).AsTransition());
    }

    /// <inheritdoc/>
    public ILightTransitionPipelineConfigurator Switch<TObservable>(LightTransition trueLightTransition,
        LightTransition falseLightTransition) where TObservable : IObservable<bool>
    {
        return Switch<TObservable>(trueLightTransition, falseLightTransition);
    }

    /// <inheritdoc/>
    public ILightTransitionPipelineConfigurator Switch(IObservable<bool> observable, LightTransition trueLightTransition,
        LightTransition falseLightTransition)
    {
        return Switch(observable, trueLightTransition, falseLightTransition);
    }

    /// <inheritdoc/>
    public ILightTransitionPipelineConfigurator Switch<TObservable>(Func<ILightPipelineContext, LightTransition> trueLightTransitionFactory,
        Func<ILightPipelineContext, LightTransition> falseLightTransitionFactory) where TObservable : IObservable<bool>
    {
        return Switch<TObservable>(
            c => new StaticLightTransitionNode(trueLightTransitionFactory(c), c.ServiceProvider.GetRequiredService<IScheduler>()), 
            c => new StaticLightTransitionNode(falseLightTransitionFactory(c), c.ServiceProvider.GetRequiredService<IScheduler>()));
    }

    /// <inheritdoc/>
    public ILightTransitionPipelineConfigurator Switch(IObservable<bool> observable, Func<ILightPipelineContext, LightTransition> trueLightTransitionFactory,
        Func<ILightPipelineContext, LightTransition> falseLightTransitionFactory)
    {
        return Switch(
            observable,
            c => new StaticLightTransitionNode(trueLightTransitionFactory(c), c.ServiceProvider.GetRequiredService<IScheduler>()),
            c => new StaticLightTransitionNode(falseLightTransitionFactory(c), c.ServiceProvider.GetRequiredService<IScheduler>()));
    }

    /// <inheritdoc/>
    public ILightTransitionPipelineConfigurator Switch<TObservable>(Func<ILightPipelineContext, IPipelineNode<LightTransition>> trueNodeFactory, Func<ILightPipelineContext, IPipelineNode<LightTransition>> falseNodeFactory) where TObservable : IObservable<bool>
    {
        var observable = serviceProvider.CreateInstanceWithinContext<TObservable>(Light);
        return Switch(observable, trueNodeFactory, falseNodeFactory);
    }

    /// <inheritdoc/>
    public ILightTransitionPipelineConfigurator Switch(IObservable<bool> observable, Func<ILightPipelineContext, IPipelineNode<LightTransition>> trueNodeFactory,
        Func<ILightPipelineContext, IPipelineNode<LightTransition>> falseNodeFactory)
    {
        return AddReactiveNode(c => c
            .On(observable.Where(x => x), trueNodeFactory)
            .On(observable.Where(x => !x), falseNodeFactory));
    }

    /// <inheritdoc/>
    public ILightTransitionPipelineConfigurator Switch<TObservable, TTrueNode, TFalseNode>() where TObservable : IObservable<bool> where TTrueNode : IPipelineNode<LightTransition> where TFalseNode : IPipelineNode<LightTransition>
    {
        var observable = serviceProvider.CreateInstanceWithinContext<TObservable>(Light);
        return Switch<TTrueNode, TFalseNode>(observable);
    }

    /// <inheritdoc/>
    public ILightTransitionPipelineConfigurator Switch<TTrueNode, TFalseNode>(IObservable<bool> observable) where TTrueNode : IPipelineNode<LightTransition> where TFalseNode : IPipelineNode<LightTransition>
    {
        return AddReactiveNode(c => c
            .On<bool, TTrueNode>(observable.Where(x => x))
            .On<bool, TFalseNode>(observable.Where(x => !x)));
    }

    /// <inheritdoc/>
    public ILightTransitionPipelineConfigurator AddReactiveNodeSwitch<TObservable>(Action<ILightTransitionReactiveNodeConfigurator> trueConfigure, Action<ILightTransitionReactiveNodeConfigurator> falseConfigure) where TObservable : IObservable<bool>
    {
        var observable = serviceProvider.CreateInstanceWithinContext<TObservable>(Light);
        return AddReactiveNodeSwitch(observable, trueConfigure, falseConfigure);
    }

    /// <inheritdoc/>
    public ILightTransitionPipelineConfigurator AddReactiveNodeSwitch(IObservable<bool> observable, Action<ILightTransitionReactiveNodeConfigurator> trueConfigure,
        Action<ILightTransitionReactiveNodeConfigurator> falseConfigure)
    {
        return AddReactiveNode(c => c
            .On(observable.Where(x => x), trueConfigure)
            .On(observable.Where(x => !x), falseConfigure));
    }

    /// <inheritdoc/>
    public ILightTransitionPipelineConfigurator AddPipelineSwitch<TObservable>(Action<ILightTransitionPipelineConfigurator> trueConfigure, Action<ILightTransitionPipelineConfigurator> falseConfigure) where TObservable : IObservable<bool>
    {
        return Switch<TObservable>(c => lightPipelineFactory.CreateLightPipeline(c.Light, trueConfigure), c => lightPipelineFactory.CreateLightPipeline(c.Light, falseConfigure));
    }

    /// <inheritdoc/>
    public ILightTransitionPipelineConfigurator AddPipelineSwitch(IObservable<bool> observable, Action<ILightTransitionPipelineConfigurator> trueConfigure,
        Action<ILightTransitionPipelineConfigurator> falseConfigure)
    {
        return Switch(observable, c => lightPipelineFactory.CreateLightPipeline(c.Light, trueConfigure), c => lightPipelineFactory.CreateLightPipeline(c.Light, falseConfigure));
    }

    /// <inheritdoc/>
    public ILightTransitionPipelineConfigurator TurnOnOff<TObservable>() where TObservable : IObservable<bool>
    {
        return Switch<TObservable>(LightTransition.On(), LightTransition.Off());
    }

    /// <inheritdoc/>
    public ILightTransitionPipelineConfigurator TurnOnOff(IObservable<bool> observable)
    {
        return Switch(observable, LightTransition.On(), LightTransition.Off());
    }
}
