using CodeCasa.AutomationPipelines.Lights.Extensions;
using CodeCasa.AutomationPipelines.Lights.Nodes;
using CodeCasa.AutomationPipelines.Lights.ReactiveNode;
using CodeCasa.Lights;
using CodeCasa.Lights.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Reactive.Concurrency;
using System.Reactive.Linq;

namespace CodeCasa.AutomationPipelines.Lights.Pipeline;

/// <summary>
/// Factory for creating and configuring light transition pipelines.
/// </summary>
public class LightPipelineFactory(
    ILogger<Pipeline<LightTransition>> logger, IServiceProvider rootServiceProvider)
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
        var pipelines = CreateLightPipelines(rootServiceProvider, light.Flatten().Cast<TLight>().ToDictionary(l => l, _ => rootServiceProvider), pipelineBuilder);
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
    public IPipeline<LightTransition> CreateLightPipeline<TLight>(TLight light,
        Action<ILightTransitionPipelineConfigurator<TLight>> pipelineBuilder) where TLight : ILight
        => CreateLightPipeline(rootServiceProvider, light, pipelineBuilder);

    /// <summary>
    /// Creates a single light pipeline for the specified light.
    /// </summary>
    /// <param name="compositeServiceProvider">The service provider passed to the configurators. This method is used within the library to allow passing the composite service provider. This is necessary because the factory will only receive the root service provider even if resolved inside the context scope.</param>
    /// <param name="light">The light to create a pipeline for.</param>
    /// <param name="pipelineBuilder">An action to configure the pipeline behavior.</param>
    /// <returns>A configured pipeline for controlling the specified light.</returns>
    internal IPipeline<LightTransition> CreateLightPipeline<TLight>(IServiceProvider compositeServiceProvider, TLight light, Action<ILightTransitionPipelineConfigurator<TLight>> pipelineBuilder) where TLight : ILight
    {
        return CreateLightPipelines(compositeServiceProvider, new Dictionary<TLight, IServiceProvider> { { light, compositeServiceProvider } }, pipelineBuilder)[light.Id];
    }

    /// <summary>
    /// Creates a mapping of light identifiers to factory delegates that resolve pipeline nodes.
    /// Each factory produces a <see cref="IPipelineNode{T}"/> that manages a shared pipeline registry 
    /// and ensures groups of pipelines are created together allowing their configuration to be aware of each other.
    /// </summary>
    /// <typeparam name="TLight">The type of light, constrained to <see cref="ILight"/>.</typeparam>
    /// <param name="pipelineConfigurator">The configuration action used to initialize the pipeline logic.</param>
    /// <param name="lightsAndProviders">Mapping to the lights to their respective service providers. This allows the factory to create context-specific scopes for each light.</param>
    /// <returns>
    /// A <see cref="Dictionary{TKey, TValue}"/> where the key is the light ID and the value is a 
    /// function that resolves the corresponding <see cref="IPipelineNode{LightTransition}"/>.
    /// </returns>
    internal Dictionary<string, Func<IServiceProvider, IPipelineNode<LightTransition>>> CreateCompositePipelineFactoryMap<TLight>(Action<ILightTransitionPipelineConfigurator<TLight>> pipelineConfigurator, Dictionary<TLight, IServiceProvider> lightsAndProviders) where TLight : ILight
    {
        var baseFactory = new CompositePipelineFactory<TLight>(pipelineConfigurator, lightsAndProviders);
        return lightsAndProviders.Keys
            .ToDictionary(
                l => l.Id,
                l => (Func<IServiceProvider, IPipelineNode<LightTransition>>)(sp => baseFactory.TakePipeline(sp, l.Id)));
    }

    /// <summary>
    /// Creates light pipelines for multiple light entities.
    /// </summary>
    /// <param name="compositeServiceProvider">The service provider passed to the configurators. This method is used within the library to allow passing the composite service provider. This is necessary because the factory will only receive the root service provider even if resolved inside the context scope.</param>
    /// <param name="lightsAndProviders">Mapping to the lights to their respective service providers. This allows the factory to create context-specific scopes for each light.</param>
    /// <param name="pipelineBuilder">An action to configure the pipeline behavior.</param>
    /// <returns>A dictionary mapping light IDs to their corresponding pipelines.</returns>
    internal Dictionary<string, IPipeline<LightTransition>> CreateLightPipelines<TLight>(IServiceProvider compositeServiceProvider, Dictionary<TLight, IServiceProvider> lightsAndProviders, Action<ILightTransitionPipelineConfigurator<TLight>> pipelineBuilder) where TLight : ILight
    {
        // Note: we simply assume that these are not groups.
        var lightArray = lightsAndProviders.Keys.ToArray();
        if (!lightArray.Any())
        {
            return new Dictionary<string, IPipeline<LightTransition>>();
        }

        var ownsPipelineContext = lightsAndProviders.ToDictionary(kvp => kvp.Key.Id, kvp => kvp.Value.OwnsLightPipelineContext(kvp.Key));
        var lightContextScopes = lightsAndProviders.ToDictionary(kvp => kvp.Key.Id, kvp => kvp.Value.CreateLightPipelineContextScope(kvp.Key));
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
                compositeServiceProvider,
                compositeServiceProvider.GetRequiredService<LightPipelineFactory>(),
                compositeServiceProvider.GetRequiredService<ReactiveNodeFactory>(),
                configurators);
        pipelineBuilder(configurator);
        ValidateLightGroupMembership(configurators);

        var groupContext = new GroupNodeContext(compositeServiceProvider.GetRequiredService<IScheduler>(), logger);

        // All group members must be registered before any pipeline pushes its default state, otherwise the first
        // pipeline reaches "consensus" on its own and the group entity is driven with partial membership.
        var groupNodes = new Dictionary<string, GroupNode>();
        foreach (var (lightId, conf) in configurators)
        {
            if (!conf.LightGroups.Any())
            {
                continue;
            }
            if (!ownsPipelineContext[lightId])
            {
                // A group node drives the group entity directly, which would bypass the root pipeline and any node after this nested pipeline.
                throw new InvalidOperationException(
                    $"{nameof(ILightTransitionPipelineConfigurator<TLight>.UseLightGroup)} can only be used on the root pipeline of a light, not on a nested pipeline ({conf.HierarchyPath}, light {lightId}).");
            }

            var groupNode = new GroupNode(groupContext, conf.DistinctEqualityComparer);
            foreach (var lightGroup in conf.LightGroups)
            {
                groupContext.Register(groupNode, lightGroup.Key, lightGroup.Value.TimeSpan, lightGroup.Value.Comparer);
            }
            groupNodes[lightId] = groupNode;
        }

        return configurators.ToDictionary(kvp => kvp.Key, kvp =>
        {
            var conf = kvp.Value;
            var nodes = conf.Nodes.ToList();
            var light = conf.Light;
            Action<LightTransition> outputHandler = light.ApplyTransition;

            if (groupNodes.TryGetValue(kvp.Key, out var groupNode))
            {
                nodes.Add(groupNode);
                outputHandler = transition =>
                {
                    if (!groupNode.OutputAppliedByGroup)
                    {
                        light.ApplyTransition(transition);
                    }
                };
            }

            // The handler is installed before the default state flows so group consensus during start-up is respected.
            // Nested pipelines only feed their parent; the root pipeline is the single place the light is driven.
            IPipeline<LightTransition> pipeline = new Pipeline<LightTransition>(nodes)
            {
                Name = conf.Name
            };
            if (ownsPipelineContext[kvp.Key])
            {
                pipeline.SetOutputHandler(outputHandler, conf.DistinctEqualityComparer);
            }
            pipeline.SetDefault(LightTransition.Off());
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

            // Only the root pipeline of a light updates the shared context; nested pipeline outputs are not what the light receives.
            if (ownsPipelineContext[kvp.Key])
            {
                var scopedSp = lightContextScopes[kvp.Key].ServiceProvider;
                var pipelineContext = scopedSp.GetRequiredService<LightPipelineContext>();
                var scheduler = scopedSp.GetRequiredService<IScheduler>();
                if (pipeline.Output != null)
                {
                    pipelineContext.Update(pipeline.Output, scheduler.Now);
                }
                var contextSubscription = pipeline.OnNewOutput
                    .Subscribe(output =>
                    {
                        pipelineContext.Update(output, scheduler.Now);
                    });
                subscriptions = [.. subscriptions, contextSubscription];
            }

            foreach (var completedCallback in conf.PipelineCompletedCallbacks)
            {
                completedCallback(new LightTransitionPipelineCreatedEvent<TLight>(pipeline, conf.Light));
            }

            return (IPipeline<LightTransition>)new ManagedPipeline<LightTransition>(lightContextScopes[kvp.Key], pipeline, subscriptions);
        });
    }

    private static void ValidateLightGroupMembership<TLight>(Dictionary<string, LightTransitionPipelineConfigurator<TLight>> configurators) where TLight : ILight
    {
        var registrationsByGroupId = configurators
            .SelectMany(kvp => kvp.Value.LightGroups.Keys.Select(lightGroup => (LightGroup: lightGroup, LightId: kvp.Key)))
            .GroupBy(registration => registration.LightGroup.Id);

        foreach (var registrations in registrationsByGroupId)
        {
            var lightGroup = registrations.First().LightGroup;
            if (!lightGroup.GetChildren().Any())
            {
                // Membership is unknown (for example a group entity without an entity_id attribute), so it cannot be validated.
                continue;
            }

            // Consensus between the registered lights drives the group entity, so every member of the group must be one of them.
            var memberIds = lightGroup.Flatten().Select(l => l.Id).ToHashSet();
            var registeredIds = registrations.Select(r => r.LightId).ToHashSet();
            if (memberIds.SetEquals(registeredIds))
            {
                continue;
            }

            var missing = memberIds.Except(registeredIds).ToArray();
            var extra = registeredIds.Except(memberIds).ToArray();
            throw new InvalidOperationException(
                $"Light group {lightGroup.Id} must be used for exactly its member lights. " +
                (missing.Any() ? $"Members not using the group: {string.Join(", ", missing)}. " : "") +
                (extra.Any() ? $"Lights using the group that are not members: {string.Join(", ", extra)}." : ""));
        }
    }

    /// <summary>
    /// Creates the pipelines for all lights in one go (a "generation") and hands each light its own instance exactly once.
    /// A light asking again means a new trigger arrived, so a fresh generation is created. Ownership of a handed-out
    /// pipeline moves to the caller; instances nobody picked up are disposed when the next generation starts.
    /// </summary>
    private class CompositePipelineFactory<TLight>(Action<ILightTransitionPipelineConfigurator<TLight>> pipelineConfigurator, Dictionary<TLight, IServiceProvider> lightsAndProviders) where TLight : ILight
    {
        private readonly Lock _lock = new();
        private Dictionary<string, IPipeline<LightTransition>>? _pending;

        public IPipeline<LightTransition> TakePipeline(IServiceProvider serviceProvider, string lightId)
        {
            lock (_lock)
            {
                if (_pending == null || !_pending.ContainsKey(lightId))
                {
                    DisposePending();
                    var pipelineFactory = serviceProvider.GetRequiredService<LightPipelineFactory>();
                    _pending = pipelineFactory.CreateLightPipelines(serviceProvider, lightsAndProviders, pipelineConfigurator);
                }

                _pending.Remove(lightId, out var pipeline);
                if (_pending.Count == 0)
                {
                    _pending = null;
                }
                return pipeline!;
            }
        }

        private void DisposePending()
        {
            if (_pending == null)
            {
                return;
            }

            foreach (var pipeline in _pending.Values)
            {
                pipeline.DisposeAsync().AsTask().GetAwaiter().GetResult();
            }
            _pending = null;
        }
    }
}