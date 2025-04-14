namespace CodeCasa.Pipeline;

public interface IPipelineNode<TState>
{
    bool Enabled { get; set; }
    TState? Input { get; set; }
    TState? Output { get; }
    IObservable<TState?> OnNewOutput { get; }
}