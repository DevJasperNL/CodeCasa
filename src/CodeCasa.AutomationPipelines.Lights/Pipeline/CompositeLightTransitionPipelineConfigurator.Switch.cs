using CodeCasa.AutomationPipelines.Lights.Context;
using CodeCasa.AutomationPipelines.Lights.Extensions;
using CodeCasa.AutomationPipelines.Lights.ReactiveNode;
using Microsoft.Extensions.DependencyInjection;


using System.Reactive.Linq;
using CodeCasa.Lights;

namespace CodeCasa.AutomationPipelines.Lights.Pipeline;

public partial class CompositeLightTransitionPipelineConfigurator
{
    /// <inheritdoc/>
    public ILightTransitionPipelineConfigurator Switch<TObservable>(LightParameters trueLightParameters,
        LightParameters falseLightParameters) where TObservable : IObservable<bool>
    {
        NodeContainers.Values.ForEach(b => b.Switch<TObservable>(trueLightParameters, falseLightParameters));
        return this;
    }

    /// <inheritdoc/>
    public ILightTransitionPipelineConfigurator Switch(IObservable<bool> observable, LightParameters trueLightParameters,
        LightParameters falseLightParameters)
    {
        NodeContainers.Values.ForEach(b => b.Switch(observable, trueLightParameters, falseLightParameters));
        return this;
    }

    /// <inheritdoc/>
    public ILightTransitionPipelineConfigurator Switch<TObservable>(Func<ILightPipelineContext, LightParameters> trueLightParametersFactory,
        Func<ILightPipelineContext, LightParameters> falseLightParametersFactory) where TObservable : IObservable<bool>
    {
        NodeContainers.Values.ForEach(b => b.Switch<TObservable>(trueLightParametersFactory, falseLightParametersFactory));
        return this;
    }

    /// <inheritdoc/>
    public ILightTransitionPipelineConfigurator Switch(IObservable<bool> observable, Func<ILightPipelineContext, LightParameters> trueLightParametersFactory,
        Func<ILightPipelineContext, LightParameters> falseLightParametersFactory)
    {
        NodeContainers.Values.ForEach(b => b.Switch(observable, trueLightParametersFactory, falseLightParametersFactory));
        return this;
    }

    /// <inheritdoc/>
    public ILightTransitionPipelineConfigurator Switch<TObservable>(LightTransition trueLightTransition,
        LightTransition falseLightTransition) where TObservable : IObservable<bool>
    {
        NodeContainers.Values.ForEach(b => b.Switch<TObservable>(trueLightTransition, falseLightTransition));
        return this;
    }

    /// <inheritdoc/>
    public ILightTransitionPipelineConfigurator Switch(IObservable<bool> observable, LightTransition trueLightTransition,
        LightTransition falseLightTransition)
    {
        NodeContainers.Values.ForEach(b => b.Switch(observable, trueLightTransition, falseLightTransition));
        return this;
    }

    /// <inheritdoc/>
    public ILightTransitionPipelineConfigurator Switch<TObservable>(Func<ILightPipelineContext, LightTransition> trueLightTransitionFactory,
        Func<ILightPipelineContext, LightTransition> falseLightTransitionFactory) where TObservable : IObservable<bool>
    {
        NodeContainers.Values.ForEach(b => b.Switch<TObservable>(trueLightTransitionFactory, falseLightTransitionFactory));
        return this;
    }

    /// <inheritdoc/>
    public ILightTransitionPipelineConfigurator Switch(IObservable<bool> observable, Func<ILightPipelineContext, LightTransition> trueLightTransitionFactory,
        Func<ILightPipelineContext, LightTransition> falseLightTransitionFactory)
    {
        NodeContainers.Values.ForEach(b => b.Switch(observable, trueLightTransitionFactory, falseLightTransitionFactory));
        return this;
    }

