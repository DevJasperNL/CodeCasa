using CodeCasa.Abstractions;

namespace CodeCasa.Lights;

/// <summary>
/// Represents a single light or group of lights.
/// </summary>
public interface ILight
{
    /// <summary>
    /// Gets the unique identifier for this light.
    /// </summary>
    public string Id { get; }

    /// <summary>
    /// Gets the parameters of the light.
    /// </summary>
    LightParameters GetParameters();
    
    /// <summary>
    /// Applies a transition to the light.
    /// </summary>
    /// <param name="transition">The transition to apply.</param>
    void ApplyTransition(LightTransition transition);
    
    /// <summary>
    /// Gets the child lights if this light represents a group.
    /// </summary>
    /// <returns>An array of child lights, or an empty array if this light has no children.</returns>
    ILight[] GetChildren();

    /// <summary>
    /// Returns an observable that emits notifications when the state of the light changes.
    /// </summary>
    /// <returns>An <see cref="IObservable{T}"/> that emits <see cref="StateChange{ILight, LightParameters}"/> events.</returns>
    IObservable<StateChange<ILight, LightParameters>> StateChanges();

    /// <summary>
    /// Returns an observable that emits the current state and then notifications when the state of the light changes.
    /// </summary>
    /// <returns>An <see cref="IObservable{T}"/> that emits <see cref="StateChange{ILight, LightParameters}"/> events.</returns>
    IObservable<StateChange<ILight, LightParameters>> StateChangesWithCurrent();
}