using CodeCasa.AutomationPipelines.Lights.Extensions;
using CodeCasa.AutomationPipelines.Lights.Nodes;
using CodeCasa.AutomationPipelines.Lights.ReactiveNode;
using CodeCasa.AutomationPipelines.Lights.Switch;
using CodeCasa.Lights;
using Microsoft.Extensions.DependencyInjection;
using System.Reactive.Concurrency;
using System.Reactive.Linq;

namespace CodeCasa.AutomationPipelines.Lights.Pipeline;

internal partial class LightTransitionPipelineConfigurator<TLight>
{
    /// <inheritdoc/>
    public ILightTransitionPipelineConfigurator<TLight> SwitchWhen(IObservable<bool> whenObservable,
        IObservable<bool> switchObservable,
        LightParameters trueLightParameters, LightParameters falseLightParameters)
    {
        return SwitchWhen(whenObservable, switchObservable, trueLightParameters.AsTransition(), falseLightParameters.AsTransition());
    }

    /// <inheritdoc/>
    public ILightTransitionPipelineConfigurator<TLight> SwitchWhen<TWhenObservable, TSwitchObservable>(
        LightParameters trueLightParameters, LightParameters falseLightParameters)
        where TWhenObservable : IObservable<bool>
        where TSwitchObservable : IObservable<bool>
    {
        var whenObservable = ActivatorUtilities.CreateInstance<TWhenObservable>(_serviceProvider);
        return SwitchWhen<TSwitchObservable>(whenObservable, trueLightParameters, falseLightParameters);
    }

    /// <inheritdoc/>
    public ILightTransitionPipelineConfigurator<TLight> SwitchWhen<TSwitchObservable>(IObservable<bool> whenObservable,
        LightParameters trueLightParameters, LightParameters falseLightParameters)
        where TSwitchObservable : IObservable<bool>
    {
        var switchObservable = ActivatorUtilities.CreateInstance<TSwitchObservable>(_serviceProvider);
        return SwitchWhen(whenObservable, switchObservable, trueLightParameters, falseLightParameters);
    }

    /// <inheritdoc/>
    public ILightTransitionPipelineConfigurator<TLight> SwitchWhen(IObservable<bool> whenObservable,
        IObservable<bool> switchObservable,
        Func<IServiceProvider, LightParameters> trueLightParametersFactory,
        Func<IServiceProvider, LightParameters> falseLightParametersFactory)
    {
        return SwitchWhen(whenObservable, switchObservable,
            c => trueLightParametersFactory(c).AsTransition(),
            c => falseLightParametersFactory(c).AsTransition());
    }

    /// <inheritdoc/>
    public ILightTransitionPipelineConfigurator<TLight> SwitchWhen<TWhenObservable, TSwitchObservable>(
        Func<IServiceProvider, LightParameters> trueLightParametersFactory,
        Func<IServiceProvider, LightParameters> falseLightParametersFactory)
        where TWhenObservable : IObservable<bool>
        where TSwitchObservable : IObservable<bool>
    {
        var whenObservable = ActivatorUtilities.CreateInstance<TWhenObservable>(_serviceProvider);
        return SwitchWhen<TSwitchObservable>(whenObservable, trueLightParametersFactory, falseLightParametersFactory);
    }

    /// <inheritdoc/>
    public ILightTransitionPipelineConfigurator<TLight> SwitchWhen<TSwitchObservable>(IObservable<bool> whenObservable,
        Func<IServiceProvider, LightParameters> trueLightParametersFactory,
        Func<IServiceProvider, LightParameters> falseLightParametersFactory)
        where TSwitchObservable : IObservable<bool>
    {
        var switchObservable = ActivatorUtilities.CreateInstance<TSwitchObservable>(_serviceProvider);
        return SwitchWhen(whenObservable, switchObservable, trueLightParametersFactory, falseLightParametersFactory);
    }

