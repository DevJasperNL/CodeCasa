namespace CodeCasa.AutomationPipelines.Lights.Nodes;

internal class PassThroughNode<TState> : PipelineNode<TState>
{
    public PassThroughNode()
    {
        Name = "Pass Through Node";
        PassThrough = true;
    }
}