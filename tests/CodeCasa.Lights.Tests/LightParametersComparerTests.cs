using System.Drawing;

namespace CodeCasa.Lights.Tests;

[TestClass]
public sealed class LightParametersComparerTests
{
    private static readonly LightParametersComparer Comparer = LightParametersComparer.Tolerant;

    [TestMethod]
    public void Equals_OffStates_AreEqualRegardlessOfColour()
    {
        Assert.IsTrue(Comparer.Equals(LightParameters.Off(), new LightParameters { Brightness = null, ColorTempKelvin = 2700 }));
    }

    [TestMethod]
    public void Equals_OffAndOn_AreNotEqual()
    {
        Assert.IsFalse(Comparer.Equals(LightParameters.Off(), new LightParameters { Brightness = 1 }));
    }

    [TestMethod]
    public void Equals_BrightnessWithinTolerance_AreEqual()
    {
        Assert.IsTrue(Comparer.Equals(new LightParameters { Brightness = 255 }, new LightParameters { Brightness = 254 }));
        Assert.IsFalse(Comparer.Equals(new LightParameters { Brightness = 255 }, new LightParameters { Brightness = 250 }));
    }

    [TestMethod]
    public void Equals_ColorTemperatureWithinTolerance_AreEqual()
    {
        Assert.IsTrue(Comparer.Equals(
            new LightParameters { Brightness = 100, ColorTempKelvin = 2700 },
            new LightParameters { Brightness = 100, ColorTempKelvin = 2688 }));
        Assert.IsFalse(Comparer.Equals(
            new LightParameters { Brightness = 100, ColorTempKelvin = 2700 },
            new LightParameters { Brightness = 100, ColorTempKelvin = 4000 }));
    }

    [TestMethod]
    public void Equals_RgbWithinTolerance_AreEqual()
    {
        Assert.IsTrue(Comparer.Equals(
            new LightParameters { Brightness = 100, RgbColor = Color.FromArgb(255, 0, 0) },
            new LightParameters { Brightness = 100, RgbColor = Color.FromArgb(250, 4, 3) }));
        Assert.IsFalse(Comparer.Equals(
            new LightParameters { Brightness = 100, RgbColor = Color.FromArgb(255, 0, 0) },
            new LightParameters { Brightness = 100, RgbColor = Color.FromArgb(0, 0, 255) }));
    }

    [TestMethod]
    public void Equals_RgbAndColorTemperature_AreNotEqual()
    {
        Assert.IsFalse(Comparer.Equals(
            new LightParameters { Brightness = 100, RgbColor = Color.White },
            new LightParameters { Brightness = 100, ColorTempKelvin = 6500 }));
    }

    [TestMethod]
    public void Equals_OneSideWithoutColour_IgnoresColour()
    {
        Assert.IsTrue(Comparer.Equals(
            new LightParameters { Brightness = 100 },
            new LightParameters { Brightness = 100, ColorTempKelvin = 2700 }));
    }

    [TestMethod]
    public void GetHashCode_EqualValues_HaveEqualHashCodes()
    {
        Assert.AreEqual(
            Comparer.GetHashCode(new LightParameters { Brightness = 255 }),
            Comparer.GetHashCode(new LightParameters { Brightness = 254, ColorTempKelvin = 2700 }));
    }

    [TestMethod]
    public void Constructor_NegativeTolerance_Throws()
    {
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => new LightParametersComparer(-1, 0, 0));
    }
}