    /// <inheritdoc/>
    public ILightTransitionPipelineConfigurator<TLight> SwitchWhen(IObservable<bool> whenObservable,
        IObservable<bool> switchObservable,
        LightTransition trueLightTransition, LightTransition falseLightTransition)
    {
        return SwitchWhen(whenObservable, switchObservable, _ => trueLightTransition, _ => falseLightTransition);
    }

    /// <inheritdoc/>
    public ILightTransitionPipelineConfigurator<TLight> SwitchWhen<TWhenObservable, TSwitchObservable>(
        LightTransition trueLightTransition, LightTransition falseLightTransition)
        where TWhenObservable : IObservable<bool>
        where TSwitchObservable : IObservable<bool>
    {
        var whenObservable = ActivatorUtilities.CreateInstance<TWhenObservable>(_serviceProvider);
        return SwitchWhen<TSwitchObservable>(whenObservable, trueLightTransition, falseLightTransition);
    }

    /// <inheritdoc/>
    public ILightTransitionPipelineConfigurator<TLight> SwitchWhen<TSwitchObservable>(IObservable<bool> whenObservable,
        LightTransition trueLightTransition, LightTransition falseLightTransition)
        where TSwitchObservable : IObservable<bool>
    {
        var switchObservable = ActivatorUtilities.CreateInstance<TSwitchObservable>(_serviceProvider);
        return SwitchWhen(whenObservable, switchObservable, trueLightTransition, falseLightTransition);
    }

    /// <inheritdoc/>
    public ILightTransitionPipelineConfigurator<TLight> SwitchWhen(IObservable<bool> whenObservable,
        IObservable<bool> switchObservable,
        Func<IServiceProvider, LightTransition> trueLightTransitionFactory,
        Func<IServiceProvider, LightTransition> falseLightTransitionFactory)
    {
        return SwitchWhen(whenObservable, switchObservable,
            c => new StaticLightTransitionNode(trueLightTransitionFactory(c), c.GetRequiredService<IScheduler>()),
            c => new StaticLightTransitionNode(falseLightTransitionFactory(c), c.GetRequiredService<IScheduler>()));
    }

    /// <inheritdoc/>
    public ILightTransitionPipelineConfigurator<TLight> SwitchWhen<TWhenObservable, TSwitchObservable>(
        Func<IServiceProvider, LightTransition> trueLightTransitionFactory,
        Func<IServiceProvider, LightTransition> falseLightTransitionFactory)
        where TWhenObservable : IObservable<bool>
        where TSwitchObservable : IObservable<bool>
    {
        var whenObservable = ActivatorUtilities.CreateInstance<TWhenObservable>(_serviceProvider);
        return SwitchWhen<TSwitchObservable>(whenObservable, trueLightTransitionFactory, falseLightTransitionFactory);
    }

    /// <inheritdoc/>
    public ILightTransitionPipelineConfigurator<TLight> SwitchWhen<TSwitchObservable>(IObservable<bool> whenObservable,
        Func<IServiceProvider, LightTransition> trueLightTransitionFactory,
        Func<IServiceProvider, LightTransition> falseLightTransitionFactory)
        where TSwitchObservable : IObservable<bool>
    {
        var switchObservable = ActivatorUtilities.CreateInstance<TSwitchObservable>(_serviceProvider);
        return SwitchWhen(whenObservable, switchObservable, trueLightTransitionFactory, falseLightTransitionFactory);
    }

    /// <inheritdoc/>
    public ILightTransitionPipelineConfigurator<TLight> SwitchWhen(IObservable<bool> whenObservable,
        IObservable<bool> switchObservable,
        Func<IServiceProvider, IPipelineNode<LightTransition>> trueNodeFactory,
        Func<IServiceProvider, IPipelineNode<LightTransition>> falseNodeFactory)
    {
        return AddReactiveNode(c => c
            .SetHierarchyContext(HierarchyPath, "SwitchWhen", LoggingEnabled ?? false)
            .On(whenObservable.Where(x => x).CombineLatest(switchObservable, (_, s) => s).Where(x => x), trueNodeFactory)
            .On(whenObservable.Where(x => x).CombineLatest(switchObservable, (_, s) => s).Where(x => !x), falseNodeFactory)
            .PassThroughOn(whenObservable.Where(x => !x)));
    }

