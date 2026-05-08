using CodeCasa.AutomationPipelines.Lights.Extensions;
using CodeCasa.AutomationPipelines.Lights.ReactiveNode;
using CodeCasa.AutomationPipelines.Lights.Switch;
using CodeCasa.Lights;
using Microsoft.Extensions.DependencyInjection;
using System.Reactive.Linq;

namespace CodeCasa.AutomationPipelines.Lights.Pipeline;

internal partial class CompositeLightTransitionPipelineConfigurator<TLight>
{
    /// <inheritdoc/>
    public ILightTransitionPipelineConfigurator<TLight> SwitchWhen(IObservable<bool> whenObservable,
        IObservable<bool> switchObservable,
        LightParameters trueLightParameters, LightParameters falseLightParameters)
    {
        var shareableWhen = _observableSharingStrategy.Apply(whenObservable);
        var shareableSwitch = _observableSharingStrategy.Apply(switchObservable);
        NodeContainers.Values.ForEach(b => b.SwitchWhen(shareableWhen, shareableSwitch, trueLightParameters, falseLightParameters));
        return this;
    }

    /// <inheritdoc/>
    public ILightTransitionPipelineConfigurator<TLight> SwitchWhen<TWhenObservable, TSwitchObservable>(
        LightParameters trueLightParameters, LightParameters falseLightParameters)
        where TWhenObservable : IObservable<bool>
        where TSwitchObservable : IObservable<bool>
    {
        var shareableWhen = _observableSharingStrategy.Apply(ActivatorUtilities.CreateInstance<TWhenObservable>(serviceProvider));
        var shareableSwitch = _observableSharingStrategy.Apply(ActivatorUtilities.CreateInstance<TSwitchObservable>(serviceProvider));
        NodeContainers.Values.ForEach(b => b.SwitchWhen(shareableWhen, shareableSwitch, trueLightParameters, falseLightParameters));
        return this;
    }

    /// <inheritdoc/>
    public ILightTransitionPipelineConfigurator<TLight> SwitchWhen<TWhenObservable>(IObservable<bool> switchObservable,
        LightParameters trueLightParameters, LightParameters falseLightParameters)
        where TWhenObservable : IObservable<bool>
    {
        var shareableWhen = _observableSharingStrategy.Apply(ActivatorUtilities.CreateInstance<TWhenObservable>(serviceProvider));
        var shareableSwitch = _observableSharingStrategy.Apply(switchObservable);
        NodeContainers.Values.ForEach(b => b.SwitchWhen(shareableWhen, shareableSwitch, trueLightParameters, falseLightParameters));
        return this;
    }

    /// <inheritdoc/>
    public ILightTransitionPipelineConfigurator<TLight> SwitchWhen(IObservable<bool> whenObservable,
        IObservable<bool> switchObservable,
        Func<IServiceProvider, LightParameters> trueLightParametersFactory,
        Func<IServiceProvider, LightParameters> falseLightParametersFactory)
    {
        var shareableWhen = _observableSharingStrategy.Apply(whenObservable);
        var shareableSwitch = _observableSharingStrategy.Apply(switchObservable);
        NodeContainers.Values.ForEach(b => b.SwitchWhen(shareableWhen, shareableSwitch, trueLightParametersFactory, falseLightParametersFactory));
        return this;
    }

    /// <inheritdoc/>
    public ILightTransitionPipelineConfigurator<TLight> SwitchWhen<TWhenObservable, TSwitchObservable>(
        Func<IServiceProvider, LightParameters> trueLightParametersFactory,
        Func<IServiceProvider, LightParameters> falseLightParametersFactory)
        where TWhenObservable : IObservable<bool>
        where TSwitchObservable : IObservable<bool>
    {
        var shareableWhen = _observableSharingStrategy.Apply(ActivatorUtilities.CreateInstance<TWhenObservable>(serviceProvider));
        var shareableSwitch = _observableSharingStrategy.Apply(ActivatorUtilities.CreateInstance<TSwitchObservable>(serviceProvider));
        NodeContainers.Values.ForEach(b => b.SwitchWhen(shareableWhen, shareableSwitch, trueLightParametersFactory, falseLightParametersFactory));
        return this;
    }

