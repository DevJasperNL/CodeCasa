using CodeCasa.AutomationPipelines.Lights.Extensions;
using CodeCasa.AutomationPipelines.Lights.Nodes;
using CodeCasa.AutomationPipelines.Lights.Pipeline;
using CodeCasa.Lights;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace CodeCasa.AutomationPipelines.Lights.ReactiveNode
{
    /// <summary>
    /// Factory for creating reactive nodes that dynamically switch between child nodes based on observable inputs.
    /// </summary>
    public class ReactiveNodeFactory(IServiceProvider rootServiceProvider, IScheduler scheduler)
    {
        /// <summary>
        /// Creates a reactive node for a single light.
        /// </summary>
        /// <typeparam name="TLight">The type of light being controlled.</typeparam>
        /// <param name="light">The light to create the reactive node for.</param>
        /// <param name="configure">An action to configure the reactive node.</param>
        /// <returns>A configured reactive node for the specified light.</returns>
        public IPipelineNode<LightTransition> CreateReactiveNode<TLight>(TLight light, Action<ILightTransitionReactiveNodeConfigurator<TLight>> configure) where TLight : ILight
            => CreateReactiveNode(rootServiceProvider, light, configure);

        /// <summary>
        /// Creates a reactive node for a single light.
        /// </summary>
        /// <typeparam name="TLight">The type of light being controlled.</typeparam>
        /// <param name="serviceProvider">The service provider passed to the configurators. This method is used within the library to allow passing the composite service provider. This is necessary because the factory will only receive the root service provider even if resolved inside the context scope.</param>
        /// <param name="light">The light to create the reactive node for.</param>
        /// <param name="configure">An action to configure the reactive node.</param>
        /// <returns>A configured reactive node for the specified light.</returns>
        public IPipelineNode<LightTransition> CreateReactiveNode<TLight>(IServiceProvider serviceProvider, TLight light, Action<ILightTransitionReactiveNodeConfigurator<TLight>> configure) where TLight : ILight
        {
            return CreateReactiveNodes(serviceProvider, [light], configure)[light.Id];
        }

        /// <summary>
        /// Creates a mapping of light identifiers to factory delegates that resolve reactive nodes.
        /// Each factory produces a <see cref="IPipelineNode{T}"/> that manages a shared node registry 
        /// and ensures groups of nodes are created together allowing their configuration to be aware of each other.
        /// </summary>
        /// <typeparam name="TLight">The type of light, constrained to <see cref="ILight"/>.</typeparam>
        /// <param name="reactiveNodeConfigurator">The configuration action used to initialize the node logic.</param>
        /// <param name="lights">The collection of lights for which reactive nodes will be created.</param>
        /// <returns>
        /// A <see cref="Dictionary{TKey, TValue}"/> where the key is the light ID and the value is a 
        /// function that resolves the corresponding <see cref="IPipelineNode{LightTransition}"/>.
        /// </returns>
        internal Dictionary<string, Func<IServiceProvider, IPipelineNode<LightTransition>>> CreateCompositeReactiveNodeFactoryMap<TLight>(Action<ILightTransitionReactiveNodeConfigurator<TLight>> reactiveNodeConfigurator, TLight[] lights) where TLight : ILight
        {
            var baseFactory = new CompositeReactiveNodeFactory<TLight>(reactiveNodeConfigurator, lights);
            return lights
                .ToDictionary(
                    l => l.Id,
                    l =>
                        (Func<IServiceProvider, IPipelineNode<LightTransition>>)(
                            sp => new ScopedPipelineNode<LightTransition>(
                                baseFactory.GetOrCreateNode(sp, l.Id),
                                Disposable.Create(() => baseFactory.Clear()))));
        }

        /// <summary>
        /// Creates reactive nodes for multiple light entities.
        /// </summary>
        /// <typeparam name="TLight">The type of light being controlled.</typeparam>
        /// <param name="serviceProvider">The service provider passed to the configurators. This method is used within the library to allow passing the composite service provider. This is necessary because the factory will only receive the root service provider even if resolved inside the context scope.</param>
        /// <param name="lights">The light entities to create reactive nodes for.</param>
        /// <param name="configure">An action to configure the reactive nodes.</param>
        /// <returns>A dictionary mapping light IDs to their corresponding reactive nodes.</returns>
        internal Dictionary<string, IPipelineNode<LightTransition>> CreateReactiveNodes<TLight>(IServiceProvider serviceProvider, IEnumerable<TLight> lights, Action<ILightTransitionReactiveNodeConfigurator<TLight>> configure) where TLight : ILight
        {
            // Note: we simply assume that these are not groups.
            var lightArray = lights.ToArray();
            if (!lightArray.Any())
            {
                return new Dictionary<string, IPipelineNode<LightTransition>>();
            }

            var lightContextScopes = lightArray.ToDictionary(l => l.Id, serviceProvider.CreateLightContextScope);
            var reactiveConfigurators = 
                lightArray.ToDictionary(l => l.Id, 
                    l =>
                    {
                        var sp = lightContextScopes[l.Id].ServiceProvider;
                        // Note: we cant resolve LightTransitionReactiveNodeConfigurator directly because it is not registered as a service.
                        return new LightTransitionReactiveNodeConfigurator<TLight>(sp, l, sp.GetRequiredService<IScheduler>());
                    });
            ILightTransitionReactiveNodeConfigurator<TLight> configurator = lightArray.Length == 1
                ? reactiveConfigurators[lightArray[0].Id]
                : new CompositeLightTransitionReactiveNodeConfigurator<TLight>(
                    serviceProvider,
                    serviceProvider.GetRequiredService<LightPipelineFactory>(),
                    serviceProvider.GetRequiredService<ReactiveNodeFactory>(),
                    reactiveConfigurators,
                    scheduler);
            configure(configurator);

            /*
             * Note: for now this implementation does not support assigning specific dimmers to specific children.
             * The nicest way to achieve this would be to create a pulse observable that emits a IDimmer[] for every pulse given, reflecting the dimmers that are currently pushed and providing the pulse.
             * This array should then be compared to a dictionary that contains which dimmer node (and entity) relate to which dimmers.
             * Then simply build the context and dim/brighten only for those dimmers.
             */
            var dimmers = reactiveConfigurators.Values
                .SelectMany(rnc => rnc.Dimmers)
                .Distinct()
                .ToArray();

            if (!dimmers.Any())
            {
                return lightArray.ToDictionary(l => l.Id, l =>
                {
                    var reactiveNode = CreateReactiveNodeInternal(serviceProvider, reactiveConfigurators[l.Id]);
                    return (IPipelineNode<LightTransition>)reactiveNode;
                });
            }

            var registrationManager = new RegistrationManager<ReactiveDimmerPipeline>();
            var dimmerNodes = new Dictionary<string, ReactiveDimmerNode>();
            var result = new Dictionary<string, IPipelineNode<LightTransition>>();
            
            foreach (var light in lightArray)
            {
                var reactiveNodeConfigurator = reactiveConfigurators[light.Id];
                var reactiveNode = CreateReactiveNodeInternal(serviceProvider, reactiveNodeConfigurator);
                var lightDimmerOptions = reactiveNodeConfigurator.DimmerOptions;
                
                var dimmerNode = new ReactiveDimmerNode(
                    reactiveNode,
                    light.Id,
                    lightDimmerOptions.MinBrightness,
                    lightDimmerOptions.BrightnessStep,
                    scheduler);

                dimmerNodes.Add(light.Id, dimmerNode);
                var innerPipeline = new ReactiveDimmerPipeline(
                    lightContextScopes[light.Id], 
                    reactiveNode, dimmerNode, registrationManager);

                result.Add(light.Id, innerPipeline);
            }

            /*
             * Note: for now this implementation does not support assigning specific dimmers to specific children.
             * The same is true for the options. We simply pick the first as all options will be set to the same value.
             * If this ever changes, time between steps and entity order should be extracted to apply to every dimmer while the other properties can be applied to individual ones.
             */
            var dimmerOptions = reactiveConfigurators.First().Value.DimmerOptions;
            var dimmer = dimmers.Length > 1 ? new CompositeDimmer(dimmers) : dimmers[0];

            var dimPulses = dimmer.Dimming.ToPulsesWhenTrue(dimmerOptions.TimeBetweenSteps, scheduler);
            var brightenPulses = dimmer.Brightening.ToPulsesWhenTrue(dimmerOptions.TimeBetweenSteps, scheduler);

            var orderedDimNodes = dimmerOptions.ValidateAndOrderMultipleLightTypes(dimmerNodes);

            var dimSubscriptionDisposables = new CompositeDisposable();
            SubscribeToPulses(dimPulses, dimmerNodes, orderedDimNodes, dimSubscriptionDisposables, 
                (context, dn) => dn.DimStep(context));
            SubscribeToPulses(brightenPulses, dimmerNodes, orderedDimNodes, dimSubscriptionDisposables,
                (context, dn) => dn.BrightenStep(context));
            
            var lastUnregisteredSubscription = registrationManager.LastUnregistered.Subscribe(_ =>
            {
                dimSubscriptionDisposables.Dispose();
            });
            
            dimSubscriptionDisposables.Add(lastUnregisteredSubscription);

            return result;
        }

        private ReactiveNode CreateReactiveNodeInternal<TLight>(IServiceProvider serviceProvider, LightTransitionReactiveNodeConfigurator<TLight> reactiveNodeConfigurator) where TLight : ILight
        {
            if (reactiveNodeConfigurator.LoggingEnabled ?? false)
            {
                return new ReactiveNode(
                    $"[{reactiveNodeConfigurator.Light.Id}] {reactiveNodeConfigurator.HierarchyPath}",
                    reactiveNodeConfigurator.NodeObservables.Merge(),
                    serviceProvider.GetRequiredService<ILogger<ReactiveNode>>(),
                    reactiveNodeConfigurator.EqualityComparer)
                {
                    Name = reactiveNodeConfigurator.Name
                };
            }
            return new ReactiveNode(
                reactiveNodeConfigurator.NodeObservables.Merge(),
                reactiveNodeConfigurator.EqualityComparer)
            {
                Name = reactiveNodeConfigurator.Name
            };
        }

        private void SubscribeToPulses(
            IObservable<Unit> pulses,
            Dictionary<string, ReactiveDimmerNode> dimmerNodes,
            OrderedDictionary<string, ReactiveDimmerNode> orderedDimNodes,
            CompositeDisposable compositeDisposable,
            Action<DimmingContext, ReactiveDimmerNode> dimmerAction)
        {
            compositeDisposable.Add(pulses.Subscribe(_ =>
            {
                var context = CreateDimmingContext(orderedDimNodes);
                dimmerNodes.Values.ForEach(dn => dimmerAction(context, dn));
            }));
        }

        private DimmingContext CreateDimmingContext(OrderedDictionary<string, ReactiveDimmerNode> orderedDimNodes)
        {
            return new DimmingContext(orderedDimNodes
                .Select(kvp => (kvp.Key, kvp.Value.Output?.LightParameters)).ToArray());
        }

        private class CompositeReactiveNodeFactory<TLight>(Action<ILightTransitionReactiveNodeConfigurator<TLight>> reactiveNodeConfigurator, IEnumerable<TLight> lights) where TLight : ILight
        {
            private readonly Lock _lock = new();
            private Dictionary<string, IPipelineNode<LightTransition>>? _nodes;

            public IPipelineNode<LightTransition> GetOrCreateNode(IServiceProvider serviceProvider, string lightId)
            {
                lock (_lock)
                {
                    if (_nodes == null)
                    {
                        var pipelineFactory = serviceProvider.GetRequiredService<ReactiveNodeFactory>();
                        _nodes = pipelineFactory.CreateReactiveNodes(serviceProvider, lights, reactiveNodeConfigurator);
                    }

                    return _nodes[lightId];
                }
            }

            public void Clear()
            {
                lock (_lock)
                {
                    // Note: this class is not responsible for the lifetime of the pipelines, it just manages their creation and provides access to them.
                    _nodes = null;
                }
            }
        }

        /// <summary>
        /// Manages registration of items and notifies when the last item is unregistered.
        /// </summary>
        private sealed class RegistrationManager<T> : IRegisterInterface<T>, IDisposable
        {
            private readonly HashSet<T> _items = new();
            private readonly Lock _lock = new();
            private readonly Subject<Unit> _lastUnregistered = new();
            private bool _isDisposed;

            /// <summary>
            /// Gets an observable that emits when the last registered item is unregistered.
            /// </summary>
            public IObservable<Unit> LastUnregistered => _lastUnregistered;

            /// <inheritdoc />
            public void Register(T reference)
            {
                lock (_lock)
                {
                    ObjectDisposedException.ThrowIf(_isDisposed, typeof(RegistrationManager<T>));
                    _items.Add(reference);
                }
            }

            /// <inheritdoc />
            public void Unregister(T reference)
            {
                bool becameEmpty = false;

                lock (_lock)
                {
                    if (_isDisposed)
                        return;

                    if (_items.Remove(reference) && _items.Count == 0)
                        becameEmpty = true;
                }

                if (becameEmpty)
                    _lastUnregistered.OnNext(Unit.Default);
            }

            /// <inheritdoc />
            public void Dispose()
            {
                lock (_lock)
                {
                    if (_isDisposed)
                        return;

                    _isDisposed = true;
                    _items.Clear();
                }

                _lastUnregistered.OnCompleted();
                _lastUnregistered.Dispose();
            }
        }
    }

    /// <summary>
    /// Interface for managing registrations and tracking when the last item is unregistered.
    /// </summary>
    internal interface IRegisterInterface<in T>
    {
        /// <summary>
        /// Registers an item.
        /// </summary>
        void Register(T reference);

        /// <summary>
        /// Unregisters an item.
        /// </summary>
        void Unregister(T reference);
    }
}