    /// <inheritdoc/>
    public ILightTransitionPipelineConfigurator<TLight> SwitchWhen<TWhenObservable, TSwitchObservable>(
        Func<IServiceProvider, IPipelineNode<LightTransition>> trueNodeFactory,
        Func<IServiceProvider, IPipelineNode<LightTransition>> falseNodeFactory)
        where TWhenObservable : IObservable<bool>
        where TSwitchObservable : IObservable<bool>
    {
        var whenObservable = ActivatorUtilities.CreateInstance<TWhenObservable>(_serviceProvider);
        return SwitchWhen<TSwitchObservable>(whenObservable, trueNodeFactory, falseNodeFactory);
    }

    /// <inheritdoc/>
    public ILightTransitionPipelineConfigurator<TLight> SwitchWhen<TSwitchObservable>(IObservable<bool> whenObservable,
        Func<IServiceProvider, IPipelineNode<LightTransition>> trueNodeFactory,
        Func<IServiceProvider, IPipelineNode<LightTransition>> falseNodeFactory)
        where TSwitchObservable : IObservable<bool>
    {
        var switchObservable = ActivatorUtilities.CreateInstance<TSwitchObservable>(_serviceProvider);
        return SwitchWhen(whenObservable, switchObservable, trueNodeFactory, falseNodeFactory);
    }

    /// <inheritdoc/>
    public ILightTransitionPipelineConfigurator<TLight> SwitchWhen<TWhenObservable, TSwitchObservable, TTrueNode, TFalseNode>()
        where TWhenObservable : IObservable<bool>
        where TSwitchObservable : IObservable<bool>
        where TTrueNode : IPipelineNode<LightTransition>
        where TFalseNode : IPipelineNode<LightTransition>
    {
        var whenObservable = ActivatorUtilities.CreateInstance<TWhenObservable>(_serviceProvider);
        return SwitchWhen<TSwitchObservable, TTrueNode, TFalseNode>(whenObservable);
    }

    /// <inheritdoc/>
    public ILightTransitionPipelineConfigurator<TLight> SwitchWhen<TSwitchObservable, TTrueNode, TFalseNode>(
        IObservable<bool> whenObservable)
        where TSwitchObservable : IObservable<bool>
        where TTrueNode : IPipelineNode<LightTransition>
        where TFalseNode : IPipelineNode<LightTransition>
    {
        var switchObservable = ActivatorUtilities.CreateInstance<TSwitchObservable>(_serviceProvider);
        return SwitchWhen<TTrueNode, TFalseNode>(whenObservable, switchObservable);
    }

    /// <inheritdoc/>
    public ILightTransitionPipelineConfigurator<TLight> SwitchWhen<TTrueNode, TFalseNode>(IObservable<bool> whenObservable,
        IObservable<bool> switchObservable)
        where TTrueNode : IPipelineNode<LightTransition>
        where TFalseNode : IPipelineNode<LightTransition>
    {
        return AddReactiveNode(c => c
            .SetHierarchyContext(HierarchyPath, "SwitchWhen", LoggingEnabled ?? false)
            .On<bool, TTrueNode>(whenObservable.Where(x => x).CombineLatest(switchObservable, (_, s) => s).Where(x => x))
            .On<bool, TFalseNode>(whenObservable.Where(x => x).CombineLatest(switchObservable, (_, s) => s).Where(x => !x))
            .PassThroughOn(whenObservable.Where(x => !x)));
    }

