using System.Drawing;

namespace CodeCasa.NetDaemon.Utilities.Extensions;

internal static class ColorExtensions
{
    public static IReadOnlyCollection<int> ToRgbCollection(this Color color)
        => new int[] { color.R, color.G, color.B }.AsReadOnly();
}