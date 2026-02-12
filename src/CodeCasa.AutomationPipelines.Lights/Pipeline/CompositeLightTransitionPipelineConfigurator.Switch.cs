using CodeCasa.AutomationPipelines.Lights.Extensions;
using CodeCasa.AutomationPipelines.Lights.ReactiveNode;
using CodeCasa.Lights;
using Microsoft.Extensions.DependencyInjection;
using System.Reactive.Linq;

namespace CodeCasa.AutomationPipelines.Lights.Pipeline;

internal partial class CompositeLightTransitionPipelineConfigurator<TLight>
{
    /// <inheritdoc/>
    public ILightTransitionPipelineConfigurator<TLight> Switch<TObservable>(LightParameters trueLightParameters,
        LightParameters falseLightParameters) where TObservable : IObservable<bool>
    {
        NodeContainers.Values.ForEach(b => b.Switch<TObservable>(trueLightParameters, falseLightParameters));
        return this;
    }

    /// <inheritdoc/>
    public ILightTransitionPipelineConfigurator<TLight> Switch(IObservable<bool> observable, LightParameters trueLightParameters,
        LightParameters falseLightParameters)
    {
        NodeContainers.Values.ForEach(b => b.Switch(observable, trueLightParameters, falseLightParameters));
        return this;
    }

    /// <inheritdoc/>
    public ILightTransitionPipelineConfigurator<TLight> Switch<TObservable>(Func<IServiceProvider, LightParameters> trueLightParametersFactory,
        Func<IServiceProvider, LightParameters> falseLightParametersFactory) where TObservable : IObservable<bool>
    {
        NodeContainers.Values.ForEach(b => b.Switch<TObservable>(trueLightParametersFactory, falseLightParametersFactory));
        return this;
    }

    /// <inheritdoc/>
    public ILightTransitionPipelineConfigurator<TLight> Switch(IObservable<bool> observable, Func<IServiceProvider, LightParameters> trueLightParametersFactory,
        Func<IServiceProvider, LightParameters> falseLightParametersFactory)
    {
        NodeContainers.Values.ForEach(b => b.Switch(observable, trueLightParametersFactory, falseLightParametersFactory));
        return this;
    }

    /// <inheritdoc/>
    public ILightTransitionPipelineConfigurator<TLight> Switch<TObservable>(LightTransition trueLightTransition,
        LightTransition falseLightTransition) where TObservable : IObservable<bool>
    {
        NodeContainers.Values.ForEach(b => b.Switch<TObservable>(trueLightTransition, falseLightTransition));
        return this;
    }

    /// <inheritdoc/>
    public ILightTransitionPipelineConfigurator<TLight> Switch(IObservable<bool> observable, LightTransition trueLightTransition,
        LightTransition falseLightTransition)
    {
        NodeContainers.Values.ForEach(b => b.Switch(observable, trueLightTransition, falseLightTransition));
        return this;
    }

    /// <inheritdoc/>
    public ILightTransitionPipelineConfigurator<TLight> Switch<TObservable>(Func<IServiceProvider, LightTransition> trueLightTransitionFactory,
        Func<IServiceProvider, LightTransition> falseLightTransitionFactory) where TObservable : IObservable<bool>
    {
        NodeContainers.Values.ForEach(b => b.Switch<TObservable>(trueLightTransitionFactory, falseLightTransitionFactory));
        return this;
    }

    /// <inheritdoc/>
    public ILightTransitionPipelineConfigurator<TLight> Switch(IObservable<bool> observable, Func<IServiceProvider, LightTransition> trueLightTransitionFactory,
        Func<IServiceProvider, LightTransition> falseLightTransitionFactory)
    {
        NodeContainers.Values.ForEach(b => b.Switch(observable, trueLightTransitionFactory, falseLightTransitionFactory));
        return this;
    }

    /// <inheritdoc/>
    public ILightTransitionPipelineConfigurator<TLight> Switch<TObservable>(Func<IServiceProvider, IPipelineNode<LightTransition>> trueNodeFactory, Func<IServiceProvider, IPipelineNode<LightTransition>> falseNodeFactory) where TObservable : IObservable<bool>
    {
        NodeContainers.Values.ForEach(b => b.Switch<TObservable>(trueNodeFactory, falseNodeFactory));
        return this;
    }

