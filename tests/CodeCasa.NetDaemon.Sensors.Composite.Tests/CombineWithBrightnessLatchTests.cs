using CodeCasa.NetDaemon.Sensors.Composite.Extensions;
using Microsoft.Reactive.Testing;
using System.Reactive.Linq;

namespace CodeCasa.NetDaemon.Sensors.Composite.Tests;

[TestClass]
public class CombineWithBrightnessLatchTests : ReactiveTest
{
    [TestMethod]
    public void CombineWithBrightness_BrightnessFlipsWhileLatched_EmitsTrueOnce()
    {
        var scheduler = new TestScheduler();

        // Motion latches at +10; the lights turning on raise the illuminance at +20 and it drops again at +30.
        var motion = scheduler.CreateHotObservable(
            OnNext(Subscribed + 10, true),
            OnNext(Subscribed + 50, false)
        );
        var brightness = scheduler.CreateHotObservable(
            OnNext(Subscribed + 20, false),
            OnNext(Subscribed + 30, true)
        );

        var res = scheduler.Start(() => motion.StartWith(false).CombineWithBrightness(brightness.StartWith(true)));

        res.Messages.AssertEqual(
            OnNext(Subscribed, false),
            OnNext(Subscribed + 10, true),
            OnNext(Subscribed + 50, false)
        );
    }

    [TestMethod]
    public void CombineWithBrightness_RepeatedMotionWhileLatched_EmitsTrueOnce()
    {
        var scheduler = new TestScheduler();

        var motion = scheduler.CreateHotObservable(
            OnNext(Subscribed + 10, true),
            OnNext(Subscribed + 20, true),
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
}
