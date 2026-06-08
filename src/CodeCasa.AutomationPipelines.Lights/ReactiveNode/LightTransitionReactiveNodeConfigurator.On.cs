using CodeCasa.AutomationPipelines.Lights.Extensions;
using CodeCasa.AutomationPipelines.Lights.Nodes;
using CodeCasa.AutomationPipelines.Lights.Pipeline;
using CodeCasa.AutomationPipelines.Lights.Timeline;
using CodeCasa.Lights;
using Microsoft.Extensions.DependencyInjection;
using Occurify;
using System.Reactive.Concurrency;
using System.Reactive.Linq;

namespace CodeCasa.AutomationPipelines.Lights.ReactiveNode;

internal partial class LightTransitionReactiveNodeConfigurator<TLight>
{
    /// <inheritdoc/>
    public ILightTransitionReactiveNodeConfigurator<TLight> On<T>(IObservable<T> triggerObservable,
        LightParameters lightParameters) => On(triggerObservable, lightParameters.AsTransition());

    /// <inheritdoc/>
    public ILightTransitionReactiveNodeConfigurator<TLight> On<T>(IObservable<T> triggerObservable,
        Func<IServiceProvider, LightParameters?> lightParametersFactory)
        => On(triggerObservable, sp => lightParametersFactory(sp)?.AsTransition());

    /// <inheritdoc/>
    public ILightTransitionReactiveNodeConfigurator<TLight> On<T>(IObservable<T> triggerObservable,
        LightTransition lightTransition) =>
        On(triggerObservable, sp => new StaticLightTransitionNode(lightTransition, sp.GetRequiredService<IScheduler>()));

    /// <inheritdoc/>
    public ILightTransitionReactiveNodeConfigurator<TLight> On<T>(IObservable<T> triggerObservable,
        Func<IServiceProvider, LightTransition?> lightTransitionFactory)
        => On(triggerObservable, sp => new StaticLightTransitionNode(lightTransitionFactory(sp), sp.GetRequiredService<IScheduler>()));

    /// <inheritdoc/>
    public ILightTransitionReactiveNodeConfigurator<TLight> On<T, TNode>(IObservable<T> triggerObservable)
        where TNode : IPipelineNode<LightTransition> =>
        AddNodeSource(triggerObservable.Select(_ =>
            new Func<IServiceProvider, IPipelineNode<LightTransition>?>(sp =>
                ActivatorUtilities.CreateInstance<TNode>(sp))));

    /// <inheritdoc/>
    public ILightTransitionReactiveNodeConfigurator<TLight> On<T>(IObservable<T> triggerObservable,
        Func<IServiceProvider, IPipelineNode<LightTransition>> nodeFactory) =>
        AddNodeSource(triggerObservable.Select(_ => nodeFactory));

    /// <inheritdoc/>
    public ILightTransitionReactiveNodeConfigurator<TLight> On<T>(IObservable<T> triggerObservable, Action<ILightTransitionPipelineConfigurator<TLight>> pipelineConfigurator, InstantiationScope _ = InstantiationScope.Shared)
    {
        return On(triggerObservable,
            sp => sp.GetRequiredService<LightPipelineFactory>().CreateLightPipeline(ServiceProvider, Light, pipelineConfigurator.ApplyHierarchySettings(HierarchyPath, LoggingEnabled ?? false)));
    }

    /// <inheritdoc/>
    public ILightTransitionReactiveNodeConfigurator<TLight> On<T>(IObservable<T> triggerObservable, Action<ILightTransitionReactiveNodeConfigurator<TLight>> configure, InstantiationScope _ = InstantiationScope.Shared)
    {
        return On(triggerObservable,
            s => s.GetRequiredService<ReactiveNodeFactory>().CreateReactiveNode(s, Light, configure.ApplyHierarchySettings(HierarchyPath, LoggingEnabled ?? false)));
    }

    /// <inheritdoc/>
    public ILightTransitionReactiveNodeConfigurator<TLight> PassThroughOn<T>(IObservable<T> triggerObservable)
    {
        AddNodeSource(triggerObservable.Select(_ => new PassThroughNode<LightTransition>()));
        return this;
    }

    /// <inheritdoc/>
    public ILightTransitionReactiveNodeConfigurator<TLight> TurnOffWhen<T>(IObservable<T> triggerObservable)
    {
        AddNodeSource(triggerObservable.Select(_ => new TurnOffThenPassThroughNode()));
        return this;
    }

    /// <inheritdoc/>
    public ILightTransitionReactiveNodeConfigurator<TLight> TurnOnWhen<T>(IObservable<T> triggerObservable)
    {
        return On(triggerObservable, LightTransition.On());
    }

    /// <inheritdoc/>
    public ILightTransitionReactiveNodeConfigurator<TLight> On<T>(IObservable<T> triggerObservable,
        Dictionary<ITimeline, LightParameters> timeline, TimeSpan? transitionTimeForTimelineState = null)
        => On(triggerObservable, sp => new TimelineNode(timeline, sp.GetRequiredService<IScheduler>(), transitionTimeForTimelineState));

    /// <inheritdoc/>
    public ILightTransitionReactiveNodeConfigurator<TLight> On<T>(IObservable<T> triggerObservable,
        Func<IServiceProvider, Dictionary<ITimeline, LightParameters>> timelineFactory,
        TimeSpan? transitionTimeForTimelineState = null)
        => On(triggerObservable, sp => new TimelineNode(timelineFactory(sp), sp.GetRequiredService<IScheduler>(), transitionTimeForTimelineState));

    /// <inheritdoc/>
    public ILightTransitionReactiveNodeConfigurator<TLight> On<T>(IObservable<T> triggerObservable,
        Action<ITimelineConfigurator> configure)
    {
        var configurator = new TimelineConfigurator();
        configure(configurator);
        return On(triggerObservable, configurator.TimelineFactory, configurator.TransitionTime);
    }
}
