namespace CodeCasa.AutomationPipelines.Lights.Nodes;

internal enum TimeoutBehaviour
{
    /// <summary>
    /// Outputs <see cref="CodeCasa.Lights.LightTransition.Off()"/> and keeps it until the next input.
    /// </summary>
    TurnOff,

    /// <summary>
    /// Passes the input through at once and stays in pass-through for the rest of the node's lifetime.
    /// </summary>
    PassThrough
}
