using System.Globalization;
using Ingweland.Fog.App.Models;
using Ingweland.Fog.Shared.Localization;

namespace Ingweland.Fog.App.Services;

/// <summary>
///     The app's language, picked as WebApp.Client's ClientLocaleService picks the website's: the one chosen in the
///     side menu, else the system's when the site supports it, else English. FogResource strings, game localization
///     data and the map font are all selected from CultureInfo.CurrentCulture / CurrentUICulture.
/// </summary>
internal static class AppCulture
{
    private const string SELECTED_CULTURE_KEY = "AppCulture.SelectedCulture";

    /// <summary>
    ///     The website's language menu: each language by its own name, e.g. DEUTSCH.
    /// </summary>
    public static IReadOnlyList<LocaleInfo> SupportedLocales { get; } = HohSupportedCultures.AllCultures
        .Select(code =>
        {
            var culture = CultureInfo.GetCultureInfo(code);
            return new LocaleInfo(code, culture.NativeName.Split(' ')[0].ToUpper(culture));
        })
        .ToList();

    public static LocaleInfo Current { get; private set; } = null!;

    public static void Apply()
    {
        var selected = Preferences.Get(SELECTED_CULTURE_KEY, string.Empty);
        Set(HohSupportedCultures.AllCultures.Contains(selected) ? selected : Resolve(CultureInfo.CurrentUICulture));
    }

    /// <summary>
    ///     Switches to the language chosen in the side menu and keeps it for later launches, as the website keeps it
    ///     in local storage.
    /// </summary>
    public static void Change(LocaleInfo locale)
    {
        Preferences.Set(SELECTED_CULTURE_KEY, locale.Code);
        Set(locale.Code);
    }

    // Only the defaults, as the website sets them. The CurrentCulture setters store a culture per async flow, which
    // outranks the defaults: set at startup, it would keep the first language on the main thread after a change.
    private static void Set(string code)
    {
        var culture = CultureInfo.GetCultureInfo(code);
        CultureInfo.DefaultThreadCurrentCulture = culture;
        CultureInfo.DefaultThreadCurrentUICulture = culture;
        Current = SupportedLocales.First(x => x.Code == code);
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
