using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using CodeCasa.AutomationPipelines.Lights.Extensions;
using CodeCasa.AutomationPipelines.Lights.Utils;
using CodeCasa.Lights;

namespace CodeCasa.AutomationPipelines.Lights.Nodes;

/// <summary>
/// Base class for pipeline nodes that work with light transitions, extending IPipelineNode functionality.
/// Provides features for managing light transition states, scheduling, and pass-through behavior.
/// </summary>
public abstract class LightTransitionNode(IScheduler scheduler) : IPipelineNode<LightTransition>
{
    private readonly Subject<LightTransition?> _newOutputSubject = new();
    // Inputs, outputs and scheduled continuations arrive on different threads; see SerializedActionQueue for why this is not a lock.
    private readonly SerializedActionQueue _stateQueue = new();
    private LightParameters? _inputLightDestinationParameters;
    private DateTime? _inputStartOfTransition;
    private DateTime? _inputEndOfTransition;
    private LightTransition? _output;
    private bool _passThroughNextInput;
    private IDisposable? _scheduledAction;
    private int _scheduleGeneration;
    private volatile bool _isDisposed;

    /// <summary>
    /// Gets the source light parameters from the previous input, useful for interpolating transitions.
    /// </summary>
    protected LightParameters? InputLightSourceParameters { get; private set; }

    /// <inheritdoc />
    public IObservable<LightTransition?> OnNewOutput => _newOutputSubject.AsObservable();

    /// <inheritdoc />
    public Guid Id { get; } = Guid.CreateVersion7();
    /// <inheritdoc />
    public string? Name { get; set; }

    /// <inheritdoc />
    public LightTransition? Input
    {
        get;
        set => RunSerialized(() =>
        {
            CancelScheduledAction(); // Always cancel scheduled actions when the input changes.
            // We save additional information on the light transition that we can later use to continue the transition if it would be interrupted.
            InputLightSourceParameters = _inputLightDestinationParameters;
            field = value;
            _inputLightDestinationParameters = value?.LightParameters;
            var transitionTime = value?.TransitionTime;
            _inputStartOfTransition = scheduler.Now.UtcDateTime;
            _inputEndOfTransition = _inputStartOfTransition + transitionTime;

            OnInputChanged(field);

            if (_passThroughNextInput)
            {
                PassThrough = true;
                return;
            }

            if (PassThrough)
            {
                SetOutputInternal(field);
                return;
            }

            InputReceived(field);
        });
    }

    /// <summary>
    /// Called for every new input, regardless of pass-through mode and before the input is passed through or handed to
    /// <see cref="InputReceived"/>. Override this method to observe all inputs, for example to forward them to a wrapped node.
    /// </summary>
    /// <param name="input">The light transition input that was received.</param>
    protected virtual void OnInputChanged(LightTransition? input)
    {
    }

    /// <summary>
    /// Called when the input is received. Override this method to implement custom input handling logic.
    /// </summary>
    /// <param name="input">The light transition input that was received.</param>
    protected virtual void InputReceived(LightTransition? input)
    {
        // Ignore input by default.
    }

    /// <summary>
    /// Enables pass-through mode for the node, causing it to pass the input directly to the output without processing.
    /// </summary>
    protected void PassInputThrough()
    {
        PassThrough = true;
    }

    /// <summary>
    /// Gets or sets the output state of the node.
    /// Setting this value will trigger output processing and disable pass-through mode.
    /// </summary>
    public LightTransition? Output
    {
        get => _output;
        protected set => RunSerialized(() =>
        {
            CancelScheduledAction(); // Always cancel scheduled actions when the output is changed directly.
            PassThrough = false;

            SetOutputInternal(value);
        });
    }

    /// <summary>
    /// Schedules an interpolated light transition that will animate from source to desired parameters using the input's transition time.
    /// </summary>
    /// <remarks>
    /// The remainder of the transition is cancelled by the next input, so a node that keeps overriding the input has to
    /// schedule again from <see cref="InputReceived"/>.
    /// </remarks>
    /// <param name="sourceLightParameters">The source light parameters to transition from.</param>
    /// <param name="desiredLightParameters">The desired light parameters to transition to.</param>
    protected void ScheduleInterpolatedLightTransitionUsingInputTransitionTime(LightParameters? sourceLightParameters, LightParameters? desiredLightParameters)
    {
        RunSerialized(() =>
        {
            // The PassThrough setter only cancels when the value changes, which it does not when scheduling twice in a row.
            CancelScheduledAction();
            PassThrough = false;
            ScheduleInterpolated(sourceLightParameters, desiredLightParameters);
        });
    }

