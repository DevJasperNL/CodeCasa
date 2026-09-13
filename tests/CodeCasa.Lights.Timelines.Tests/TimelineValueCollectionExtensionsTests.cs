using CodeCasa.Lights.Timelines.Extensions;
using Microsoft.Reactive.Testing;
using Occurify;
using Occurify.Extensions;

namespace CodeCasa.Lights.Timelines.Tests;

[TestClass]
public sealed class TimelineValueCollectionExtensionsTests
{
    private static readonly DateTime Now = new(2026, 1, 1, 21, 0, 0, DateTimeKind.Utc);
    private static readonly TimeSpan Fade = TimeSpan.FromMilliseconds(400);

    private static TestScheduler CreateScheduler(DateTime at)
    {
        var scheduler = new TestScheduler();
        scheduler.AdvanceTo(at.Ticks);
        return scheduler;
    }

    private static DateTime At(int hour, int minute = 0, int second = 0, int millisecond = 0) =>
        new(2026, 1, 1, hour, minute, second, millisecond, DateTimeKind.Utc);

    private static LightParameters Brightness(double brightness) => new() { Brightness = brightness };

    private static Dictionary<ITimeline, LightParameters> Timeline(params (DateTime Instant, LightParameters Parameters)[] points) =>
        points.ToDictionary(p => p.Instant.AsTimeline(), p => p.Parameters);

    private sealed record Emission(LightTransition Transition, DateTime EmittedAt);

    private static List<Emission> Subscribe(Dictionary<ITimeline, LightParameters> timeline, TestScheduler scheduler)
    {
        var emitted = new List<Emission>();
        timeline.ToLightTransitionObservableIncludingCurrent(scheduler).Subscribe(t => emitted.Add(new Emission(t, scheduler.Now.UtcDateTime)));
        return emitted;
    }

    private static void AssertTransition(Emission emission, double brightness, TimeSpan transitionTime) =>
        AssertTransition(emission.Transition, brightness, transitionTime);

    private static void AssertTransition(LightTransition transition, double brightness, TimeSpan transitionTime)
    {
        Assert.AreEqual(brightness, transition.LightParameters.Brightness);
        Assert.AreEqual(transitionTime, transition.TransitionTime);
    }

    [TestMethod]
    public void IncludingCurrent_EmitsInterpolatedValueFirst()
    {
        var scheduler = CreateScheduler(Now);
        var emitted = Subscribe(Timeline((At(20), Brightness(100)), (At(22), Brightness(200))), scheduler);

        Assert.HasCount(1, emitted);
        AssertTransition(emitted[0], 150, Fade);
    }

    [TestMethod]
    public void IncludingCurrent_AfterInitialFade_ResumesRampTowardsNextInstant()
    {
        var scheduler = CreateScheduler(Now);
        var emitted = Subscribe(Timeline((At(20), Brightness(100)), (At(22), Brightness(200))), scheduler);

        scheduler.AdvanceBy(Fade.Ticks);

        Assert.HasCount(2, emitted);
        AssertTransition(emitted[1], 200, TimeSpan.FromHours(1) - Fade);
    }

    [TestMethod]
    public void IncludingCurrent_SameValueOnBothSides_DoesNotResumeRamp()
    {
        var scheduler = CreateScheduler(Now);
        var emitted = Subscribe(Timeline((At(20), Brightness(100)), (At(22), Brightness(100))), scheduler);

        scheduler.AdvanceBy(Fade.Ticks);

        Assert.HasCount(1, emitted);
        AssertTransition(emitted[0], 100, Fade);
    }

    [TestMethod]
    public void IncludingCurrent_AtNextInstant_EmitsRampTowardsFollowingInstant()
    {
        var scheduler = CreateScheduler(Now);
        var emitted = Subscribe(Timeline((At(20), Brightness(100)), (At(22), Brightness(200)), (At(23), Brightness(50))), scheduler);

        scheduler.AdvanceTo(At(22).Ticks);

        Assert.HasCount(3, emitted);
        AssertTransition(emitted[2], 50, TimeSpan.FromHours(1));
    }

    [TestMethod]
    public void IncludingCurrent_InstantInsideFadeWindow_IsNotSkipped()
    {
        var scheduler = CreateScheduler(Now);
        var emitted = Subscribe(Timeline((At(20), Brightness(100)), (At(21, millisecond: 100), Brightness(200)), (At(22), Brightness(50))), scheduler);

        Assert.HasCount(1, emitted);
        AssertTransition(emitted[0], 200, Fade);

        // The TestScheduler runs work scheduled from within a callback a tick later, hence the extra millisecond.
        scheduler.AdvanceBy(Fade.Ticks + TimeSpan.FromMilliseconds(1).Ticks);

        Assert.HasCount(2, emitted);
        Assert.IsTrue(emitted[1].EmittedAt >= Now + Fade, $"Ramp was emitted before the fade ended, at {emitted[1].EmittedAt:O}");
        AssertTransition(emitted[1], 50, At(22) - emitted[1].EmittedAt);
    }

