using Microsoft.Extensions.Logging;
using System.Reactive.Concurrency;
using CodeCasa.AutomationPipelines.Lights.ReactiveNode;
using CodeCasa.Lights;
using CodeCasa.Lights.Extensions;

namespace CodeCasa.AutomationPipelines.Lights.Pipeline
{
    /// <summary>
    /// Factory for creating and configuring light transition pipelines.
    /// </summary>
    public class LightPipelineFactory(
        ILogger<Pipeline<LightTransition>> logger, IServiceProvider serviceProvider, ReactiveNodeFactory reactiveNodeFactory, IScheduler scheduler)
    {
        /// <summary>
        /// Sets up a light pipeline for the specified light entity and configures it with the provided builder action.
        /// </summary>
        /// <param name="lightEntity">The light entity to set up the pipeline for.</param>
        /// <param name="pipelineBuilder">An action to configure the pipeline behavior.</param>
        /// <returns>An async disposable representing the created pipeline(s) that can be disposed to clean up resources.</returns>
        public IAsyncDisposable SetupLightPipeline(ILight lightEntity,
            Action<ILightTransitionPipelineConfigurator> pipelineBuilder)
        {
            var disposables = new CompositeAsyncDisposable();
            var pipelines = CreateLightPipelines(lightEntity.Flatten(), pipelineBuilder);
            foreach (var pipeline in pipelines.Values)
            {
                disposables.Add(pipeline);
            }
            return disposables;
        }

        /// <summary>
        /// Creates a single light pipeline for the specified light entity.
        /// </summary>
        /// <param name="lightEntity">The light entity to create a pipeline for.</param>
        /// <param name="pipelineBuilder">An action to configure the pipeline behavior.</param>
        /// <returns>A configured pipeline for controlling the specified light.</returns>
        internal IPipeline<LightTransition> CreateLightPipeline(ILight lightEntity, Action<ILightTransitionPipelineConfigurator> pipelineBuilder)
        {
            return CreateLightPipelines([lightEntity], pipelineBuilder)[lightEntity.Id];
        }

        /// <summary>
        /// Creates light pipelines for multiple light entities.
        /// </summary>
        /// <param name="lightEntities">The light entities to create pipelines for.</param>
        /// <param name="pipelineBuilder">An action to configure the pipeline behavior.</param>
        /// <returns>A dictionary mapping light entity IDs to their corresponding pipelines.</returns>
        internal Dictionary<string, IPipeline<LightTransition>> CreateLightPipelines(IEnumerable<ILight> lightEntities, Action<ILightTransitionPipelineConfigurator> pipelineBuilder)
        {
            // Note: we simply assume that these are not groups.
            var lightEntityArray = lightEntities.ToArray();
            if (!lightEntityArray.Any())
            {
                return new Dictionary<string, IPipeline<LightTransition>>();
            }

            var configurators = lightEntityArray.ToDictionary(l => l.Id, l => new LightTransitionPipelineConfigurator(serviceProvider, this, reactiveNodeFactory, l));
            ILightTransitionPipelineConfigurator configurator = lightEntityArray.Length == 1
                ? configurators[lightEntityArray[0].Id]
                : new CompositeLightTransitionPipelineConfigurator(
                    serviceProvider,
                    this,
                    reactiveNodeFactory,
                    configurators, scheduler);
            pipelineBuilder(configurator);

            return configurators.ToDictionary(kvp => kvp.Key, kvp =>
            {
                var conf = kvp.Value;
                return (IPipeline<LightTransition>)new Pipeline<LightTransition>(
                    conf.Name ?? conf.LightEntity.Id,
                    LightTransition.Off(),
                    conf.Nodes,
                    conf.LightEntity.ApplyTransition,
                    logger);
            });
        }
    }
}
