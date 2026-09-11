namespace CodeCasa.AutomationPipelines.Tests;

[TestClass]
public sealed class PipelineNodeDisposalTests
{
    [TestMethod]
    public async Task Output_AfterDispose_DoesNotThrow()
    {
        var node = new TestablePipelineNode<string>();
        node.Output = "before";

        await node.DisposeAsync();

        node.Output = "after";

        Assert.AreEqual("before", node.Output);
    }

    [TestMethod]
    public async Task Input_AfterDispose_DoesNotThrow()
    {
        var node = new TestablePipelineNode<string>();
        node.PassThrough = true;
        node.Input = "before";

        await node.DisposeAsync();

        node.Input = "after";

        Assert.AreEqual("before", node.Output);
    }

    [TestMethod]
    public async Task PassThrough_AfterDispose_DoesNotThrow()
    {
        var node = new TestablePipelineNode<string>();
        node.Input = "input";
        node.Output = "output";

        await node.DisposeAsync();

        node.PassThrough = true;

        Assert.AreEqual("output", node.Output);
    }

    [TestMethod]
    public async Task ChangeOutputAndTurnOnPassThroughOnNextInput_AfterDispose_DoesNotThrow()
    {
        var node = new TestablePipelineNode<string>();

        await node.DisposeAsync();

        node.ChangeOutputAndTurnOnPassThroughOnNextInput("after");

        Assert.IsNull(node.Output);
    }

    [TestMethod]
    public async Task DisposeAsync_Twice_DoesNotThrow()
    {
        var node = new TestablePipelineNode<string>();

        await node.DisposeAsync();
        await node.DisposeAsync();
    }
}
