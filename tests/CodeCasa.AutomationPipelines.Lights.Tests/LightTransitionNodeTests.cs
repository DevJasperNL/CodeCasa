using System.Reactive.Concurrency;
using CodeCasa.AutomationPipelines.Lights.Nodes;
using CodeCasa.Lights;
using Microsoft.Reactive.Testing;

namespace CodeCasa.AutomationPipelines.Lights.Tests;

[TestClass]
public sealed class LightTransitionNodeTests
{
    private static readonly TimeSpan LongTransition = TimeSpan.FromSeconds(10);

    [TestMethod]
    public void ScheduleInterpolated_Twice_OnlyLastContinuationEmits()
    {
        var scheduler = new TestScheduler();
        var node = CreateNodeWithLongInputTransition(scheduler);
        var outputs = new List<LightTransition?>();
        node.OnNewOutput.Subscribe(outputs.Add);

        node.Schedule(Parameters(50), Parameters(150));
        scheduler.AdvanceBy(TimeSpan.FromMilliseconds(200).Ticks);
        node.Schedule(Parameters(40), Parameters(140));
        outputs.Clear();

        scheduler.AdvanceBy(TimeSpan.FromSeconds(1).Ticks);

        Assert.AreEqual(1, outputs.Count);
        Assert.AreEqual(140d, outputs[0]!.LightParameters.Brightness);
    }

    [TestMethod]
    public void ScheduleInterpolated_TwiceThenNewInput_NoStaleContinuationEmits()
    {
        var scheduler = new TestScheduler();
        var node = CreateNodeWithLongInputTransition(scheduler);

        node.Schedule(Parameters(50), Parameters(150));
        scheduler.AdvanceBy(TimeSpan.FromMilliseconds(200).Ticks);
        node.Schedule(Parameters(40), Parameters(140));

        var outputs = new List<LightTransition?>();
        node.OnNewOutput.Subscribe(outputs.Add);
        node.Input = Parameters(10).AsTransition();
        scheduler.AdvanceBy(TimeSpan.FromSeconds(1).Ticks);

        Assert.AreEqual(0, outputs.Count);
    }

    [TestMethod]
    public void ScheduleInterpolated_TwiceThenPassThrough_NoStaleContinuationOverridesInput()
    {
        var scheduler = new TestScheduler();
        var node = CreateNodeWithLongInputTransition(scheduler);

        node.Schedule(Parameters(50), Parameters(150));
        scheduler.AdvanceBy(TimeSpan.FromMilliseconds(200).Ticks);
        node.Schedule(Parameters(40), Parameters(140));

        var outputs = new List<LightTransition?>();
        node.OnNewOutput.Subscribe(outputs.Add);
        node.PassThrough = true;
        scheduler.AdvanceBy(TimeSpan.FromSeconds(1).Ticks);

        Assert.IsFalse(outputs.Any(o => o?.LightParameters.Brightness is 150d or 140d));
        Assert.AreEqual(200d, node.Output!.LightParameters.Brightness);
    }

    [TestMethod]
    public async Task ScheduleInterpolated_AfterDispose_DoesNotEmit()
    {
        var scheduler = new TestScheduler();
        var node = CreateNodeWithLongInputTransition(scheduler);
        node.Schedule(Parameters(50), Parameters(150));
        var outputAtDisposal = node.Output;

        await node.DisposeAsync();
        node.Schedule(Parameters(40), Parameters(140));
        scheduler.AdvanceBy(TimeSpan.FromSeconds(1).Ticks);

        Assert.AreEqual(outputAtDisposal, node.Output);
    }

    [TestMethod]
    public async Task OnNewOutput_SubscribeAfterDispose_Completes()
    {
        var node = new TestNode(new TestScheduler());
        await node.DisposeAsync();

        var completed = false;
        node.OnNewOutput.Subscribe(_ => { }, () => completed = true);

        Assert.IsTrue(completed);
    }

    [TestMethod]
    public void ToString_NameSet_ReturnsName()
    {
        var node = new TestNode(new TestScheduler()) { Name = "My Node" };

        Assert.AreEqual("My Node", node.ToString());
    }

    [TestMethod]
    public void ToString_NameNotSet_ReturnsTypeName()
    {
        var node = new TestNode(new TestScheduler());

        Assert.AreEqual(nameof(TestNode), node.ToString());
    }

    private static TestNode CreateNodeWithLongInputTransition(TestScheduler scheduler)
    {
        var node = new TestNode(scheduler);
        // Two inputs are needed so the node knows both ends of the in-flight transition it has to continue.
        node.Input = Parameters(100).AsTransition();
        node.Input = Parameters(200).AsTransition(LongTransition);
        return node;
    }

    private static LightParameters Parameters(double brightness) => new() { Brightness = brightness, ColorTempKelvin = 3000 };

    private sealed class TestNode(IScheduler scheduler) : LightTransitionNode(scheduler)
    {
        public void Schedule(LightParameters? source, LightParameters? desired) =>
            ScheduleInterpolatedLightTransitionUsingInputTransitionTime(source, desired);
    }
}