    /// <inheritdoc/>
    public ILightTransitionPipelineConfigurator Switch<TObservable>(Func<ILightPipelineContext, IPipelineNode<LightTransition>> trueNodeFactory, Func<ILightPipelineContext, IPipelineNode<LightTransition>> falseNodeFactory) where TObservable : IObservable<bool>
    {
        NodeContainers.Values.ForEach(b => b.Switch<TObservable>(trueNodeFactory, falseNodeFactory));
        return this;
    }

    /// <inheritdoc/>
    public ILightTransitionPipelineConfigurator Switch(IObservable<bool> observable, Func<ILightPipelineContext, IPipelineNode<LightTransition>> trueNodeFactory,
        Func<ILightPipelineContext, IPipelineNode<LightTransition>> falseNodeFactory)
    {
        NodeContainers.Values.ForEach(b => b.Switch(observable, trueNodeFactory, falseNodeFactory));
        return this;
    }

    /// <inheritdoc/>
    public ILightTransitionPipelineConfigurator Switch<TObservable, TTrueNode, TFalseNode>() where TObservable : IObservable<bool> where TTrueNode : IPipelineNode<LightTransition> where TFalseNode : IPipelineNode<LightTransition>
    {
        NodeContainers.Values.ForEach(b => b.Switch<TObservable, TTrueNode, TFalseNode>());
        return this;
    }

    /// <inheritdoc/>
    public ILightTransitionPipelineConfigurator Switch<TTrueNode, TFalseNode>(IObservable<bool> observable) where TTrueNode : IPipelineNode<LightTransition> where TFalseNode : IPipelineNode<LightTransition>
    {
        NodeContainers.Values.ForEach(b => b.Switch<TTrueNode, TFalseNode>(observable));
        return this;
    }

    /// <inheritdoc/>
    public ILightTransitionPipelineConfigurator AddReactiveNodeSwitch<TObservable>(Action<ILightTransitionReactiveNodeConfigurator> trueConfigure,
        Action<ILightTransitionReactiveNodeConfigurator> falseConfigure) where TObservable : IObservable<bool>
    {
        /*
         * For this implementation we can either instantiate the TObservable for each container and pass configure to them individual, breaking composite dimming behavior.
         * Or we can create a single TObservable without light context.
         * I decided to go with the latter to preserve composite dimming behavior.
         */
        var observable = ActivatorUtilities.CreateInstance<TObservable>(serviceProvider);
        return AddReactiveNodeSwitch(observable, trueConfigure, falseConfigure);
    }

    /// <inheritdoc/>
    public ILightTransitionPipelineConfigurator AddReactiveNodeSwitch(IObservable<bool> observable, Action<ILightTransitionReactiveNodeConfigurator> trueConfigure,
        Action<ILightTransitionReactiveNodeConfigurator> falseConfigure)
    {
        // Note: we use CompositeLightTransitionPipelineConfigurator.AddReactiveNode so configure is also applied on the composite context.
        return AddReactiveNode(c => c
            .On(observable.Where(x => x), trueConfigure)
            .On(observable.Where(x => !x), falseConfigure));
    }

    /// <inheritdoc/>
    public ILightTransitionPipelineConfigurator AddPipelineSwitch<TObservable>(Action<ILightTransitionPipelineConfigurator> trueConfigure, Action<ILightTransitionPipelineConfigurator> falseConfigure) where TObservable : IObservable<bool>
    {
        var observable = ActivatorUtilities.CreateInstance<TObservable>(serviceProvider);
        return AddPipelineSwitch(observable, trueConfigure, falseConfigure);
    }

    /// <inheritdoc/>
    public ILightTransitionPipelineConfigurator AddPipelineSwitch(IObservable<bool> observable, Action<ILightTransitionPipelineConfigurator> trueConfigure,
        Action<ILightTransitionPipelineConfigurator> falseConfigure)
    {
        // Note: we use CompositeLightTransitionPipelineConfigurator.AddReactiveNode so configure is also applied on the composite context.
        return AddReactiveNode(c => c
            .On(observable.Where(x => x), trueConfigure)
            .On(observable.Where(x => x), falseConfigure));
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