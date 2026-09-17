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
        var node = new GroupNode(new GroupNodeContext(new TestScheduler(), null));
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
        var node = new GroupNode(new GroupNodeContext(new TestScheduler(), null));
        var emitted = new List<LightTransition?>();
        node.OnNewOutput.Subscribe(emitted.Add);
        var transition = new LightParameters { Brightness = 100 }.AsTransition();

        node.SetOutput(transition, appliedByGroup: true);
        node.SetOutput(transition);

        Assert.IsFalse(node.WasAppliedByGroup(emitted[1]!));
    }
}