    [TestMethod]
    public void IncludingCurrent_FinalInstantInsideFadeWindow_IsApplied()
    {
        var scheduler = CreateScheduler(Now);
        var emitted = Subscribe(Timeline((At(20, 59, 59, 900), Brightness(100)), (At(21, millisecond: 100), Brightness(200))), scheduler);

        Assert.HasCount(1, emitted);
        AssertTransition(emitted[0], 200, Fade);

        scheduler.AdvanceBy(TimeSpan.FromHours(1).Ticks);

        Assert.HasCount(1, emitted);
    }

    [TestMethod]
    public void IncludingCurrent_SubscribedExactlyOnInstant_RampsTowardsFollowingInstantOnce()
    {
        var scheduler = CreateScheduler(Now);
        var emitted = Subscribe(Timeline((At(20), Brightness(100)), (At(21), Brightness(200)), (At(22), Brightness(50))), scheduler);

        Assert.HasCount(1, emitted);
        AssertTransition(emitted[0], 200, Fade);

        scheduler.AdvanceBy(Fade.Ticks);

        Assert.HasCount(2, emitted);
        AssertTransition(emitted[1], 50, TimeSpan.FromHours(1) - Fade);

        scheduler.AdvanceTo(At(23).Ticks);

        Assert.HasCount(2, emitted);
    }

    [TestMethod]
    public void IncludingCurrent_LateSubscription_UsesSubscriptionTime()
    {
        var scheduler = CreateScheduler(Now);
        var observable = Timeline((At(20), Brightness(100)), (At(22), Brightness(200))).ToLightTransitionObservableIncludingCurrent(scheduler);
        var emitted = new List<Emission>();

        scheduler.AdvanceTo(At(21, 30).Ticks);
        observable.Subscribe(t => emitted.Add(new Emission(t, scheduler.Now.UtcDateTime)));

        Assert.HasCount(1, emitted);
        AssertTransition(emitted[0], 175, Fade);
    }

    [TestMethod]
    public void IncludingCurrent_TimelineNotStarted_AppliesFirstValueAtFirstInstant()
    {
        var scheduler = CreateScheduler(Now);
        var emitted = Subscribe(Timeline((At(22), Brightness(100)), (At(23), Brightness(200))), scheduler);

        Assert.IsEmpty(emitted);

        scheduler.AdvanceTo(At(22).Ticks);

        Assert.HasCount(1, emitted);
        AssertTransition(emitted[0], 100, Fade);

        scheduler.AdvanceBy(Fade.Ticks);

        Assert.HasCount(2, emitted);
        AssertTransition(emitted[1], 200, TimeSpan.FromHours(1) - Fade);
    }

    [TestMethod]
    public void IncludingCurrent_TimelineEnded_HoldsLastValue()
    {
        var scheduler = CreateScheduler(Now);
        var emitted = Subscribe(Timeline((At(19), Brightness(100)), (At(20), Brightness(200))), scheduler);

        scheduler.AdvanceBy(TimeSpan.FromHours(2).Ticks);

        Assert.HasCount(1, emitted);
        AssertTransition(emitted[0], 200, Fade);
    }

    [TestMethod]
    public void IncludingCurrent_EmptyTimeline_EmitsNothing()
    {
        var scheduler = CreateScheduler(Now);
        var emitted = Subscribe(new Dictionary<ITimeline, LightParameters>(), scheduler);

        scheduler.AdvanceBy(TimeSpan.FromHours(2).Ticks);

        Assert.IsEmpty(emitted);
    }

    [TestMethod]
    public void SceneIncludingCurrent_LightsPresentOnOneSideOnly_AreKept()
    {
        var scheduler = CreateScheduler(Now);
        var timeline = new Dictionary<ITimeline, Dictionary<string, LightParameters>>
        {
            { At(20).AsTimeline(), new Dictionary<string, LightParameters> { { "a", Brightness(100) }, { "b", Brightness(100) } } },
            { At(22).AsTimeline(), new Dictionary<string, LightParameters> { { "a", Brightness(200) }, { "c", Brightness(50) } } }
        };
        var emitted = new List<Dictionary<string, LightTransition>>();

        timeline.ToLightTransitionSceneObservableIncludingCurrent(scheduler).Subscribe(emitted.Add);

        Assert.HasCount(1, emitted);
        Assert.HasCount(3, emitted[0]);
        AssertTransition(emitted[0]["a"], 150, Fade);
        AssertTransition(emitted[0]["b"], 100, Fade);
        AssertTransition(emitted[0]["c"], 50, Fade);
    }

    [TestMethod]
    public void DictionaryComparer_EqualDictionariesWithDifferentInsertionOrder_HaveEqualHashCodes()
    {
        var comparer = new DictionaryComparer<string, LightParameters>();
        var x = new Dictionary<string, LightParameters> { { "a", Brightness(1) }, { "b", Brightness(2) } };
        var y = new Dictionary<string, LightParameters> { { "b", Brightness(2) }, { "a", Brightness(1) } };

        Assert.IsTrue(comparer.Equals(x, y));
        Assert.AreEqual(comparer.GetHashCode(x), comparer.GetHashCode(y));
    }
}
