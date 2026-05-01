using CodeCasa.NetDaemon.Sensors.Composite.Extensions;
using Microsoft.Reactive.Testing;
using System.Reactive.Linq;

namespace CodeCasa.NetDaemon.Sensors.Composite.Tests
{
    [TestClass]
    public class BooleanObservableExtensionsTests : ReactiveTest
    {
        [TestMethod]
        public void CombineWithBrightness_ShouldEmitTrue_WhenMotionAndBrightnessAreBothTrue()
        {
            var scheduler = new TestScheduler();

            // Motion: true at Subscribed + 10
            // Brightness: true (below threshold) from the start
            var motion = scheduler.CreateHotObservable(
                OnNext(Subscribed + 10, true)
            );
            var brightness = scheduler.CreateHotObservable<bool>();

            var res = scheduler.Start(() => motion.StartWith(false).CombineWithBrightness(brightness.StartWith(true)));

            res.Messages.AssertEqual(
                OnNext(Subscribed, false),
                OnNext(Subscribed + 10, true)
            );
        }

        [TestMethod]
        public void CombineWithBrightness_ShouldNotEmitTrue_WhenMotionTrueButBrightnessAboveThreshold()
        {
            var scheduler = new TestScheduler();

            // Motion: true at Subscribed + 10
            // Brightness: false (above threshold) from the start
            var motion = scheduler.CreateHotObservable(
                OnNext(Subscribed + 10, true)
            );
            var brightness = scheduler.CreateHotObservable<bool>();

            var res = scheduler.Start(() => motion.StartWith(false).CombineWithBrightness(brightness.StartWith(false)));

            // (true, false): motion triggered but brightness too high, latch stays false and emits false
            res.Messages.AssertEqual(
                OnNext(Subscribed, false),
                OnNext(Subscribed + 10, false)
            );
        }

        [TestMethod]
        public void CombineWithBrightness_ShouldEmitFalse_WhenMotionClears()
        {
            var scheduler = new TestScheduler();

            // Motion: true at Subscribed + 10, false at Subscribed + 50
            // Brightness: always true (below threshold)
            var motion = scheduler.CreateHotObservable(
                OnNext(Subscribed + 10, true),
                OnNext(Subscribed + 50, false)
            );
            var brightness = scheduler.CreateHotObservable<bool>();

            var res = scheduler.Start(() => motion.StartWith(false).CombineWithBrightness(brightness.StartWith(true)));

            res.Messages.AssertEqual(
                OnNext(Subscribed, false),
                OnNext(Subscribed + 10, true),
                OnNext(Subscribed + 50, false)
            );
        }

        [TestMethod]
        public void CombineWithBrightness_ShouldStayTrue_WhenBrightnessIncreasesAfterTrigger()
        {
            var scheduler = new TestScheduler();

            // Motion: true at Subscribed + 10, false at Subscribed + 80
            // Brightness: true initially, flips to false at Subscribed + 30 (light came on)
            var motion = scheduler.CreateHotObservable(
                OnNext(Subscribed + 10, true),
                OnNext(Subscribed + 80, false)
            );
            var brightness = scheduler.CreateHotObservable(
                OnNext(Subscribed + 30, false)
            );

            var res = scheduler.Start(() => motion.StartWith(false).CombineWithBrightness(brightness.StartWith(true)));

            // Triggered at +10, brightness change at +30 re-emits the latched true (does not reset), only motion clearing at +80 resets
            res.Messages.AssertEqual(
                OnNext(Subscribed, false),
                OnNext(Subscribed + 10, true),
                OnNext(Subscribed + 30, true),
                OnNext(Subscribed + 80, false)
            );
        }

        [TestMethod]
        public void CombineWithBrightness_ShouldNotEmitDuplicateFalse_WhenNotYetTriggered()
        {
            var scheduler = new TestScheduler();

            // Motion stays false; brightness changes several times — no true should ever be emitted
            var motion = scheduler.CreateHotObservable<bool>();
            var brightness = scheduler.CreateHotObservable(
                OnNext(Subscribed + 20, true),
                OnNext(Subscribed + 40, false)
            );

            var res = scheduler.Start(() => motion.StartWith(false).CombineWithBrightness(brightness.StartWith(false)));

            // No emissions at all: triggered was never set so false is suppressed
            res.Messages.AssertEqual(
                OnNext(Subscribed, false)
            );
        }

