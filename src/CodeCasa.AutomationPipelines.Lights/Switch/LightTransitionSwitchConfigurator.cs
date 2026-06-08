using CodeCasa.AutomationPipelines.Lights.Nodes;
using CodeCasa.AutomationPipelines.Lights.Timeline;
using CodeCasa.Lights;
using Microsoft.Extensions.DependencyInjection;
using Occurify;
using System.Reactive.Concurrency;

namespace CodeCasa.AutomationPipelines.Lights.Switch;

internal sealed class LightTransitionSwitchConfigurator<TLight>
    : ILightTransitionSwitchConfigurator<TLight>
    where TLight : ILight
{
    internal LightTransitionSwitchFalseConfigurator<TLight>? FalseConfigurator { get; private set; }

    public ILightTransitionSwitchFalseConfigurator<TLight> WhenTrue(LightParameters lightParameters)
        => WhenTrue(_ => lightParameters.AsTransition());

    public ILightTransitionSwitchFalseConfigurator<TLight> WhenTrue(LightTransition lightTransition)
        => WhenTrue(_ => lightTransition);

    public ILightTransitionSwitchFalseConfigurator<TLight> WhenTrue(Func<IServiceProvider, LightParameters?> lightParametersFactory)
        => WhenTrue(sp => lightParametersFactory(sp)?.AsTransition());

    public ILightTransitionSwitchFalseConfigurator<TLight> WhenTrue(Func<IServiceProvider, LightTransition?> lightTransitionFactory)
        => WhenTrue(sp => new StaticLightTransitionNode(lightTransitionFactory(sp), sp.GetRequiredService<IScheduler>()));

    public ILightTransitionSwitchFalseConfigurator<TLight> WhenTrue(Func<IServiceProvider, IPipelineNode<LightTransition>> nodeFactory)
    {
        var falseConfigurator = new LightTransitionSwitchFalseConfigurator<TLight>(nodeFactory);
        FalseConfigurator = falseConfigurator;
        return falseConfigurator;
    }

    public ILightTransitionSwitchFalseConfigurator<TLight> WhenTrue<TNode>() where TNode : IPipelineNode<LightTransition>
        => WhenTrue(sp => ActivatorUtilities.CreateInstance<TNode>(sp));

    public ILightTransitionSwitchFalseConfigurator<TLight> WhenTrue(Dictionary<ITimeline, LightParameters> timeline,
        TimeSpan? transitionTimeForTimelineState = null)
        => WhenTrue(sp => new TimelineNode(timeline, sp.GetRequiredService<IScheduler>(), transitionTimeForTimelineState));

    public ILightTransitionSwitchFalseConfigurator<TLight> WhenTrue(
        Func<IServiceProvider, Dictionary<ITimeline, LightParameters>> timelineFactory,
        TimeSpan? transitionTimeForTimelineState = null)
        => WhenTrue(sp => new TimelineNode(timelineFactory(sp), sp.GetRequiredService<IScheduler>(), transitionTimeForTimelineState));

    public ILightTransitionSwitchFalseConfigurator<TLight> WhenTrue(Action<ITimelineConfigurator> configure)
    {
        var configurator = new TimelineConfigurator();
        configure(configurator);
        return WhenTrue(configurator.TimelineFactory, configurator.TransitionTime);
    }
}
