using CodeCasa.AutomationPipelines.Lights.Extensions;
using CodeCasa.AutomationPipelines.Lights.Pipeline;
using CodeCasa.Lights;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Reactive.Testing;
using System.Reactive.Subjects;

namespace CodeCasa.AutomationPipelines.Lights.Tests;

[TestClass]
public sealed class InteractionNodeTests
{
    private TestLight _light = null!;
    private TestScheduler _scheduler = null!;
    private Subject<int> _trigger = null!;
    private IPipeline<LightTransition>? _pipeline;

    [TestInitialize]
    public void Initialize()
    {
        _light = new TestLight("a");
        _scheduler = new TestScheduler();
        _trigger = new Subject<int>();
    }

    private IAsyncDisposable Setup(ServiceProvider sp, InteractionNodeOptions? options)
    {
        var disposable = sp.GetRequiredService<LightPipelineFactory>().SetupLightPipeline(_light, p =>
        {
            p.AddReactiveNode(r => r.On(_trigger, new LightParameters { Brightness = 100 }));
            if (options == null)
            {
                p.AddInteractionNode();
            }
            else
            {
                p.AddInteractionNode(options);
            }
            p.OnCompleted(e => _pipeline = e.Pipeline);
        });
        _trigger.OnNext(1);
        _scheduler.AdvanceBy(TimeSpan.FromSeconds(5).Ticks);
        return disposable;
    }

    [TestMethod]
    public async Task DefaultOptions_ExternalBrightnessChange_IsNotHeld()
    {
        await using var sp = LightPipelineTestSetup.CreateServiceProvider(_scheduler);
        await using var pipelines = Setup(sp, null);

        _light.ReportExternalState(new LightParameters { Brightness = 30 });

        Assert.AreEqual(100, _pipeline?.Output?.LightParameters.Brightness);
    }

    [TestMethod]
    public async Task DefaultOptions_ExternalOff_IsStillHandled()
    {
        await using var sp = LightPipelineTestSetup.CreateServiceProvider(_scheduler);
        await using var pipelines = Setup(sp, null);

        _light.ReportExternalState(LightParameters.Off());

        Assert.AreEqual(0, _pipeline?.Output?.LightParameters.Brightness);
    }

    [TestMethod]
    public async Task HoldExternalChanges_ExternalChange_IsHeldUntilUpstreamChanges()
    {
        await using var sp = LightPipelineTestSetup.CreateServiceProvider(_scheduler);
        await using var pipelines = Setup(sp, new InteractionNodeOptions { HoldExternalChanges = true });

        _light.ReportExternalState(new LightParameters { Brightness = 30 });
        Assert.AreEqual(30, _pipeline?.Output?.LightParameters.Brightness);

        _light.ReportExternalState(new LightParameters { Brightness = 60 });
        Assert.AreEqual(60, _pipeline?.Output?.LightParameters.Brightness, "A further external change right after a held one should be held as well.");

        _trigger.OnNext(2);
        Assert.AreEqual(100, _pipeline?.Output?.LightParameters.Brightness);
        Assert.AreEqual(100, _light.Current.Brightness);
    }

    [TestMethod]
    public async Task HoldExternalChanges_StateReportedDuringSettleTime_IsIgnored()
    {
        await using var sp = LightPipelineTestSetup.CreateServiceProvider(_scheduler);
        await using var pipelines = Setup(sp, new InteractionNodeOptions { HoldExternalChanges = true });

        _trigger.OnNext(2);
        _light.ReportExternalState(new LightParameters { Brightness = 50 });

        Assert.AreEqual(100, _pipeline?.Output?.LightParameters.Brightness);
    }

    [TestMethod]
    public async Task HoldExternalChanges_ReportedStateWithinTolerance_IsNotAnExternalChange()
    {
        await using var sp = LightPipelineTestSetup.CreateServiceProvider(_scheduler);
        await using var pipelines = Setup(sp, new InteractionNodeOptions { HoldExternalChanges = true });
        var appliedBefore = _light.Applied.Count;

        _light.ReportExternalState(new LightParameters { Brightness = 101 });

        Assert.AreEqual(100, _pipeline?.Output?.LightParameters.Brightness);
        Assert.HasCount(appliedBefore, _light.Applied);
    }

    [TestMethod]
    public async Task HoldExternalChanges_HoldTimeoutElapsed_ReturnsToUpstreamOutput()
    {
        await using var sp = LightPipelineTestSetup.CreateServiceProvider(_scheduler);
        await using var pipelines = Setup(sp, new InteractionNodeOptions { HoldExternalChanges = true, HoldTimeout = TimeSpan.FromMinutes(10) });

        _light.ReportExternalState(new LightParameters { Brightness = 30 });
        _scheduler.AdvanceBy(TimeSpan.FromMinutes(9).Ticks);
        Assert.AreEqual(30, _pipeline?.Output?.LightParameters.Brightness);

        _scheduler.AdvanceBy(TimeSpan.FromMinutes(2).Ticks);
        Assert.AreEqual(100, _pipeline?.Output?.LightParameters.Brightness);
        Assert.AreEqual(100, _light.Current.Brightness);
    }
}
