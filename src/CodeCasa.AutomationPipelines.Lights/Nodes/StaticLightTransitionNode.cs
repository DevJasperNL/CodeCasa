using System.Reactive.Concurrency;
using CodeCasa.Lights;

namespace CodeCasa.AutomationPipelines.Lights.Nodes;

internal class StaticLightTransitionNode : LightTransitionNode
{
    public StaticLightTransitionNode(LightTransition? output, IScheduler scheduler) : base(scheduler)
    {
        Output = output;
    }
}