    /// <summary>
    /// Gets or sets a value indicating whether the node should pass its input directly to the output.
    /// When true, the node does not call InputReceived and instead passes the input through unchanged.
    /// </summary>
    public bool PassThrough
    {
        get;
        set => RunSerialized(() =>
        {
            // Always reset _passThroughNextInput when PassThrough is explicitly called.
            _passThroughNextInput = false;

            if (field == value)
            {
                return;
            }

            CancelScheduledAction(); // Always cancel scheduled actions when the pass through value changes.

            field = value;
            if (field)
            {
                ScheduleInterpolated(InputLightSourceParameters, _inputLightDestinationParameters);
            }
        });
    }

    /// <summary>
    /// Changes the output state of the node and enables pass-through mode after the next input is received.
    /// This is useful for nodes that should influence pipeline behavior once, such as light switches or motion sensors.
    /// </summary>
    /// <param name="output">The output light transition to set.</param>
    protected void ChangeOutputAndTurnOnPassThroughOnNextInput(LightTransition? output)
    {
        RunSerialized(() =>
        {
            Output = output;
            TurnOnPassThroughOnNextInput();
        });
    }

    /// <summary>
    /// Keeps the current output but enables pass-through mode after receiving the next input.
    /// This is useful for nodes that should influence pipeline behavior once, such as light switches or motion sensors.
    /// </summary>
    protected void TurnOnPassThroughOnNextInput()
    {
        RunSerialized(() =>
        {
            if (PassThrough)
            {
                return;
            }

            _passThroughNextInput = true;
        });
    }

    /// <summary>
    /// Runs <paramref name="action"/> serialised with every other state change of this node, so a node that reacts to
    /// timers or observables can change several things without an input or a continuation running in between. The action
    /// is skipped once the node is disposed.
    /// </summary>
    private protected void RunSerialized(Action action)
    {
        _stateQueue.Run(() =>
        {
            if (!_isDisposed)
            {
                action();
            }
        });
    }

    private void CancelScheduledAction()
    {
        // Disposing does not stop a continuation that already started on the scheduler thread; the generation makes it drop its output.
        _scheduleGeneration++;
        _scheduledAction?.Dispose();
        _scheduledAction = null;
    }

    private void ScheduleInterpolated(LightParameters? sourceLightParameters, LightParameters? desiredLightParameters)
    {
        var generation = _scheduleGeneration;
        var scheduledAction = scheduler.ScheduleInterpolatedLightTransition(sourceLightParameters,
            desiredLightParameters, _inputStartOfTransition, _inputEndOfTransition, output => RunSerialized(() =>
            {
                // Checked inside the queue: a cancellation on another thread is either fully applied by now or runs after this output.
                if (generation == _scheduleGeneration)
                {
                    SetOutputInternal(output);
                }
            }));

        // The first output is emitted synchronously, so a downstream reaction may already have cancelled or replaced this schedule.
        if (generation != _scheduleGeneration)
        {
            scheduledAction?.Dispose();
            return;
        }
        _scheduledAction = scheduledAction;
    }

    private void SetOutputInternal(LightTransition? output)
    {
        // Scheduled continuations may still fire after disposal and must not emit anymore.
        if (_isDisposed)
        {
            return;
        }

        _output = output;
        _newOutputSubject.OnNext(output);
    }

    /// <inheritdoc />
    public override string ToString() => Name ?? GetType().Name;

    /// <inheritdoc />
    public virtual ValueTask DisposeAsync()
    {
        if (_isDisposed)
        {
            return ValueTask.CompletedTask;
        }
        _isDisposed = true;
        // Queued behind a state change that is still running on another thread, so completion is always the last thing the subject sees.
        _stateQueue.Run(() =>
        {
            CancelScheduledAction();
            // The subject is completed but not disposed, so subscribing to a disposed node completes instead of throwing.
            _newOutputSubject.OnCompleted();
        });
        return ValueTask.CompletedTask;
    }
}