namespace CodeCasa.AutomationPipelines.Tests;

[TestClass]
public sealed class PipelineTests
{
    [TestMethod]
    public void DefaultValue()
    {
        // Arrange
        const string defaultValue = "Test";

        string? emittedOutput = null;

        var pipeline = new Pipeline<string>();
        pipeline.OnNewOutput.Subscribe(o => emittedOutput = o);

        // Act
        pipeline.SetDefault(defaultValue);

        // Assert
        Assert.AreEqual(defaultValue, emittedOutput);
        Assert.AreEqual(defaultValue, pipeline.Output);
    }

    [TestMethod]
    public void OutputHandler_ExecutesDefaultValue()
    {
        // Arrange
        const string defaultValue = "Test";
        string? emittedOutput = null;
        string? outputHandlerResult = null;

        var pipeline = new Pipeline<string>();
        pipeline.OnNewOutput.Subscribe(o => emittedOutput = o);

        pipeline.SetDefault(defaultValue);

        // Act
        pipeline.SetOutputHandler(s => outputHandlerResult = s);

        // Assert
        Assert.AreEqual(defaultValue, emittedOutput);
        Assert.AreEqual(defaultValue, outputHandlerResult);
        Assert.AreEqual(defaultValue, pipeline.Output);
    }

    [TestMethod]
    public void OutputHandler_UpdatedByDefaultValue()
    {
        // Arrange
        const string defaultValue = "Test";
        string? emittedOutput = null;
        string? outputHandlerResult = null;

        var pipeline = new Pipeline<string>();
        pipeline.OnNewOutput.Subscribe(o => emittedOutput = o);
        pipeline.SetOutputHandler(s => outputHandlerResult = s);

        // Act
        pipeline.SetDefault(defaultValue);

        // Assert
        Assert.AreEqual(defaultValue, emittedOutput);
        Assert.AreEqual(defaultValue, outputHandlerResult);
        Assert.AreEqual(defaultValue, pipeline.Output);
    }

    [TestMethod]
    public void OutputHandler_ExecutesNodeValue()
    {
        const string defaultValue = "Test1";
        const string nodeValue = "Test2";

        string? emittedOutput = null;
        string? outputHandlerResult = null;

        var pipeline = new Pipeline<string>();
        pipeline.OnNewOutput.Subscribe(o => emittedOutput = o);

        pipeline.SetDefault(defaultValue);
        pipeline.RegisterNode(new TestablePipelineNode<string>
        {
            Output = nodeValue
        });

        pipeline.SetOutputHandler(s => outputHandlerResult = s);

        Assert.AreEqual(nodeValue, emittedOutput);
        Assert.AreEqual(nodeValue, outputHandlerResult);
        Assert.AreEqual(nodeValue, pipeline.Output);
    }

    [TestMethod]
    public void OutputHandler_UpdatedWithNewNode()
    {
        const string defaultValue = "Test1";
        const string nodeValue = "Test2";

        string? emittedOutput = null;
        string? outputHandlerResult = null;

        var pipeline = new Pipeline<string>();
        pipeline.OnNewOutput.Subscribe(o => emittedOutput = o);
        pipeline.SetOutputHandler(s => outputHandlerResult = s);

        pipeline.SetDefault(defaultValue);

        Assert.AreEqual(defaultValue, emittedOutput);
        Assert.AreEqual(defaultValue, outputHandlerResult);
        Assert.AreEqual(defaultValue, pipeline.Output);

        pipeline.RegisterNode(new TestablePipelineNode<string>
        {
            Output = nodeValue
        });

        Assert.AreEqual(nodeValue, emittedOutput);
        Assert.AreEqual(nodeValue, outputHandlerResult);
        Assert.AreEqual(nodeValue, pipeline.Output);
    }

    [TestMethod]
    public void OutputHandler_UpdatedWithNewNodeValue()
    {
        const string defaultValue = "Test1";
        const string nodeValue1 = "OriginalValue";
        const string nodeValue2 = "NewValue";

        string? emittedOutput = null;
        string? outputHandlerResult = null;

        var pipeline = new Pipeline<string>();
        pipeline.OnNewOutput.Subscribe(o => emittedOutput = o);
        pipeline.SetOutputHandler(s => outputHandlerResult = s);

        var node = new TestablePipelineNode<string>
        {
            Output = nodeValue1
        };

        pipeline.SetDefault(defaultValue);
        pipeline.RegisterNode(node);

        Assert.AreEqual(nodeValue1, emittedOutput);
        Assert.AreEqual(nodeValue1, outputHandlerResult);
        Assert.AreEqual(nodeValue1, pipeline.Output);

        node.Output = nodeValue2;

        Assert.AreEqual(nodeValue2, emittedOutput);
        Assert.AreEqual(nodeValue2, outputHandlerResult);
        Assert.AreEqual(nodeValue2, pipeline.Output);
    }

