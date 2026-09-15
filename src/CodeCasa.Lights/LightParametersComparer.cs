namespace CodeCasa.Lights;

/// <summary>
/// Compares <see cref="LightParameters"/> with tolerances, so that a state reported back by a light (which is typically
/// rounded or converted between colour spaces) still matches the parameters that were sent to it.
/// </summary>
/// <remarks>
/// <list type="bullet">
/// <item>A brightness of 0 or <see langword="null"/> means off; two off states are equal regardless of their colour.</item>
/// <item>Colours are only compared when both sides specify the same kind of colour. If one side has no colour information, colour is ignored. An RGB colour never equals a colour temperature.</item>
/// <item>Because of the tolerances this comparer is not transitive, and <see cref="GetHashCode(LightParameters)"/> only distinguishes on from off. Do not use it as a key comparer for hash based collections.</item>
/// </list>
/// </remarks>
public sealed class LightParametersComparer : IEqualityComparer<LightParameters>
{
    private readonly double _brightnessTolerance;
    private readonly int _colorTempKelvinTolerance;
    private readonly int _rgbChannelTolerance;

    /// <summary>
    /// Initializes a new instance of the <see cref="LightParametersComparer"/> class.
    /// </summary>
    /// <param name="brightnessTolerance">The maximum allowed brightness difference (on a 0-255 scale).</param>
    /// <param name="colorTempKelvinTolerance">The maximum allowed colour temperature difference in kelvin.</param>
    /// <param name="rgbChannelTolerance">The maximum allowed difference per RGB channel.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when a tolerance is negative.</exception>
    public LightParametersComparer(double brightnessTolerance, int colorTempKelvinTolerance, int rgbChannelTolerance)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(brightnessTolerance);
        ArgumentOutOfRangeException.ThrowIfNegative(colorTempKelvinTolerance);
        ArgumentOutOfRangeException.ThrowIfNegative(rgbChannelTolerance);
        _brightnessTolerance = brightnessTolerance;
        _colorTempKelvinTolerance = colorTempKelvinTolerance;
        _rgbChannelTolerance = rgbChannelTolerance;
    }

    /// <summary>
    /// Gets a comparer with tolerances suited for comparing a light's reported state against the parameters sent to it:
    /// brightness ±2, colour temperature ±50 K and RGB ±10 per channel.
    /// </summary>
    public static LightParametersComparer Tolerant { get; } = new(2, 50, 10);

    /// <inheritdoc/>
    public bool Equals(LightParameters? x, LightParameters? y)
    {
        if (ReferenceEquals(x, y))
        {
            return true;
        }
        if (x is null || y is null)
        {
            return false;
        }

        var xOff = IsOff(x);
        var yOff = IsOff(y);
        if (xOff || yOff)
        {
            return xOff && yOff;
        }

        if (Math.Abs(x.Brightness!.Value - y.Brightness!.Value) > _brightnessTolerance)
        {
            return false;
        }

        if (x.RgbColor is { } xRgb && y.RgbColor is { } yRgb)
        {
            return Math.Abs(xRgb.R - yRgb.R) <= _rgbChannelTolerance &&
                   Math.Abs(xRgb.G - yRgb.G) <= _rgbChannelTolerance &&
                   Math.Abs(xRgb.B - yRgb.B) <= _rgbChannelTolerance;
        }
        if (x.RgbColor != null && y.ColorTempKelvin != null || y.RgbColor != null && x.ColorTempKelvin != null)
        {
            return false;
        }
        if (x.ColorTempKelvin is { } xKelvin && y.ColorTempKelvin is { } yKelvin)
        {
            return Math.Abs(xKelvin - yKelvin) <= _colorTempKelvinTolerance;
        }

        return true;
    }

    /// <inheritdoc/>
    public int GetHashCode(LightParameters obj) => IsOff(obj) ? 0 : 1;

    private static bool IsOff(LightParameters parameters) => (parameters.Brightness ?? 0) == 0;
}