    /// <inheritdoc/>
    public ILightTransitionPipelineConfigurator<TLight> SwitchWhen<TWhenObservable>(IObservable<bool> switchObservable,
        Func<IServiceProvider, LightParameters> trueLightParametersFactory,
        Func<IServiceProvider, LightParameters> falseLightParametersFactory)
        where TWhenObservable : IObservable<bool>
    {
        var shareableWhen = _observableSharingStrategy.Apply(ActivatorUtilities.CreateInstance<TWhenObservable>(serviceProvider));
        var shareableSwitch = _observableSharingStrategy.Apply(switchObservable);
        NodeContainers.Values.ForEach(b => b.SwitchWhen(shareableWhen, shareableSwitch, trueLightParametersFactory, falseLightParametersFactory));
        return this;
    }

    /// <inheritdoc/>
    public ILightTransitionPipelineConfigurator<TLight> SwitchWhen(IObservable<bool> whenObservable,
        IObservable<bool> switchObservable,
        LightTransition trueLightTransition, LightTransition falseLightTransition)
    {
        var shareableWhen = _observableSharingStrategy.Apply(whenObservable);
        var shareableSwitch = _observableSharingStrategy.Apply(switchObservable);
        NodeContainers.Values.ForEach(b => b.SwitchWhen(shareableWhen, shareableSwitch, trueLightTransition, falseLightTransition));
        return this;
    }

    /// <inheritdoc/>
    public ILightTransitionPipelineConfigurator<TLight> SwitchWhen<TWhenObservable, TSwitchObservable>(
        LightTransition trueLightTransition, LightTransition falseLightTransition)
        where TWhenObservable : IObservable<bool>
        where TSwitchObservable : IObservable<bool>
    {
        var shareableWhen = _observableSharingStrategy.Apply(ActivatorUtilities.CreateInstance<TWhenObservable>(serviceProvider));
        var shareableSwitch = _observableSharingStrategy.Apply(ActivatorUtilities.CreateInstance<TSwitchObservable>(serviceProvider));
        NodeContainers.Values.ForEach(b => b.SwitchWhen(shareableWhen, shareableSwitch, trueLightTransition, falseLightTransition));
        return this;
    }

    /// <inheritdoc/>
    public ILightTransitionPipelineConfigurator<TLight> SwitchWhen<TWhenObservable>(IObservable<bool> switchObservable,
        LightTransition trueLightTransition, LightTransition falseLightTransition)
        where TWhenObservable : IObservable<bool>
    {
        var shareableWhen = _observableSharingStrategy.Apply(ActivatorUtilities.CreateInstance<TWhenObservable>(serviceProvider));
        var shareableSwitch = _observableSharingStrategy.Apply(switchObservable);
        NodeContainers.Values.ForEach(b => b.SwitchWhen(shareableWhen, shareableSwitch, trueLightTransition, falseLightTransition));
        return this;
    }

    /// <inheritdoc/>
    public ILightTransitionPipelineConfigurator<TLight> SwitchWhen(IObservable<bool> whenObservable,
        IObservable<bool> switchObservable,
        Func<IServiceProvider, LightTransition> trueLightTransitionFactory,
        Func<IServiceProvider, LightTransition> falseLightTransitionFactory)
    {
        var shareableWhen = _observableSharingStrategy.Apply(whenObservable);
        var shareableSwitch = _observableSharingStrategy.Apply(switchObservable);
        NodeContainers.Values.ForEach(b => b.SwitchWhen(shareableWhen, shareableSwitch, trueLightTransitionFactory, falseLightTransitionFactory));
        return this;
    }