    /// <inheritdoc/>
    public ILightTransitionPipelineConfigurator<TLight> Switch(IObservable<bool> observable, Func<IServiceProvider, IPipelineNode<LightTransition>> trueNodeFactory,
        Func<IServiceProvider, IPipelineNode<LightTransition>> falseNodeFactory)
    {
        NodeContainers.Values.ForEach(b => b.Switch(observable, trueNodeFactory, falseNodeFactory));
        return this;
    }

    /// <inheritdoc/>
    public ILightTransitionPipelineConfigurator<TLight> Switch<TObservable, TTrueNode, TFalseNode>() where TObservable : IObservable<bool> where TTrueNode : IPipelineNode<LightTransition> where TFalseNode : IPipelineNode<LightTransition>
    {
        NodeContainers.Values.ForEach(b => b.Switch<TObservable, TTrueNode, TFalseNode>());
        return this;
    }

    /// <inheritdoc/>
    public ILightTransitionPipelineConfigurator<TLight> Switch<TTrueNode, TFalseNode>(IObservable<bool> observable) where TTrueNode : IPipelineNode<LightTransition> where TFalseNode : IPipelineNode<LightTransition>
    {
        NodeContainers.Values.ForEach(b => b.Switch<TTrueNode, TFalseNode>(observable));
        return this;
    }

    /// <inheritdoc/>
    public ILightTransitionPipelineConfigurator<TLight> AddReactiveNodeSwitch<TObservable>(Action<ILightTransitionReactiveNodeConfigurator<TLight>> trueConfigure,
        Action<ILightTransitionReactiveNodeConfigurator<TLight>> falseConfigure, InstantiationScope instantiationScope = InstantiationScope.Shared) where TObservable : IObservable<bool>
    {
        if (instantiationScope == InstantiationScope.Shared)
        {
            var observable = ActivatorUtilities.CreateInstance<TObservable>(serviceProvider);
            return AddReactiveNodeSwitch(observable, trueConfigure, falseConfigure);
        }
        NodeContainers.Values.ForEach(b => b.AddReactiveNodeSwitch<TObservable>(trueConfigure, falseConfigure, instantiationScope));
        return this;
    }

    /// <inheritdoc/>
    public ILightTransitionPipelineConfigurator<TLight> AddReactiveNodeSwitch(IObservable<bool> observable, Action<ILightTransitionReactiveNodeConfigurator<TLight>> trueConfigure,
        Action<ILightTransitionReactiveNodeConfigurator<TLight>> falseConfigure, InstantiationScope instantiationScope = InstantiationScope.Shared)
    {
        return AddReactiveNode(c => c
            .On(observable.Where(x => x), trueConfigure, instantiationScope)
            .On(observable.Where(x => !x), falseConfigure, instantiationScope));
    }

    /// <inheritdoc/>
    public ILightTransitionPipelineConfigurator<TLight> AddPipelineSwitch<TObservable>(Action<ILightTransitionPipelineConfigurator<TLight>> trueConfigure, Action<ILightTransitionPipelineConfigurator<TLight>> falseConfigure, InstantiationScope instantiationScope = InstantiationScope.Shared) where TObservable : IObservable<bool>
    {
        if (instantiationScope == InstantiationScope.Shared)
        {
            var observable = ActivatorUtilities.CreateInstance<TObservable>(serviceProvider);
            return AddPipelineSwitch(observable, trueConfigure, falseConfigure);
        }
        NodeContainers.Values.ForEach(b => b.AddPipelineSwitch<TObservable>(trueConfigure, falseConfigure, instantiationScope));
        return this;
    }

    /// <inheritdoc/>
    public ILightTransitionPipelineConfigurator<TLight> AddPipelineSwitch(IObservable<bool> observable, Action<ILightTransitionPipelineConfigurator<TLight>> trueConfigure,
        Action<ILightTransitionPipelineConfigurator<TLight>> falseConfigure, InstantiationScope instantiationScope = InstantiationScope.Shared)
    {
        return AddReactiveNode(c => c
            .On(observable.Where(x => x), trueConfigure, instantiationScope)
            .On(observable.Where(x => !x), falseConfigure, instantiationScope));
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
