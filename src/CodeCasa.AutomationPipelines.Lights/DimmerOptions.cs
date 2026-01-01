using CodeCasa.Lights;

namespace CodeCasa.AutomationPipelines.Lights;

/// <summary>
/// Configuration options for dimmer behavior, including brightness levels, step sizes, and timing.
/// </summary>
public record DimmerOptions
{
    /// <summary>
    /// Gets or sets the minimum brightness level. Defaults to 2.
    /// </summary>
    public int MinBrightness { get; set; } = 2;

    /// <summary>
    /// Gets or sets the brightness step size for each dimming increment. Defaults to 51.
    /// </summary>
    public int BrightnessStep { get; set; } = 51;

    /// <summary>
    /// Gets or sets the time delay between each brightness step. Defaults to 500 milliseconds.
    /// </summary>
    public TimeSpan TimeBetweenSteps { get; set; } = TimeSpan.FromMilliseconds(500);

    /// <summary>
    /// Gets or sets the collection of light IDs that define the order for dimming operations.
    /// </summary>
    public IEnumerable<string>? DimOrderLights { get; set; }

    /// <summary>
    /// Sets the light order for dimming operations based on the provided collection of light entities.
    /// </summary>
    /// <param name="lights">The light entities that define the dimming order.</param>
    public void SetLightOrder(IEnumerable<ILight> lights) =>
        DimOrderLights = lights.Select(l => l.Id).ToArray();

    /// <summary>
    /// Sets the light order for dimming operations based on the provided light entities.
    /// </summary>
    /// <param name="lights">The light entities that define the dimming order.</param>
    public void SetLightOrder(params ILight[] lights) =>
        DimOrderLights = lights.Select(l => l.Id).ToArray();
}