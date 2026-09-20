using Ingweland.Fog.Application.Client.Web.ViewModels;

namespace Ingweland.Fog.App.Views.Guides;

public partial class GuideCard : ContentView
{
    public GuideCard()
    {
        InitializeComponent();
    }

    // The card opens its own guide, so the page's list template needs no wiring.
    private async void OnTapped(object? sender, TappedEventArgs e)
    {
        if (BindingContext is AlliedCultureCityGuideViewModel guide)
        {
            await CityGuidePage.OpenAsync(guide);
        }
    }
}
