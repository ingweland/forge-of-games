using Ingweland.Fog.Models.Fog.Entities;
using Ingweland.Fog.Models.Fog.Enums;

namespace Ingweland.Fog.App.Views.Guides;

/// <summary>
///     One row of the guide's timeline, the web's <c>.timeline-item-content</c>: the item's number in a circle,
///     its type icon and its title.
/// </summary>
/// <remarks>
///     Built in code, and selection is a plain property rather than a binding: the shared timeline items raise
///     no change notifications, and <see cref="TimelineView" /> holds these rows anyway.
/// </remarks>
internal sealed class TimelineItemView : ContentView
{
    private bool _isSelected;

    public TimelineItemView(CityStrategyTimelineItemBase item, int index)
    {
        Item = item;

        var number = new Border
        {
            Style = ThemeResources.Style("FogTimelineIndex"),
            VerticalOptions = LayoutOptions.Center,
            Content = new Label
            {
                Text = index.ToString(),
                Style = ThemeResources.Style("FogTimelineIndexLabel"),
            },
        };

        var icon = new Image
        {
            Source = IconFor(item.Type),
            WidthRequest = 24,
            HeightRequest = 24,
            VerticalOptions = LayoutOptions.Center,
        };

        var title = new Label
        {
            Text = item.Title,
            Style = ThemeResources.Style("FogTimelineItemLabel"),
        };

        var tap = new TapGestureRecognizer();
        tap.Tapped += (_, _) => Tapped?.Invoke(this, EventArgs.Empty);
        GestureRecognizers.Add(tap);

        var grid = new Grid {ColumnSpacing = 4};
        grid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Auto));
        // The web gives the icon a fixed slot of its own so the titles line up.
        grid.ColumnDefinitions.Add(new ColumnDefinition(32));
        grid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));
        grid.Add(number);
        grid.Add(icon, 1);
        grid.Add(title, 2);

        Padding = new Thickness(4, 6);
        Content = grid;
    }

    public CityStrategyTimelineItemBase Item { get; }

    public bool IsSelected
    {
        get => _isSelected;
        set
        {
            _isSelected = value;
            // The palette's highlighted surface, where the web leans on MudBlazor's own selected-item tint.
            BackgroundColor = value ? ThemeResources.Color("FogSurface2Color") : Colors.Transparent;
        }
    }

    public event EventHandler? Tapped;

    // The icons CityStrategyTimelineComponentBase.GetTypeIcon names, as Material Symbols files.
    private static string IconFor(CityStrategyTimelineItemType type)
    {
        return type switch
        {
            CityStrategyTimelineItemType.Research => "timeline_research.png",
            CityStrategyTimelineItemType.Layout => "timeline_layout.png",
            CityStrategyTimelineItemType.Intro => "timeline_intro.png",
            _ => "timeline_description.png",
        };
    }
}