    /// <inheritdoc/>
    public ILightTransitionPipelineConfigurator<TLight> SwitchWhen<TWhenObservable, TSwitchObservable>(
        Func<IServiceProvider, LightTransition> trueLightTransitionFactory,
        Func<IServiceProvider, LightTransition> falseLightTransitionFactory)
        where TWhenObservable : IObservable<bool>
        where TSwitchObservable : IObservable<bool>
    {
        var shareableWhen = _observableSharingStrategy.Apply(ActivatorUtilities.CreateInstance<TWhenObservable>(serviceProvider));
        var shareableSwitch = _observableSharingStrategy.Apply(ActivatorUtilities.CreateInstance<TSwitchObservable>(serviceProvider));
        NodeContainers.Values.ForEach(b => b.SwitchWhen(shareableWhen, shareableSwitch, trueLightTransitionFactory, falseLightTransitionFactory));
        return this;
    }

    /// <inheritdoc/>
    public ILightTransitionPipelineConfigurator<TLight> SwitchWhen<TWhenObservable>(IObservable<bool> switchObservable,
        Func<IServiceProvider, LightTransition> trueLightTransitionFactory,
        Func<IServiceProvider, LightTransition> falseLightTransitionFactory)
        where TWhenObservable : IObservable<bool>
    {
        var shareableWhen = _observableSharingStrategy.Apply(ActivatorUtilities.CreateInstance<TWhenObservable>(serviceProvider));
        var shareableSwitch = _observableSharingStrategy.Apply(switchObservable);
        NodeContainers.Values.ForEach(b => b.SwitchWhen(shareableWhen, shareableSwitch, trueLightTransitionFactory, falseLightTransitionFactory));
        return this;
    }

    /// <inheritdoc/>
    public ILightTransitionPipelineConfigurator<TLight> SwitchWhen(IObservable<bool> whenObservable,
        IObservable<bool> switchObservable,
        Func<IServiceProvider, IPipelineNode<LightTransition>> trueNodeFactory,
        Func<IServiceProvider, IPipelineNode<LightTransition>> falseNodeFactory)
    {
        var shareableWhen = _observableSharingStrategy.Apply(whenObservable);
        var shareableSwitch = _observableSharingStrategy.Apply(switchObservable);
        NodeContainers.Values.ForEach(b => b.SwitchWhen(shareableWhen, shareableSwitch, trueNodeFactory, falseNodeFactory));
        return this;
    }

    /// <inheritdoc/>
    public ILightTransitionPipelineConfigurator<TLight> SwitchWhen<TWhenObservable, TSwitchObservable>(
        Func<IServiceProvider, IPipelineNode<LightTransition>> trueNodeFactory,
        Func<IServiceProvider, IPipelineNode<LightTransition>> falseNodeFactory)
        where TWhenObservable : IObservable<bool>
        where TSwitchObservable : IObservable<bool>
    {
        var shareableWhen = _observableSharingStrategy.Apply(ActivatorUtilities.CreateInstance<TWhenObservable>(serviceProvider));
        var shareableSwitch = _observableSharingStrategy.Apply(ActivatorUtilities.CreateInstance<TSwitchObservable>(serviceProvider));
        NodeContainers.Values.ForEach(b => b.SwitchWhen(shareableWhen, shareableSwitch, trueNodeFactory, falseNodeFactory));
        return this;
    }

    /// <inheritdoc/>
    public ILightTransitionPipelineConfigurator<TLight> SwitchWhen<TWhenObservable>(IObservable<bool> switchObservable,
        Func<IServiceProvider, IPipelineNode<LightTransition>> trueNodeFactory,
        Func<IServiceProvider, IPipelineNode<LightTransition>> falseNodeFactory)
        where TWhenObservable : IObservable<bool>
    {
        var shareableWhen = _observableSharingStrategy.Apply(ActivatorUtilities.CreateInstance<TWhenObservable>(serviceProvider));
        var shareableSwitch = _observableSharingStrategy.Apply(switchObservable);
        NodeContainers.Values.ForEach(b => b.SwitchWhen(shareableWhen, shareableSwitch, trueNodeFactory, falseNodeFactory));
        return this;
    }

