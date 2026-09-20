using Ingweland.Fog.Application.Client.Web.ViewModels;

namespace Ingweland.Fog.App.Views;

public partial class CityGuidePage : ContentPage, IQueryAttributable
{
    public const string GUIDE_QUERY_KEY = "guide";

    public CityGuidePage()
    {
        InitializeComponent();
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue(GUIDE_QUERY_KEY, out var value) && value is AlliedCultureCityGuideViewModel guide)
        {
            Title = guide.Wonder.Name;
        }
    }

    public static Task OpenAsync(AlliedCultureCityGuideViewModel guide)
    {
        return Shell.Current.GoToAsync(nameof(CityGuidePage),
            new ShellNavigationQueryParameters {{GUIDE_QUERY_KEY, guide}});
    }
}
