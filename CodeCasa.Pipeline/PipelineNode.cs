using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace CodeCasa.Pipeline
{
    public abstract class PipelineNode<TState> : IPipelineNode<TState>
    {
        private readonly Subject<TState?> _newOutputSubject = new();
        private TState? _input;
        private TState? _output;

        public IObservable<TState?> OnNewOutput => _newOutputSubject.AsObservable();

        public TState? Input
        {
            get => _input;
            set
            {
                _input = value;
                InputReceived(_input);
            }
        }

        protected virtual void InputReceived(TState? state)
        {
            // Ignore input by default.
        }

        protected void DisableNode()
        {
            Output = Input;
        }

        public TState? Output
        {
            get => _output;
            protected set
            {
                if (value == null || EqualityComparer<TState>.Default.Equals(_output, value))
                {
                    return;
                }

                _output = value;
                _newOutputSubject.OnNext(value);
            }
        }
    }
}
