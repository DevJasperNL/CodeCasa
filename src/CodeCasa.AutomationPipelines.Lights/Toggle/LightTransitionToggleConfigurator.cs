using System.Reactive.Concurrency;
using CodeCasa.AutomationPipelines.Lights.Context;
using CodeCasa.AutomationPipelines.Lights.Extensions;
using CodeCasa.AutomationPipelines.Lights.Nodes;
using CodeCasa.Lights;
using Microsoft.Extensions.DependencyInjection;

namespace CodeCasa.AutomationPipelines.Lights.Toggle
{
    internal class LightTransitionToggleConfigurator(ILight light, IScheduler scheduler) : ILightTransitionToggleConfigurator
    {
        public ILight Light { get; } = light;
        internal TimeSpan? ToggleTimeout { get; private set; }
        internal bool? IncludeOffValue { get; private set; }
        internal List<Func<ILightPipelineContext, IPipelineNode<LightTransition>>> NodeFactories
        {
            get;
        } = [];

        public ILightTransitionToggleConfigurator SetToggleTimeout(TimeSpan timeout)
        {
            ToggleTimeout = timeout;
            return this;
        }

        public ILightTransitionToggleConfigurator IncludeOffInToggleCycle()
        {
            IncludeOffValue = true;
            return this;
        }

        public ILightTransitionToggleConfigurator ExcludeOffFromToggleCycle()
        {
            IncludeOffValue = false;
            return this;
        }

        public ILightTransitionToggleConfigurator AddOff()
        {
            return Add<TurnOffThenPassThroughNode>();
        }

        public ILightTransitionToggleConfigurator AddOn()
        {
            return Add(LightTransition.On());
        }


        public ILightTransitionToggleConfigurator Add(LightParameters lightParameters)
        {
            return Add(lightParameters.AsTransition());
        }

        public ILightTransitionToggleConfigurator Add(Func<ILightPipelineContext, LightParameters?> lightParametersFactory)
        {
            return Add(c => lightParametersFactory(c)?.AsTransition());
        }

        public ILightTransitionToggleConfigurator Add(Func<ILightPipelineContext, LightTransition?, LightParameters?> lightParametersFactory)
        {
            return Add((c, t) => lightParametersFactory(c, t)?.AsTransition());
        }

        public ILightTransitionToggleConfigurator Add(LightTransition lightTransition)
        {
            return Add(new StaticLightTransitionNode(lightTransition, scheduler));
        }

        public ILightTransitionToggleConfigurator Add(Func<ILightPipelineContext, LightTransition?> lightTransitionFactory)
        {
            return Add(c => new StaticLightTransitionNode(lightTransitionFactory(c), c.ServiceProvider.GetRequiredService<IScheduler>()));
        }

        public ILightTransitionToggleConfigurator Add(Func<ILightPipelineContext, LightTransition?, LightTransition?> lightTransitionFactory)
        {
            return Add(c => new FactoryNode<LightTransition>(t => lightTransitionFactory(c, t)));
        }

        public ILightTransitionToggleConfigurator Add<TNode>() where TNode : IPipelineNode<LightTransition>
        {
            return Add(c => c.ServiceProvider.CreateInstanceWithinContext<TNode>(c));
        }

        public ILightTransitionToggleConfigurator Add(IPipelineNode<LightTransition> node)
        {
            return Add(_ => node);
        }

        public ILightTransitionToggleConfigurator Add(Func<ILightPipelineContext, IPipelineNode<LightTransition>> nodeFactory)
        {
            NodeFactories.Add(nodeFactory);
            return this;
        }

        public ILightTransitionToggleConfigurator AddPassThrough()
        {
            return Add(new PassThroughNode<LightTransition>());
        }

        public ILightTransitionToggleConfigurator ForLight(string lightId, Action<ILightTransitionToggleConfigurator> configure, ExcludedLightBehaviours excludedLightBehaviour = ExcludedLightBehaviours.None) => ForLights([lightId], configure, excludedLightBehaviour);

        public ILightTransitionToggleConfigurator ForLight(ILight light, Action<ILightTransitionToggleConfigurator> configure, ExcludedLightBehaviours excludedLightBehaviour = ExcludedLightBehaviours.None) => ForLights([light], configure, excludedLightBehaviour);

        public ILightTransitionToggleConfigurator ForLights(IEnumerable<string> lightIds, Action<ILightTransitionToggleConfigurator> configure, ExcludedLightBehaviours excludedLightBehaviour = ExcludedLightBehaviours.None)
        {
            CompositeHelper.ValidateLightSupported(lightIds, Light.Id);
            return this;
        }

        public ILightTransitionToggleConfigurator ForLights(IEnumerable<ILight> lights, Action<ILightTransitionToggleConfigurator> configure, ExcludedLightBehaviours excludedLightBehaviour = ExcludedLightBehaviours.None)
        {
            CompositeHelper.ResolveGroupsAndValidateLightSupported(lights, Light.Id);
            return this;
        }

    }
}
