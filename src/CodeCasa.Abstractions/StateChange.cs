namespace CodeCasa.Abstractions;

/// <summary>
/// Represents a change in state for an entity.
/// </summary>
/// <typeparam name="TEntity">The type of the entity.</typeparam>
/// <typeparam name="TState">The type of the state.</typeparam>
/// <param name="Entity">The entity whose state changed.</param>
/// <param name="Old">The old state.</param>
/// <param name="New">The new state.</param>
public record StateChange<TEntity, TState>(TEntity Entity, TState? Old, TState? New);