namespace Ingweland.Fog.App.ViewModels.CityViewer;

internal static class CssColor
{
    /// <summary>
    ///     Converts the CSS colors the web view models carry (e.g. <c>AgeViewModel.Color</c>: <c>#RRGGBB</c> or
    ///     <c>transparent</c>). Returns null when there's nothing usable, so XAML can fall back.
    /// </summary>
    public static Color? Parse(string? css)
    {
        if (string.IsNullOrWhiteSpace(css))
        {
            return null;
        }

        if (string.Equals(css, "transparent", StringComparison.OrdinalIgnoreCase))
        {
            return Colors.Transparent;
        }

        return css.StartsWith('#') ? Color.FromArgb(css) : null;
    }
}