    /// <inheritdoc/>
    public ILightTransitionPipelineConfigurator<TLight> SwitchWhen(IObservable<bool> whenObservable,
        IObservable<bool> switchObservable, Action<ILightTransitionSwitchConfigurator<TLight>> configure)
    {
        var switchConfigurator = new LightTransitionSwitchConfigurator<TLight>();
        configure(switchConfigurator);
        var falseConfigurator = switchConfigurator.FalseConfigurator
            ?? throw new InvalidOperationException($"{nameof(ILightTransitionSwitchConfigurator<TLight>.WhenTrue)} must be called exactly once inside the switch configure action.");
        var trueNodeFactory = falseConfigurator.TrueNodeFactory;
        var falseNodeFactory = falseConfigurator.FalseNodeFactory
            ?? throw new InvalidOperationException($"{nameof(ILightTransitionSwitchFalseConfigurator<TLight>.WhenFalse)} must be called exactly once inside the switch configure action.");
        return SwitchWhen(whenObservable, switchObservable, trueNodeFactory, falseNodeFactory);
    }

    /// <inheritdoc/>
    public ILightTransitionPipelineConfigurator<TLight> SwitchWhen<TWhenObservable, TSwitchObservable>(
        Action<ILightTransitionSwitchConfigurator<TLight>> configure)
        where TWhenObservable : IObservable<bool>
        where TSwitchObservable : IObservable<bool>
    {
        var whenObservable = ActivatorUtilities.CreateInstance<TWhenObservable>(_serviceProvider);
        return SwitchWhen<TSwitchObservable>(whenObservable, configure);
    }

    /// <inheritdoc/>
    public ILightTransitionPipelineConfigurator<TLight> SwitchWhen<TSwitchObservable>(IObservable<bool> whenObservable,
        Action<ILightTransitionSwitchConfigurator<TLight>> configure)
        where TSwitchObservable : IObservable<bool>
    {
        var switchObservable = ActivatorUtilities.CreateInstance<TSwitchObservable>(_serviceProvider);
        return SwitchWhen(whenObservable, switchObservable, configure);
    }

    /// <inheritdoc/>
    public ILightTransitionPipelineConfigurator<TLight> AddReactiveNodeSwitchWhen(IObservable<bool> whenObservable,
        IObservable<bool> switchObservable,
        Action<ILightTransitionReactiveNodeConfigurator<TLight>> trueConfigure,
        Action<ILightTransitionReactiveNodeConfigurator<TLight>> falseConfigure,
        InstantiationScope instantiationScope = InstantiationScope.Shared)
    {
        return AddReactiveNode(c => c
            .SetHierarchyContext(HierarchyPath, "SwitchWhen", LoggingEnabled ?? false)
            .On(whenObservable.Where(x => x).CombineLatest(switchObservable, (_, s) => s).Where(x => x), trueConfigure.ApplyHierarchySettings(c), instantiationScope)
            .On(whenObservable.Where(x => x).CombineLatest(switchObservable, (_, s) => s).Where(x => !x), falseConfigure.ApplyHierarchySettings(c), instantiationScope)
            .PassThroughOn(whenObservable.Where(x => !x)));
    }

    /// <inheritdoc/>
    public ILightTransitionPipelineConfigurator<TLight> AddReactiveNodeSwitchWhen<TWhenObservable, TSwitchObservable>(
        Action<ILightTransitionReactiveNodeConfigurator<TLight>> trueConfigure,
        Action<ILightTransitionReactiveNodeConfigurator<TLight>> falseConfigure,
        InstantiationScope instantiationScope = InstantiationScope.Shared)
        where TWhenObservable : IObservable<bool>
        where TSwitchObservable : IObservable<bool>
    {
        var whenObservable = ActivatorUtilities.CreateInstance<TWhenObservable>(_serviceProvider);
        return AddReactiveNodeSwitchWhen<TSwitchObservable>(whenObservable, trueConfigure, falseConfigure, instantiationScope);
    }

    /// <inheritdoc/>
    public ILightTransitionPipelineConfigurator<TLight> AddReactiveNodeSwitchWhen<TSwitchObservable>(
        IObservable<bool> whenObservable,
        Action<ILightTransitionReactiveNodeConfigurator<TLight>> trueConfigure,
        Action<ILightTransitionReactiveNodeConfigurator<TLight>> falseConfigure,
        InstantiationScope instantiationScope = InstantiationScope.Shared)
        where TSwitchObservable : IObservable<bool>
    {
        var switchObservable = ActivatorUtilities.CreateInstance<TSwitchObservable>(_serviceProvider);
        return AddReactiveNodeSwitchWhen(whenObservable, switchObservable, trueConfigure, falseConfigure, instantiationScope);
    }