    [TestMethod]
    public void OutputHandler_Distinct()
    {
        // Arrange
        const string defaultValue = "Test1";

        string? emittedOutput = null;
        List<string> outputHandlerResults = new();

        var pipeline = new Pipeline<string>();
        pipeline.OnNewOutput.Subscribe(o => emittedOutput = o);

        pipeline.SetOutputHandler(outputHandlerResults.Add, EqualityComparer<string>.Default);

        // Act
        pipeline.SetDefault(defaultValue);
        pipeline.SetDefault(defaultValue);

        // Assert
        Assert.AreEqual(defaultValue, emittedOutput);
        CollectionAssert.AreEqual(new []{defaultValue}, outputHandlerResults);
        Assert.AreEqual(defaultValue, pipeline.Output);
    }

    [TestMethod]
    public void OutputHandler_NotDistinct()
    {
        // Arrange
        const string defaultValue = "Test1";

        string? emittedOutput = null;
        List<string> outputHandlerResults = new();

        var pipeline = new Pipeline<string>();
        pipeline.OnNewOutput.Subscribe(o => emittedOutput = o);

        pipeline.SetOutputHandler(outputHandlerResults.Add, equalityComparer: null);

        // Act
        pipeline.SetDefault(defaultValue);
        pipeline.SetDefault(defaultValue);

        // Assert
        Assert.AreEqual(defaultValue, emittedOutput);
        CollectionAssert.AreEqual(new[] { defaultValue, defaultValue }, outputHandlerResults);
        Assert.AreEqual(defaultValue, pipeline.Output);
    }

    [TestMethod]
    public void PassThroughNode_ChangesOutputToDefault()
    {
        const string defaultValue = "Test1";
        const string nodeValue = "Test2";

        string? emittedOutput = null;
        string? outputHandlerResult = null;

        var pipeline = new Pipeline<string>();
        pipeline.OnNewOutput.Subscribe(o => emittedOutput = o);
        pipeline.SetOutputHandler(s => outputHandlerResult = s);

        var node1 = new TestablePipelineNode<string>
        {
            Output = nodeValue
        };

        pipeline.SetDefault(defaultValue);
        pipeline.RegisterNode(node1);

        Assert.AreEqual(nodeValue, emittedOutput);
        Assert.AreEqual(nodeValue, outputHandlerResult);
        Assert.AreEqual(nodeValue, pipeline.Output);

        node1.PassThrough = true;

        Assert.AreEqual(defaultValue, emittedOutput);
        Assert.AreEqual(defaultValue, outputHandlerResult);
        Assert.AreEqual(defaultValue, pipeline.Output);
    }

    [TestMethod]
    public void PassThroughNode_ChangesOutputToPreviousNodeOutput()
    {
        const string defaultValue = "Test1";
        const string firstNodeValue = "Test2";
        const string secondNodeValue = "Test2";

        string? emittedOutput = null;
        string? outputHandlerResult = null;
            
        var pipeline = new Pipeline<string>();
        pipeline.OnNewOutput.Subscribe(o => emittedOutput = o);
        pipeline.SetOutputHandler(s => outputHandlerResult = s);

        var node1 = new TestablePipelineNode<string>
        {
            Output = firstNodeValue
        };
        var node2 = new TestablePipelineNode<string>
        {
            Output = secondNodeValue
        };

        pipeline.SetDefault(defaultValue);
        pipeline.RegisterNode(node1);
        pipeline.RegisterNode(node2);

        Assert.AreEqual(secondNodeValue, emittedOutput);
        Assert.AreEqual(secondNodeValue, outputHandlerResult);
        Assert.AreEqual(secondNodeValue, pipeline.Output);

        node2.PassThrough = true;

        Assert.AreEqual(firstNodeValue, emittedOutput);
        Assert.AreEqual(firstNodeValue, outputHandlerResult);
        Assert.AreEqual(firstNodeValue, pipeline.Output);
    }
}