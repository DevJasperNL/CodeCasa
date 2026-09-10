namespace CodeCasa.Lights.NetDaemon;

internal static class ColorModes
{
    public const string ColorTemp = "color_temp";
    public const string Brightness = "brightness";
    public const string OnOff = "onoff";
    public const string Xy = "xy";
    public const string Hs = "hs";
    public const string Rgb = "rgb";
    public const string Rgbw = "rgbw";
    public const string Rgbww = "rgbww";
    public const string White = "white";

    /// <summary>
    /// Colour modes for which Home Assistant always reports an <c>rgb_color</c> attribute.
    /// </summary>
    public static readonly string[] RgbBased = [Xy, Hs, Rgb, Rgbw, Rgbww];
}
