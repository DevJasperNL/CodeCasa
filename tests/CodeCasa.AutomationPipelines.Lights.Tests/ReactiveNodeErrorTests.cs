using CodeCasa.Lights;
using System.Reactive.Subjects;
using ReactiveNodeClass = CodeCasa.AutomationPipelines.Lights.ReactiveNode.ReactiveNode;

namespace CodeCasa.AutomationPipelines.Lights.Tests;

[TestClass]
public sealed class ReactiveNodeErrorTests
{
    [TestMethod]
    public void NodeSourceError_DoesNotThrowAndPassesThrough()
    {
        var source = new Subject<IPipelineNode<LightTransition>?>();
        var node = new ReactiveNodeClass(source);
        LightTransition? last = null;
        node.OnNewOutput.Subscribe(o => last = o);

        source.OnError(new InvalidOperationException("boom"));

        var input = new LightParameters { Brightness = 100 }.AsTransition();
        node.Input = input;
        Assert.AreEqual(input, last);
    }

    [TestMethod]
    public async Task NodeSourceError_WithActiveNode_DeactivatesIt()
    {
        var source = new Subject<IPipelineNode<LightTransition>?>();
        var node = new ReactiveNodeClass(source);
        LightTransition? last = null;
        node.OnNewOutput.Subscribe(o => last = o);
        var active = new BrightnessNode(200);
        source.OnNext(active);
        Assert.AreEqual(200, last?.LightParameters.Brightness);

        source.OnError(new InvalidOperationException("boom"));

        Assert.IsNull(node.ActiveNode);
        var input = new LightParameters { Brightness = 100 }.AsTransition();
        node.Input = input;
        Assert.AreEqual(input, last);
        await node.DisposeAsync();
    }
}
