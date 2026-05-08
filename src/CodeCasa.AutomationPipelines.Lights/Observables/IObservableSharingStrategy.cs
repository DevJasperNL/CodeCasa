namespace CodeCasa.AutomationPipelines.Lights.Observables;

/// <summary>
/// Defines a contract for transforming an input <see cref="IObservable{T}"/> into a shared or specialized stream.
/// </summary>
/// <remarks>
/// This interface allows the library to abstract away the "multicasting" logic. 
/// Implementations determine whether an observable is cold (unique path per subscriber), 
/// hot (shared path), or buffered (replaying previous values to late subscribers).
/// </remarks>
public interface IObservableSharingStrategy
{
    /// <summary>
    /// Applies a sharing or transformation logic to the provided source observable.
    /// </summary>
    /// <typeparam name="T">The type of the elements in the source sequence.</typeparam>
    /// <param name="source">The original observable provided by the user.</param>
    /// <remarks>
    /// Common implementations include using <c>.Publish().RefCount()</c> for live event broadcasting 
    /// or <c>.Replay(1).RefCount()</c> to ensure initial state/prepended values are preserved.
    /// </remarks>
    IObservable<T> Apply<T>(IObservable<T> source);
}