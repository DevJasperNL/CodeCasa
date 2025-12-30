using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using CodeCasa.AutomationPipelines.Lights.Extensions;
using CodeCasa.AutomationPipelines.Lights.Nodes;
using CodeCasa.AutomationPipelines.Lights.Pipeline;
using CodeCasa.Lights;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace CodeCasa.AutomationPipelines.Lights.ReactiveNode
{
    /// <summary>
    /// Factory for creating reactive nodes that dynamically switch between child nodes based on observable inputs.
    /// </summary>
    public class ReactiveNodeFactory(IServiceProvider serviceProvider, IScheduler scheduler)
    {
        /// <summary>
        /// Creates a reactive node for a single light entity.
        /// </summary>
        /// <param name="lightEntity">The light entity to create the reactive node for.</param>
        /// <param name="configure">An action to configure the reactive node.</param>
        /// <returns>A configured reactive node for the specified light entity.</returns>
        public IPipelineNode<LightTransition> CreateReactiveNode(ILight lightEntity, Action<ILightTransitionReactiveNodeConfigurator> configure)
        {
            return CreateReactiveNodes([lightEntity], configure)[lightEntity.Id];
        }

        /// <summary>
        /// Creates reactive nodes for multiple light entities.
        /// </summary>
        /// <param name="lightEntities">The light entities to create reactive nodes for.</param>
        /// <param name="configure">An action to configure the reactive nodes.</param>
        /// <returns>A dictionary mapping light entity IDs to their corresponding reactive nodes.</returns>
        internal Dictionary<string, IPipelineNode<LightTransition>> CreateReactiveNodes(IEnumerable<ILight> lightEntities, Action<ILightTransitionReactiveNodeConfigurator> configure)
        {
            // Note: we simply assume that these are not groups.
            var lightEntityArray = lightEntities.ToArray();
            if (!lightEntityArray.Any())
            {
                return new Dictionary<string, IPipelineNode<LightTransition>>();
            }

            var lightPipelineFactory = serviceProvider.GetRequiredService<LightPipelineFactory>();
            var reactiveConfigurators = lightEntityArray.ToDictionary(l => l.Id, l => new LightTransitionReactiveNodeConfigurator(serviceProvider, lightPipelineFactory,
                this, l, scheduler));
            ILightTransitionReactiveNodeConfigurator configurator = lightEntityArray.Length == 1
                ? reactiveConfigurators[lightEntityArray[0].Id]
                : new CompositeLightTransitionReactiveNodeConfigurator(
                    serviceProvider, 
                    lightPipelineFactory,
                    this,
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
                return lightEntityArray.ToDictionary(l => l.Id, l =>
                {
                    var reactiveNode = CreateReactiveNode(reactiveConfigurators[l.Id]);
                    return (IPipelineNode<LightTransition>)reactiveNode;
                });
            }

            var registrationManager = new RegistrationManager<ReactiveDimmerPipeline>();
            var dimmerNodes = new Dictionary<string, ReactiveDimmerNode>();
            var result = new Dictionary<string, IPipelineNode<LightTransition>>();
            
            foreach (var lightEntity in lightEntityArray)
            {
                var reactiveNodeConfigurator = reactiveConfigurators[lightEntity.Id];
                var reactiveNode = CreateReactiveNode(reactiveNodeConfigurator);
                var lightDimmerOptions = reactiveNodeConfigurator.DimmerOptions;
                
                var dimmerNode = new ReactiveDimmerNode(
                    reactiveNode,
                    lightEntity.Id,
                    lightDimmerOptions.MinBrightness,
                    lightDimmerOptions.BrightnessStep,
                    scheduler);

                dimmerNodes.Add(lightEntity.Id, dimmerNode);
                var innerPipeline = new ReactiveDimmerPipeline(reactiveNode, dimmerNode, registrationManager);

                result.Add(lightEntity.Id, innerPipeline);
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

            var orderedDimNodes = dimmerOptions.ValidateAndOrderMultipleLightEntityTypes(dimmerNodes);

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

        private ReactiveNode CreateReactiveNode(LightTransitionReactiveNodeConfigurator reactiveNodeConfigurator)
        {
            return new ReactiveNode(
                reactiveNodeConfigurator.Name,
                reactiveNodeConfigurator.NodeObservables.Merge(),
                serviceProvider.GetRequiredService<ILogger<ReactiveNode>>());
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
    }

    /// <summary>
    /// A pipeline that combines a reactive node with a reactive dimmer node for managing light transitions.
    /// </summary>
    internal class ReactiveDimmerPipeline : Pipeline<LightTransition>
    {
        private readonly IRegisterInterface<ReactiveDimmerPipeline> _registerInterface;

        /// <summary>
        /// Initializes a new instance of the <see cref="ReactiveDimmerPipeline"/> class.
        /// </summary>
        public ReactiveDimmerPipeline(
            ReactiveNode reactiveNode, 
            ReactiveDimmerNode reactiveDimmerNode,
            IRegisterInterface<ReactiveDimmerPipeline> registerInterface)
        {
            _registerInterface = registerInterface;
            _registerInterface.Register(this);
            RegisterNode(reactiveNode);
            RegisterNode(reactiveDimmerNode);
        }

        /// <inheritdoc />
        public override ValueTask DisposeAsync()
        {
            _registerInterface.Unregister(this);
            return base.DisposeAsync();
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

    /// <summary>
    /// Manages registration of items and notifies when the last item is unregistered.
    /// </summary>
    internal sealed class RegistrationManager<T> : IRegisterInterface<T>, IDisposable
    {
        private readonly HashSet<T> _items = new();
        private readonly Lock _lock = new();
        private readonly Subject<Unit> _lastUnregistered = new ();
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
