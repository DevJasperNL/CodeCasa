using CodeCasa.AutomationPipelines.Lights.Nodes;
using CodeCasa.AutomationPipelines.Lights.ReactiveNode;
using CodeCasa.AutomationPipelines.Lights.Timeline;
using CodeCasa.Lights;
using Microsoft.Extensions.DependencyInjection;
using Occurify;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using CodeCasa.AutomationPipelines.Lights.Extensions;

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
        Func<IServiceProvider, LightParameters?> lightParametersFactory) where TObservable : IObservable<bool>
    {
        return When<TObservable>(c => lightParametersFactory(c)?.AsTransition());
    }

    /// <inheritdoc/>
    public ILightTransitionPipelineConfigurator<TLight> When(IObservable<bool> observable,
        Func<IServiceProvider, LightParameters?> lightParametersFactory)
    {
        return When(observable, c => lightParametersFactory(c)?.AsTransition());
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
        Func<IServiceProvider, LightTransition?> lightTransitionFactory) where TObservable : IObservable<bool>
    {
        return When<TObservable>(sp => new StaticLightTransitionNode(lightTransitionFactory(sp), sp.GetRequiredService<IScheduler>()));
    }

    /// <inheritdoc/>
    public ILightTransitionPipelineConfigurator<TLight> When(IObservable<bool> observable,
        Func<IServiceProvider, LightTransition?> lightTransitionFactory)
    {
        return When(observable, sp => new StaticLightTransitionNode(lightTransitionFactory(sp), sp.GetRequiredService<IScheduler>()));
    }

    /// <inheritdoc/>
    public ILightTransitionPipelineConfigurator<TLight> When<TObservable>(
        Func<IServiceProvider, IPipelineNode<LightTransition>> nodeFactory) where TObservable : IObservable<bool>
    {
        var observable = ActivatorUtilities.CreateInstance<TObservable>(ServiceProvider);
        return When(observable, nodeFactory);
    }

    /// <inheritdoc/>
    public ILightTransitionPipelineConfigurator<TLight> When(IObservable<bool> observable,
        Func<IServiceProvider, IPipelineNode<LightTransition>> nodeFactory)
    {
        return AddReactiveNode(c => c
            .SetHierarchyContext(HierarchyPath, "Condition", LoggingEnabled ?? false)
            .On(observable.Where(x => x), nodeFactory)
            .PassThroughOn(observable.Where(x => !x)));
    }

    /// <inheritdoc/>
    public ILightTransitionPipelineConfigurator<TLight> When<TObservable, TNode>()
        where TObservable : IObservable<bool>
        where TNode : IPipelineNode<LightTransition>
    {
        var observable = ActivatorUtilities.CreateInstance<TObservable>(ServiceProvider);
        return When<TNode>(observable);
    }

    /// <inheritdoc/>
    public ILightTransitionPipelineConfigurator<TLight> When<TNode>(IObservable<bool> observable)
        where TNode : IPipelineNode<LightTransition>
    {
        return AddReactiveNode(c => c
            .SetHierarchyContext(HierarchyPath, "Condition", LoggingEnabled ?? false)
            .On<bool, TNode>(observable.Where(x => x))
            .PassThroughOn(observable.Where(x => !x)));
    }

    /// <inheritdoc/>
    public ILightTransitionPipelineConfigurator<TLight> AddReactiveNodeWhen<TObservable>(Action<ILightTransitionReactiveNodeConfigurator<TLight>> configure, InstantiationScope instantiationScope = InstantiationScope.Shared) where TObservable : IObservable<bool>
    {
        var observable = ActivatorUtilities.CreateInstance<TObservable>(ServiceProvider);
        return AddReactiveNodeWhen(observable, configure, instantiationScope);
    }

    /// <inheritdoc/>
    public ILightTransitionPipelineConfigurator<TLight> AddReactiveNodeWhen(IObservable<bool> observable, Action<ILightTransitionReactiveNodeConfigurator<TLight>> configure, InstantiationScope instantiationScope = InstantiationScope.Shared)
    {
        return AddReactiveNode(sp => sp
            .SetHierarchyContext(HierarchyPath, "Conditional Reactive Node", LoggingEnabled ?? false)
            .On(observable.Where(x => x), configure.ApplyHierarchySettings(sp), instantiationScope)
            .PassThroughOn(observable.Where(x => !x)));
    }

    /// <inheritdoc/>
    public ILightTransitionPipelineConfigurator<TLight> AddPipelineWhen<TObservable>(Action<ILightTransitionPipelineConfigurator<TLight>> pipelineConfigurator, InstantiationScope instantiationScope = InstantiationScope.Shared) where TObservable : IObservable<bool>
    {
        return When<TObservable>(sp =>
            sp.GetRequiredService<LightPipelineFactory>()
                .CreateLightPipeline(sp, sp.GetRequiredService<TLight>(), pipelineConfigurator.ApplyHierarchySettings($"{HierarchyPath}->Condition", LoggingEnabled ?? false)));
    }

    /// <inheritdoc/>
    public ILightTransitionPipelineConfigurator<TLight> AddPipelineWhen(IObservable<bool> observable, Action<ILightTransitionPipelineConfigurator<TLight>> pipelineConfigurator, InstantiationScope instantiationScope = InstantiationScope.Shared)
    {
        return When(observable, sp =>
            sp.GetRequiredService<LightPipelineFactory>()
                .CreateLightPipeline(sp, sp.GetRequiredService<TLight>(), pipelineConfigurator.ApplyHierarchySettings($"{HierarchyPath}->Condition", LoggingEnabled ?? false)));
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

    /// <inheritdoc/>
    public ILightTransitionPipelineConfigurator<TLight> When<TObservable>(Dictionary<ITimeline, LightParameters> timeline,
        TimeSpan? transitionTimeForTimelineState = null) where TObservable : IObservable<bool>
    {
        var observable = ActivatorUtilities.CreateInstance<TObservable>(ServiceProvider);
        return When(observable, timeline, transitionTimeForTimelineState);
    }

    /// <inheritdoc/>
    public ILightTransitionPipelineConfigurator<TLight> When(IObservable<bool> observable,
        Dictionary<ITimeline, LightParameters> timeline, TimeSpan? transitionTimeForTimelineState = null)
    {
        return When(observable, sp => new TimelineNode(timeline, sp.GetRequiredService<IScheduler>(), transitionTimeForTimelineState));
    }

    /// <inheritdoc/>
    public ILightTransitionPipelineConfigurator<TLight> When<TObservable>(
        Func<IServiceProvider, Dictionary<ITimeline, LightParameters>> timelineFactory,
        TimeSpan? transitionTimeForTimelineState = null) where TObservable : IObservable<bool>
    {
        var observable = ActivatorUtilities.CreateInstance<TObservable>(ServiceProvider);
        return When(observable, timelineFactory, transitionTimeForTimelineState);
    }

    /// <inheritdoc/>
    public ILightTransitionPipelineConfigurator<TLight> When(IObservable<bool> observable,
        Func<IServiceProvider, Dictionary<ITimeline, LightParameters>> timelineFactory,
        TimeSpan? transitionTimeForTimelineState = null)
    {
        return When(observable, sp => new TimelineNode(timelineFactory(sp), sp.GetRequiredService<IScheduler>(), transitionTimeForTimelineState));
    }

    /// <inheritdoc/>
    public ILightTransitionPipelineConfigurator<TLight> When<TObservable>(Action<ITimelineConfigurator> configure)
        where TObservable : IObservable<bool>
    {
        var configurator = new TimelineConfigurator();
        configure(configurator);
        return When<TObservable>(configurator.TimelineFactory, configurator.TransitionTime);
    }

    /// <inheritdoc/>
    public ILightTransitionPipelineConfigurator<TLight> When(IObservable<bool> observable,
        Action<ITimelineConfigurator> configure)
    {
        var configurator = new TimelineConfigurator();
        configure(configurator);
        return When(observable, configurator.TimelineFactory, configurator.TransitionTime);
    }
}
