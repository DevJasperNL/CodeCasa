using CodeCasa.AutomationPipelines.Lights.Extensions;
using CodeCasa.AutomationPipelines.Lights.Nodes;
using Microsoft.Reactive.Testing;
using System.Reactive.Subjects;
using CodeCasa.Lights;
using Moq;

namespace CodeCasa.AutomationPipelines.Lights.Tests;

[TestClass]
public sealed class ResettableTimeoutNodePassThroughTests
{
    private TestScheduler _scheduler = null!;
    private Mock<IPipelineNode<LightTransition>> _childNodeMock = null!;
    private Subject<LightTransition?> _childOutputSubject = null!;
    private Subject<bool> _persistSubject = null!;
    private List<LightTransition?> _outputs = null!;
    private static readonly TimeSpan DefaultTimeout = TimeSpan.FromMinutes(5);
    private static readonly LightTransition ChildOutput = new LightParameters { Brightness = 200 }.AsTransition();
    private static readonly LightTransition UpstreamOutput = new LightParameters { Brightness = 50 }.AsTransition();

    [TestInitialize]
    public void Initialize()
    {
        _scheduler = new TestScheduler();
        _childNodeMock = new Mock<IPipelineNode<LightTransition>>();
        _childOutputSubject = new Subject<LightTransition?>();
        _persistSubject = new Subject<bool>();
        _outputs = [];

        _childNodeMock.Setup(x => x.OnNewOutput).Returns(_childOutputSubject);
        _childNodeMock.Setup(x => x.Output).Returns(ChildOutput);
    }

    private ResettableTimeoutNode CreateNode()
    {
        var node = new ResettableTimeoutNode(_childNodeMock.Object, DefaultTimeout, _persistSubject, _scheduler,
            TimeoutBehaviour.PassThrough);
        node.OnNewOutput.Subscribe(_outputs.Add);
        return node;
    }

    [TestMethod]
    public async Task Constructor_AppliesChildNodeOutput()
    {
        await using var node = CreateNode();

        Assert.AreEqual(ChildOutput, node.Output);
        Assert.IsFalse(node.PassThrough);
    }

    [TestMethod]
    public async Task TimeoutElapsed_WithoutInput_PassesThroughNullWithoutTurningOff()
    {
        await using var node = CreateNode();

        _scheduler.AdvanceBy(DefaultTimeout.Ticks + 1);

        Assert.IsTrue(node.PassThrough);
        Assert.IsNull(node.Output);
        Assert.IsFalse(_outputs.Any(o => o?.LightParameters.Brightness == 0), "The node must not emit an off transition.");
    }

    [TestMethod]
    public async Task TimeoutElapsed_WithInput_HandsOverToInput()
    {
        await using var node = CreateNode();
        node.Input = UpstreamOutput;

        _scheduler.AdvanceBy(DefaultTimeout.Ticks + 1);

        Assert.IsTrue(node.PassThrough);
        Assert.AreEqual(UpstreamOutput, node.Output);
        Assert.IsFalse(_outputs.Any(o => o?.LightParameters.Brightness == 0), "The node must not emit an off transition.");
    }

    [TestMethod]
    public async Task InputAfterTimeout_IsPassedThrough()
    {
        await using var node = CreateNode();
        node.Input = UpstreamOutput;
        _scheduler.AdvanceBy(DefaultTimeout.Ticks + 1);
        var newInput = new LightParameters { Brightness = 80 }.AsTransition();

        node.Input = newInput;

        Assert.AreEqual(newInput, node.Output);
    }

    [TestMethod]
    public async Task InputChangedBeforeTimeout_DoesNotLeakThrough_LatestInputIsHandedOver()
    {
        await using var node = CreateNode();
        var latestInput = new LightParameters { Brightness = 80 }.AsTransition();

        node.Input = UpstreamOutput;
        _scheduler.AdvanceBy(DefaultTimeout.Ticks / 2);
        node.Input = latestInput;
        _scheduler.AdvanceBy(DefaultTimeout.Ticks / 2 - 10);

        Assert.AreEqual(ChildOutput, node.Output);
        Assert.AreEqual(0, _outputs.Count, "Upstream changes must not reach the output before the timeout.");

        _scheduler.AdvanceBy(20);

        Assert.AreEqual(latestInput, node.Output);
    }

    [TestMethod]
    public async Task PersistTrue_HoldsTimeout()
    {
        await using var node = CreateNode();
        node.Input = UpstreamOutput;

        _persistSubject.OnNext(true);
        _scheduler.AdvanceBy(DefaultTimeout.Ticks * 2);

        Assert.IsFalse(node.PassThrough);
        Assert.AreEqual(ChildOutput, node.Output);
    }

