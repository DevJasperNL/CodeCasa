using CodeCasa.Lights;
using System.Reactive.Subjects;
using ReactiveNodeClass = CodeCasa.AutomationPipelines.Lights.ReactiveNode.ReactiveNode;

namespace CodeCasa.AutomationPipelines.Lights.Tests;

[TestClass]
public sealed class ReactiveNodeConcurrencyTests
{
    [TestMethod]
    public async Task NestedReactiveNodes_OuterInputAndInnerTriggerOnDifferentThreads_DoNotDeadlock()
    {
        var innerSource = new Subject<IPipelineNode<LightTransition>?>();
        var inner = new ReactiveNodeClass(innerSource);
        var nested = new Pipeline<LightTransition>(inner);
        var outerSource = new Subject<IPipelineNode<LightTransition>?>();
        var outer = new ReactiveNodeClass(outerSource);
        outerSource.OnNext(nested);
        innerSource.OnNext(new BrightnessNode(1));

        const int iterations = 20_000;
        using var start = new ManualResetEventSlim();
        var outerInputs = Task.Factory.StartNew(() =>
        {
            start.Wait();
            for (var i = 0; i < iterations; i++)
            {
                outer.Input = new LightParameters { Brightness = i % 255 }.AsTransition();
            }
        }, TaskCreationOptions.LongRunning);
        var innerTriggers = Task.Factory.StartNew(() =>
        {
            start.Wait();
            for (var i = 0; i < iterations; i++)
            {
                innerSource.OnNext(new BrightnessNode(i % 255));
            }
        }, TaskCreationOptions.LongRunning);
        start.Set();

        var all = Task.WhenAll(outerInputs, innerTriggers);
        var completed = await Task.WhenAny(all, Task.Delay(TimeSpan.FromSeconds(30)));

        Assert.AreSame(all, completed, "Nested reactive nodes deadlocked.");
        await outer.DisposeAsync();
    }
}
