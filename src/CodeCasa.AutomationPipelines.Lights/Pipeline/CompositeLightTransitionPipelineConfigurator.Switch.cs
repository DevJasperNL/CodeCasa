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
        var shareableObservable = _observableSharingStrategy.Apply(ActivatorUtilities.CreateInstance<TObservable>(serviceProvider));
        NodeContainers.Values.ForEach(b => b.Switch(shareableObservable, trueLightParameters, falseLightParameters));
        return this;
    }

    /// <inheritdoc/>
    public ILightTransitionPipelineConfigurator<TLight> Switch(IObservable<bool> observable, LightParameters trueLightParameters,
        LightParameters falseLightParameters)
    {
        var shareableObservable = _observableSharingStrategy.Apply(observable);
        NodeContainers.Values.ForEach(b => b.Switch(shareableObservable, trueLightParameters, falseLightParameters));
        return this;
    }

    /// <inheritdoc/>
    public ILightTransitionPipelineConfigurator<TLight> Switch<TObservable>(Func<IServiceProvider, LightParameters> trueLightParametersFactory,
        Func<IServiceProvider, LightParameters> falseLightParametersFactory) where TObservable : IObservable<bool>
    {
        var shareableObservable = _observableSharingStrategy.Apply(ActivatorUtilities.CreateInstance<TObservable>(serviceProvider));
        NodeContainers.Values.ForEach(b => b.Switch(shareableObservable, trueLightParametersFactory, falseLightParametersFactory));
        return this;
    }

    /// <inheritdoc/>
    public ILightTransitionPipelineConfigurator<TLight> Switch(IObservable<bool> observable, Func<IServiceProvider, LightParameters> trueLightParametersFactory,
        Func<IServiceProvider, LightParameters> falseLightParametersFactory)
    {
        var shareableObservable = _observableSharingStrategy.Apply(observable);
        NodeContainers.Values.ForEach(b => b.Switch(shareableObservable, trueLightParametersFactory, falseLightParametersFactory));
        return this;
    }

    /// <inheritdoc/>
    public ILightTransitionPipelineConfigurator<TLight> Switch<TObservable>(LightTransition trueLightTransition,
        LightTransition falseLightTransition) where TObservable : IObservable<bool>
    {
        var shareableObservable = _observableSharingStrategy.Apply(ActivatorUtilities.CreateInstance<TObservable>(serviceProvider));
        NodeContainers.Values.ForEach(b => b.Switch(shareableObservable, trueLightTransition, falseLightTransition));
        return this;
    }

    /// <inheritdoc/>
    public ILightTransitionPipelineConfigurator<TLight> Switch(IObservable<bool> observable, LightTransition trueLightTransition,
        LightTransition falseLightTransition)
    {
        var shareableObservable = _observableSharingStrategy.Apply(observable);
        NodeContainers.Values.ForEach(b => b.Switch(shareableObservable, trueLightTransition, falseLightTransition));
        return this;
    }

    /// <inheritdoc/>
    public ILightTransitionPipelineConfigurator<TLight> Switch<TObservable>(Func<IServiceProvider, LightTransition> trueLightTransitionFactory,
        Func<IServiceProvider, LightTransition> falseLightTransitionFactory) where TObservable : IObservable<bool>
    {
        var shareableObservable = _observableSharingStrategy.Apply(ActivatorUtilities.CreateInstance<TObservable>(serviceProvider));
        NodeContainers.Values.ForEach(b => b.Switch(shareableObservable, trueLightTransitionFactory, falseLightTransitionFactory));
        return this;
    }

    /// <inheritdoc/>
    public ILightTransitionPipelineConfigurator<TLight> Switch(IObservable<bool> observable, Func<IServiceProvider, LightTransition> trueLightTransitionFactory,
        Func<IServiceProvider, LightTransition> falseLightTransitionFactory)
    {
        var shareableObservable = _observableSharingStrategy.Apply(observable);
        NodeContainers.Values.ForEach(b => b.Switch(shareableObservable, trueLightTransitionFactory, falseLightTransitionFactory));
        return this;
    }

    /// <inheritdoc/>
    public ILightTransitionPipelineConfigurator<TLight> Switch<TObservable>(Func<IServiceProvider, IPipelineNode<LightTransition>> trueNodeFactory, Func<IServiceProvider, IPipelineNode<LightTransition>> falseNodeFactory) where TObservable : IObservable<bool>
    {
        var shareableObservable = _observableSharingStrategy.Apply(ActivatorUtilities.CreateInstance<TObservable>(serviceProvider));
        NodeContainers.Values.ForEach(b => b.Switch(shareableObservable, trueNodeFactory, falseNodeFactory));
        return this;
    }

    /// <inheritdoc/>
    public ILightTransitionPipelineConfigurator<TLight> Switch(IObservable<bool> observable, Func<IServiceProvider, IPipelineNode<LightTransition>> trueNodeFactory,
        Func<IServiceProvider, IPipelineNode<LightTransition>> falseNodeFactory)
    {
        var shareableObservable = _observableSharingStrategy.Apply(observable);
        NodeContainers.Values.ForEach(b => b.Switch(shareableObservable, trueNodeFactory, falseNodeFactory));
        return this;
    }

    /// <inheritdoc/>
    public ILightTransitionPipelineConfigurator<TLight> Switch<TObservable, TTrueNode, TFalseNode>() where TObservable : IObservable<bool> where TTrueNode : IPipelineNode<LightTransition> where TFalseNode : IPipelineNode<LightTransition>
    {
        var shareableObservable = _observableSharingStrategy.Apply(ActivatorUtilities.CreateInstance<TObservable>(serviceProvider));
        NodeContainers.Values.ForEach(b => b.Switch<TTrueNode, TFalseNode>(shareableObservable));
        return this;
    }

    /// <inheritdoc/>
    public ILightTransitionPipelineConfigurator<TLight> Switch<TTrueNode, TFalseNode>(IObservable<bool> observable) where TTrueNode : IPipelineNode<LightTransition> where TFalseNode : IPipelineNode<LightTransition>
    {
        var shareableObservable = _observableSharingStrategy.Apply(observable);
        NodeContainers.Values.ForEach(b => b.Switch<TTrueNode, TFalseNode>(shareableObservable));
        return this;
    }

    /// <inheritdoc/>
    public ILightTransitionPipelineConfigurator<TLight> AddReactiveNodeSwitch<TObservable>(Action<ILightTransitionReactiveNodeConfigurator<TLight>> trueConfigure,
        Action<ILightTransitionReactiveNodeConfigurator<TLight>> falseConfigure, InstantiationScope instantiationScope = InstantiationScope.Shared) where TObservable : IObservable<bool>
    {
        if (instantiationScope == InstantiationScope.Shared)
        {
            var observable = ActivatorUtilities.CreateInstance<TObservable>(serviceProvider);
            return AddReactiveNodeSwitch(observable, trueConfigure, falseConfigure, instantiationScope);
        }
        var shareableObservable = _observableSharingStrategy.Apply(ActivatorUtilities.CreateInstance<TObservable>(serviceProvider));
        NodeContainers.Values.ForEach(b => b.AddReactiveNodeSwitch(shareableObservable, trueConfigure, falseConfigure, instantiationScope));
        return this;
    }

    /// <inheritdoc/>
    public ILightTransitionPipelineConfigurator<TLight> AddReactiveNodeSwitch(IObservable<bool> observable, Action<ILightTransitionReactiveNodeConfigurator<TLight>> trueConfigure,
        Action<ILightTransitionReactiveNodeConfigurator<TLight>> falseConfigure, InstantiationScope instantiationScope = InstantiationScope.Shared)
    {
        var shareableObservable = _observableSharingStrategy.Apply(observable);
        return AddReactiveNode(c => c
            .SetHierarchyContext(HierarchyPath, "Toggle", LoggingEnabled ?? false)
            .On(shareableObservable.Where(x => x), trueConfigure, instantiationScope)
            .On(shareableObservable.Where(x => !x), falseConfigure, instantiationScope));
    }

    /// <inheritdoc/>
    public ILightTransitionPipelineConfigurator<TLight> AddPipelineSwitch<TObservable>(Action<ILightTransitionPipelineConfigurator<TLight>> trueConfigure, Action<ILightTransitionPipelineConfigurator<TLight>> falseConfigure, InstantiationScope instantiationScope = InstantiationScope.Shared) where TObservable : IObservable<bool>
    {
        if (instantiationScope == InstantiationScope.Shared)
        {
            var observable = ActivatorUtilities.CreateInstance<TObservable>(serviceProvider);
            return AddPipelineSwitch(observable, trueConfigure, falseConfigure, instantiationScope);
        }
        var shareableObservable = _observableSharingStrategy.Apply(ActivatorUtilities.CreateInstance<TObservable>(serviceProvider));
        NodeContainers.Values.ForEach(b => b.AddPipelineSwitch(shareableObservable, trueConfigure, falseConfigure, instantiationScope));
        return this;
    }

    /// <inheritdoc/>
    public ILightTransitionPipelineConfigurator<TLight> AddPipelineSwitch(IObservable<bool> observable, Action<ILightTransitionPipelineConfigurator<TLight>> trueConfigure,
        Action<ILightTransitionPipelineConfigurator<TLight>> falseConfigure, InstantiationScope instantiationScope = InstantiationScope.Shared)
    {
        var shareableObservable = _observableSharingStrategy.Apply(observable);
        return AddReactiveNode(c => c
            .SetHierarchyContext(HierarchyPath, "Switch", LoggingEnabled ?? false)
            .On(shareableObservable.Where(x => x), trueConfigure.ApplyHierarchySettings(c), instantiationScope)
            .On(shareableObservable.Where(x => !x), falseConfigure.ApplyHierarchySettings(c), instantiationScope));
    }

    /// <inheritdoc/>
    public ILightTransitionPipelineConfigurator<TLight> TurnOnOff<TObservable>() where TObservable : IObservable<bool>
    {
        var shareableObservable = _observableSharingStrategy.Apply(ActivatorUtilities.CreateInstance<TObservable>(serviceProvider));
        return Switch(shareableObservable, LightTransition.On(), LightTransition.Off());
    }

    /// <inheritdoc/>
    public ILightTransitionPipelineConfigurator<TLight> TurnOnOff(IObservable<bool> observable)
    {
        var shareableObservable = _observableSharingStrategy.Apply(observable);
        return Switch(shareableObservable, LightTransition.On(), LightTransition.Off());
    }
}
