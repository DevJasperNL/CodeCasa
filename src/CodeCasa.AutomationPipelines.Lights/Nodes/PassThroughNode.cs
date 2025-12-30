namespace CodeCasa.AutomationPipelines.Lights.Nodes;

internal class PassThroughNode<TState> : PipelineNode<TState>
{
    public PassThroughNode()
    {
        PassThrough = true;
    }
}