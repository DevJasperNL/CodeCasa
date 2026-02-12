namespace CodeCasa.AutomationPipelines.Lights.ReactiveNode;

public enum CompositeAddBehavior
{
    /// <summary>
    /// Gets or sets a value indicating whether instances should be created immediately when registered in a composite
    /// context.
    /// </summary>
    /// <remarks>When set to <see langword="true"/>, objects registered in a composite context are
    /// instantiated as soon as they are added, rather than being created when the corresponding observable emits true.
    /// This way, configuration can be applied to the composite context rather than the context of the individual nodes.</remarks>
    ImmediatelyInstantiateInCompositeContext,
    InstantiateInChildContainer,
}