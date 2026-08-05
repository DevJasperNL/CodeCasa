using CodeCasa.AutomationPipelines.Lights.Timeline;
using CodeCasa.AutomationPipelines.Lights.Toggle;
using CodeCasa.Lights;
using Occurify;

namespace CodeCasa.AutomationPipelines.Lights.Pipeline;

public partial interface ILightTransitionPipelineConfigurator<TLight> where TLight : ILight
{
    /// <summary>
    /// Adds a toggle node that switches between the specified light parameters when triggered by <paramref name="triggerObservable"/>.
    /// Each trigger toggles to the next set of parameters in the collection, wrapping back to the first after the last.
    /// </summary>
    /// <typeparam name="T">The type of values emitted by the trigger observable.</typeparam>
    /// <param name="triggerObservable">The observable that triggers toggling to the next parameters.</param>
    /// <param name="lightParameters">The collection of light parameters to toggle between.</param>
    /// <returns>The configurator instance for method chaining.</returns>
    ILightTransitionPipelineConfigurator<TLight> AddToggle<T>(IObservable<T> triggerObservable,
        IEnumerable<LightParameters> lightParameters);

    /// <summary>
    /// Adds a toggle node that switches between the specified light parameters when triggered by <paramref name="triggerObservable"/>.
    /// Each trigger toggles to the next set of parameters in the array, wrapping back to the first after the last.
    /// </summary>
    /// <typeparam name="T">The type of values emitted by the trigger observable.</typeparam>
    /// <param name="triggerObservable">The observable that triggers toggling to the next parameters.</param>
    /// <param name="lightParameters">The array of light parameters to toggle between.</param>
    /// <returns>The configurator instance for method chaining.</returns>
    ILightTransitionPipelineConfigurator<TLight> AddToggle<T>(IObservable<T> triggerObservable,
        params LightParameters[] lightParameters);

    /// <summary>
    /// Adds a toggle node that switches between the specified light transitions when triggered by <paramref name="triggerObservable"/>.
    /// Each trigger toggles to the next transition in the collection, wrapping back to the first after the last.
    /// </summary>
    /// <typeparam name="T">The type of values emitted by the trigger observable.</typeparam>
    /// <param name="triggerObservable">The observable that triggers toggling to the next transition.</param>
    /// <param name="lightTransitions">The collection of light transitions to toggle between.</param>
    /// <returns>The configurator instance for method chaining.</returns>
    ILightTransitionPipelineConfigurator<TLight> AddToggle<T>(IObservable<T> triggerObservable,
        IEnumerable<LightTransition> lightTransitions);

    /// <summary>
    /// Adds a toggle node that switches between the specified light transitions when triggered by <paramref name="triggerObservable"/>.
    /// Each trigger toggles to the next transition in the array, wrapping back to the first after the last.
    /// </summary>
    /// <typeparam name="T">The type of values emitted by the trigger observable.</typeparam>
    /// <param name="triggerObservable">The observable that triggers toggling to the next transition.</param>
    /// <param name="lightTransitions">The array of light transitions to toggle between.</param>
    /// <returns>The configurator instance for method chaining.</returns>
    ILightTransitionPipelineConfigurator<TLight> AddToggle<T>(IObservable<T> triggerObservable,
        params LightTransition[] lightTransitions);

    /// <summary>
    /// Adds a toggle node that switches between nodes created by the specified factory functions when triggered by <paramref name="triggerObservable"/>.
    /// Each trigger toggles to the next node factory in the collection, wrapping back to the first after the last.
    /// </summary>
    /// <typeparam name="T">The type of values emitted by the trigger observable.</typeparam>
    /// <param name="triggerObservable">The observable that triggers toggling to the next node.</param>
    /// <param name="nodeFactories">The collection of factory functions that create pipeline nodes.</param>
    /// <returns>The configurator instance for method chaining.</returns>
    ILightTransitionPipelineConfigurator<TLight> AddToggle<T>(IObservable<T> triggerObservable,
        IEnumerable<Func<IServiceProvider, IPipelineNode<LightTransition>>> nodeFactories);