    /// <inheritdoc/>
    public ILightTransitionPipelineConfigurator<TLight> AddPipelineSwitchWhen(IObservable<bool> whenObservable,
        IObservable<bool> switchObservable,
        Action<ILightTransitionPipelineConfigurator<TLight>> trueConfigure,
        Action<ILightTransitionPipelineConfigurator<TLight>> falseConfigure,
        InstantiationScope instantiationScope = InstantiationScope.Shared)
    {
        return AddReactiveNode(c => c
            .SetHierarchyContext(HierarchyPath, "SwitchWhen", LoggingEnabled ?? false)
            .On(whenObservable.Where(x => x).CombineLatest(switchObservable, (_, s) => s).Where(x => x), trueConfigure.ApplyHierarchySettings(c), instantiationScope)
            .On(whenObservable.Where(x => x).CombineLatest(switchObservable, (_, s) => s).Where(x => !x), falseConfigure.ApplyHierarchySettings(c), instantiationScope)
            .PassThroughOn(whenObservable.Where(x => !x)));
    }

    /// <inheritdoc/>
    public ILightTransitionPipelineConfigurator<TLight> AddPipelineSwitchWhen<TWhenObservable, TSwitchObservable>(
        Action<ILightTransitionPipelineConfigurator<TLight>> trueConfigure,
        Action<ILightTransitionPipelineConfigurator<TLight>> falseConfigure,
        InstantiationScope instantiationScope = InstantiationScope.Shared)
        where TWhenObservable : IObservable<bool>
        where TSwitchObservable : IObservable<bool>
    {
        var whenObservable = ActivatorUtilities.CreateInstance<TWhenObservable>(_serviceProvider);
        return AddPipelineSwitchWhen<TSwitchObservable>(whenObservable, trueConfigure, falseConfigure, instantiationScope);
    }

    /// <inheritdoc/>
    public ILightTransitionPipelineConfigurator<TLight> AddPipelineSwitchWhen<TSwitchObservable>(
        IObservable<bool> whenObservable,
        Action<ILightTransitionPipelineConfigurator<TLight>> trueConfigure,
        Action<ILightTransitionPipelineConfigurator<TLight>> falseConfigure,
        InstantiationScope instantiationScope = InstantiationScope.Shared)
        where TSwitchObservable : IObservable<bool>
    {
        var switchObservable = ActivatorUtilities.CreateInstance<TSwitchObservable>(_serviceProvider);
        return AddPipelineSwitchWhen(whenObservable, switchObservable, trueConfigure, falseConfigure, instantiationScope);
    }

    /// <inheritdoc/>
    public ILightTransitionPipelineConfigurator<TLight> TurnOnOffWhen(IObservable<bool> whenObservable,
        IObservable<bool> switchObservable)
    {
        return SwitchWhen(whenObservable, switchObservable, LightTransition.On(), LightTransition.Off());
    }

    /// <inheritdoc/>
    public ILightTransitionPipelineConfigurator<TLight> TurnOnOffWhen<TWhenObservable, TSwitchObservable>()
        where TWhenObservable : IObservable<bool>
        where TSwitchObservable : IObservable<bool>
    {
        var whenObservable = ActivatorUtilities.CreateInstance<TWhenObservable>(_serviceProvider);
        return TurnOnOffWhen<TSwitchObservable>(whenObservable);
    }

    /// <inheritdoc/>
    public ILightTransitionPipelineConfigurator<TLight> TurnOnOffWhen<TSwitchObservable>(IObservable<bool> whenObservable)
        where TSwitchObservable : IObservable<bool>
    {
        var switchObservable = ActivatorUtilities.CreateInstance<TSwitchObservable>(_serviceProvider);
        return TurnOnOffWhen(whenObservable, switchObservable);
    }
}
