using CodeCasa.AutomationPipelines.Lights.Nodes;
using CodeCasa.Lights;
using Microsoft.Extensions.DependencyInjection;
using System.Reactive.Concurrency;

namespace CodeCasa.AutomationPipelines.Lights.Switch;

internal sealed class LightTransitionSwitchConfigurator<TLight>
    : ILightTransitionSwitchConfigurator<TLight>
    where TLight : ILight
{
    internal LightTransitionSwitchFalseConfigurator<TLight>? FalseConfigurator { get; private set; }

    public ILightTransitionSwitchFalseConfigurator<TLight> WhenTrue(LightParameters lightParameters)
        => WhenTrue(_ => lightParameters.AsTransition());

    public ILightTransitionSwitchFalseConfigurator<TLight> WhenTrue(LightTransition lightTransition)
        => WhenTrue(_ => lightTransition);

    public ILightTransitionSwitchFalseConfigurator<TLight> WhenTrue(Func<IServiceProvider, LightParameters> lightParametersFactory)
        => WhenTrue(c => lightParametersFactory(c).AsTransition());

    public ILightTransitionSwitchFalseConfigurator<TLight> WhenTrue(Func<IServiceProvider, LightTransition> lightTransitionFactory)
        => WhenTrue(c => (IPipelineNode<LightTransition>)new StaticLightTransitionNode(lightTransitionFactory(c), c.GetRequiredService<IScheduler>()));

    public ILightTransitionSwitchFalseConfigurator<TLight> WhenTrue(Func<IServiceProvider, IPipelineNode<LightTransition>> nodeFactory)
    {
        var falseConfigurator = new LightTransitionSwitchFalseConfigurator<TLight>(nodeFactory);
        FalseConfigurator = falseConfigurator;
        return falseConfigurator;
    }

    public ILightTransitionSwitchFalseConfigurator<TLight> WhenTrue<TNode>() where TNode : IPipelineNode<LightTransition>
        => WhenTrue(c => ActivatorUtilities.CreateInstance<TNode>(c));
}
