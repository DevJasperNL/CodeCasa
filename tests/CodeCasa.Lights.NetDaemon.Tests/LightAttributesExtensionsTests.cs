using System.Drawing;
using CodeCasa.Lights.NetDaemon.Extensions;
using CodeCasa.Lights.NetDaemon.Generated;

namespace CodeCasa.Lights.NetDaemon.Tests;

[TestClass]
public sealed class LightAttributesExtensionsTests
{
    [TestMethod]
    [DataRow("hs")]
    [DataRow("rgb")]
    [DataRow("rgbw")]
    [DataRow("rgbww")]
    [DataRow("xy")]
    public void ToLightParameters_ColorModes_UseRgbColor(string colorMode)
    {
        var attributes = new LightAttributes
        {
            ColorMode = colorMode,
            Brightness = 100,
            RgbColor = new List<double> { 255, 0, 0 }
        };

        var parameters = attributes.ToLightParameters();

        Assert.AreEqual(100, parameters.Brightness);
        Assert.AreEqual(Color.FromArgb(255, 0, 0).ToArgb(), parameters.RgbColor?.ToArgb());
        Assert.IsNull(parameters.ColorTempKelvin);
    }

    [TestMethod]
    public void ToLightParameters_ColorTemp_UsesKelvin()
    {
        var attributes = new LightAttributes { ColorMode = "color_temp", Brightness = 50, ColorTempKelvin = 2700 };

        var parameters = attributes.ToLightParameters();

        Assert.AreEqual(50, parameters.Brightness);
        Assert.AreEqual(2700, parameters.ColorTempKelvin);
        Assert.IsNull(parameters.RgbColor);
    }

    [TestMethod]
    [DataRow("white")]
    [DataRow("brightness")]
    public void ToLightParameters_BrightnessOnlyModes_UseBrightness(string colorMode)
    {
        var attributes = new LightAttributes { ColorMode = colorMode, Brightness = 42 };

        var parameters = attributes.ToLightParameters();

        Assert.AreEqual(42, parameters.Brightness);
        Assert.IsNull(parameters.RgbColor);
        Assert.IsNull(parameters.ColorTempKelvin);
    }

    [TestMethod]
    public void ToLightParameters_OnOff_IsFullBrightness()
    {
        var parameters = new LightAttributes { ColorMode = "onoff" }.ToLightParameters();

        Assert.AreEqual(255, parameters.Brightness);
    }

    [TestMethod]
    public void ToLightParameters_UnknownMode_DoesNotThrowAndFallsBackToBrightness()
    {
        var parameters = new LightAttributes { ColorMode = "unknown", Brightness = 10 }.ToLightParameters();

        Assert.AreEqual(10, parameters.Brightness);
        Assert.IsNull(parameters.RgbColor);
        Assert.IsNull(parameters.ColorTempKelvin);
    }

    [TestMethod]
    public void ToLightParameters_NoColorMode_IsOff()
    {
        var parameters = new LightAttributes { Brightness = 10 }.ToLightParameters();

        Assert.AreEqual(LightParameters.Off(), parameters);
    }
}
