using CodeCasa.Lights.NetDaemon.Extensions;
using CodeCasa.Lights.NetDaemon.Generated;
using NetDaemon.HassModel.Entities;
using System.Reactive.Linq;

namespace CodeCasa.Lights.NetDaemon
{
    /// <summary>
    /// Adapts a NetDaemon light entity core to the <see cref="ILight"/> interface.
    /// </summary>
    public class NetDaemonLight : ILight
    {
        private readonly LightEntity _lightEntity;

        /// <summary>
        /// Adapts a NetDaemon light entity core to the <see cref="ILight"/> interface.
        /// </summary>
        public NetDaemonLight(ILightEntityCore lightEntity)
        {
            _lightEntity = new LightEntity(lightEntity);
        }

        /// <summary>
        /// Gets the unique identifier for this light entity.
        /// </summary>
        public string Id => _lightEntity.EntityId;

        /// <summary>
        /// Gets the current parameters of the light entity.
        /// </summary>
        /// <returns>A <see cref="LightParameters"/> object representing the current state of the light.</returns>
        public LightParameters GetParameters() => _lightEntity.GetLightParameters();

        /// <summary>
        /// Applies a light transition to this light entity.
        /// </summary>
        /// <param name="transition">The transition to apply to the light.</param>
        public void ApplyTransition(LightTransition transition)
        {
            _lightEntity.ApplyTransition(transition);
        }

        /// <summary>
        /// Gets all child lights if this light represents a group.
        /// </summary>
        /// <returns>An array of child light entities wrapped as <see cref="ILight"/> instances, or an empty array if this light has no children.</returns>
        public ILight[] GetChildren()
        {
            var childEntityIds = new LightEntity(_lightEntity).Attributes?.EntityId;
            if (childEntityIds == null || !childEntityIds.Any())
            {
                return [];
            }
            return childEntityIds.Select(id =>
            {
                // A group light may include itself in its list of children; avoid infinite recursion by returning this instance.
                if (id == _lightEntity.EntityId)
                {
                    return this;
                }
                return new NetDaemonLight(new LightEntity(_lightEntity.HaContext, id));
            }).ToArray<ILight>();
        }

        /// <inheritdoc/>
        public IObservable<Abstractions.StateChange<ILight, LightParameters>> StateChanges() =>
            _lightEntity.StateChanges().Select(sc => new Abstractions.StateChange<ILight, LightParameters>(this,
                sc.Old?.Attributes?.ToLightParameters(), sc.New?.Attributes?.ToLightParameters()));

        /// <inheritdoc/>
        public IObservable<Abstractions.StateChange<ILight, LightParameters>> StateChangesWithCurrent() =>
            _lightEntity.StateChangesWithCurrent().Select(sc => new Abstractions.StateChange<ILight, LightParameters>(this,
                sc.Old?.Attributes?.ToLightParameters(), sc.New?.Attributes?.ToLightParameters()));
    }
}
