using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace CodeCasa.AutomationPipelines;

/// <summary>
/// Implementation of <see cref="IPipelineNode{TState}"/> meant to manage its own output.
/// Has convenient protected methods to control the output and pass-through behavior.
/// </summary>
public abstract class PipelineNode<TState> : IPipelineNode<TState>
{
    private readonly Subject<TState?> _newOutputSubject = new();
    private TState? _input;
    private TState? _output;
    private bool _passThroughNextInput;

    /// <inheritdoc />
    public IObservable<TState?> OnNewOutput => _newOutputSubject.AsObservable();

    /// <inheritdoc />
    public Guid Id { get; } = Guid.CreateVersion7();

    /// <inheritdoc />
    public string? Name { get; set; }

    /// <inheritdoc />
    public TState? Input
    {
        get => _input;
        set
        {
            _input = value;
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
    /// Called when the input is received.
    /// </summary>
    protected virtual void InputReceived(TState? input)
    {
        // As most node implementations will set their own output, we ignore input as the default behavior.
    }

    /// <summary>
    /// Sets the output state of the node.
    /// If pass-through mode is enabled for this node it will be disabled when setting an output value.
    /// </summary>
    public TState? Output
    {
        get => _output;
        protected set
        {
            PassThrough = false;
            _passThroughNextInput = false;

            SetOutputInternal(value);
        }
    }

    /// <summary>
    /// If set to true, the node will pass its input to the output without processing it.
    /// If true, InputReceived is not called.
    /// </summary>
    protected bool PassThrough
    {
        get;
        set
        {
            // Always reset _passThroughNextInput when PassThrough is explicitly called.
            _passThroughNextInput = false;

            if (field == value)
            {
                return;
            }

            field = value;
            if (field)
            {
                SetOutputInternal(_input);
            }
        }
    }

    /// <summary>
    /// Changes the output state of the node and enables pass-through mode after the next input.
    /// This can be useful for nodes that should influence pipeline behavior once. For example a light switch or a motion sensor detection.
    /// </summary>
    protected void ChangeOutputAndTurnOnPassThroughOnNextInput(TState? output)
    {
        Output = output;
        TurnOnPassThroughOnNextInput();
    }

    /// <summary>
    /// Keeps the current output but enables pass-through mode after receiving the next input.
    /// This can be useful for nodes that  should influence pipeline behavior once. For example a light switch or a motion sensor detection.
    /// </summary>
    protected void TurnOnPassThroughOnNextInput()
    {
        if (PassThrough)
        {
            return;
        }

        _passThroughNextInput = true;
    }

    private void SetOutputInternal(TState? output)
    {
        _output = output;
        _newOutputSubject.OnNext(output);
    }

    /// <inheritdoc />
    public override string ToString() => Name ?? GetType().Name;

    /// <inheritdoc />
    public virtual ValueTask DisposeAsync()
    {
        if (_newOutputSubject.IsDisposed)
        {
            return ValueTask.CompletedTask;
        }
        _newOutputSubject.OnCompleted();
        _newOutputSubject.Dispose();
        return ValueTask.CompletedTask;
    }
}