    [TestMethod]
    public async Task PersistFalse_RestartsFullTimeout()
    {
        await using var node = CreateNode();
        node.Input = UpstreamOutput;
        _persistSubject.OnNext(true);
        _scheduler.AdvanceBy(DefaultTimeout.Ticks * 2);

        _persistSubject.OnNext(false);
        _scheduler.AdvanceBy(DefaultTimeout.Ticks - 10);

        Assert.AreEqual(ChildOutput, node.Output);

        _scheduler.AdvanceBy(20);

        Assert.IsTrue(node.PassThrough);
        Assert.AreEqual(UpstreamOutput, node.Output);
    }

    [TestMethod]
    public async Task ChildOutputBeforeTimeout_RestartsTimeout()
    {
        await using var node = CreateNode();
        node.Input = UpstreamOutput;
        _scheduler.AdvanceBy(DefaultTimeout.Ticks / 2);
        var newChildOutput = new LightParameters { Brightness = 150 }.AsTransition();

        _childOutputSubject.OnNext(newChildOutput);
        _scheduler.AdvanceBy(DefaultTimeout.Ticks - 10);

        Assert.AreEqual(newChildOutput, node.Output);

        _scheduler.AdvanceBy(20);

        Assert.AreEqual(UpstreamOutput, node.Output);
    }

    [TestMethod]
    public async Task AfterTimeout_PersistAndChildOutputAreIgnored()
    {
        await using var node = CreateNode();
        node.Input = UpstreamOutput;
        _scheduler.AdvanceBy(DefaultTimeout.Ticks + 1);
        var outputCount = _outputs.Count;

        _persistSubject.OnNext(true);
        _scheduler.AdvanceBy(1);
        _persistSubject.OnNext(false);
        _scheduler.AdvanceBy(DefaultTimeout.Ticks * 2);
        _childOutputSubject.OnNext(ChildOutput);
        _scheduler.AdvanceBy(DefaultTimeout.Ticks * 2);

        Assert.IsTrue(node.PassThrough);
        Assert.AreEqual(UpstreamOutput, node.Output);
        Assert.AreEqual(outputCount, _outputs.Count);
        Assert.IsFalse(_persistSubject.HasObservers);
        Assert.IsFalse(_childOutputSubject.HasObservers);
    }

    [TestMethod]
    public async Task DisposeAsync_BeforeTimeout_TimerNoLongerFiresAndChildIsDisposed()
    {
        var node = CreateNode();
        node.Input = UpstreamOutput;

        await node.DisposeAsync();
        _scheduler.AdvanceBy(DefaultTimeout.Ticks * 2);

        Assert.AreEqual(0, _outputs.Count);
        Assert.IsFalse(_persistSubject.HasObservers);
        Assert.IsFalse(_childOutputSubject.HasObservers);
        _childNodeMock.Verify(x => x.DisposeAsync(), Times.Once);
        _childOutputSubject.OnCompleted();
        _persistSubject.OnCompleted();
    }

    [TestMethod]
    public async Task DisposeAsync_AfterTimeout_DoesNotThrow()
    {
        var node = CreateNode();
        _scheduler.AdvanceBy(DefaultTimeout.Ticks + 1);

        await node.DisposeAsync();

        _childNodeMock.Verify(x => x.DisposeAsync(), Times.Once);
    }

    [TestMethod]
    public async Task PassThroughAfter_WithoutPersistObservable_HandsOverToInputAfterTimeout()
    {
        await using var node = _childNodeMock.Object.PassThroughAfter(DefaultTimeout, _scheduler);
        node.Input = UpstreamOutput;

        _scheduler.AdvanceBy(DefaultTimeout.Ticks - 10);
        Assert.AreEqual(ChildOutput, node.Output);

        _scheduler.AdvanceBy(20);
        Assert.AreEqual(UpstreamOutput, node.Output);
    }

    [TestMethod]
    public async Task TurnOffBehaviour_IsStillTheDefault()
    {
        await using var node = new ResettableTimeoutNode(_childNodeMock.Object, DefaultTimeout, _persistSubject, _scheduler);
        node.Input = UpstreamOutput;

        _scheduler.AdvanceBy(DefaultTimeout.Ticks + 1);

        Assert.IsFalse(node.PassThrough);
        Assert.AreEqual(LightTransition.Off(), node.Output);
    }
}
