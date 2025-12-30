using CodeCasa.AutomationPipelines.Lights.Cycle;
using CodeCasa.Lights;

namespace CodeCasa.AutomationPipelines.Lights.Pipeline;

public partial interface ILightTransitionPipelineConfigurator
{
    /// <summary>
    /// Adds a cycle node that rotates through the specified light parameters when triggered by <paramref name="triggerObservable"/>.
    /// Each trigger cycles to the next set of parameters in the collection.
    /// </summary>
    /// <typeparam name="T">The type of values emitted by the trigger observable.</typeparam>
    /// <param name="triggerObservable">The observable that triggers cycling to the next parameters.</param>
    /// <param name="lightParameters">The collection of light parameters to cycle through.</param>
    /// <returns>The configurator instance for method chaining.</returns>
    ILightTransitionPipelineConfigurator AddCycle<T>(IObservable<T> triggerObservable,
        IEnumerable<LightParameters> lightParameters);

    /// <summary>
    /// Adds a cycle node that rotates through the specified light parameters when triggered by <paramref name="triggerObservable"/>.
    /// Each trigger cycles to the next set of parameters in the array.
    /// </summary>
    /// <typeparam name="T">The type of values emitted by the trigger observable.</typeparam>
    /// <param name="triggerObservable">The observable that triggers cycling to the next parameters.</param>
    /// <param name="lightParameters">The array of light parameters to cycle through.</param>
    /// <returns>The configurator instance for method chaining.</returns>
    ILightTransitionPipelineConfigurator AddCycle<T>(IObservable<T> triggerObservable,
        params LightParameters[] lightParameters);

    /// <summary>
    /// Adds a cycle node that rotates through the specified light transitions when triggered by <paramref name="triggerObservable"/>.
    /// Each trigger cycles to the next transition in the collection.
    /// </summary>
    /// <typeparam name="T">The type of values emitted by the trigger observable.</typeparam>
    /// <param name="triggerObservable">The observable that triggers cycling to the next transition.</param>
    /// <param name="lightTransitions">The collection of light transitions to cycle through.</param>
    /// <returns>The configurator instance for method chaining.</returns>
    ILightTransitionPipelineConfigurator AddCycle<T>(IObservable<T> triggerObservable,
        IEnumerable<LightTransition> lightTransitions);

    /// <summary>
    /// Adds a cycle node that rotates through the specified light transitions when triggered by <paramref name="triggerObservable"/>.
    /// Each trigger cycles to the next transition in the array.
    /// </summary>
    /// <typeparam name="T">The type of values emitted by the trigger observable.</typeparam>
    /// <param name="triggerObservable">The observable that triggers cycling to the next transition.</param>
    /// <param name="lightTransitions">The array of light transitions to cycle through.</param>
    /// <returns>The configurator instance for method chaining.</returns>
    ILightTransitionPipelineConfigurator AddCycle<T>(IObservable<T> triggerObservable,
        params LightTransition[] lightTransitions);

    /// <summary>
    /// Adds a cycle node configured by the specified <paramref name="configure"/> action when triggered by <paramref name="triggerObservable"/>.
    /// Each trigger cycles to the next configured state.
    /// </summary>
    /// <typeparam name="T">The type of values emitted by the trigger observable.</typeparam>
    /// <param name="triggerObservable">The observable that triggers cycling to the next state.</param>
    /// <param name="configure">An action to configure the cycle behavior.</param>
    /// <returns>The configurator instance for method chaining.</returns>
    ILightTransitionPipelineConfigurator AddCycle<T>(IObservable<T> triggerObservable,
        Action<ILightTransitionCycleConfigurator> configure);
}