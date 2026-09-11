namespace CodeCasa.AutomationPipelines.Tests;

[TestClass]
public sealed class PipelineOutputOrderingTests
{
    [TestMethod]
    public void ReentrantOutput_HandlerReceivesOutputsInOrder()
    {
        var node = new TestablePipelineNode<string>();
        var handled = new List<string>();
        var pipeline = new Pipeline<string>(node);
        pipeline.SetOutputHandler(handled.Add);

        // Emitting a new output from within the pipeline's own output emission must not let the
        // older value be applied after the newer one.
        var reentered = false;
        pipeline.OnNewOutput.Subscribe(o =>
        {
            if (o == "first" && !reentered)
            {
                reentered = true;
                node.Output = "second";
            }
        });

        node.Output = "first";

        CollectionAssert.AreEqual(new[] { "first", "second" }, handled);
        Assert.AreEqual("second", pipeline.Output);
    }

    [TestMethod]
    public void ReentrantOutput_DistinctComparer_ComparesAgainstLastAppliedValue()
    {
        var node = new TestablePipelineNode<string>();
        var handled = new List<string>();
        var pipeline = new Pipeline<string>(node);
        pipeline.SetOutputHandler(handled.Add, StringComparer.Ordinal);

        var reentered = false;
        pipeline.OnNewOutput.Subscribe(o =>
        {
            if (o == "first" && !reentered)
            {
                reentered = true;
                node.Output = "first";
            }
        });

        node.Output = "first";

        CollectionAssert.AreEqual(new[] { "first" }, handled);
    }

    [TestMethod]
    public void ConcurrentOutputs_AllReachHandler()
    {
        var node = new TestablePipelineNode<string>();
        var handled = new System.Collections.Concurrent.ConcurrentBag<string>();
        var pipeline = new Pipeline<string>(node);
        pipeline.SetOutputHandler(handled.Add);

        const int perThread = 500;
        var threads = Enumerable.Range(0, 4).Select(t => new Thread(() =>
        {
            for (var i = 0; i < perThread; i++)
            {
                node.Output = (t * perThread + i).ToString();
            }
        })).ToArray();
        foreach (var thread in threads) thread.Start();
        foreach (var thread in threads) thread.Join();

        Assert.AreEqual(4 * perThread, handled.Count);
    }

    [TestMethod]
    public async Task DisposeAsync_NodeEmittingDuringDisposal_DoesNotThrow()
    {
        var emitting = new EmitOnDisposeNode();
        var pipeline = new Pipeline<string>(emitting, new TestablePipelineNode<string>());

        await pipeline.DisposeAsync();
    }

    private sealed class EmitOnDisposeNode : PipelineNode<string>
    {
        public override ValueTask DisposeAsync()
        {
            Output = "bye";
            return base.DisposeAsync();
        }
    }
}