    /// <inheritdoc/>
    public ILightTransitionPipelineConfigurator<TLight> SwitchWhen<TWhenObservable, TSwitchObservable, TTrueNode, TFalseNode>()
        where TWhenObservable : IObservable<bool>
        where TSwitchObservable : IObservable<bool>
        where TTrueNode : IPipelineNode<LightTransition>
        where TFalseNode : IPipelineNode<LightTransition>
    {
        var shareableWhen = _observableSharingStrategy.Apply(ActivatorUtilities.CreateInstance<TWhenObservable>(serviceProvider));
        var shareableSwitch = _observableSharingStrategy.Apply(ActivatorUtilities.CreateInstance<TSwitchObservable>(serviceProvider));
        NodeContainers.Values.ForEach(b => b.SwitchWhen<TTrueNode, TFalseNode>(shareableWhen, shareableSwitch));
        return this;
    }

    /// <inheritdoc/>
    public ILightTransitionPipelineConfigurator<TLight> SwitchWhen<TWhenObservable, TTrueNode, TFalseNode>(
        IObservable<bool> switchObservable)
        where TWhenObservable : IObservable<bool>
        where TTrueNode : IPipelineNode<LightTransition>
        where TFalseNode : IPipelineNode<LightTransition>
    {
        var shareableWhen = _observableSharingStrategy.Apply(ActivatorUtilities.CreateInstance<TWhenObservable>(serviceProvider));
        var shareableSwitch = _observableSharingStrategy.Apply(switchObservable);
        NodeContainers.Values.ForEach(b => b.SwitchWhen<TTrueNode, TFalseNode>(shareableWhen, shareableSwitch));
        return this;
    }

    /// <inheritdoc/>
    public ILightTransitionPipelineConfigurator<TLight> SwitchWhen<TTrueNode, TFalseNode>(IObservable<bool> whenObservable,
        IObservable<bool> switchObservable)
        where TTrueNode : IPipelineNode<LightTransition>
        where TFalseNode : IPipelineNode<LightTransition>
    {
        var shareableWhen = _observableSharingStrategy.Apply(whenObservable);
        var shareableSwitch = _observableSharingStrategy.Apply(switchObservable);
        NodeContainers.Values.ForEach(b => b.SwitchWhen<TTrueNode, TFalseNode>(shareableWhen, shareableSwitch));
        return this;
    }

    /// <inheritdoc/>
    public ILightTransitionPipelineConfigurator<TLight> SwitchWhen(IObservable<bool> whenObservable,
        IObservable<bool> switchObservable, Action<ILightTransitionSwitchConfigurator<TLight>> configure)
    {
        var shareableWhen = _observableSharingStrategy.Apply(whenObservable);
        var shareableSwitch = _observableSharingStrategy.Apply(switchObservable);
        NodeContainers.Values.ForEach(b => b.SwitchWhen(shareableWhen, shareableSwitch, configure));
        return this;
    }

    /// <inheritdoc/>
    public ILightTransitionPipelineConfigurator<TLight> SwitchWhen<TWhenObservable, TSwitchObservable>(
        Action<ILightTransitionSwitchConfigurator<TLight>> configure)
        where TWhenObservable : IObservable<bool>
        where TSwitchObservable : IObservable<bool>
    {
        var shareableWhen = _observableSharingStrategy.Apply(ActivatorUtilities.CreateInstance<TWhenObservable>(serviceProvider));
        var shareableSwitch = _observableSharingStrategy.Apply(ActivatorUtilities.CreateInstance<TSwitchObservable>(serviceProvider));
        NodeContainers.Values.ForEach(b => b.SwitchWhen(shareableWhen, shareableSwitch, configure));
        return this;
    }

