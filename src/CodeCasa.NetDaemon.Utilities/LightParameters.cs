using System.Drawing;

namespace CodeCasa.NetDaemon.Utilities;

/// <summary>
/// This record represents the state of a light entity.
/// Using a separate record rather than LightTurnOnParameters makes it easier to see what properties are being used. Especially as this record will also be used to store current state in later automations.
/// </summary>
public record LightParameters
{
    public double? Brightness { get; init; }
    public Color? RgbColor { get; init; }
    public int? ColorTemp { get; init; }
    public TimeSpan? Transition { get; init; }

    public static LightParameters Off()
    {
        return new LightParameters { Brightness = 0 };
    }
}