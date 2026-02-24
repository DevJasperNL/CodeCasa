using CodeCasa.Abstractions;
using CodeCasa.AutomationPipelines.Lights.Extensions;
using CodeCasa.AutomationPipelines.Lights.ReactiveNode;
using CodeCasa.Lights;

namespace CodeCasa.AutomationPipelines.Lights.Pipeline
{
    /// <summary>
    /// Configures light transition pipelines for multiple light entities as a composite.
    /// This configurator applies configurations across all included lights and allows for selective scoping to subsets of lights.
    /// </summary>
    internal partial class CompositeLightTransitionPipelineConfigurator<TLight>(
        IServiceProvider serviceProvider,
        LightPipelineFactory lightPipelineFactory,
        ReactiveNodeFactory reactiveNodeFactory,
        Dictionary<string, LightTransitionPipelineConfigurator<TLight>> nodeContainers)
        : ILightTransitionPipelineConfigurator<TLight> where TLight : ILight
    {
        public Dictionary<string, LightTransitionPipelineConfigurator<TLight>> NodeContainers { get; } = nodeContainers;

        /// <inheritdoc/>
        public ILightTransitionPipelineConfigurator<TLight> AddNode<TNode>() where TNode : IPipelineNode<LightTransition>
        {
            NodeContainers.Values.ForEach(b => b.AddNode<TNode>());
            return this;
        }

        /// <inheritdoc/>
        public ILightTransitionPipelineConfigurator<TLight> AddNode(Func<IServiceProvider, IPipelineNode<LightTransition>> nodeFactory)
        {
            NodeContainers.Values.ForEach(b => b.AddNode(nodeFactory));
            return this;
        }

        /// <inheritdoc/>
        public ILightTransitionPipelineConfigurator<TLight> AddReactiveNode(
            Action<ILightTransitionReactiveNodeConfigurator<TLight>> configure)
        {
            var nodes = reactiveNodeFactory.CreateReactiveNodes(NodeContainers.Select(nc => nc.Value.Light),
                configure.SetLoggingContext(LogName, LoggingEnabled ?? false));
            NodeContainers.ForEach(kvp => kvp.Value.AddNode(nodes[kvp.Key]));
            return this;
        }

        /// <inheritdoc/>
        public ILightTransitionPipelineConfigurator<TLight> AddPipeline(Action<ILightTransitionPipelineConfigurator<TLight>> configure)
        {
            var pipelines = lightPipelineFactory.CreateLightPipelines(NodeContainers.Select(c => c.Value.Light), configure.SetLoggingContext(LogName, LoggingEnabled ?? false));
            NodeContainers.ForEach(kvp => kvp.Value.AddNode(pipelines[kvp.Key]));
            return this;
        }

        /// <inheritdoc/>
        public ILightTransitionPipelineConfigurator<TLight> AddDimmer(IDimmer dimmer)
        {
            return AddDimmer(dimmer, _ => { });
        }

        /// <inheritdoc/>
        public ILightTransitionPipelineConfigurator<TLight> AddDimmer(IDimmer dimmer, Action<DimmerOptions> dimOptions)
        {
            return AddReactiveNode(c =>
            {
                c.AddUncoupledDimmer(dimmer, dimOptions);
            });
        }

        /// <inheritdoc/>
        public ILightTransitionPipelineConfigurator<TLight> ForLight(string lightId,
            Action<ILightTransitionPipelineConfigurator<TLight>> compositeNodeBuilder) => ForLights([lightId], compositeNodeBuilder);

        /// <inheritdoc/>
        public ILightTransitionPipelineConfigurator<TLight> ForLight(TLight light,
            Action<ILightTransitionPipelineConfigurator<TLight>> compositeNodeBuilder) => ForLights([light], compositeNodeBuilder);

        /// <inheritdoc/>
        public ILightTransitionPipelineConfigurator<TLight> ForLights(IEnumerable<string> lightIds,
            Action<ILightTransitionPipelineConfigurator<TLight>> compositeNodeBuilder)
        {
            var lightIdsArray =
                CompositeHelper.ValidateLightsSupported(lightIds, NodeContainers.Keys);

            if (lightIdsArray.Length == NodeContainers.Count)
            {
                compositeNodeBuilder(this);
                return this;
            }
            if (lightIdsArray.Length == 1)
            {
                compositeNodeBuilder(NodeContainers[lightIdsArray.First()]);
                return this;
            }

            compositeNodeBuilder(new CompositeLightTransitionPipelineConfigurator<TLight>(serviceProvider, lightPipelineFactory, reactiveNodeFactory, NodeContainers
                    .Where(kvp => lightIdsArray.Contains(kvp.Key)).ToDictionary(kvp => kvp.Key, kvp => kvp.Value)));
            return this;
        }

        /// <inheritdoc/>
        public ILightTransitionPipelineConfigurator<TLight> ForLights(IEnumerable<TLight> lights,
            Action<ILightTransitionPipelineConfigurator<TLight>> compositeNodeBuilder)
        {
            var lightIds = CompositeHelper.ResolveGroupsAndValidateLightsSupported(lights, NodeContainers.Keys);
            return ForLights(lightIds, compositeNodeBuilder);
        }
    }
}
