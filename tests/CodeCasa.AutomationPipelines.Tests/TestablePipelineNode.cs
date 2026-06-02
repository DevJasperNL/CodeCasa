
namespace CodeCasa.AutomationPipelines.Tests;

internal class TestablePipelineNode<TState> : PipelineNode<TState>
{
    public new TState? Output
    {
        get => base.Output;
        set => base.Output = value;
    }

    public new bool PassThrough
    {
        get => base.PassThrough;
        set => base.PassThrough = value;
    }

    public new void ChangeOutputAndTurnOnPassThroughOnNextInput(TState? output)
    {
        base.ChangeOutputAndTurnOnPassThroughOnNextInput(output);
    }
}