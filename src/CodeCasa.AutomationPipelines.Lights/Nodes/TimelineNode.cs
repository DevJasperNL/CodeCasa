using System.Reactive.Concurrency;
using CodeCasa.Lights;
using CodeCasa.Lights.Timelines.Extensions;
using Occurify;

namespace CodeCasa.AutomationPipelines.Lights.Nodes;

/// <summary>
/// A pipeline node that drives its output from a time-based timeline.
/// The output follows the observable produced by the timeline dictionary and updates automatically as time progresses.
/// </summary>
internal class TimelineNode : LightTransitionNode
{
    private readonly IDisposable _subscription;

    /// <summary>
    /// Initializes a new instance of the <see cref="TimelineNode"/> class.
    /// </summary>
    /// <param name="timeline">The dictionary mapping timeline points to <see cref="LightParameters"/>.</param>
    /// <param name="scheduler">The Rx scheduler used to manage timing.</param>
    /// <param name="transitionTimeForTimelineState">
    /// The duration of the initial fade from the current state. Defaults to 400ms if null.
    /// </param>
    public TimelineNode(
        Dictionary<ITimeline, LightParameters> timeline,
        IScheduler scheduler,
        TimeSpan? transitionTimeForTimelineState = null) : base(scheduler)
    {
        Name = "Timeline Node";
        _subscription = timeline
            .ToLightTransitionObservableIncludingCurrent(scheduler, transitionTimeForTimelineState)
            .Subscribe(transition =>
            {
                Output = transition;
            });
    }

    /// <inheritdoc />
    public override async ValueTask DisposeAsync()
    {
        _subscription.Dispose();
        await base.DisposeAsync();
    }
}
