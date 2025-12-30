namespace CodeCasa.AutomationPipelines.Lights;

/// <summary>
/// Specifies the behavior for lights that are excluded from specific pipeline operations.
/// </summary>
public enum ExcludedLightBehaviours
{
    /// <summary>
    /// No behavior will be defined for excluded lights.
    /// Either add behavior specifically, or they will be out of sync when toggling or cycling through behaviors.
    /// </summary>
    None,
    /// <summary>
    /// A simple pass-through node is added for excluded lights to ensure they stay in sync when toggling or cycling through behaviors.
    /// </summary>
    PassThrough
}