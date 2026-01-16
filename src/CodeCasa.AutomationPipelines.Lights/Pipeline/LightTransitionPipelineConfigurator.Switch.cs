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
    public ILightTransitionPipelineConfigurator<TLight> Switch<TObservable>(LightParameters trueLightParameters,
        LightParameters falseLightParameters) where TObservable : IObservable<bool>
    {
        return Switch<TObservable>(trueLightParameters.AsTransition(), falseLightParameters.AsTransition());
    }

    /// <inheritdoc/>
    public ILightTransitionPipelineConfigurator<TLight> Switch(IObservable<bool> observable, LightParameters trueLightParameters,
        LightParameters falseLightParameters)
    {
        return Switch(observable, trueLightParameters.AsTransition(), falseLightParameters.AsTransition());
    }

    /// <inheritdoc/>
    public ILightTransitionPipelineConfigurator<TLight> Switch<TObservable>(Func<ILightPipelineContext<TLight>, LightParameters> trueLightParametersFactory,
        Func<ILightPipelineContext<TLight>, LightParameters> falseLightParametersFactory) where TObservable : IObservable<bool>
    {
        return Switch<TObservable>(c => falseLightParametersFactory(c).AsTransition(), c => trueLightParametersFactory(c).AsTransition());
    }

    /// <inheritdoc/>
    public ILightTransitionPipelineConfigurator<TLight> Switch(IObservable<bool> observable, Func<ILightPipelineContext<TLight>, LightParameters> trueLightParametersFactory,
        Func<ILightPipelineContext<TLight>, LightParameters> falseLightParametersFactory)
    {
        return Switch(observable, c => trueLightParametersFactory(c).AsTransition(), c => falseLightParametersFactory(c).AsTransition());
    }

    /// <inheritdoc/>
    public ILightTransitionPipelineConfigurator<TLight> Switch<TObservable>(LightTransition trueLightTransition,
        LightTransition falseLightTransition) where TObservable : IObservable<bool>
    {
        return Switch<TObservable>(trueLightTransition, falseLightTransition);
    }

    /// <inheritdoc/>
    public ILightTransitionPipelineConfigurator<TLight> Switch(IObservable<bool> observable, LightTransition trueLightTransition,
        LightTransition falseLightTransition)
    {
        return Switch(observable, trueLightTransition, falseLightTransition);
    }

    /// <inheritdoc/>
    public ILightTransitionPipelineConfigurator<TLight> Switch<TObservable>(Func<ILightPipelineContext<TLight>, LightTransition> trueLightTransitionFactory,
        Func<ILightPipelineContext<TLight>, LightTransition> falseLightTransitionFactory) where TObservable : IObservable<bool>
    {
        return Switch<TObservable>(
            c => new StaticLightTransitionNode(trueLightTransitionFactory(c), c.ServiceProvider.GetRequiredService<IScheduler>()), 
            c => new StaticLightTransitionNode(falseLightTransitionFactory(c), c.ServiceProvider.GetRequiredService<IScheduler>()));
    }

    /// <inheritdoc/>
    public ILightTransitionPipelineConfigurator<TLight> Switch(IObservable<bool> observable, Func<ILightPipelineContext<TLight>, LightTransition> trueLightTransitionFactory,
        Func<ILightPipelineContext<TLight>, LightTransition> falseLightTransitionFactory)
    {
        return Switch(
            observable,
            c => new StaticLightTransitionNode(trueLightTransitionFactory(c), c.ServiceProvider.GetRequiredService<IScheduler>()),
            c => new StaticLightTransitionNode(falseLightTransitionFactory(c), c.ServiceProvider.GetRequiredService<IScheduler>()));
    }

    /// <inheritdoc/>
    public ILightTransitionPipelineConfigurator<TLight> Switch<TObservable>(Func<ILightPipelineContext<TLight>, IPipelineNode<LightTransition>> trueNodeFactory, Func<ILightPipelineContext<TLight>, IPipelineNode<LightTransition>> falseNodeFactory) where TObservable : IObservable<bool>
    {
        var observable = serviceProvider.CreateInstanceWithinContext<TObservable, TLight>(Light);
        return Switch(observable, trueNodeFactory, falseNodeFactory);
    }

    /// <inheritdoc/>
    public ILightTransitionPipelineConfigurator<TLight> Switch(IObservable<bool> observable, Func<ILightPipelineContext<TLight>, IPipelineNode<LightTransition>> trueNodeFactory,
        Func<ILightPipelineContext<TLight>, IPipelineNode<LightTransition>> falseNodeFactory)
    {
        return AddReactiveNode(c => c
            .On(observable.Where(x => x), trueNodeFactory)
            .On(observable.Where(x => !x), falseNodeFactory));
    }

    /// <inheritdoc/>
    public ILightTransitionPipelineConfigurator<TLight> Switch<TObservable, TTrueNode, TFalseNode>() where TObservable : IObservable<bool> where TTrueNode : IPipelineNode<LightTransition> where TFalseNode : IPipelineNode<LightTransition>
    {
        var observable = serviceProvider.CreateInstanceWithinContext<TObservable, TLight>(Light);
        return Switch<TTrueNode, TFalseNode>(observable);
    }

    /// <inheritdoc/>
    public ILightTransitionPipelineConfigurator<TLight> Switch<TTrueNode, TFalseNode>(IObservable<bool> observable) where TTrueNode : IPipelineNode<LightTransition> where TFalseNode : IPipelineNode<LightTransition>
    {
        return AddReactiveNode(c => c
            .On<bool, TTrueNode>(observable.Where(x => x))
            .On<bool, TFalseNode>(observable.Where(x => !x)));
    }

    /// <inheritdoc/>
    public ILightTransitionPipelineConfigurator<TLight> AddReactiveNodeSwitch<TObservable>(Action<ILightTransitionReactiveNodeConfigurator<TLight>> trueConfigure, Action<ILightTransitionReactiveNodeConfigurator<TLight>> falseConfigure) where TObservable : IObservable<bool>
    {
        var observable = serviceProvider.CreateInstanceWithinContext<TObservable, TLight>(Light);
        return AddReactiveNodeSwitch(observable, trueConfigure, falseConfigure);
    }

    /// <inheritdoc/>
    public ILightTransitionPipelineConfigurator<TLight> AddReactiveNodeSwitch(IObservable<bool> observable, Action<ILightTransitionReactiveNodeConfigurator<TLight>> trueConfigure,
        Action<ILightTransitionReactiveNodeConfigurator<TLight>> falseConfigure)
    {
        return AddReactiveNode(c => c
            .On(observable.Where(x => x), trueConfigure)
            .On(observable.Where(x => !x), falseConfigure));
    }

    /// <inheritdoc/>
    public ILightTransitionPipelineConfigurator<TLight> AddPipelineSwitch<TObservable>(Action<ILightTransitionPipelineConfigurator<TLight>> trueConfigure, Action<ILightTransitionPipelineConfigurator<TLight>> falseConfigure) where TObservable : IObservable<bool>
    {
        return Switch<TObservable>(c => lightPipelineFactory.CreateLightPipeline(c.Light, trueConfigure), c => lightPipelineFactory.CreateLightPipeline(c.Light, falseConfigure));
    }

    /// <inheritdoc/>
    public ILightTransitionPipelineConfigurator<TLight> AddPipelineSwitch(IObservable<bool> observable, Action<ILightTransitionPipelineConfigurator<TLight>> trueConfigure,
        Action<ILightTransitionPipelineConfigurator<TLight>> falseConfigure)
    {
        return Switch(observable, c => lightPipelineFactory.CreateLightPipeline(c.Light, trueConfigure), c => lightPipelineFactory.CreateLightPipeline(c.Light, falseConfigure));
    }

    /// <inheritdoc/>
    public ILightTransitionPipelineConfigurator<TLight> TurnOnOff<TObservable>() where TObservable : IObservable<bool>
    {
        return Switch<TObservable>(LightTransition.On(), LightTransition.Off());
    }

    /// <inheritdoc/>
    public ILightTransitionPipelineConfigurator<TLight> TurnOnOff(IObservable<bool> observable)
    {
        return Switch(observable, LightTransition.On(), LightTransition.Off());
    }
}
