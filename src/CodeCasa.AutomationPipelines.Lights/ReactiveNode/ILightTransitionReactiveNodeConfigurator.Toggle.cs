using CodeCasa.AutomationPipelines.Lights.Context;
using CodeCasa.AutomationPipelines.Lights.Toggle;
using CodeCasa.Lights;

namespace CodeCasa.AutomationPipelines.Lights.ReactiveNode;

public partial interface ILightTransitionReactiveNodeConfigurator
{
    /// <summary>
    /// Adds a time-based toggle trigger that switches between the specified light parameters when triggered by <paramref name="triggerObservable"/>.
    /// Quick consecutive triggers advance through all parameter sets sequentially. After a timeout period, the next trigger restarts from the beginning.
    /// </summary>
    /// <typeparam name="T">The type of values emitted by the trigger observable.</typeparam>
    /// <param name="triggerObservable">The observable that triggers toggling to the next parameters.</param>
    /// <param name="lightParameters">The collection of light parameters to toggle between.</param>
    /// <returns>The configurator instance for method chaining.</returns>
    ILightTransitionReactiveNodeConfigurator AddToggle<T>(IObservable<T> triggerObservable,
        IEnumerable<LightParameters> lightParameters);

    /// <summary>
    /// Adds a time-based toggle trigger that switches between the specified light parameters when triggered by <paramref name="triggerObservable"/>.
    /// Quick consecutive triggers advance through all parameter sets sequentially. After a timeout period, the next trigger restarts from the beginning.
    /// </summary>
    /// <typeparam name="T">The type of values emitted by the trigger observable.</typeparam>
    /// <param name="triggerObservable">The observable that triggers toggling to the next parameters.</param>
    /// <param name="lightParameters">The array of light parameters to toggle between.</param>
    /// <returns>The configurator instance for method chaining.</returns>
    ILightTransitionReactiveNodeConfigurator AddToggle<T>(IObservable<T> triggerObservable,
        params LightParameters[] lightParameters);

    /// <summary>
    /// Adds a time-based toggle trigger that switches between the specified light transitions when triggered by <paramref name="triggerObservable"/>.
    /// Quick consecutive triggers advance through all transitions sequentially. After a timeout period, the next trigger restarts from the beginning.
    /// </summary>
    /// <typeparam name="T">The type of values emitted by the trigger observable.</typeparam>
    /// <param name="triggerObservable">The observable that triggers toggling to the next transition.</param>
    /// <param name="lightTransitions">The collection of light transitions to toggle between.</param>
    /// <returns>The configurator instance for method chaining.</returns>
    ILightTransitionReactiveNodeConfigurator AddToggle<T>(IObservable<T> triggerObservable,
        IEnumerable<LightTransition> lightTransitions);

    /// <summary>
    /// Adds a time-based toggle trigger that switches between the specified light transitions when triggered by <paramref name="triggerObservable"/>.
    /// Quick consecutive triggers advance through all transitions sequentially. After a timeout period, the next trigger restarts from the beginning.
    /// </summary>
    /// <typeparam name="T">The type of values emitted by the trigger observable.</typeparam>
    /// <param name="triggerObservable">The observable that triggers toggling to the next transition.</param>
    /// <param name="lightTransitions">The array of light transitions to toggle between.</param>
    /// <returns>The configurator instance for method chaining.</returns>
    ILightTransitionReactiveNodeConfigurator AddToggle<T>(IObservable<T> triggerObservable,
        params LightTransition[] lightTransitions);

    /// <summary>
    /// Adds a time-based toggle trigger that switches between nodes created by the specified factory functions when triggered by <paramref name="triggerObservable"/>.
    /// Quick consecutive triggers advance through all node factories sequentially. After a timeout period, the next trigger restarts from the beginning.
    /// </summary>
    /// <typeparam name="T">The type of values emitted by the trigger observable.</typeparam>
    /// <param name="triggerObservable">The observable that triggers toggling to the next node.</param>
    /// <param name="nodeFactories">The collection of factory functions that create pipeline nodes.</param>
    /// <returns>The configurator instance for method chaining.</returns>
    ILightTransitionReactiveNodeConfigurator AddToggle<T>(IObservable<T> triggerObservable,
        IEnumerable<Func<ILightPipelineContext, IPipelineNode<LightTransition>>> nodeFactories);

    /// <summary>
    /// Adds a time-based toggle trigger that switches between nodes created by the specified factory functions when triggered by <paramref name="triggerObservable"/>.
    /// Quick consecutive triggers advance through all node factories sequentially. After a timeout period, the next trigger restarts from the beginning.
    /// </summary>
    /// <typeparam name="T">The type of values emitted by the trigger observable.</typeparam>
    /// <param name="triggerObservable">The observable that triggers toggling to the next node.</param>
    /// <param name="nodeFactories">The array of factory functions that create pipeline nodes.</param>
    /// <returns>The configurator instance for method chaining.</returns>
    ILightTransitionReactiveNodeConfigurator AddToggle<T>(IObservable<T> triggerObservable,
        params Func<ILightPipelineContext, IPipelineNode<LightTransition>>[] nodeFactories);

    /// <summary>
    /// Adds a time-based toggle trigger configured by the specified <paramref name="configure"/> action when triggered by <paramref name="triggerObservable"/>.
    /// Quick consecutive triggers advance through all configured states sequentially. After a timeout period, the next trigger restarts from the beginning.
    /// </summary>
    /// <typeparam name="T">The type of values emitted by the trigger observable.</typeparam>
    /// <param name="triggerObservable">The observable that triggers toggling to the next state.</param>
    /// <param name="configure">An action to configure the toggle behavior.</param>
    /// <returns>The configurator instance for method chaining.</returns>
    ILightTransitionReactiveNodeConfigurator AddToggle<T>(IObservable<T> triggerObservable,
        Action<ILightTransitionToggleConfigurator> configure);
}