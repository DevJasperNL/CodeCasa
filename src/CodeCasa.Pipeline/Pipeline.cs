using Microsoft.Extensions.DependencyInjection;

namespace CodeCasa.Pipeline;

public class Pipeline<TState>(IServiceProvider serviceProvider) : PipelineNode<TState>, IPipeline<TState>
{
    private readonly List<IPipelineNode<TState>> _nodes = new();

    private bool _callActionDistinct = true;
    private Action<TState>? _action;
    private IDisposable? _subscription;

    public IPipeline<TState> SetDefault(TState state)
    {
        Input = state;
        return this;
    }

    public IPipeline<TState> RegisterNode<TNode>() where TNode : IPipelineNode<TState>
    {
        return RegisterNode(ActivatorUtilities.CreateInstance<TNode>(serviceProvider));
    }

    public IPipeline<TState> RegisterNode<TNode>(TNode node) where TNode : IPipelineNode<TState>
    {
        _subscription?.Dispose(); // Dispose old subscription if any.
        _subscription = node.OnNewOutput.Subscribe(SetOutputAndCallActionWhenApplicable);

        if (_nodes.Any())
        {
            var previousNode = _nodes.Last();
            previousNode.OnNewOutput.Subscribe(output => node.Input = output);

            node.Input = previousNode.Output;
        }

        _nodes.Add(node);

        if (_nodes.Count == 1)
        {
            node.Input = Input;
        }

        SetOutputAndCallActionWhenApplicable(node.Output);

        return this;
    }

    public IPipeline<TState> SetOutputHandler(Action<TState> action, bool callActionDistinct = true)
    {
        _callActionDistinct = callActionDistinct;
        _action = action;
        if (Output != null)
        {
            _action(Output);
        }

        return this;
    }

    protected override void InputReceived(TState? state)
    {
        if (!_nodes.Any())
        {
            SetOutputAndCallActionWhenApplicable(Input);
            return;
        }

        _nodes.First().Input = Input;
    }

    private void SetOutputAndCallActionWhenApplicable(TState? output)
    {
        var outputChanged = !EqualityComparer<TState>.Default.Equals(Output, output);

        Output = output;

        if (output != null && (!_callActionDistinct || outputChanged))
        {
            // Note that _action will be called AFTER OnNewOutput.
            _action?.Invoke(output);
        }
    }
}