    /// <inheritdoc/>
    public ILightTransitionPipelineConfigurator<TLight> SwitchWhen<TWhenObservable>(IObservable<bool> switchObservable,
        Action<ILightTransitionSwitchConfigurator<TLight>> configure)
        where TWhenObservable : IObservable<bool>
    {
        var shareableWhen = _observableSharingStrategy.Apply(ActivatorUtilities.CreateInstance<TWhenObservable>(serviceProvider));
        var shareableSwitch = _observableSharingStrategy.Apply(switchObservable);
        NodeContainers.Values.ForEach(b => b.SwitchWhen(shareableWhen, shareableSwitch, configure));
        return this;
    }

    /// <inheritdoc/>
    public ILightTransitionPipelineConfigurator<TLight> AddReactiveNodeSwitchWhen(IObservable<bool> whenObservable,
        IObservable<bool> switchObservable,
        Action<ILightTransitionReactiveNodeConfigurator<TLight>> trueConfigure,
        Action<ILightTransitionReactiveNodeConfigurator<TLight>> falseConfigure,
        InstantiationScope instantiationScope = InstantiationScope.Shared)
    {
        var shareableWhen = _observableSharingStrategy.Apply(whenObservable);
        var shareableSwitch = _observableSharingStrategy.Apply(switchObservable);
        NodeContainers.Values.ForEach(b => b.AddReactiveNodeSwitchWhen(shareableWhen, shareableSwitch, trueConfigure, falseConfigure, instantiationScope));
        return this;
    }

    /// <inheritdoc/>
    public ILightTransitionPipelineConfigurator<TLight> AddReactiveNodeSwitchWhen<TWhenObservable, TSwitchObservable>(
        Action<ILightTransitionReactiveNodeConfigurator<TLight>> trueConfigure,
        Action<ILightTransitionReactiveNodeConfigurator<TLight>> falseConfigure,
        InstantiationScope instantiationScope = InstantiationScope.Shared)
        where TWhenObservable : IObservable<bool>
        where TSwitchObservable : IObservable<bool>
    {
        var shareableWhen = _observableSharingStrategy.Apply(ActivatorUtilities.CreateInstance<TWhenObservable>(serviceProvider));
        var shareableSwitch = _observableSharingStrategy.Apply(ActivatorUtilities.CreateInstance<TSwitchObservable>(serviceProvider));
        NodeContainers.Values.ForEach(b => b.AddReactiveNodeSwitchWhen(shareableWhen, shareableSwitch, trueConfigure, falseConfigure, instantiationScope));
        return this;
    }

    /// <inheritdoc/>
    public ILightTransitionPipelineConfigurator<TLight> AddReactiveNodeSwitchWhen<TWhenObservable>(
        IObservable<bool> switchObservable,
        Action<ILightTransitionReactiveNodeConfigurator<TLight>> trueConfigure,
        Action<ILightTransitionReactiveNodeConfigurator<TLight>> falseConfigure,
        InstantiationScope instantiationScope = InstantiationScope.Shared)
        where TWhenObservable : IObservable<bool>
    {
        var shareableWhen = _observableSharingStrategy.Apply(ActivatorUtilities.CreateInstance<TWhenObservable>(serviceProvider));
        var shareableSwitch = _observableSharingStrategy.Apply(switchObservable);
        NodeContainers.Values.ForEach(b => b.AddReactiveNodeSwitchWhen(shareableWhen, shareableSwitch, trueConfigure, falseConfigure, instantiationScope));
        return this;
    }

    /// <inheritdoc/>
    public ILightTransitionPipelineConfigurator<TLight> AddPipelineSwitchWhen(IObservable<bool> whenObservable,
        IObservable<bool> switchObservable,
        Action<ILightTransitionPipelineConfigurator<TLight>> trueConfigure,
        Action<ILightTransitionPipelineConfigurator<TLight>> falseConfigure,
        InstantiationScope instantiationScope = InstantiationScope.Shared)
    {
        var shareableWhen = _observableSharingStrategy.Apply(whenObservable);
        var shareableSwitch = _observableSharingStrategy.Apply(switchObservable);
        NodeContainers.Values.ForEach(b => b.AddPipelineSwitchWhen(shareableWhen, shareableSwitch, trueConfigure, falseConfigure, instantiationScope));
        return this;
    }

