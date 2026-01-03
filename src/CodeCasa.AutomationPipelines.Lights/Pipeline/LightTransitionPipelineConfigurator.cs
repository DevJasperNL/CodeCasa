using CodeCasa.AutomationPipelines.Lights.Context;
using CodeCasa.AutomationPipelines.Lights.Extensions;
using CodeCasa.AutomationPipelines.Lights.ReactiveNode;
using CodeCasa.Abstractions;
using CodeCasa.Lights;

namespace CodeCasa.AutomationPipelines.Lights.Pipeline
{
    /// <summary>
    /// Configures a light transition pipeline for a single light entity.
    /// This configurator allows adding pipeline nodes, reactive nodes, dimmers, and conditional logic to light automation.
    /// </summary>
    internal partial class LightTransitionPipelineConfigurator(
        IServiceProvider serviceProvider,
        LightPipelineFactory lightPipelineFactory,
        ReactiveNodeFactory reactiveNodeFactory,
        ILight light)
        : ILightTransitionPipelineConfigurator
    {
        private readonly List<IPipelineNode<LightTransition>> _nodes = new();

        internal ILight Light { get; } = light;
        internal string? Name { get; private set; }
        internal bool? Log { get; private set; }

        public IReadOnlyCollection<IPipelineNode<LightTransition>> Nodes => _nodes.AsReadOnly();

        public ILightTransitionPipelineConfigurator
            AddConditional(IObservable<bool> observable, Action<ILightTransitionPipelineConfigurator> trueConfigure, Action<ILightTransitionPipelineConfigurator> falseConfigure)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public ILightTransitionPipelineConfigurator EnableLogging(string? pipelineName = null)
        {
            Name = pipelineName;
            Log = true;
            return this;
        }

        /// <inheritdoc/>
        public ILightTransitionPipelineConfigurator DisableLogging()
        {
            Log = false;
            return this;
        }

        /// <inheritdoc/>
        public ILightTransitionPipelineConfigurator AddNode<TNode>() where TNode : IPipelineNode<LightTransition>
        {
            _nodes.Add(serviceProvider.CreateInstanceWithinContext<TNode>(Light));
            return this;
        }

        /// <summary>
        /// Adds a pipeline node to the pipeline.
        /// </summary>
        /// <param name="node">The pipeline node to add.</param>
        /// <returns>The configurator instance for method chaining.</returns>
        public ILightTransitionPipelineConfigurator AddNode(IPipelineNode<LightTransition> node)
        {
            _nodes.Add(node);
            return this;
        }

        /// <inheritdoc/>
        public ILightTransitionPipelineConfigurator AddNode(Func<ILightPipelineContext, IPipelineNode<LightTransition>> nodeFactory)
        {
            _nodes.Add(nodeFactory(new LightPipelineContext(serviceProvider, Light)));
            return this;
        }

        /// <inheritdoc/>
        public ILightTransitionPipelineConfigurator AddReactiveNode(
            Action<ILightTransitionReactiveNodeConfigurator> configure)
        {
            return AddNode(reactiveNodeFactory.CreateReactiveNode(Light, c =>
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
        public ILightTransitionPipelineConfigurator AddDimmer(IDimmer dimmer)
        {
            return AddDimmer(dimmer, _ => { });
        }

        /// <inheritdoc/>
        public ILightTransitionPipelineConfigurator AddDimmer(IDimmer dimmer, Action<DimmerOptions> dimOptions)
        {
            return AddReactiveNode(c =>
            {
                c.AddUncoupledDimmer(dimmer, dimOptions);
            });
        }

        /// <inheritdoc/>
        public ILightTransitionPipelineConfigurator ForLight(string lightId,
            Action<ILightTransitionPipelineConfigurator> compositeNodeBuilder) =>
            ForLights([lightId], compositeNodeBuilder);

        /// <inheritdoc/>
        public ILightTransitionPipelineConfigurator ForLight(ILight light,
            Action<ILightTransitionPipelineConfigurator> compositeNodeBuilder) => ForLights([light], compositeNodeBuilder);

        /// <inheritdoc/>
        public ILightTransitionPipelineConfigurator ForLights(IEnumerable<string> lightIds,
            Action<ILightTransitionPipelineConfigurator> compositeNodeBuilder)
        {
            CompositeHelper.ValidateLightSupported(lightIds, Light.Id);
            return this;
        }

        /// <inheritdoc/>
        public ILightTransitionPipelineConfigurator ForLights(IEnumerable<ILight> lights,
            Action<ILightTransitionPipelineConfigurator> compositeNodeBuilder)
        {
            CompositeHelper.ResolveGroupsAndValidateLightSupported(lights, Light.Id);
            return this;
        }

        /// <inheritdoc/>
        public ILightTransitionPipelineConfigurator AddPipeline(Action<ILightTransitionPipelineConfigurator> pipelineNodeOptions) => AddNode(lightPipelineFactory.CreateLightPipeline(Light, pipelineNodeOptions));
    }
}
