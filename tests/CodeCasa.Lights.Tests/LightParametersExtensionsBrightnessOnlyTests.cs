using CodeCasa.Lights.Extensions;

namespace CodeCasa.Lights.Tests;

[TestClass]
public sealed class LightParametersExtensionsBrightnessOnlyTests
{
    [TestMethod]
    public void Interpolate_BrightnessOnly_ReturnsInterpolatedBrightness()
    {
        var from = new LightParameters { Brightness = 100 };
        var to = new LightParameters { Brightness = 200 };

        var result = from.Interpolate(to, 0.5);

        Assert.AreEqual(150, result.Brightness);
        Assert.IsNull(result.RgbColor);
        Assert.IsNull(result.ColorTempKelvin);
    }

    [TestMethod]
    public void Interpolate_OnToOffTemplates_ReturnsInterpolatedBrightness()
    {
        var result = LightParameters.On().Interpolate(LightParameters.Off(), 0.5);

        Assert.AreEqual(128, result.Brightness);
        Assert.IsNull(result.RgbColor);
        Assert.IsNull(result.ColorTempKelvin);
    }

    [TestMethod]
    public void Interpolate_OnlyOneSideHasColorTemp_KeepsThatColorTemp()
    {
        var from = new LightParameters { Brightness = 100 };
        var to = new LightParameters { Brightness = 200, ColorTempKelvin = 3000 };

        var result = from.Interpolate(to, 0.5);

        Assert.AreEqual(150, result.Brightness);
        Assert.AreEqual(3000, result.ColorTempKelvin);
    }
}
