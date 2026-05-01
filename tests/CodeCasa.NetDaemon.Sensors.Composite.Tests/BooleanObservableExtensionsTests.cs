using CodeCasa.NetDaemon.Sensors.Composite.Extensions;
using Microsoft.Reactive.Testing;
using System.Reactive.Linq;

namespace CodeCasa.NetDaemon.Sensors.Composite.Tests
{
    [TestClass]
    public class BooleanObservableExtensionsTests : ReactiveTest
    {
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
