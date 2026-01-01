namespace CodeCasa.AutomationPipelines.Lights.Tests;

using AutomationPipelines;
using Nodes;

[TestClass]
public sealed class FactoryNodeTests
{
    [TestMethod]
    public void FactoryNode_TransformsInput()
    {
        // Arrange
        const string inputValue = "Test";
        const string expectedOutput = "Test_Transformed";
        string? emittedOutput = null;

        var factoryNode = new FactoryNode<string>(input => $"{input}_Transformed");
        factoryNode.OnNewOutput.Subscribe(o => emittedOutput = o);

        // Act
        factoryNode.Input = inputValue;

        // Assert
        Assert.AreEqual(expectedOutput, emittedOutput);
        Assert.AreEqual(expectedOutput, factoryNode.Output);
    }

    [TestMethod]
    public void FactoryNode_HandlesNullInput()
    {
        // Arrange
        string? emittedOutput = null;
        var factoryNode = new FactoryNode<string?>(input => input ?? "NULL");
        factoryNode.OnNewOutput.Subscribe(o => emittedOutput = o);

        // Act
        factoryNode.Input = null;

        // Assert
        Assert.AreEqual("NULL", emittedOutput);
        Assert.AreEqual("NULL", factoryNode.Output);
    }

    [TestMethod]
    public void FactoryNode_ProducesNullOutput()
    {
        // Arrange
        var emittedOutput = "NotNull";
        var factoryNode = new FactoryNode<string?>(_ => null);
        factoryNode.OnNewOutput.Subscribe(o => emittedOutput = o);

        // Act
        factoryNode.Input = "Test";

        // Assert
        Assert.IsNull(emittedOutput);
        Assert.IsNull(factoryNode.Output);
    }

    [TestMethod]
    public void FactoryNode_MultipleInputs()
    {
        // Arrange
        var outputs = new List<string?>();
        var factoryNode = new FactoryNode<string>(input => $"{input}_transformed");
        factoryNode.OnNewOutput.Subscribe(o => outputs.Add(o));

        // Act
        factoryNode.Input = "First";
        factoryNode.Input = "Second";
        factoryNode.Input = "Third";

        // Assert
        CollectionAssert.AreEqual(
            new[] { "First_transformed", "Second_transformed", "Third_transformed" },
            outputs);
        Assert.AreEqual("Third_transformed", factoryNode.Output);
    }

    [TestMethod]
    public void FactoryNode_WithIntegerTransformation()
    {
        // Arrange
        var emittedOutputs = new List<int?>();
        var factoryNode = new FactoryNode<int>(input => input * 2);
        factoryNode.OnNewOutput.Subscribe(o => emittedOutputs.Add(o));

        // Act
        factoryNode.Input = 5;
        factoryNode.Input = 10;
        factoryNode.Input = 0;

        // Assert
        CollectionAssert.AreEqual(new[] { 10, 20, 0 }, emittedOutputs);
        Assert.AreEqual(0, factoryNode.Output);
    }

    [TestMethod]
    public void FactoryNode_OutputNotificationCalledForEachInput()
    {
        // Arrange
        var outputNotificationCount = 0;
        var factoryNode = new FactoryNode<string>(input => input);
        factoryNode.OnNewOutput.Subscribe(_ => outputNotificationCount++);

        // Act
        factoryNode.Input = "Test1";
        factoryNode.Input = "Test2";
        factoryNode.Input = "Test3";

        // Assert
        Assert.AreEqual(3, outputNotificationCount);
    }

    [TestMethod]
    public void FactoryNode_ChainedWithOtherNodes()
    {
        // Arrange
        string? finalOutput = null;
        var pipeline = new Pipeline<string>();
        pipeline.OnNewOutput.Subscribe(o => finalOutput = o);

        var factoryNode1 = new FactoryNode<string>(input => $"{input}_1");
        var factoryNode2 = new FactoryNode<string>(input => $"{input}_2");

        // Act
        pipeline.SetDefault("Test");
        pipeline.RegisterNode(factoryNode1);
        pipeline.RegisterNode(factoryNode2);

        // Assert
        Assert.AreEqual("Test_1_2", finalOutput);
        Assert.AreEqual("Test_1_2", pipeline.Output);
    }
}
