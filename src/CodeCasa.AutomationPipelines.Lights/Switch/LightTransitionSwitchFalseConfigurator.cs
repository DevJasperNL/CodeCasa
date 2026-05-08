using CodeCasa.AutomationPipelines.Lights.Nodes;
using CodeCasa.Lights;
using Microsoft.Extensions.DependencyInjection;
using System.Reactive.Concurrency;

namespace CodeCasa.AutomationPipelines.Lights.Switch;

internal sealed class LightTransitionSwitchFalseConfigurator<TLight>(
    Func<IServiceProvider, IPipelineNode<LightTransition>> trueNodeFactory)
    : ILightTransitionSwitchFalseConfigurator<TLight>
    where TLight : ILight
{
    internal Func<IServiceProvider, IPipelineNode<LightTransition>> TrueNodeFactory { get; } = trueNodeFactory;
    internal Func<IServiceProvider, IPipelineNode<LightTransition>>? FalseNodeFactory { get; private set; }

    public void WhenFalse(LightParameters lightParameters)
        => WhenFalse(_ => lightParameters.AsTransition());

    public void WhenFalse(LightTransition lightTransition)
        => WhenFalse(_ => lightTransition);

    public void WhenFalse(Func<IServiceProvider, LightParameters> lightParametersFactory)
        => WhenFalse(c => lightParametersFactory(c).AsTransition());

    public void WhenFalse(Func<IServiceProvider, LightTransition> lightTransitionFactory)
        => WhenFalse(c => (IPipelineNode<LightTransition>)new StaticLightTransitionNode(lightTransitionFactory(c), c.GetRequiredService<IScheduler>()));

    public void WhenFalse(Func<IServiceProvider, IPipelineNode<LightTransition>> nodeFactory)
    {
        FalseNodeFactory = nodeFactory;
    }

    public void WhenFalse<TNode>() where TNode : IPipelineNode<LightTransition>
        => WhenFalse(c => ActivatorUtilities.CreateInstance<TNode>(c));
}
