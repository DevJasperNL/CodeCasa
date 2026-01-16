using Microsoft.Extensions.Logging;
using CodeCasa.AutomationPipelines.Lights.ReactiveNode;
using CodeCasa.Lights;
using CodeCasa.Lights.Extensions;

namespace CodeCasa.AutomationPipelines.Lights.Pipeline
{
    /// <summary>
    /// Factory for creating and configuring light transition pipelines.
    /// </summary>
    public class LightPipelineFactory(
        ILogger<Pipeline<LightTransition>> logger, IServiceProvider serviceProvider, ReactiveNodeFactory reactiveNodeFactory)
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
        internal IPipeline<LightTransition> CreateLightPipeline<TLight>(TLight light, Action<ILightTransitionPipelineConfigurator<TLight>> pipelineBuilder) where TLight : ILight
        {
            return CreateLightPipelines([light], pipelineBuilder)[light.Id];
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

            var configurators = lightArray.ToDictionary(l => l.Id, l => new LightTransitionPipelineConfigurator<TLight>(serviceProvider, this, reactiveNodeFactory, l));
            ILightTransitionPipelineConfigurator<TLight> configurator = lightArray.Length == 1
                ? configurators[lightArray[0].Id]
                : new CompositeLightTransitionPipelineConfigurator<TLight>(
                    serviceProvider,
                    this,
                    reactiveNodeFactory,
                    configurators);
            pipelineBuilder(configurator);

            return configurators.ToDictionary(kvp => kvp.Key, kvp =>
            {
                var conf = kvp.Value;
                if (conf.Log ?? false)
                {
                    return new Pipeline<LightTransition>(
                        conf.Name ?? conf.Light.Id,
                        LightTransition.Off(),
                        conf.Nodes,
                        conf.Light.ApplyTransition,
                        logger);
                }

                return (IPipeline<LightTransition>)new Pipeline<LightTransition>(
                    LightTransition.Off(),
                    conf.Nodes,
                    conf.Light.ApplyTransition);
            });
        }
    }
}
