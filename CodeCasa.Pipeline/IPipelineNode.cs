namespace CodeCasa.Pipeline;

public interface IPipelineNode<TState>
{
    TState? Input { get; set; }
    TState? Output { get; }
    IObservable<TState?> OnNewOutput { get; }
}