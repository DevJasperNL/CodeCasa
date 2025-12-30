using System.Reactive.Concurrency;
using CodeCasa.Abstractions;
using CodeCasa.AutomationPipelines.Lights.Context;
using CodeCasa.AutomationPipelines.Lights.Extensions;
using CodeCasa.AutomationPipelines.Lights.ReactiveNode;
using CodeCasa.Lights;

namespace CodeCasa.AutomationPipelines.Lights.Pipeline
{
    public partial class CompositeLightTransitionPipelineConfigurator(
        IServiceProvider serviceProvider,
        LightPipelineFactory lightPipelineFactory,
        ReactiveNodeFactory reactiveNodeFactory,
        Dictionary<string, LightTransitionPipelineConfigurator> nodeContainers)
        : ILightTransitionPipelineConfigurator
    {
        public Dictionary<string, LightTransitionPipelineConfigurator> NodeContainers { get; } = nodeContainers;

        public ILightTransitionPipelineConfigurator SetName(string name)
        {
            NodeContainers.Values.ForEach(b => b.SetName(name));
            return this;
        }

        public ILightTransitionPipelineConfigurator AddNode<TNode>() where TNode : IPipelineNode<LightTransition>
        {
            NodeContainers.Values.ForEach(b => b.AddNode<TNode>());
            return this;
        }

        public ILightTransitionPipelineConfigurator AddNode(Func<ILightPipelineContext, IPipelineNode<LightTransition>> nodeFactory)
        {
            NodeContainers.Values.ForEach(b => b.AddNode(nodeFactory));
            return this;
        }

        public ILightTransitionPipelineConfigurator AddReactiveNode(
            Action<ILightTransitionReactiveNodeConfigurator> configure)
        {
            var nodes = reactiveNodeFactory.CreateReactiveNodes(NodeContainers.Select(nc => nc.Value.Light),
                configure);
            NodeContainers.ForEach(kvp => kvp.Value.AddNode(nodes[kvp.Key]));
            return this;
        }

        public ILightTransitionPipelineConfigurator AddPipeline(Action<ILightTransitionPipelineConfigurator> pipelineNodeOptions)
        {
            var pipelines = lightPipelineFactory.CreateLightPipelines(NodeContainers.Select(c => c.Value.Light),
                pipelineNodeOptions);
            NodeContainers.ForEach(kvp => kvp.Value.AddNode(pipelines[kvp.Key]));
            return this;
        }

        public ILightTransitionPipelineConfigurator AddDimmer(IDimmer dimmer)
        {
            return AddDimmer(dimmer, _ => { });
        }

        public ILightTransitionPipelineConfigurator AddDimmer(IDimmer dimmer, Action<DimmerOptions> dimOptions)
        {
            return AddReactiveNode(c =>
            {
                c.AddUncoupledDimmer(dimmer, dimOptions);
            });
        }

        public ILightTransitionPipelineConfigurator ForLight(string lightId,
            Action<ILightTransitionPipelineConfigurator> compositeNodeBuilder) => ForLights([lightId], compositeNodeBuilder);

        public ILightTransitionPipelineConfigurator ForLight(ILight light,
            Action<ILightTransitionPipelineConfigurator> compositeNodeBuilder) => ForLights([light], compositeNodeBuilder);

        public ILightTransitionPipelineConfigurator ForLights(IEnumerable<string> lightIds,
            Action<ILightTransitionPipelineConfigurator> compositeNodeBuilder)
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

            compositeNodeBuilder(new CompositeLightTransitionPipelineConfigurator(serviceProvider, lightPipelineFactory, reactiveNodeFactory, NodeContainers
                    .Where(kvp => lightIdsArray.Contains(kvp.Key)).ToDictionary(kvp => kvp.Key, kvp => kvp.Value)));
            return this;
        }

        public ILightTransitionPipelineConfigurator ForLights(IEnumerable<ILight> lightEntities,
            Action<ILightTransitionPipelineConfigurator> compositeNodeBuilder)
        {
            var lightIds = CompositeHelper.ResolveGroupsAndValidateLightsSupported(lightEntities, NodeContainers.Keys);
            return ForLights(lightIds, compositeNodeBuilder);
        }
    }
}
