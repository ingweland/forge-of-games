namespace Ingweland.Fog.App.Views;

/// <summary>
///     Looks up the app-level styles and colors (Resources/Styles) for views built in code.
/// </summary>
internal static class ThemeResources
{
    public static Style Style(string key)
    {
        return (Style) Get(key);
    }

    public static Color Color(string key)
    {
        return (Color) Get(key);
    }

    private static object Get(string key)
    {
        // Fully qualified: a bare `Application` binds to the Ingweland.Fog.Application namespace here.
        var resources = Microsoft.Maui.Controls.Application.Current?.Resources ??
            throw new InvalidOperationException("The application resources are not available yet.");
        return resources.TryGetValue(key, out var value)
            ? value
            : throw new KeyNotFoundException($"Resource `{key}` was not found.");
    }
}
