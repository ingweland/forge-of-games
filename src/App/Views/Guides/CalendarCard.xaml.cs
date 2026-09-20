using Ingweland.Fog.App.ViewModels.Guides;

namespace Ingweland.Fog.App.Views.Guides;

public partial class CalendarCard : ContentView
{
    public CalendarCard()
    {
        InitializeComponent();
    }

    // As on the website: the card opens the wonder's guide, and does nothing when the wonder has none.
    private async void OnTapped(object? sender, TappedEventArgs e)
    {
        if (BindingContext is CalendarCardModel {Guide: { } guide})
        {
            await CityGuidePage.OpenAsync(guide);
        }
    }
}
