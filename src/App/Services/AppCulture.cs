using System.Globalization;
using Ingweland.Fog.Shared.Localization;

namespace Ingweland.Fog.App.Services;

internal static class AppCulture
{
    /// <summary>
    ///     Pins the process culture to one the site supports, as WebApp.Client's Program.cs does.
    ///     FogResource strings, game localization data and the map font are all selected from
    ///     CultureInfo.CurrentCulture / CurrentUICulture.
    /// </summary>
    public static void Apply()
    {
        var culture = CultureInfo.GetCultureInfo(Resolve(CultureInfo.CurrentUICulture));
        CultureInfo.DefaultThreadCurrentCulture = culture;
        CultureInfo.DefaultThreadCurrentUICulture = culture;
        CultureInfo.CurrentCulture = culture;
        CultureInfo.CurrentUICulture = culture;
    }

    private static string Resolve(CultureInfo culture)
    {
        if (HohSupportedCultures.AllCultures.Contains(culture.Name))
        {
            return culture.Name;
        }

        // Fall back on language alone, e.g. en-GB -> en-DK, de-AT -> de-DE.
        var prefix = culture.TwoLetterISOLanguageName + "-";
        return HohSupportedCultures.AllCultures.FirstOrDefault(x =>
                x.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            ?? HohSupportedCultures.DefaultCulture;
    }
}
