using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace CodeCasa.Pipeline
{
    public abstract class PipelineNode<TState> : IPipelineNode<TState>
    {
        private readonly Subject<TState?> _newOutputSubject = new();
        private TState? _input;
        private TState? _output;
        private bool _enabled = true;

        public IObservable<TState?> OnNewOutput => _newOutputSubject.AsObservable();

        public TState? Input
        {
            get => _input;
            set
            {
                _input = value;
                if (!Enabled)
                {
                    SetOutputInternal(_input);
                    return;
                }
                InputReceived(_input);
            }
        }

        /// <summary>
        /// Called when the input is received.
        /// </summary>
        protected virtual void InputReceived(TState? state)
        {
            // Ignore input by default.
        }

        /// <summary>
        /// Disables the node. If the node is disabled, it will pass its input to the output without processing it.
        /// </summary>
        protected void DisableNode()
        {
            Enabled = false;
        }

        /// <summary>
        /// Sets the output state of the node. This will trigger the processing of the input.
        /// If the node is disabled, it will be enabled when setting an output value.
        /// </summary>
        public TState? Output
        {
            get => _output;
            protected set
            {
                Enabled = true;

                SetOutputInternal(value);
            }
        }

        public bool Enabled
        {
            get => _enabled;
            set
            {
                if (_enabled == value)
                {
                    return;
                }
                _enabled = value;
                if (!_enabled)
                {
                    SetOutputInternal(_input);
                }
            }
        }

        private void SetOutputInternal(TState? output)
        {
            _output = output;
            _newOutputSubject.OnNext(output);
        }
    }
}