        [TestMethod]
        public void CombineWithBrightness_ShouldTriggerAgain_AfterMotionClearsAndRetriggers()
        {
            var scheduler = new TestScheduler();

            // First trigger cycle: motion true at +10, false at +50
            // Second trigger cycle: motion true at +100, false at +150
            // Brightness always below threshold
            var motion = scheduler.CreateHotObservable(
                OnNext(Subscribed + 10, true),
                OnNext(Subscribed + 50, false),
                OnNext(Subscribed + 100, true),
                OnNext(Subscribed + 150, false)
            );
            var brightness = scheduler.CreateHotObservable<bool>();

            var res = scheduler.Start(() => motion.StartWith(false).CombineWithBrightness(brightness.StartWith(true)));

            res.Messages.AssertEqual(
                OnNext(Subscribed, false),
                OnNext(Subscribed + 10, true),
                OnNext(Subscribed + 50, false),
                OnNext(Subscribed + 100, true),
                OnNext(Subscribed + 150, false)
            );
        }

        [TestMethod]
        public void PersistTrue_ShouldDelayFalse_WhenEmittedBeforePersistentTimeout()
        {
            var scheduler = new TestScheduler();
            var persistentFor = TimeSpan.FromTicks(100);

            // Input: False at Subscribed (initial, simulates BehaviorSubject), True at Subscribed + 10, False at Subscribed + 50 (before persistentFor)
            var source = scheduler.CreateHotObservable(
                OnNext(Subscribed + 10, true),
                OnNext(Subscribed + 50, false)
            );

            var res = scheduler.Start(() => source.StartWith(false).PersistTrue(persistentFor, scheduler));

            // Expected: False at Subscribed (initial), True at Subscribed + 10, False at Subscribed + 110 (Subscribed + 10 + persistentFor)
            res.Messages.AssertEqual(
                OnNext(Subscribed, false),
                OnNext(Subscribed + 10, true),
                OnNext(Subscribed + 110, false)
            );
        }

        [TestMethod]
        public void PersistTrue_ShouldReset_WhenNewTrueEmittedDuringDelay()
        {
            var scheduler = new TestScheduler();
            var persistentFor = TimeSpan.FromTicks(100);

            // Input: False at Subscribed (initial, simulates BehaviorSubject), True at Subscribed + 10, False at Subscribed + 50, True at Subscribed + 80 (resets window), False at Subscribed + 90
            var source = scheduler.CreateHotObservable(
                OnNext(Subscribed + 10, true),
                OnNext(Subscribed + 50, false),
                OnNext(Subscribed + 80, true),
                OnNext(Subscribed + 90, false)
            );

            var res = scheduler.Start(() => source.StartWith(false).PersistTrue(persistentFor, scheduler));

            // Expected: False at Subscribed (initial), True at Subscribed + 10, True at Subscribed + 80, False at Subscribed + 180 (Subscribed + 80 + persistentFor)
            res.Messages.AssertEqual(
                OnNext(Subscribed, false),
                OnNext(Subscribed + 10, true),
                OnNext(Subscribed + 80, true),
                OnNext(Subscribed + 180, false)
            );
        }

        [TestMethod]
        public void PersistTrue_ShouldEmitFalseImmediately_IfAfterPersistentTimeout()
        {
            var scheduler = new TestScheduler();
            var persistentFor = TimeSpan.FromTicks(100);

            // Input: False at Subscribed (initial, simulates BehaviorSubject), True at Subscribed + 10, False at Subscribed + 150 (after persistentFor has elapsed)
            var source = scheduler.CreateHotObservable(
                OnNext(Subscribed + 10, true),
                OnNext(Subscribed + 150, false)
            );

            var res = scheduler.Start(() => source.StartWith(false).PersistTrue(persistentFor, scheduler));

            // Expected: False at Subscribed (initial), True at Subscribed + 10, False at Subscribed + 150 (immediate, persistentFor already elapsed)
            res.Messages.AssertEqual(
                OnNext(Subscribed, false),
                OnNext(Subscribed + 10, true),
                OnNext(Subscribed + 150, false)
            );
        }

        [TestMethod]
        public void PersistTrue_ShouldEmitFalseImmediately_IfAfterPersistentTimeout_AfterReset()
        {
            var scheduler = new TestScheduler();
            var persistentFor = TimeSpan.FromTicks(100);

            // Input: False at Subscribed (initial, simulates BehaviorSubject), True at Subscribed + 10, False at Subscribed + 50, True at Subscribed + 80 (resets window), False at Subscribed + 90
            var source = scheduler.CreateHotObservable(
                OnNext(Subscribed + 10, true),
                OnNext(Subscribed + 50, false),
                OnNext(Subscribed + 80, true),
                OnNext(Subscribed + 230, false)
            );

            var res = scheduler.Start(() => source.StartWith(false).PersistTrue(persistentFor, scheduler));

            // Expected: False at Subscribed (initial), True at Subscribed + 10, True at Subscribed + 80, False at Subscribed + 180 (Subscribed + 80 + persistentFor)
            res.Messages.AssertEqual(
                OnNext(Subscribed, false),
                OnNext(Subscribed + 10, true),
                OnNext(Subscribed + 80, true),
                OnNext(Subscribed + 230, false)
            );
        }
    }
}
