using CodeCasa.AutomationPipelines.Lights.Extensions;
using CodeCasa.AutomationPipelines.Lights.Nodes;
using CodeCasa.AutomationPipelines.Lights.ReactiveNode;
using CodeCasa.AutomationPipelines.Lights.Switch;
using CodeCasa.AutomationPipelines.Lights.Timeline;
using CodeCasa.Lights;
using Microsoft.Extensions.DependencyInjection;
using Occurify;
using System.Reactive.Concurrency;
using System.Reactive.Linq;

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
    public ILightTransitionPipelineConfigurator<TLight> Switch<TObservable>(Func<IServiceProvider, LightParameters?> trueLightParametersFactory,
        Func<IServiceProvider, LightParameters?> falseLightParametersFactory) where TObservable : IObservable<bool>
    {
        return Switch<TObservable>(sp => falseLightParametersFactory(sp)?.AsTransition(), c => trueLightParametersFactory(c)?.AsTransition());
    }

    /// <inheritdoc/>
    public ILightTransitionPipelineConfigurator<TLight> Switch(IObservable<bool> observable, Func<IServiceProvider, LightParameters?> trueLightParametersFactory,
        Func<IServiceProvider, LightParameters?> falseLightParametersFactory)
    {
        return Switch(observable, sp => trueLightParametersFactory(sp)?.AsTransition(), c => falseLightParametersFactory(c)?.AsTransition());
    }

    /// <inheritdoc/>
    public ILightTransitionPipelineConfigurator<TLight> Switch<TObservable>(LightTransition trueLightTransition,
        LightTransition falseLightTransition) where TObservable : IObservable<bool>
    {
        return Switch<TObservable>(_ => trueLightTransition, _ => falseLightTransition);
    }

    /// <inheritdoc/>
    public ILightTransitionPipelineConfigurator<TLight> Switch(IObservable<bool> observable, LightTransition trueLightTransition,
        LightTransition falseLightTransition)
    {
        return Switch(observable, _ => trueLightTransition, _ => falseLightTransition);
    }

    /// <inheritdoc/>
    public ILightTransitionPipelineConfigurator<TLight> Switch<TObservable>(Func<IServiceProvider, LightTransition?> trueLightTransitionFactory,
        Func<IServiceProvider, LightTransition?> falseLightTransitionFactory) where TObservable : IObservable<bool>
    {
        return Switch<TObservable>(
            sp => new StaticLightTransitionNode(trueLightTransitionFactory(sp), sp.GetRequiredService<IScheduler>()), 
            sp => new StaticLightTransitionNode(falseLightTransitionFactory(sp), sp.GetRequiredService<IScheduler>()));
    }

    /// <inheritdoc/>
    public ILightTransitionPipelineConfigurator<TLight> Switch(IObservable<bool> observable, Func<IServiceProvider, LightTransition?> trueLightTransitionFactory,
        Func<IServiceProvider, LightTransition?> falseLightTransitionFactory)
    {
        return Switch(
            observable,
            sp => new StaticLightTransitionNode(trueLightTransitionFactory(sp), sp.GetRequiredService<IScheduler>()),
            sp => new StaticLightTransitionNode(falseLightTransitionFactory(sp), sp.GetRequiredService<IScheduler>()));
    }

    /// <inheritdoc/>
    public ILightTransitionPipelineConfigurator<TLight> Switch<TObservable>(Func<IServiceProvider, IPipelineNode<LightTransition>> trueNodeFactory, Func<IServiceProvider, IPipelineNode<LightTransition>> falseNodeFactory) where TObservable : IObservable<bool>
    {
        var observable = ActivatorUtilities.CreateInstance<TObservable>(ServiceProvider);
        return Switch(observable, trueNodeFactory, falseNodeFactory);
    }

    /// <inheritdoc/>
    public ILightTransitionPipelineConfigurator<TLight> Switch(IObservable<bool> observable, Func<IServiceProvider, IPipelineNode<LightTransition>> trueNodeFactory,
        Func<IServiceProvider, IPipelineNode<LightTransition>> falseNodeFactory)
    {
        return AddReactiveNode(c => c
            .SetHierarchyContext(HierarchyPath, "Switch", LoggingEnabled ?? false)
            .On(observable.Where(x => x), trueNodeFactory)
            .On(observable.Where(x => !x), falseNodeFactory));
    }

    /// <inheritdoc/>
    public ILightTransitionPipelineConfigurator<TLight> Switch<TObservable, TTrueNode, TFalseNode>() where TObservable : IObservable<bool> where TTrueNode : IPipelineNode<LightTransition> where TFalseNode : IPipelineNode<LightTransition>
    {
        var observable = ActivatorUtilities.CreateInstance<TObservable>(ServiceProvider);
        return Switch<TTrueNode, TFalseNode>(observable);
    }

    /// <inheritdoc/>
    public ILightTransitionPipelineConfigurator<TLight> Switch<TTrueNode, TFalseNode>(IObservable<bool> observable) where TTrueNode : IPipelineNode<LightTransition> where TFalseNode : IPipelineNode<LightTransition>
    {
        return AddReactiveNode(c => c
            .SetHierarchyContext(HierarchyPath, "Switch", LoggingEnabled ?? false)
            .On<bool, TTrueNode>(observable.Where(x => x))
            .On<bool, TFalseNode>(observable.Where(x => !x)));
    }

    /// <inheritdoc/>
    public ILightTransitionPipelineConfigurator<TLight> AddReactiveNodeSwitch<TObservable>(Action<ILightTransitionReactiveNodeConfigurator<TLight>> trueConfigure, Action<ILightTransitionReactiveNodeConfigurator<TLight>> falseConfigure, InstantiationScope instantiationScope = InstantiationScope.Shared) where TObservable : IObservable<bool>
    {
        var observable = ActivatorUtilities.CreateInstance<TObservable>(ServiceProvider);
        return AddReactiveNodeSwitch(observable, trueConfigure, falseConfigure, instantiationScope);
    }

    /// <inheritdoc/>
    public ILightTransitionPipelineConfigurator<TLight> AddReactiveNodeSwitch(IObservable<bool> observable, Action<ILightTransitionReactiveNodeConfigurator<TLight>> trueConfigure,
        Action<ILightTransitionReactiveNodeConfigurator<TLight>> falseConfigure, InstantiationScope instantiationScope = InstantiationScope.Shared)
    {
        return AddReactiveNode(c => c
            .SetHierarchyContext(HierarchyPath, "Switch", LoggingEnabled ?? false)
            .On(observable.Where(x => x), trueConfigure.ApplyHierarchySettings(c), instantiationScope)
            .On(observable.Where(x => !x), falseConfigure.ApplyHierarchySettings(c), instantiationScope));
    }

    /// <inheritdoc/>
    public ILightTransitionPipelineConfigurator<TLight> AddPipelineSwitch<TObservable>(Action<ILightTransitionPipelineConfigurator<TLight>> trueConfigure, Action<ILightTransitionPipelineConfigurator<TLight>> falseConfigure, InstantiationScope instantiationScope = InstantiationScope.Shared) where TObservable : IObservable<bool>
    {
        var observable = ActivatorUtilities.CreateInstance<TObservable>(ServiceProvider);
        return AddPipelineSwitch(observable, trueConfigure, falseConfigure, instantiationScope);
    }

    /// <inheritdoc/>
    public ILightTransitionPipelineConfigurator<TLight> AddPipelineSwitch(IObservable<bool> observable, Action<ILightTransitionPipelineConfigurator<TLight>> trueConfigure,
        Action<ILightTransitionPipelineConfigurator<TLight>> falseConfigure, InstantiationScope instantiationScope = InstantiationScope.Shared)
    {
        return AddReactiveNode(c => c
            .SetHierarchyContext(HierarchyPath, "Switch", LoggingEnabled ?? false)
            .On(observable.Where(x => x), trueConfigure.ApplyHierarchySettings(c), instantiationScope)
            .On(observable.Where(x => !x), falseConfigure.ApplyHierarchySettings(c), instantiationScope));
    }

    /// <inheritdoc/>
    public ILightTransitionPipelineConfigurator<TLight> Switch<TObservable>(Action<ILightTransitionSwitchConfigurator<TLight>> configure)
        where TObservable : IObservable<bool>
    {
        var observable = ActivatorUtilities.CreateInstance<TObservable>(ServiceProvider);
        return Switch(observable, configure);
    }

    /// <inheritdoc/>
    public ILightTransitionPipelineConfigurator<TLight> Switch(IObservable<bool> observable, Action<ILightTransitionSwitchConfigurator<TLight>> configure)
    {
        var switchConfigurator = new LightTransitionSwitchConfigurator<TLight>();
        configure(switchConfigurator);
        var falseConfigurator = switchConfigurator.FalseConfigurator
            ?? throw new InvalidOperationException($"{nameof(ILightTransitionSwitchConfigurator<TLight>.WhenTrue)} must be called exactly once inside the switch configure action.");
        var trueNodeFactory = falseConfigurator.TrueNodeFactory;
        var falseNodeFactory = falseConfigurator.FalseNodeFactory
            ?? throw new InvalidOperationException($"{nameof(ILightTransitionSwitchFalseConfigurator<TLight>.WhenFalse)} must be called exactly once inside the switch configure action.");
        return Switch(observable, trueNodeFactory, falseNodeFactory);
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

    /// <inheritdoc/>
    public ILightTransitionPipelineConfigurator<TLight> Switch<TObservable>(Dictionary<ITimeline, LightParameters> trueTimeline,
        Dictionary<ITimeline, LightParameters> falseTimeline, TimeSpan? transitionTimeForTimelineState = null)
        where TObservable : IObservable<bool>
    {
        var observable = ActivatorUtilities.CreateInstance<TObservable>(ServiceProvider);
        return Switch(observable, trueTimeline, falseTimeline, transitionTimeForTimelineState);
    }

    /// <inheritdoc/>
    public ILightTransitionPipelineConfigurator<TLight> Switch(IObservable<bool> observable,
        Dictionary<ITimeline, LightParameters> trueTimeline, Dictionary<ITimeline, LightParameters> falseTimeline,
        TimeSpan? transitionTimeForTimelineState = null)
    {
        return Switch(observable,
            sp => new TimelineNode(trueTimeline, sp.GetRequiredService<IScheduler>(), transitionTimeForTimelineState),
            sp => new TimelineNode(falseTimeline, sp.GetRequiredService<IScheduler>(), transitionTimeForTimelineState));
    }

    /// <inheritdoc/>
    public ILightTransitionPipelineConfigurator<TLight> Switch<TObservable>(
        Func<IServiceProvider, Dictionary<ITimeline, LightParameters>> trueTimelineFactory,
        Func<IServiceProvider, Dictionary<ITimeline, LightParameters>> falseTimelineFactory,
        TimeSpan? transitionTimeForTimelineState = null) where TObservable : IObservable<bool>
    {
        var observable = ActivatorUtilities.CreateInstance<TObservable>(ServiceProvider);
        return Switch(observable, trueTimelineFactory, falseTimelineFactory, transitionTimeForTimelineState);
    }

    /// <inheritdoc/>
    public ILightTransitionPipelineConfigurator<TLight> Switch(IObservable<bool> observable,
        Func<IServiceProvider, Dictionary<ITimeline, LightParameters>> trueTimelineFactory,
        Func<IServiceProvider, Dictionary<ITimeline, LightParameters>> falseTimelineFactory,
        TimeSpan? transitionTimeForTimelineState = null)
    {
        return Switch(observable,
            sp => new TimelineNode(trueTimelineFactory(sp), sp.GetRequiredService<IScheduler>(), transitionTimeForTimelineState),
            sp => new TimelineNode(falseTimelineFactory(sp), sp.GetRequiredService<IScheduler>(), transitionTimeForTimelineState));
    }

    /// <inheritdoc/>
    public ILightTransitionPipelineConfigurator<TLight> Switch<TObservable>(Action<ITimelineConfigurator> trueConfigure,
        Action<ITimelineConfigurator> falseConfigure) where TObservable : IObservable<bool>
    {
        var observable = ActivatorUtilities.CreateInstance<TObservable>(ServiceProvider);
        return Switch(observable, trueConfigure, falseConfigure);
    }

    /// <inheritdoc/>
    public ILightTransitionPipelineConfigurator<TLight> Switch(IObservable<bool> observable,
        Action<ITimelineConfigurator> trueConfigure, Action<ITimelineConfigurator> falseConfigure)
    {
        var trueConfigurator = new TimelineConfigurator();
        trueConfigure(trueConfigurator);
        var falseConfigurator = new TimelineConfigurator();
        falseConfigure(falseConfigurator);
        return Switch(observable, trueConfigurator.TimelineFactory, falseConfigurator.TimelineFactory,
            trueConfigurator.TransitionTime ?? falseConfigurator.TransitionTime);
    }
}
