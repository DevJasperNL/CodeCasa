using CodeCasa.Lights;

namespace CodeCasa.AutomationPipelines.Lights;

/// <summary>
/// Options for an interaction node that reacts to changes made to a light outside of its pipeline, for example with a
/// physical switch or the Home Assistant app.
/// </summary>
public class InteractionNodeOptions
{
    /// <summary>
    /// Gets or sets a value indicating whether an external change that leaves the light on (a different brightness or colour,
    /// or turning it on) is held as the pipeline output. When <see langword="false"/> (the default) only turning the light off
    /// externally is handled.
    /// </summary>
    public bool HoldExternalChanges { get; set; }

    /// <summary>
    /// Gets or sets how long an external change is held. When <see langword="null"/> (the default) it is held until the
    /// pipeline output upstream of the interaction node changes.
    /// </summary>
    public TimeSpan? HoldTimeout { get; set; }

    /// <summary>
    /// Gets or sets how long after the pipeline changed its output (plus that output's transition time) reported light states
    /// are ignored. Lights report intermediate or delayed states while they follow a new output, which must not be mistaken for
    /// external changes. Defaults to 3 seconds.
    /// </summary>
    public TimeSpan SettleTime { get; set; } = TimeSpan.FromSeconds(3);

    /// <summary>
    /// Gets or sets the comparer used to decide whether a reported light state differs from the pipeline output.
    /// Defaults to <see cref="LightParametersComparer.Tolerant"/>.
    /// </summary>
    public IEqualityComparer<LightParameters> Comparer { get; set; } = LightParametersComparer.Tolerant;
}
