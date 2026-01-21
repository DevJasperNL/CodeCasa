using System.Reactive.Concurrency;
using CodeCasa.AutomationPipelines.Lights.Extensions;
using CodeCasa.AutomationPipelines.Lights.Nodes;
using CodeCasa.Lights;
using Microsoft.Extensions.DependencyInjection;

namespace CodeCasa.AutomationPipelines.Lights.Toggle
{
    internal class CompositeLightTransitionToggleConfigurator<TLight>(
        Dictionary<string, LightTransitionToggleConfigurator<TLight>> activeConfigurators,
        Dictionary<string, LightTransitionToggleConfigurator<TLight>> inactiveConfigurators) : ILightTransitionToggleConfigurator<TLight>
        where TLight : ILight
    {
        public ILightTransitionToggleConfigurator<TLight> SetToggleTimeout(TimeSpan timeout)
        {
            activeConfigurators.Values.ForEach(c => c.SetToggleTimeout(timeout));
            inactiveConfigurators.Values.ForEach(c => c.SetToggleTimeout(timeout));
            return this;
        }

        public ILightTransitionToggleConfigurator<TLight> IncludeOffInToggleCycle()
        {
            activeConfigurators.Values.ForEach(c => c.IncludeOffInToggleCycle());
            inactiveConfigurators.Values.ForEach(c => c.IncludeOffInToggleCycle());
            return this;
        }

        public ILightTransitionToggleConfigurator<TLight> ExcludeOffFromToggleCycle()
        {
            activeConfigurators.Values.ForEach(c => c.ExcludeOffFromToggleCycle());
            inactiveConfigurators.Values.ForEach(c => c.ExcludeOffFromToggleCycle());
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

        public ILightTransitionToggleConfigurator<TLight> Add(Func<IServiceProvider, LightParameters?> lightParametersFactory)
        {
            return Add(c => lightParametersFactory(c)?.AsTransition());
        }

        public ILightTransitionToggleConfigurator<TLight> Add(Func<IServiceProvider, LightTransition?, LightParameters?> lightParametersFactory)
        {
            return Add((c, t) => lightParametersFactory(c, t)?.AsTransition());
        }

        public ILightTransitionToggleConfigurator<TLight> Add(LightTransition lightTransition)
        {
            return Add(_ => lightTransition);
        }

        public ILightTransitionToggleConfigurator<TLight> Add(Func<IServiceProvider, LightTransition?> lightTransitionFactory)
        {
            return Add(c => new StaticLightTransitionNode(lightTransitionFactory(c), c.GetRequiredService<IScheduler>()));
        }

        public ILightTransitionToggleConfigurator<TLight> Add(Func<IServiceProvider, LightTransition?, LightTransition?> lightTransitionFactory)
        {
            return Add(c => new FactoryNode<LightTransition>(t => lightTransitionFactory(c, t)));
        }

        public ILightTransitionToggleConfigurator<TLight> Add<TNode>() where TNode : IPipelineNode<LightTransition>
        {
            activeConfigurators.Values.ForEach(c => c.Add<TNode>());
            inactiveConfigurators.Values.ForEach(c => c.AddPassThrough());
            return this;
        }

        public ILightTransitionToggleConfigurator<TLight> Add(Func<IServiceProvider, IPipelineNode<LightTransition>> nodeFactory)
        {
            activeConfigurators.Values.ForEach(c => c.Add(nodeFactory));
            inactiveConfigurators.Values.ForEach(c => c.AddPassThrough());
            return this;
        }

        public ILightTransitionToggleConfigurator<TLight> AddPassThrough()
        {
            activeConfigurators.Values.ForEach(c => c.AddPassThrough());
            inactiveConfigurators.Values.ForEach(c => c.AddPassThrough());
            return this;
        }

        public ILightTransitionToggleConfigurator<TLight> ForLight(string lightId, Action<ILightTransitionToggleConfigurator<TLight>> configure, ExcludedLightBehaviours excludedLightBehaviour = ExcludedLightBehaviours.None) => ForLights([lightId], configure, excludedLightBehaviour);

        public ILightTransitionToggleConfigurator<TLight> ForLight(TLight light, Action<ILightTransitionToggleConfigurator<TLight>> configure, ExcludedLightBehaviours excludedLightBehaviour = ExcludedLightBehaviours.None) => ForLights([light], configure, excludedLightBehaviour);

        public ILightTransitionToggleConfigurator<TLight> ForLights(IEnumerable<string> lightIds,
            Action<ILightTransitionToggleConfigurator<TLight>> configure,
            ExcludedLightBehaviours excludedLightBehaviour = ExcludedLightBehaviours.None)
        {
            var lightIdsArray =
                CompositeHelper.ValidateLightsSupported(lightIds, activeConfigurators.Keys);

            if (lightIdsArray.Length == activeConfigurators.Count)
            {
                configure(this);
                return this;
            }

            if (excludedLightBehaviour == ExcludedLightBehaviours.None)
            {
                if (lightIdsArray.Length == 1)
                {
                    configure(activeConfigurators[lightIdsArray.First()]);
                    return this;
                }

                configure(new CompositeLightTransitionToggleConfigurator<TLight>(
                    activeConfigurators.Where(kvp => lightIdsArray.Contains(kvp.Key))
                        .ToDictionary(kvp => kvp.Key, kvp => kvp.Value), []));
                return this;
            }

            configure(new CompositeLightTransitionToggleConfigurator<TLight>(
                activeConfigurators.Where(kvp => lightIdsArray.Contains(kvp.Key))
                    .ToDictionary(kvp => kvp.Key, kvp => kvp.Value),
                activeConfigurators.Where(kvp => !lightIdsArray.Contains(kvp.Key))
                    .ToDictionary(kvp => kvp.Key, kvp => kvp.Value)));
            return this;
        }

        public ILightTransitionToggleConfigurator<TLight> ForLights(IEnumerable<TLight> lights, Action<ILightTransitionToggleConfigurator<TLight>> configure, ExcludedLightBehaviours excludedLightBehaviour = ExcludedLightBehaviours.None)
        {
            var lightIds = CompositeHelper.ResolveGroupsAndValidateLightsSupported(lights, activeConfigurators.Keys);
            return ForLights(lightIds, configure, excludedLightBehaviour);
        }
    }
}
