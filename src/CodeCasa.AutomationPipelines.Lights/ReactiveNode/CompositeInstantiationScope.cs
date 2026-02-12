namespace CodeCasa.AutomationPipelines.Lights.ReactiveNode;

/// <summary>
/// Specifies the service provider scope in which composite child objects (pipelines, reactive nodes) are instantiated.
/// </summary>
public enum CompositeInstantiationScope
{
    /// <summary>
    /// Objects are instantiated immediately in the shared composite context.
    /// </summary>
    /// <remarks>
    /// When using this scope, objects are created as soon as they are registered, allowing configuration
    /// to be applied uniformly across all composite members. The nested pipeline or node shares the same
    /// lifetime as the parent pipeline—it is created once and remains active until the parent is disposed.
    /// This is useful when you want shared state or behavior across all lights in a composite group.
    /// </remarks>
    Shared,

    /// <summary>
    /// Objects are instantiated in individual child containers, each with its own light-specific service scope.
    /// </summary>
    /// <remarks>
    /// When using this scope, a new instance is created each time the corresponding observable emits,
    /// and each light receives its own instance within its own service scope. The instance is disposed
    /// whenever it is replaced by a new node (e.g., when another trigger fires). This is useful when you
    /// need light-specific dependencies, isolated state per light, or fresh instances on each activation.
    /// </remarks>
    PerChild,
}