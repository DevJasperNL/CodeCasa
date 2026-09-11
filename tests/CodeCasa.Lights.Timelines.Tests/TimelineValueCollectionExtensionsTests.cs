using CodeCasa.Lights.Timelines.Extensions;
using Microsoft.Reactive.Testing;
using Occurify;
using Occurify.Extensions;

namespace CodeCasa.Lights.Timelines.Tests;

[TestClass]
public sealed class TimelineValueCollectionExtensionsTests
{
    private static readonly TimeSpan DefaultTransition = TimeSpan.FromMilliseconds(400);

    private static Dictionary<ITimeline, LightParameters> CreateTimelineAroundNow(DateTime utcNow, LightParameters previous, LightParameters next)
    {
        return new Dictionary<ITimeline, LightParameters>
        {
            { (utcNow - TimeSpan.FromHours(1)).AsTimeline(), previous },
            { (utcNow + TimeSpan.FromHours(1)).AsTimeline(), next }
        };
    }

    [TestMethod]
    public void IncludingCurrent_EmitsInterpolatedValueFirst()
    {
        var scheduler = new TestScheduler();
        var timeline = CreateTimelineAroundNow(DateTime.UtcNow, new LightParameters { Brightness = 100 }, new LightParameters { Brightness = 200 });
        var emitted = new List<LightTransition>();

        timeline.ToLightTransitionObservableIncludingCurrent(scheduler).Subscribe(emitted.Add);

        Assert.HasCount(1, emitted);
        Assert.AreEqual(DefaultTransition, emitted[0].TransitionTime);
        Assert.IsTrue(emitted[0].LightParameters.Brightness is >= 149 and <= 151, $"Expected ~150 but was {emitted[0].LightParameters.Brightness}");
    }

    [TestMethod]
    public void IncludingCurrent_AfterInitialFade_ResumesRampTowardsNextInstant()
    {
        var scheduler = new TestScheduler();
        var timeline = CreateTimelineAroundNow(DateTime.UtcNow, new LightParameters { Brightness = 100 }, new LightParameters { Brightness = 200 });
        var emitted = new List<LightTransition>();
        timeline.ToLightTransitionObservableIncludingCurrent(scheduler).Subscribe(emitted.Add);

        scheduler.AdvanceBy(DefaultTransition.Ticks);

        Assert.HasCount(2, emitted);
        Assert.AreEqual(200, emitted[1].LightParameters.Brightness);
        Assert.IsNotNull(emitted[1].TransitionTime);
        Assert.IsTrue(emitted[1].TransitionTime > TimeSpan.FromMinutes(59), $"Remaining ramp time should be close to an hour but was {emitted[1].TransitionTime}");
        Assert.IsTrue(emitted[1].TransitionTime < TimeSpan.FromHours(1) - DefaultTransition + TimeSpan.FromSeconds(1));
    }

    [TestMethod]
    public void IncludingCurrent_SameValueOnBothSides_DoesNotResumeRamp()
    {
        var scheduler = new TestScheduler();
        var parameters = new LightParameters { Brightness = 100 };
        var timeline = CreateTimelineAroundNow(DateTime.UtcNow, parameters, parameters);
        var emitted = new List<LightTransition>();
        timeline.ToLightTransitionObservableIncludingCurrent(scheduler).Subscribe(emitted.Add);

        scheduler.AdvanceBy(DefaultTransition.Ticks);

        Assert.HasCount(1, emitted);
        Assert.AreEqual(100, emitted[0].LightParameters.Brightness);
    }
}
