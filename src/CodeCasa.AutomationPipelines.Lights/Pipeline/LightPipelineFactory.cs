using Microsoft.Extensions.Logging;
using System.Reactive.Concurrency;
using CodeCasa.AutomationPipelines.Lights.ReactiveNode;
using CodeCasa.Lights;
using CodeCasa.Lights.Extensions;

namespace CodeCasa.AutomationPipelines.Lights.Pipeline
{
    public class LightPipelineFactory(
        ILogger<Pipeline<LightTransition>> logger, IServiceProvider serviceProvider, ReactiveNodeFactory reactiveNodeFactory, IScheduler scheduler)
    {
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

        internal IPipeline<LightTransition> CreateLightPipeline(ILight lightEntity, Action<ILightTransitionPipelineConfigurator> pipelineBuilder)
        {
            return CreateLightPipelines([lightEntity], pipelineBuilder)[lightEntity.Id];
        }

        internal Dictionary<string, IPipeline<LightTransition>> CreateLightPipelines(IEnumerable<ILight> lightEntities, Action<ILightTransitionPipelineConfigurator> pipelineBuilder)
        {
            // Note: we simply assume that these are not groups.
            var lightEntityArray = lightEntities.ToArray();
            if (!lightEntityArray.Any())
            {
                return new Dictionary<string, IPipeline<LightTransition>>();
            }

            var configurators = lightEntityArray.ToDictionary(l => l.Id, l => new LightTransitionPipelineConfigurator(serviceProvider, this, reactiveNodeFactory, l, scheduler));
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