    /// <inheritdoc/>
    public ILightTransitionPipelineConfigurator<TLight> AddPipelineSwitchWhen<TWhenObservable, TSwitchObservable>(
        Action<ILightTransitionPipelineConfigurator<TLight>> trueConfigure,
        Action<ILightTransitionPipelineConfigurator<TLight>> falseConfigure,
        InstantiationScope instantiationScope = InstantiationScope.Shared)
        where TWhenObservable : IObservable<bool>
        where TSwitchObservable : IObservable<bool>
    {
        var shareableWhen = _observableSharingStrategy.Apply(ActivatorUtilities.CreateInstance<TWhenObservable>(serviceProvider));
        var shareableSwitch = _observableSharingStrategy.Apply(ActivatorUtilities.CreateInstance<TSwitchObservable>(serviceProvider));
        NodeContainers.Values.ForEach(b => b.AddPipelineSwitchWhen(shareableWhen, shareableSwitch, trueConfigure, falseConfigure, instantiationScope));
        return this;
    }

    /// <inheritdoc/>
    public ILightTransitionPipelineConfigurator<TLight> AddPipelineSwitchWhen<TWhenObservable>(
        IObservable<bool> switchObservable,
        Action<ILightTransitionPipelineConfigurator<TLight>> trueConfigure,
        Action<ILightTransitionPipelineConfigurator<TLight>> falseConfigure,
        InstantiationScope instantiationScope = InstantiationScope.Shared)
        where TWhenObservable : IObservable<bool>
    {
        var shareableWhen = _observableSharingStrategy.Apply(ActivatorUtilities.CreateInstance<TWhenObservable>(serviceProvider));
        var shareableSwitch = _observableSharingStrategy.Apply(switchObservable);
        NodeContainers.Values.ForEach(b => b.AddPipelineSwitchWhen(shareableWhen, shareableSwitch, trueConfigure, falseConfigure, instantiationScope));
        return this;
    }

    /// <inheritdoc/>
    public ILightTransitionPipelineConfigurator<TLight> TurnOnOffWhen(IObservable<bool> whenObservable,
        IObservable<bool> switchObservable)
    {
        var shareableWhen = _observableSharingStrategy.Apply(whenObservable);
        var shareableSwitch = _observableSharingStrategy.Apply(switchObservable);
        NodeContainers.Values.ForEach(b => b.TurnOnOffWhen(shareableWhen, shareableSwitch));
        return this;
    }

    /// <inheritdoc/>
    public ILightTransitionPipelineConfigurator<TLight> TurnOnOffWhen<TWhenObservable, TSwitchObservable>()
        where TWhenObservable : IObservable<bool>
        where TSwitchObservable : IObservable<bool>
    {
        var shareableWhen = _observableSharingStrategy.Apply(ActivatorUtilities.CreateInstance<TWhenObservable>(serviceProvider));
        var shareableSwitch = _observableSharingStrategy.Apply(ActivatorUtilities.CreateInstance<TSwitchObservable>(serviceProvider));
        NodeContainers.Values.ForEach(b => b.TurnOnOffWhen(shareableWhen, shareableSwitch));
        return this;
    }

    /// <inheritdoc/>
    public ILightTransitionPipelineConfigurator<TLight> TurnOnOffWhen<TWhenObservable>(IObservable<bool> switchObservable)
        where TWhenObservable : IObservable<bool>
    {
        var shareableWhen = _observableSharingStrategy.Apply(ActivatorUtilities.CreateInstance<TWhenObservable>(serviceProvider));
        var shareableSwitch = _observableSharingStrategy.Apply(switchObservable);
        NodeContainers.Values.ForEach(b => b.TurnOnOffWhen(shareableWhen, shareableSwitch));
        return this;
    }
}
