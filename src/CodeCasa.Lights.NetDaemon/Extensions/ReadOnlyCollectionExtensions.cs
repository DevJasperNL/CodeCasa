using System.Drawing;

namespace CodeCasa.Lights.NetDaemon.Extensions;

internal static class ReadOnlyCollectionExtensions
{
    /// <summary>
    /// Converts a read-only collection of double values (RGB components) to a <see cref="Color"/> object.
    /// </summary>
    /// <param name="rgbCollection">A read-only collection containing exactly 3 elements representing red, green, and blue values (0-255).</param>
    /// <returns>A <see cref="Color"/> object created from the RGB values.</returns>
    /// <exception cref="ArgumentException">Thrown if the collection does not contain exactly 3 elements.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if any RGB value is outside the 0-255 range.</exception>
    public static Color ToColor(this IReadOnlyCollection<double> rgbCollection)
    {
        return rgbCollection.Select(x => (int)x).ToArray().ToColor();
    }

    /// <summary>
    /// Converts a read-only collection of integer values (RGB components) to a <see cref="Color"/> object.
    /// </summary>
    /// <param name="rgbCollection">A read-only collection containing exactly 3 elements representing red, green, and blue values (0-255).</param>
    /// <returns>A <see cref="Color"/> object created from the RGB values.</returns>
    /// <exception cref="ArgumentException">Thrown if the collection does not contain exactly 3 elements.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if any RGB value is outside the 0-255 range.</exception>
    public static Color ToColor(this IReadOnlyCollection<int> rgbCollection)
    {
        if (rgbCollection.Count != 3)
        {
            throw new ArgumentException("The collection must contain exactly 3 elements representing R, G, and B.");
        }

        var r = rgbCollection.ElementAt(0);
        var g = rgbCollection.ElementAt(1);
        var b = rgbCollection.ElementAt(2);

        if (r is < 0 or > 255)
        {
            throw new ArgumentOutOfRangeException(nameof(r), "R value must be between 0 and 255.");
        }

        if (g is < 0 or > 255)
        {
            throw new ArgumentOutOfRangeException(nameof(g), "G value must be between 0 and 255.");
        }

        if (b is < 0 or > 255)
        {
            throw new ArgumentOutOfRangeException(nameof(b), "B value must be between 0 and 255.");
        }

        return Color.FromArgb(r, g, b);
    }
}