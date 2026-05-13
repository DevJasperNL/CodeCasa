using CodeCasa.AutomationPipelines.Lights.Extensions;
using CodeCasa.AutomationPipelines.Lights.Nodes;
using CodeCasa.AutomationPipelines.Lights.ReactiveNode;
using CodeCasa.Lights;
using CodeCasa.Lights.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;

namespace CodeCasa.AutomationPipelines.Lights.Pipeline
{
    /// <summary>
    /// Factory for creating and configuring light transition pipelines.
    /// </summary>
    public class LightPipelineFactory(
        ILogger<Pipeline<LightTransition>> logger, IServiceProvider serviceProvider)
    {
        /// <summary>
        /// Sets up a light pipeline for the specified light and configures it with the provided builder action.
        /// </summary>
        /// <typeparam name="TLight">The specific type of light.</typeparam>
        /// <param name="light">The light to set up the pipeline for.</param>
        /// <param name="pipelineBuilder">An action to configure the pipeline behavior.</param>
        /// <returns>An async disposable representing the created pipeline(s) that can be disposed to clean up resources.</returns>
        public IAsyncDisposable SetupLightPipeline<TLight>(TLight light,
            Action<ILightTransitionPipelineConfigurator<TLight>> pipelineBuilder) where TLight : ILight
        {
            var disposables = new CompositeAsyncDisposable();
            var pipelines = CreateLightPipelines(light.Flatten().Cast<TLight>(), pipelineBuilder);
            foreach (var pipeline in pipelines.Values)
            {
                disposables.Add(pipeline);
            }
            return disposables;
        }

        /// <summary>
        /// Creates a single light pipeline for the specified light.
        /// </summary>
        /// <param name="light">The light to create a pipeline for.</param>
        /// <param name="pipelineBuilder">An action to configure the pipeline behavior.</param>
        /// <returns>A configured pipeline for controlling the specified light.</returns>
        public IPipeline<LightTransition> CreateLightPipeline<TLight>(TLight light, Action<ILightTransitionPipelineConfigurator<TLight>> pipelineBuilder) where TLight : ILight
        {
            return CreateLightPipelines([light], pipelineBuilder)[light.Id];
        }

        /// <summary>
        /// Creates a mapping of light identifiers to factory delegates that resolve pipeline nodes.
        /// Each factory produces a <see cref="IPipelineNode{T}"/> that manages a shared pipeline registry 
        /// and ensures groups of pipelines are created together allowing their configuration to be aware of each other.
        /// </summary>
        /// <typeparam name="TLight">The type of light, constrained to <see cref="ILight"/>.</typeparam>
        /// <param name="pipelineConfigurator">The configuration action used to initialize the pipeline logic.</param>
        /// <param name="lights">The collection of lights for which pipelines will be created.</param>
        /// <returns>
        /// A <see cref="Dictionary{TKey, TValue}"/> where the key is the light ID and the value is a 
        /// function that resolves the corresponding <see cref="IPipelineNode{LightTransition}"/>.
        /// </returns>
        internal Dictionary<string, Func<IServiceProvider, IPipelineNode<LightTransition>>> CreateCompositePipelineFactoryMap<TLight>(Action<ILightTransitionPipelineConfigurator<TLight>> pipelineConfigurator, TLight[] lights) where TLight : ILight
        {
            var baseFactory = new CompositePipelineFactory<TLight>(pipelineConfigurator, lights);
            return lights
                .ToDictionary(
                    l => l.Id,
                    l =>
                        (Func<IServiceProvider, IPipelineNode<LightTransition>>)(
                            sp => new ScopedPipelineNode<LightTransition>(
                                baseFactory.GetOrCreatePipeline(sp, l.Id),
                                Disposable.Create(() => baseFactory.Clear()))));
        }

        /// <summary>
        /// Creates light pipelines for multiple light entities.
        /// </summary>
        /// <param name="lights">The light entities to create pipelines for.</param>
        /// <param name="pipelineBuilder">An action to configure the pipeline behavior.</param>
        /// <returns>A dictionary mapping light IDs to their corresponding pipelines.</returns>
        internal Dictionary<string, IPipeline<LightTransition>> CreateLightPipelines<TLight>(IEnumerable<TLight> lights, Action<ILightTransitionPipelineConfigurator<TLight>> pipelineBuilder) where TLight : ILight
        {
            // Note: we simply assume that these are not groups.
            var lightArray = lights.ToArray();
            if (!lightArray.Any())
            {
                return new Dictionary<string, IPipeline<LightTransition>>();
            }

            var lightContextScopes = lightArray.ToDictionary(l => l.Id, serviceProvider.CreateLightContextScope);
            var configurators = 
                lightArray.ToDictionary(l => l.Id,
                    l =>
                    {
                        var sp = lightContextScopes[l.Id].ServiceProvider;
                        // Note: we cant resolve LightTransitionPipelineConfigurator directly because it is not registered as a service.
                        return new LightTransitionPipelineConfigurator<TLight>(sp, l);
                    });
            ILightTransitionPipelineConfigurator<TLight> configurator = lightArray.Length == 1
                ? configurators[lightArray[0].Id]
                : new CompositeLightTransitionPipelineConfigurator<TLight>(
                    serviceProvider,
                    serviceProvider.GetRequiredService<LightPipelineFactory>(),
                    serviceProvider.GetRequiredService<ReactiveNodeFactory>(),
                    configurators);
            pipelineBuilder(configurator);

            return configurators.ToDictionary(kvp => kvp.Key, kvp =>
            {
                var conf = kvp.Value;
                IPipeline<LightTransition> pipeline = new Pipeline<LightTransition>(
                    LightTransition.Off(),
                    conf.Nodes,
                    conf.Light.ApplyTransition,
                    conf.EqualityComparer)
                {
                    Name = conf.Name
                };
                if (conf.LoggingEnabled ?? false)
                {
                    var pipelineLogger = new PipelineLogger<LightTransition>(logger, $"[{conf.Light.Id}] {conf.HierarchyPath}");
                    pipeline.Telemetry.Subscribe(t => pipelineLogger.Log(t));
                }

                var telemetryStream = pipeline.Telemetry
                    .Select(t => new LightTransitionPipelineTelemetry<TLight>(
                        pipeline,
                        conf.Light, t.SourceNodeIndex, t.SourceNodeName, t.DestinationNodeIndex,
                        t.DestinationNodeName, t.StateValue))
                    .Publish()
                    .RefCount();
                var subscriptions = conf.TelemetrySubscriptionFactories
                    .Select(factory => factory(telemetryStream))
                    .ToArray();

                var scopedSp = lightContextScopes[kvp.Key].ServiceProvider;
                var pipelineContext = scopedSp.GetRequiredService<LightPipelineContext>();
                var scheduler = scopedSp.GetRequiredService<IScheduler>();
                var contextSubscription = pipeline.OnNewOutput
                    .Subscribe(output => pipelineContext.Update(output, scheduler.Now));
                subscriptions = [.. subscriptions, contextSubscription];

                foreach (var completedCallback in conf.PipelineCompletedCallbacks)
                {
                    completedCallback(new LightTransitionPipelineCreatedEvent<TLight>(pipeline, conf.Light));
                }

                return (IPipeline<LightTransition>)new ManagedPipeline<LightTransition>(lightContextScopes[kvp.Key], pipeline, subscriptions);
            });
        }

        private class CompositePipelineFactory<TLight>(Action<ILightTransitionPipelineConfigurator<TLight>> pipelineConfigurator, IEnumerable<TLight> lights) where TLight : ILight
        {
            private readonly Lock _lock = new();
            private Dictionary<string, IPipeline<LightTransition>>? _pipelines;

            public IPipeline<LightTransition> GetOrCreatePipeline(IServiceProvider serviceProvider, string lightId)
            {
                lock (_lock)
                {
                    if (_pipelines == null)
                    {
                        var pipelineFactory = serviceProvider.GetRequiredService<LightPipelineFactory>();
                        _pipelines = pipelineFactory.CreateLightPipelines(lights, pipelineConfigurator);
                    }

                    return _pipelines[lightId];
                }
            }

            public void Clear()
            {
                lock (_lock)
                {
                    // Note: this class is not responsible for the lifetime of the pipelines, it just manages their creation and provides access to them.
                    _pipelines = null;
                }
            }
        }
    }
}
