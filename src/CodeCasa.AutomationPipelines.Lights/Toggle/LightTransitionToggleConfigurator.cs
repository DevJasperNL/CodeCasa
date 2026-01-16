using System.Reactive.Concurrency;
using CodeCasa.AutomationPipelines.Lights.Context;
using CodeCasa.AutomationPipelines.Lights.Extensions;
using CodeCasa.AutomationPipelines.Lights.Nodes;
using CodeCasa.Lights;
using Microsoft.Extensions.DependencyInjection;

namespace CodeCasa.AutomationPipelines.Lights.Toggle
{
    internal class LightTransitionToggleConfigurator<TLight>(TLight light, IScheduler scheduler) : ILightTransitionToggleConfigurator<TLight>
        where TLight : ILight
    {
        public TLight Light { get; } = light;
        internal TimeSpan? ToggleTimeout { get; private set; }
        internal bool? IncludeOffValue { get; private set; }
        internal List<Func<ILightPipelineContext<TLight>, IPipelineNode<LightTransition>>> NodeFactories
        {
            get;
        } = [];

        public ILightTransitionToggleConfigurator<TLight> SetToggleTimeout(TimeSpan timeout)
        {
            ToggleTimeout = timeout;
            return this;
        }

        public ILightTransitionToggleConfigurator<TLight> IncludeOffInToggleCycle()
        {
            IncludeOffValue = true;
            return this;
        }

        public ILightTransitionToggleConfigurator<TLight> ExcludeOffFromToggleCycle()
        {
            IncludeOffValue = false;
            return this;
        }

        public ILightTransitionToggleConfigurator<TLight> AddOff()
        {
            return Add<TurnOffThenPassThroughNode>();
        }

        public ILightTransitionToggleConfigurator<TLight> AddOn()
        {
            return Add(LightTransition.On());
        }


        public ILightTransitionToggleConfigurator<TLight> Add(LightParameters lightParameters)
        {
            return Add(lightParameters.AsTransition());
        }

        public ILightTransitionToggleConfigurator<TLight> Add(Func<ILightPipelineContext<TLight>, LightParameters?> lightParametersFactory)
        {
            return Add(c => lightParametersFactory(c)?.AsTransition());
        }

        public ILightTransitionToggleConfigurator<TLight> Add(Func<ILightPipelineContext<TLight>, LightTransition?, LightParameters?> lightParametersFactory)
        {
            return Add((c, t) => lightParametersFactory(c, t)?.AsTransition());
        }

        public ILightTransitionToggleConfigurator<TLight> Add(LightTransition lightTransition)
        {
            return Add(new StaticLightTransitionNode(lightTransition, scheduler));
        }

        public ILightTransitionToggleConfigurator<TLight> Add(Func<ILightPipelineContext<TLight>, LightTransition?> lightTransitionFactory)
        {
            return Add(c => new StaticLightTransitionNode(lightTransitionFactory(c), c.ServiceProvider.GetRequiredService<IScheduler>()));
        }

        public ILightTransitionToggleConfigurator<TLight> Add(Func<ILightPipelineContext<TLight>, LightTransition?, LightTransition?> lightTransitionFactory)
        {
            return Add(c => new FactoryNode<LightTransition>(t => lightTransitionFactory(c, t)));
        }

        public ILightTransitionToggleConfigurator<TLight> Add<TNode>() where TNode : IPipelineNode<LightTransition>
        {
            return Add(c => c.ServiceProvider.CreateInstanceWithinContext<TNode, TLight>(c));
        }

        public ILightTransitionToggleConfigurator<TLight> Add(IPipelineNode<LightTransition> node)
        {
            return Add(_ => node);
        }

        public ILightTransitionToggleConfigurator<TLight> Add(Func<ILightPipelineContext<TLight>, IPipelineNode<LightTransition>> nodeFactory)
        {
            NodeFactories.Add(nodeFactory);
            return this;
        }

        public ILightTransitionToggleConfigurator<TLight> AddPassThrough()
        {
            return Add(new PassThroughNode<LightTransition>());
        }

        public ILightTransitionToggleConfigurator<TLight> ForLight(string lightId, Action<ILightTransitionToggleConfigurator<TLight>> configure, ExcludedLightBehaviours excludedLightBehaviour = ExcludedLightBehaviours.None) => ForLights([lightId], configure, excludedLightBehaviour);

        public ILightTransitionToggleConfigurator<TLight> ForLight(TLight light, Action<ILightTransitionToggleConfigurator<TLight>> configure, ExcludedLightBehaviours excludedLightBehaviour = ExcludedLightBehaviours.None) => ForLights([light], configure, excludedLightBehaviour);

        public ILightTransitionToggleConfigurator<TLight> ForLights(IEnumerable<string> lightIds, Action<ILightTransitionToggleConfigurator<TLight>> configure, ExcludedLightBehaviours excludedLightBehaviour = ExcludedLightBehaviours.None)
        {
            CompositeHelper.ValidateLightSupported(lightIds, Light.Id);
            return this;
        }

        public ILightTransitionToggleConfigurator<TLight> ForLights(IEnumerable<TLight> lights, Action<ILightTransitionToggleConfigurator<TLight>> configure, ExcludedLightBehaviours excludedLightBehaviour = ExcludedLightBehaviours.None)
        {
            CompositeHelper.ResolveGroupsAndValidateLightSupported(lights, Light.Id);
            return this;
        }

    }
}
