using System.Drawing;
using CodeCasa.Lights.Extensions;

namespace CodeCasa.Lights.Tests
{
    [TestClass]
    public sealed class LightParametersExtensionsTests
    {
        [TestMethod]
        public void Interpolate_BothOff_ReturnsOff()
        {
            var from = LightParameters.Off();
            var to = LightParameters.Off();

            var result = from.Interpolate(to, 0.5);

            Assert.AreEqual(0, result.Brightness);
            Assert.IsNull(result.RgbColor);
            Assert.IsNull(result.ColorTempKelvin);
        }

        [TestMethod]
        public void Interpolate_FromOffWithoutColor_CopiesToRgbColor()
        {
            var from = LightParameters.Off();
            var to = new LightParameters { Brightness = 50, RgbColor = Color.Yellow };

            var result = from.Interpolate(to, 0.5);

            Assert.AreEqual(25, result.Brightness);
            Assert.AreEqual(Color.Yellow.ToArgb(), result.RgbColor?.ToArgb());
            Assert.IsNull(result.ColorTempKelvin);
        }

        [TestMethod]
        public void Interpolate_ToOffWithoutColor_CopiesFromRgbColor()
        {
            var from = new LightParameters { Brightness = 50, RgbColor = Color.Yellow };
            var to = LightParameters.Off();

            var result = from.Interpolate(to, 0.5);

            Assert.AreEqual(25, result.Brightness);
            Assert.AreEqual(Color.Yellow.ToArgb(), result.RgbColor?.ToArgb());
            Assert.IsNull(result.ColorTempKelvin);
        }

        [TestMethod]
        public void Interpolate_FromOffWithoutColor_CopiesToColorTemp()
        {
            var from = LightParameters.Off();
            var to = new LightParameters { Brightness = 50, ColorTempKelvin = 3333 };

            var result = from.Interpolate(to, 0.5);

            Assert.AreEqual(25, result.Brightness);
            Assert.IsNull(result.RgbColor);
            Assert.AreEqual(3333, result.ColorTempKelvin);
        }

        [TestMethod]
        public void Interpolate_ToOffWithoutColor_CopiesFromColorTemp()
        {
            var from = new LightParameters { Brightness = 50, ColorTempKelvin = 2500 };
            var to = LightParameters.Off();

            var result = from.Interpolate(to, 0.5);

            Assert.AreEqual(25, result.Brightness);
            Assert.IsNull(result.RgbColor);
            Assert.AreEqual(2500, result.ColorTempKelvin);
        }

        [TestMethod]
        public void Interpolate_RgbColor_BlendsColors()
        {
            var from = new LightParameters { Brightness = 100, RgbColor = Color.Red };
            var to = new LightParameters { Brightness = 100, RgbColor = Color.Blue };

            var result = from.Interpolate(to, 0.5);

            Assert.AreEqual(100, result.Brightness);
            var expected = Color.FromArgb(199, 0, 199);
            Assert.AreEqual(expected, result.RgbColor);
            Assert.IsNull(result.ColorTempKelvin);
        }

        [TestMethod]
        public void Interpolate_RgbColor_BlendsColors_From()
        {
            var from = new LightParameters { Brightness = 25, RgbColor = Color.Red };
            var to = new LightParameters { Brightness = 100, RgbColor = Color.Blue };

            var result = from.Interpolate(to, 0);

            Assert.AreEqual(25, result.Brightness);
            Assert.AreEqual(Color.Red.ToArgb(), result.RgbColor?.ToArgb());
            Assert.IsNull(result.ColorTempKelvin);
        }

        [TestMethod]
        public void Interpolate_RgbColor_BlendsColors_To()
        {
            var from = new LightParameters { Brightness = 25, RgbColor = Color.Red };
            var to = new LightParameters { Brightness = 100, RgbColor = Color.Blue };

            var result = from.Interpolate(to, 1);

            Assert.AreEqual(100, result.Brightness);
            Assert.AreEqual(Color.Blue.ToArgb(), result.RgbColor?.ToArgb());
            Assert.IsNull(result.ColorTempKelvin);
        }

        [TestMethod]
        public void Interpolate_ColorTemp_BlendsMiredLinearly()
        {
            var from = new LightParameters { Brightness = 100, ColorTempKelvin = 5000 };
            var to = new LightParameters { Brightness = 100, ColorTempKelvin = 2500 };

            var result = from.Interpolate(to, 0.5);

            Assert.AreEqual(100, result.Brightness);
            Assert.AreEqual(3750, result.ColorTempKelvin);
        }

        [TestMethod]
        public void Interpolate_MissingBothRgbAndTemp_Throws()
        {
            var from = new LightParameters { Brightness = 50 };
            var to = new LightParameters { Brightness = 50 };

            Assert.Throws<InvalidOperationException>(() => from.Interpolate(to, 0.5));
        }
    }
}
