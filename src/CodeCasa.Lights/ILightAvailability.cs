namespace CodeCasa.Lights;

/// <summary>
/// Optional capability of an <see cref="ILight"/> that can report whether it is reachable.
/// </summary>
/// <remarks>
/// Automation pipelines use this to re-apply their current output when a light comes back, for example after a power cut
/// or a Zigbee device rejoining, because transitions sent while it was unavailable were lost.
/// </remarks>
public interface ILightAvailability
{
    /// <summary>
    /// Gets a value indicating whether the light is currently available.
    /// </summary>
    bool IsAvailable { get; }

    /// <summary>
    /// Returns an observable that emits the new availability whenever it changes. The current value is not emitted on subscribe.
    /// </summary>
    /// <returns>An observable emitting <see langword="true"/> when the light becomes available and <see langword="false"/> when it becomes unavailable.</returns>
    IObservable<bool> AvailabilityChanges();
}
