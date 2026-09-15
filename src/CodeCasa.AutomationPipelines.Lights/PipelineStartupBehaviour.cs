namespace CodeCasa.AutomationPipelines.Lights;

/// <summary>
/// Specifies what a light pipeline does with the light when it is created, for example when the application restarts.
/// </summary>
public enum PipelineStartupBehaviour
{
    /// <summary>
    /// The pipeline starts from an off state and its initial output is applied to the light straight away.
    /// Unless a node overrides it, the light is turned off when the pipeline is created.
    /// </summary>
    TurnOff,

    /// <summary>
    /// The pipeline starts from an off state, but its initial output is not applied. The light is left as it is until the
    /// pipeline output changes. With distinct output, a later output equal to the initial output is not applied either.
    /// </summary>
    SkipInitialOutput,

    /// <summary>
    /// The pipeline starts from the light's current state instead of off, and its initial output is not applied.
    /// Nodes that pass their input through keep the light as it is.
    /// </summary>
    StartFromCurrentLightState
}
