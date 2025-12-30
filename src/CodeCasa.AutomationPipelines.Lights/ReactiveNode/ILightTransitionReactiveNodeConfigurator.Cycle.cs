using CodeCasa.AutomationPipelines.Lights.Cycle;
using CodeCasa.Lights;

namespace CodeCasa.AutomationPipelines.Lights.ReactiveNode;

public partial interface ILightTransitionReactiveNodeConfigurator
{
    /// <summary>
    /// Adds a state-based cycle trigger that rotates through the specified light parameters when triggered by <paramref name="triggerObservable"/>.
    /// The cycle advances based on the current light state: if the light matches parameters in the cycle, it advances to the next set.
    /// If the current state is not recognized, the cycle starts from the beginning.
    /// </summary>
    /// <typeparam name="T">The type of values emitted by the trigger observable.</typeparam>
    /// <param name="triggerObservable">The observable that triggers cycling to the next parameters.</param>
    /// <param name="lightParameters">The collection of light parameters to cycle through.</param>
    /// <returns>The configurator instance for method chaining.</returns>
    ILightTransitionReactiveNodeConfigurator AddCycle<T>(IObservable<T> triggerObservable,
        IEnumerable<LightParameters> lightParameters);

    /// <summary>
    /// Adds a state-based cycle trigger that rotates through the specified light parameters when triggered by <paramref name="triggerObservable"/>.
    /// The cycle advances based on the current light state: if the light matches parameters in the cycle, it advances to the next set.
    /// If the current state is not recognized, the cycle starts from the beginning.
    /// </summary>
    /// <typeparam name="T">The type of values emitted by the trigger observable.</typeparam>
    /// <param name="triggerObservable">The observable that triggers cycling to the next parameters.</param>
    /// <param name="lightParameters">The array of light parameters to cycle through.</param>
    /// <returns>The configurator instance for method chaining.</returns>
    ILightTransitionReactiveNodeConfigurator AddCycle<T>(IObservable<T> triggerObservable,
        params LightParameters[] lightParameters);

    /// <summary>
    /// Adds a state-based cycle trigger that rotates through the specified light transitions when triggered by <paramref name="triggerObservable"/>.
    /// The cycle advances based on the current light state: if the light matches a transition in the cycle, it advances to the next transition.
    /// If the current state is not recognized, the cycle starts from the beginning.
    /// </summary>
    /// <typeparam name="T">The type of values emitted by the trigger observable.</typeparam>
    /// <param name="triggerObservable">The observable that triggers cycling to the next transition.</param>
    /// <param name="lightTransitions">The collection of light transitions to cycle through.</param>
    /// <returns>The configurator instance for method chaining.</returns>
    ILightTransitionReactiveNodeConfigurator AddCycle<T>(IObservable<T> triggerObservable,
        IEnumerable<LightTransition> lightTransitions);

    /// <summary>
    /// Adds a state-based cycle trigger that rotates through the specified light transitions when triggered by <paramref name="triggerObservable"/>.
    /// The cycle advances based on the current light state: if the light matches a transition in the cycle, it advances to the next transition.
    /// If the current state is not recognized, the cycle starts from the beginning.
    /// </summary>
    /// <typeparam name="T">The type of values emitted by the trigger observable.</typeparam>
    /// <param name="triggerObservable">The observable that triggers cycling to the next transition.</param>
    /// <param name="lightTransitions">The array of light transitions to cycle through.</param>
    /// <returns>The configurator instance for method chaining.</returns>
    ILightTransitionReactiveNodeConfigurator AddCycle<T>(IObservable<T> triggerObservable,
        params LightTransition[] lightTransitions);

    /// <summary>
    /// Adds a state-based cycle trigger configured by the specified <paramref name="configure"/> action when triggered by <paramref name="triggerObservable"/>.
    /// The cycle advances based on the current light state: if the light matches a state in the cycle, it advances to the next state.
    /// If the current state is not recognized, the cycle starts from the beginning.
    /// </summary>
    /// <typeparam name="T">The type of values emitted by the trigger observable.</typeparam>
    /// <param name="triggerObservable">The observable that triggers cycling to the next state.</param>
    /// <param name="configure">An action to configure the cycle behavior.</param>
    /// <returns>The configurator instance for method chaining.</returns>
    ILightTransitionReactiveNodeConfigurator AddCycle<T>(IObservable<T> triggerObservable,
        Action<ILightTransitionCycleConfigurator> configure);
}