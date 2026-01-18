using CodeCasa.AutomationPipelines.Lights.Extensions;
using CodeCasa.AutomationPipelines.Lights.ReactiveNode;
using CodeCasa.AutomationPipelines.Lights.Utils;
using CodeCasa.Lights;
using CodeCasa.Lights.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Linq;

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
                IPipeline<LightTransition> pipeline;
                if (conf.Log ?? false)
                {
                    pipeline= new Pipeline<LightTransition>(
                        conf.Name ?? conf.Light.Id,
                        LightTransition.Off(),
                        conf.Nodes,
                        conf.Light.ApplyTransition,
                        logger);
                }
                else
                {
                    pipeline = new Pipeline<LightTransition>(
                        LightTransition.Off(),
                        conf.Nodes,
                        conf.Light.ApplyTransition);
                }

                return (IPipeline<LightTransition>)new ScopedPipeline<LightTransition>(lightContextScopes[kvp.Key], pipeline);
            });
        }
    }

    public sealed class ScopedPipeline<TNode> : IPipeline<TNode>, IDisposable, IAsyncDisposable
    {
        private readonly IServiceScope _scope;

        public IPipeline<TNode> Instance { get; }

        public ScopedPipeline(IServiceScope scope, IPipeline<TNode> pipeline)
        {
            _scope = scope ?? throw new ArgumentNullException(nameof(scope));
            Instance = pipeline ?? throw new ArgumentNullException(nameof(pipeline));
        }

        public void Dispose()
        {
            (Instance as IDisposable)?.Dispose();
            _scope.Dispose();
        }

        public async ValueTask DisposeAsync()
        {
            await Instance.DisposeOrDisposeAsync();
            await _scope.DisposeOrDisposeAsync();
        }

        public TNode? Input { get; set; }
        public TNode? Output { get; }
        public IObservable<TNode?> OnNewOutput { get; }
        public IPipeline<TNode> SetDefault(TNode state)
        {
            throw new NotImplementedException();
        }

        public IPipeline<TNode> RegisterNode<TNode1>() where TNode1 : IPipelineNode<TNode>
        {
            throw new NotImplementedException();
        }

        public IPipeline<TNode> RegisterNode(IPipelineNode<TNode> node)
        {
            throw new NotImplementedException();
        }

        public IPipeline<TNode> SetOutputHandler(Action<TNode> action, bool callActionDistinct = true)
        {
            throw new NotImplementedException();
        }
    }
}
