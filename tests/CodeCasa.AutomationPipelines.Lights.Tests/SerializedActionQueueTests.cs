using CodeCasa.AutomationPipelines.Lights.Utils;

namespace CodeCasa.AutomationPipelines.Lights.Tests;

[TestClass]
public sealed class SerializedActionQueueTests
{
    [TestMethod]
    public void Run_FromWithinRunningAction_ExecutesInline()
    {
        var queue = new SerializedActionQueue();
        var order = new List<string>();

        queue.Run(() =>
        {
            order.Add("outer start");
            queue.Run(() => order.Add("inner"));
            order.Add("outer end");
        });

        CollectionAssert.AreEqual(new[] { "outer start", "inner", "outer end" }, order);
    }

    [TestMethod]
    public void Run_FromOtherThreadWhileDraining_DoesNotWaitAndRunsAfterTheRunningAction()
    {
        var queue = new SerializedActionQueue();
        var order = new List<string>();
        using var firstActionStarted = new ManualResetEventSlim();
        using var secondActionEnqueued = new ManualResetEventSlim();

        var draining = Task.Run(() => queue.Run(() =>
        {
            order.Add("first start");
            firstActionStarted.Set();
            Assert.IsTrue(secondActionEnqueued.Wait(TimeSpan.FromSeconds(30)));
            order.Add("first end");
        }));
        Assert.IsTrue(firstActionStarted.Wait(TimeSpan.FromSeconds(30)));

        queue.Run(() => order.Add("second"));
        secondActionEnqueued.Set();
        draining.Wait(TimeSpan.FromSeconds(30));

        CollectionAssert.AreEqual(new[] { "first start", "first end", "second" }, order);
    }

    [TestMethod]
    public void Run_ConcurrentCallers_ActionsNeverOverlapAndAllRun()
    {
        var queue = new SerializedActionQueue();
        const int iterations = 50_000;
        var running = 0;
        var overlapped = false;
        var count = 0;

        Parallel.For(0, iterations, _ => queue.Run(() =>
        {
            if (Interlocked.Increment(ref running) != 1)
            {
                overlapped = true;
            }
            count++;
            Interlocked.Decrement(ref running);
        }));

        Assert.IsFalse(overlapped);
        Assert.AreEqual(iterations, count);
    }

    [TestMethod]
    public void Run_ActionThrows_RethrowsAndKeepsProcessingLaterActions()
    {
        var queue = new SerializedActionQueue();
        var laterActionRan = false;

        Assert.ThrowsExactly<InvalidOperationException>(() => queue.Run(() => throw new InvalidOperationException()));
        queue.Run(() => laterActionRan = true);

        Assert.IsTrue(laterActionRan);
    }
}
