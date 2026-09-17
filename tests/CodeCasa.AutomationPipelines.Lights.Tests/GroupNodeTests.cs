using CodeCasa.AutomationPipelines.Lights.Nodes;
using CodeCasa.Lights;
using Microsoft.Reactive.Testing;

namespace CodeCasa.AutomationPipelines.Lights.Tests;

[TestClass]
public sealed class GroupNodeTests
{
    [TestMethod]
    public void SetOutput_AppliedByGroup_OutputStaysMarkedAfterEmission()
    {
        var node = new GroupNode(new TestLight("a"), new GroupNodeContext(new TestScheduler(), null));
        var emitted = new List<LightTransition?>();
        node.OnNewOutput.Subscribe(emitted.Add);
        var transition = new LightParameters { Brightness = 100 }.AsTransition();

        node.SetOutput(transition, appliedByGroup: true);

        Assert.HasCount(1, emitted);
        Assert.AreEqual(transition, emitted[0]);
        Assert.IsTrue(node.WasAppliedByGroup(emitted[0]!), "The pipeline may process the output after SetOutput returned.");
    }

    [TestMethod]
    public void SetOutput_SameTransitionIndividuallyAfterGroup_IsNotMarked()
    {
        var node = new GroupNode(new TestLight("a"), new GroupNodeContext(new TestScheduler(), null));
        var emitted = new List<LightTransition?>();
        node.OnNewOutput.Subscribe(emitted.Add);
        var transition = new LightParameters { Brightness = 100 }.AsTransition();

        node.SetOutput(transition, appliedByGroup: true);
        node.SetOutput(transition);

        Assert.IsFalse(node.WasAppliedByGroup(emitted[1]!));
    }

    [TestMethod]
    public void NullInput_CancelsPendingInputAndClearsOutput()
    {
        var scheduler = new TestScheduler();
        var context = new GroupNodeContext(scheduler, null);
        var group = new TestLight("group");
        var a = new GroupNode(new TestLight("a"), context);
        var b = new GroupNode(new TestLight("b"), context);
        context.Register(a, group, TimeSpan.FromMilliseconds(20), EqualityComparer<LightTransition>.Default);
        context.Register(b, group, TimeSpan.FromMilliseconds(20), EqualityComparer<LightTransition>.Default);
        var emitted = new List<LightTransition?>();
        a.OnNewOutput.Subscribe(emitted.Add);

        a.Input = new LightParameters { Brightness = 100 }.AsTransition();
        a.Input = null;
        scheduler.AdvanceBy(TimeSpan.FromMilliseconds(25).Ticks);

        Assert.HasCount(1, emitted, "The superseded transition must not be applied after the window.");
        Assert.IsNull(emitted[0]);
        Assert.IsEmpty(group.Applied);
    }

    [TestMethod]
    public async Task MemberRemoved_PendingInputOfRemainingMember_IsNotAppliedAfterANewerOne()
    {
        var scheduler = new TestScheduler();
        var context = new GroupNodeContext(scheduler, null);
        var group = new TestLight("group");
        var a = new GroupNode(new TestLight("a"), context);
        var b = new GroupNode(new TestLight("b"), context);
        context.Register(a, group, TimeSpan.FromMilliseconds(20), EqualityComparer<LightTransition>.Default);
        context.Register(b, group, TimeSpan.FromMilliseconds(20), EqualityComparer<LightTransition>.Default);
        var emitted = new List<LightTransition?>();
        a.OnNewOutput.Subscribe(emitted.Add);

        a.Input = new LightParameters { Brightness = 50 }.AsTransition();
        await b.DisposeAsync();
        a.Input = new LightParameters { Brightness = 100 }.AsTransition();
        scheduler.AdvanceBy(TimeSpan.FromMilliseconds(25).Ticks);

        Assert.HasCount(1, emitted, "The pending transition was superseded and must not be applied after the window.");
        Assert.AreEqual(100, emitted[0]?.LightParameters.Brightness);
    }
}
