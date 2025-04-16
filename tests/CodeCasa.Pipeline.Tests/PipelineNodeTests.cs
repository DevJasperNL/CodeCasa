using Moq;

namespace CodeCasa.Pipeline.Tests;

[TestClass]
public sealed class PipelineNodeTests
{
    [TestMethod]
    public void Input_Ignored()
    {
        // Arrange
        string? emittedOutput = null;

        var pipelineNode = new Mock<PipelineNode<string>>().Object;
        pipelineNode.OnNewOutput.Subscribe(o => emittedOutput = o);

        // Act
        pipelineNode.Input = "Test";

        // Assert
        Assert.IsNull(emittedOutput);
        Assert.IsNull(pipelineNode.Output);
    }

    [TestMethod]
    public void Disabled_Input_ExistingInputPassed()
    {
        const string testValue = "Test";
        string? emittedOutput = null;

        var pipelineNode = new Mock<PipelineNode<string>>().Object;
        pipelineNode.OnNewOutput.Subscribe(o => emittedOutput = o);

        pipelineNode.Input = testValue;

        Assert.IsNull(emittedOutput);
        Assert.IsNull(pipelineNode.Output);

        pipelineNode.Enabled = false;
            
        Assert.AreEqual(testValue, emittedOutput);
        Assert.AreEqual(testValue, pipelineNode.Output);
    }

    [TestMethod]
    public void Disabled_Input_NewInputPassed()
    {
        const string testValue = "Test";
        string? emittedOutput = null;

        var pipelineNode = new Mock<PipelineNode<string>>().Object;
        pipelineNode.OnNewOutput.Subscribe(o => emittedOutput = o);

        pipelineNode.Enabled = false;
            
        Assert.IsNull(emittedOutput);
        Assert.IsNull(pipelineNode.Output);

        pipelineNode.Input = testValue;

        Assert.AreEqual(testValue, emittedOutput);
        Assert.AreEqual(testValue, pipelineNode.Output);
    }
}