using CodeCasa.AutomationPipelines.Lights.Nodes;
using CodeCasa.Lights;
using Microsoft.Reactive.Testing;
using Moq;
using System.Reactive.Subjects;

namespace CodeCasa.AutomationPipelines.Lights.Tests;

[TestClass]
public sealed class NodeDisposalTests
{
    [TestMethod]
    public async Task ResettableTimeoutNode_DisposeAsync_TimerNoLongerFires()
    {
        var scheduler = new TestScheduler();
        var childOutput = new Subject<LightTransition?>();
        var childNode = new Mock<IPipelineNode<LightTransition>>();
        childNode.Setup(x => x.OnNewOutput).Returns(childOutput);
        childNode.Setup(x => x.Output).Returns((LightTransition?)null);
        var timeout = TimeSpan.FromSeconds(30);

        var node = new ResettableTimeoutNode(childNode.Object, timeout, new Subject<bool>(), scheduler);
        childOutput.OnNext(LightTransition.On());
        scheduler.AdvanceBy(TimeSpan.FromMilliseconds(1).Ticks);
        Assert.AreEqual(LightTransition.On(), node.Output);

        await node.DisposeAsync();
        scheduler.AdvanceBy(timeout.Ticks * 2);

        Assert.AreEqual(LightTransition.On(), node.Output);
    }

    [TestMethod]
    public async Task ResettableTimeoutNode_DisposeAsync_DisposesChildNode()
    {
        var scheduler = new TestScheduler();
        var childNode = new Mock<IPipelineNode<LightTransition>>();
        childNode.Setup(x => x.OnNewOutput).Returns(new Subject<LightTransition?>());
        childNode.Setup(x => x.Output).Returns((LightTransition?)null);

        var node = new ResettableTimeoutNode(childNode.Object, TimeSpan.FromSeconds(30), new Subject<bool>(), scheduler);

        await node.DisposeAsync();

        childNode.Verify(x => x.DisposeAsync(), Times.Once);
    }

    [TestMethod]
    public async Task ResettableTimeoutNode_DisposeAsync_ChildOutputAfterDisposalIgnored()
    {
        var scheduler = new TestScheduler();
        var childOutput = new Subject<LightTransition?>();
        var childNode = new Mock<IPipelineNode<LightTransition>>();
        childNode.Setup(x => x.OnNewOutput).Returns(childOutput);
        childNode.Setup(x => x.Output).Returns((LightTransition?)null);

        var node = new ResettableTimeoutNode(childNode.Object, TimeSpan.FromSeconds(30), new Subject<bool>(), scheduler);
        await node.DisposeAsync();

        childOutput.OnNext(LightTransition.On());
        scheduler.AdvanceBy(TimeSpan.FromMilliseconds(1).Ticks);

        Assert.IsNull(node.Output);
        Assert.IsFalse(childOutput.HasObservers);
    }

    [TestMethod]
    public async Task LightTransitionNode_DisposeAsync_CancelsScheduledContinuation()
    {
        var scheduler = new TestScheduler();
        var node = new StaticLightTransitionNode(new LightParameters { Brightness = 10, ColorTempKelvin = 3000 }.AsTransition(), scheduler);

        // Two inputs are needed so the node knows both ends of the in-flight transition it has to continue.
        node.Input = new LightParameters { Brightness = 100, ColorTempKelvin = 3000 }.AsTransition();
        node.Input = new LightParameters { Brightness = 200, ColorTempKelvin = 3000 }.AsTransition(TimeSpan.FromSeconds(10));
        node.PassThrough = true;
        var outputAtDisposal = node.Output;

        await node.DisposeAsync();
        scheduler.AdvanceBy(TimeSpan.FromSeconds(1).Ticks);

        Assert.AreEqual(outputAtDisposal, node.Output);
    }

    [TestMethod]
    public async Task LightTransitionNode_AfterDispose_InputAndOutputIgnored()
    {
        var scheduler = new TestScheduler();
        var initial = new LightParameters { Brightness = 10 }.AsTransition();
        var node = new StaticLightTransitionNode(initial, scheduler);

        await node.DisposeAsync();
        node.PassThrough = true;
        node.Input = new LightParameters { Brightness = 100 }.AsTransition();

        Assert.AreEqual(initial, node.Output);
    }
}
