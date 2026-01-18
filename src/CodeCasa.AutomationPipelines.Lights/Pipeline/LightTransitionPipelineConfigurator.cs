using CodeCasa.Abstractions;
using CodeCasa.AutomationPipelines.Lights.Context;
using CodeCasa.AutomationPipelines.Lights.Extensions;
using CodeCasa.AutomationPipelines.Lights.ReactiveNode;
using CodeCasa.Lights;
using Microsoft.Extensions.DependencyInjection;

namespace CodeCasa.AutomationPipelines.Lights.Pipeline
{
    /// <summary>
    /// Configures a light transition pipeline for a single light entity.
    /// This configurator allows adding pipeline nodes, reactive nodes, dimmers, and conditional logic to light automation.
    /// </summary>
    internal partial class LightTransitionPipelineConfigurator<TLight>(
        IServiceProvider serviceProvider,
        TLight light)
        : ILightTransitionPipelineConfigurator<TLight> where TLight : ILight
    {
        private readonly List<IPipelineNode<LightTransition>> _nodes = new();

        internal TLight Light { get; } = light;
        internal string? Name { get; private set; }
        internal bool? Log { get; private set; }

        public IReadOnlyCollection<IPipelineNode<LightTransition>> Nodes => _nodes.AsReadOnly();

        public ILightTransitionPipelineConfigurator<TLight>
            AddConditional(IObservable<bool> observable, Action<ILightTransitionPipelineConfigurator<TLight>> trueConfigure, Action<ILightTransitionPipelineConfigurator<TLight>> falseConfigure)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public ILightTransitionPipelineConfigurator<TLight> EnableLogging(string? pipelineName = null)
        {
            Name = pipelineName;
            Log = true;
            return this;
        }

        /// <inheritdoc/>
        public ILightTransitionPipelineConfigurator<TLight> DisableLogging()
        {
            Log = false;
            return this;
        }

        /// <inheritdoc/>
        public ILightTransitionPipelineConfigurator<TLight> AddNode<TNode>() where TNode : IPipelineNode<LightTransition>
        {
            _nodes.Add(ActivatorUtilities.CreateInstance<TNode>(serviceProvider));
            return this;
        }

        /// <summary>
        /// Adds a pipeline node to the pipeline.
        /// </summary>
        /// <param name="node">The pipeline node to add.</param>
        /// <returns>The configurator instance for method chaining.</returns>
        public ILightTransitionPipelineConfigurator<TLight> AddNode(IPipelineNode<LightTransition> node)
        {
            _nodes.Add(node);
            return this;
        }

        /// <inheritdoc/>
        public ILightTransitionPipelineConfigurator<TLight> AddNode(Func<ILightPipelineContext<TLight>, IPipelineNode<LightTransition>> nodeFactory)
        {
            _nodes.Add(nodeFactory(new LightPipelineContext<TLight>(serviceProvider)));
            return this;
        }

        /// <inheritdoc/>
        public ILightTransitionPipelineConfigurator<TLight> AddReactiveNode(
            Action<ILightTransitionReactiveNodeConfigurator<TLight>> configure)
        {
            var factory = serviceProvider.GetRequiredService<ReactiveNodeFactory>();
            return AddNode(factory.CreateReactiveNode(Light, c =>
            {
                if (Log ?? false)
                {
                    // If logging is enabled, enable it on the reactive node configurator by default.
                    c.EnableLogging($"Reactive Node in {Name ?? Light.Id}");
                }
                configure(c);
            }));
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
            Action<ILightTransitionPipelineConfigurator<TLight>> compositeNodeBuilder) =>
            ForLights([lightId], compositeNodeBuilder);

        /// <inheritdoc/>
        public ILightTransitionPipelineConfigurator<TLight> ForLight(TLight light,
            Action<ILightTransitionPipelineConfigurator<TLight>> compositeNodeBuilder) => ForLights([light], compositeNodeBuilder);

        /// <inheritdoc/>
        public ILightTransitionPipelineConfigurator<TLight> ForLights(IEnumerable<string> lightIds,
            Action<ILightTransitionPipelineConfigurator<TLight>> compositeNodeBuilder)
        {
            CompositeHelper.ValidateLightSupported(lightIds, Light.Id);
            return this;
        }

        /// <inheritdoc/>
        public ILightTransitionPipelineConfigurator<TLight> ForLights(IEnumerable<TLight> lights,
            Action<ILightTransitionPipelineConfigurator<TLight>> compositeNodeBuilder)
        {
            CompositeHelper.ResolveGroupsAndValidateLightSupported(lights, Light.Id);
            return this;
        }

        /// <inheritdoc/>
        public ILightTransitionPipelineConfigurator<TLight> AddPipeline(Action<ILightTransitionPipelineConfigurator<TLight>> pipelineNodeOptions) => 
            AddNode(
                serviceProvider.GetRequiredService<LightPipelineFactory>()
                    .CreateLightPipeline(Light, pipelineNodeOptions));
    }
}
