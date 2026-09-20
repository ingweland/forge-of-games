using Ingweland.Fog.Application.Client.Core.Localization;
using Ingweland.Fog.Models.Fog.Entities;

namespace Ingweland.Fog.App.Views.Guides;

/// <summary>
///     The guide's timeline: the MAUI counterpart of CityStrategyTimelineViewerComponent. The "Timeline" tag
///     hanging under the panel's top edge, then the guide's items, one row each, the selected one highlighted.
/// </summary>
internal sealed class TimelineView : ContentView
{
    private readonly List<TimelineItemView> _rows = [];
    private readonly VerticalStackLayout _stack;

    public TimelineView()
    {
        var header = new Border
        {
            Style = ThemeResources.Style("FogTimelineHeader"),
            Content = new Label
            {
                Text = FogResource.CityStrategy_Timeline,
                Style = ThemeResources.Style("FogTimelineHeaderLabel"),
            },
        };

        _stack = new VerticalStackLayout {Spacing = 6};

        var grid = new Grid {RowSpacing = 6};
        grid.RowDefinitions.Add(new RowDefinition(GridLength.Auto));
        grid.RowDefinitions.Add(new RowDefinition(GridLength.Star));
        grid.Add(header);
        grid.Add(new ScrollView {Content = _stack}, 0, 1);

        BackgroundColor = ThemeResources.Color("FogSurfaceColor");
        Content = grid;
    }

    public event EventHandler<string>? ItemSelected;

    public void SetItems(IEnumerable<CityStrategyTimelineItemBase> items)
    {
        _stack.Clear();
        _rows.Clear();

        var index = 1;
        foreach (var item in items)
        {
            var row = new TimelineItemView(item, index);
            row.Tapped += OnRowTapped;
            _rows.Add(row);
            _stack.Add(row);
            index++;
        }
    }

    public void Select(string? id)
    {
        foreach (var row in _rows)
        {
            row.IsSelected = row.Item.Id == id;
        }
    }

    private void OnRowTapped(object? sender, EventArgs e)
    {
        if (sender is TimelineItemView row)
        {
            ItemSelected?.Invoke(this, row.Item.Id);
        }
    }
}
