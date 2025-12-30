using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using CodeCasa.AutomationPipelines.Lights.Extensions;
using CodeCasa.Lights;

namespace CodeCasa.AutomationPipelines.Lights.Nodes
{
    /// <summary>
    /// Base class for pipeline nodes that work with light transitions, extending IPipelineNode functionality.
    /// Provides features for managing light transition states, scheduling, and pass-through behavior.
    /// </summary>
    public abstract class LightTransitionNode(IScheduler scheduler) : IPipelineNode<LightTransition>
    {
        private readonly Subject<LightTransition?> _newOutputSubject = new();
        private LightTransition? _input;
        private LightParameters? _inputLightDestinationParameters;
        private DateTime? _inputStartOfTransition;
        private DateTime? _inputEndOfTransition;
        private LightTransition? _output;
        private bool _passThroughNextInput;
        private bool _passThrough;
        private IDisposable? _scheduledAction;

        /// <summary>
        /// Gets the source light parameters from the previous input, useful for interpolating transitions.
        /// </summary>
        protected LightParameters? InputLightSourceParameters { get; private set; }

        /// <inheritdoc />
        public IObservable<LightTransition?> OnNewOutput => _newOutputSubject.AsObservable();

        /// <inheritdoc />
        public LightTransition? Input
        {
            get => _input;
            set
            {
                _scheduledAction?.Dispose(); // Always cancel scheduled actions when the input changes.
                // We save additional information on the light transition that we can later use to continue the transition if it would be interrupted.
                InputLightSourceParameters = _inputLightDestinationParameters;
                _input = value;
                _inputLightDestinationParameters = value?.LightParameters;
                var transitionTime = value?.TransitionTime;
                _inputStartOfTransition = DateTime.UtcNow;
                _inputEndOfTransition = transitionTime == null ? null : _inputStartOfTransition + transitionTime;

                if (_passThroughNextInput)
                {
                    PassThrough = true;
                    return;
                }
                if (PassThrough)
                {
                    SetOutputInternal(_input);
                    return;
                }
                InputReceived(_input);
            }
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
            protected set
            {
                _scheduledAction?.Dispose(); // Always cancel scheduled actions when the output is changed directly.
                PassThrough = false;

                SetOutputInternal(value);
            }
        }

        /// <summary>
        /// Schedules an interpolated light transition that will animate from source to desired parameters using the input's transition time.
        /// </summary>
        /// <param name="sourceLightParameters">The source light parameters to transition from.</param>
        /// <param name="desiredLightParameters">The desired light parameters to transition to.</param>
        protected void ScheduleInterpolatedLightTransitionUsingInputTransitionTime(LightParameters? sourceLightParameters, LightParameters? desiredLightParameters)
        {
            PassThrough = false;
            _scheduledAction = scheduler.ScheduleInterpolatedLightTransition(sourceLightParameters,
                desiredLightParameters, _inputStartOfTransition, _inputEndOfTransition, SetOutputInternal);
        }

        /// <summary>
        /// Gets or sets a value indicating whether the node should pass its input directly to the output.
        /// When true, the node does not call InputReceived and instead passes the input through unchanged.
        /// </summary>
        public bool PassThrough
        {
            get => _passThrough;
            set
            {
                // Always reset _passThroughNextInput when PassThrough is explicitly called.
                _passThroughNextInput = false;

                if (_passThrough == value)
                {
                    return;
                }

                _scheduledAction?.Dispose(); // Always cancel scheduled actions when the pass through value changes.

                _passThrough = value;
                if (_passThrough)
                {
                    _scheduledAction = scheduler.ScheduleInterpolatedLightTransition(InputLightSourceParameters,
                        _inputLightDestinationParameters, _inputStartOfTransition, _inputEndOfTransition, SetOutputInternal);
                }
            }
        }

        /// <summary>
        /// Changes the output state of the node and enables pass-through mode after the next input is received.
        /// This is useful for nodes that should influence pipeline behavior once, such as light switches or motion sensors.
        /// </summary>
        /// <param name="output">The output light transition to set.</param>
        protected void ChangeOutputAndTurnOnPassThroughOnNextInput(LightTransition? output)
        {
            Output = output;
            TurnOnPassThroughOnNextInput();
        }

        /// <summary>
        /// Keeps the current output but enables pass-through mode after receiving the next input.
        /// This is useful for nodes that should influence pipeline behavior once, such as light switches or motion sensors.
        /// </summary>
        protected void TurnOnPassThroughOnNextInput()
        {
            if (PassThrough)
            {
                return;
            }

            _passThroughNextInput = true;
        }

        private void SetOutputInternal(LightTransition? output)
        {
            _output = output;
            _newOutputSubject.OnNext(output);
        }

        /// <inheritdoc />
        public override string ToString() => GetType().Name;
    }
}
