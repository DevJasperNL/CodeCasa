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

        protected virtual void InputReceived(TState? state)
        {
            // Ignore input by default.
        }

        protected void DisableNode()
        {
            Enabled = false;
        }

        public TState? Output
        {
            get => _output;
            protected set
            {
                Enabled = true;

                SetOutputInternal(value);
            }
        }

        private void SetOutputInternal(TState? output)
        {
            if (output == null || EqualityComparer<TState>.Default.Equals(_output, output))
            {
                return;
            }

            _output = output;
            _newOutputSubject.OnNext(output);
        }
    }
}
