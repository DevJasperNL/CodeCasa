using Microsoft.Extensions.DependencyInjection;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace CodeCasa.Pipeline
{
    public class Pipeline<TState>(IServiceProvider serviceProvider) : IPipeline<TState>
    {
        private readonly Subject<TState?> _newOutputSubject = new();
        private readonly List<IPipelineNode<TState>> _nodes = new();

        private Action<TState>? _action;
        private IDisposable? _subscription;

        public IPipeline<TState> SetDefault(TState state)
        {
            Input = state;
            return this;
        }

        public IPipeline<TState> RegisterNode<TNode>() where TNode : IPipelineNode<TState>
        {
            var newNode = ActivatorUtilities.CreateInstance<TNode>(serviceProvider);

            _subscription?.Dispose(); // Dispose old subscription if any.
            _subscription = newNode.OnNewOutput.Subscribe(output => Output = output);

            if (_nodes.Any())
            {
                var previousNode = _nodes.Last();
                previousNode.OnNewOutput.Subscribe(output => newNode.Input = output);

                newNode.Input = previousNode.Output;
            }

            _nodes.Add(newNode);

            if (_nodes.Count == 1)
            {
                newNode.Input = Input;
            }

            Output = newNode.Output;

            return this;
        }

        public IPipeline<TState> SetOutputHandler(Action<TState> action)
        {
            _action = action;
            if (Output != null)
            {
                _action(Output);
            }

            return this;
        }

        public IObservable<TState?> OnNewOutput => _newOutputSubject.AsObservable();

        private TState? _input;
        public bool Enabled
        {
            get => throw new NotImplementedException();
            set => throw new NotImplementedException();
        }

        public TState? Input
        {
            get => _input;
            set
            {
                _input = value;
                if (!_nodes.Any())
                {
                    Output = _input;
                    return;
                }

                _nodes.First().Input = _input;
            }
        }

        private TState? _output;
        public TState? Output
        {
            get => _output;
            private set
            {
                if (value == null || EqualityComparer<TState>.Default.Equals(_output, value))
                {
                    return;
                }

                _output = value;
                _action?.Invoke(value);
                _newOutputSubject.OnNext(value);
            }
        }
    }
}