    /// <summary>
    /// Adds a toggle node that switches between nodes created by the specified factory functions when triggered by <paramref name="triggerObservable"/>.
    /// Each trigger toggles to the next node factory in the array, wrapping back to the first after the last.
    /// </summary>
    /// <typeparam name="T">The type of values emitted by the trigger observable.</typeparam>
    /// <param name="triggerObservable">The observable that triggers toggling to the next node.</param>
    /// <param name="nodeFactories">The array of factory functions that create pipeline nodes.</param>
    /// <returns>The configurator instance for method chaining.</returns>
    ILightTransitionPipelineConfigurator<TLight> AddToggle<T>(IObservable<T> triggerObservable,
        params Func<IServiceProvider, IPipelineNode<LightTransition>>[] nodeFactories);

    /// <summary>
    /// Adds a toggle node configured by the specified <paramref name="configure"/> action when triggered by <paramref name="triggerObservable"/>.
    /// Each trigger toggles to the next configured state.
    /// </summary>
    /// <typeparam name="T">The type of values emitted by the trigger observable.</typeparam>
    /// <param name="triggerObservable">The observable that triggers toggling to the next state.</param>
    /// <param name="configure">An action to configure the toggle behavior.</param>
    /// <returns>The configurator instance for method chaining.</returns>
    ILightTransitionPipelineConfigurator<TLight> AddToggle<T>(IObservable<T> triggerObservable,
        Action<ILightTransitionToggleConfigurator<TLight>> configure);

    /// <summary>
    /// Adds a toggle node with a timeline entry when triggered by <paramref name="triggerObservable"/>.
    /// The timeline drives the output automatically as time progresses. State matching is determined
    /// by comparing the light's current parameters against the timeline's current output.
    /// </summary>
    /// <typeparam name="T">The type of values emitted by the trigger observable.</typeparam>
    /// <param name="triggerObservable">The observable that triggers toggling to the next state.</param>
    /// <param name="timeline">The dictionary mapping timeline points to <see cref="LightParameters"/>.</param>
    /// <param name="transitionTimeForTimelineState">The duration of the initial fade from the current state. Defaults to 400ms if null.</param>
    /// <returns>The configurator instance for method chaining.</returns>
    ILightTransitionPipelineConfigurator<TLight> AddToggle<T>(IObservable<T> triggerObservable,
        Dictionary<ITimeline, LightParameters> timeline, TimeSpan? transitionTimeForTimelineState = null);

    /// <summary>
    /// Adds a toggle node with a timeline entry created by a factory function when triggered by <paramref name="triggerObservable"/>.
    /// The timeline drives the output automatically as time progresses. State matching is determined
    /// by comparing the light's current parameters against the timeline's current output.
    /// </summary>
    /// <typeparam name="T">The type of values emitted by the trigger observable.</typeparam>
    /// <param name="triggerObservable">The observable that triggers toggling to the next state.</param>
    /// <param name="timelineFactory">A factory function that creates the timeline mapping based on the pipeline context.</param>
    /// <param name="transitionTimeForTimelineState">The duration of the initial fade from the current state. Defaults to 400ms if null.</param>
    /// <returns>The configurator instance for method chaining.</returns>
    ILightTransitionPipelineConfigurator<TLight> AddToggle<T>(IObservable<T> triggerObservable,
        Func<IServiceProvider, Dictionary<ITimeline, LightParameters>> timelineFactory,
        TimeSpan? transitionTimeForTimelineState = null);

    /// <summary>
    /// Adds a toggle node with a timeline entry configured by <paramref name="configure"/> when triggered by <paramref name="triggerObservable"/>.
    /// The timeline drives the output automatically as time progresses. State matching is determined
    /// by comparing the light's current parameters against the timeline's current output.
    /// </summary>
    /// <typeparam name="T">The type of values emitted by the trigger observable.</typeparam>
    /// <param name="triggerObservable">The observable that triggers toggling to the next state.</param>
    /// <param name="configure">An action to configure the timeline entries and optional transition time.</param>
    /// <returns>The configurator instance for method chaining.</returns>
    ILightTransitionPipelineConfigurator<TLight> AddToggle<T>(IObservable<T> triggerObservable,
        Action<ITimelineConfigurator> configure);
}