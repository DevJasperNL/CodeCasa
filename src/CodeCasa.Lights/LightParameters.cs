using System.Drawing;
using System.Text;

namespace CodeCasa.Lights;

/// <summary>
/// This record represents the state of a light.
/// Using a separate record rather than LightTurnOnParameters makes it easier to see what properties are being used. Especially as this record will also be used to store current state in later automations.
/// </summary>
public partial record LightParameters
{
    /// <summary>
    /// Gets the brightness level of the light, ranging from 0 to 255. A value of 0 or null indicates the light is off.
    /// </summary>
    public double? Brightness { get; init; }

    /// <summary>
    /// Gets the RGB color of the light, if applicable. This property is used when the light supports color modes.
    /// </summary>
    public Color? RgbColor { get; init; }

    /// <summary>
    /// Gets the color temperature of the light in kelvin, if applicable. This property is used when the light supports color temperature modes.
    /// </summary>
    public int? ColorTempKelvin { get; init; }

    /// <summary>
    /// Creates a <see cref="LightParameters"/> object representing an off state.
    /// </summary>
    /// <returns>A <see cref="LightParameters"/> object with brightness set to 0.</returns>
    public static LightParameters Off()
    {
        return new LightParameters { Brightness = 0 };
    }

    /// <summary>
    /// Creates a <see cref="LightParameters"/> object representing an on state with maximum brightness.
    /// This is intended to be used for binary lights that only support on/off states.
    /// </summary>
    /// <returns>A <see cref="LightParameters"/> object with brightness set to maximum (255).</returns>
    public static LightParameters On()
    {
        // Note: this is meant to be used by binary lights.
        return new LightParameters { Brightness = byte.MaxValue };
    }

    /// <summary>
    /// Converts this <see cref="LightParameters"/> to a <see cref="LightTransition"/> with no transition time.
    /// </summary>
    /// <returns>A <see cref="LightTransition"/> representing an instant transition to these parameters.</returns>
    public LightTransition AsTransition()
    {
        return new LightTransition
        {
            LightParameters = this
        };
    }

    /// <summary>
    /// Converts this <see cref="LightParameters"/> to a <see cref="LightTransition"/> with the specified transition time.
    /// </summary>
    /// <param name="transitionTime">The duration of the transition.</param>
    /// <returns>A <see cref="LightTransition"/> with the specified transition time.</returns>
    public LightTransition AsTransition(TimeSpan transitionTime)
    {
        return new LightTransition
        {
            LightParameters = this,
            TransitionTime = transitionTime
        };
    }

    /// <summary>
    /// Converts this <see cref="LightParameters"/> to a <see cref="LightTransition"/> with the specified transition time in seconds.
    /// </summary>
    /// <param name="transitionTimeInSeconds">The duration of the transition in seconds.</param>
    /// <returns>A <see cref="LightTransition"/> with the specified transition time.</returns>
    public LightTransition AsTransitionInSeconds(int transitionTimeInSeconds)
    {
        return new LightTransition
        {
            LightParameters = this,
            TransitionTime = TimeSpan.FromSeconds(transitionTimeInSeconds)
        };
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        StringBuilder sb = new StringBuilder();
        if (Brightness == 0)
        {
            sb.Append("off");
        }
        else
        {
            if (RgbColor != null)
            {
                sb.Append($"R,G,B: {RgbColor.Value.R},{RgbColor.Value.G},{RgbColor.Value.B}");
            }
            else if (ColorTempKelvin != null)
            {
                sb.Append($"temperature {ColorTempKelvin}");
            }

            if (sb.Length == 0)
            {
                sb.Append($"brightness {Brightness}");
            }
            else
            {
                sb.Append($" ({Brightness})");
            }
        }

        if (sb.Length == 0)
        {
            sb.Append("unknown light parameters");
        }
        return sb.ToString();
    }
}