namespace Ingweland.Fog.App.Views.CityViewer;

/// <summary>
///     The shell both cost calculators share: the card, the tag naming it in the top-left corner, and the
///     padded column under it. This is <c>.component-root</c> plus <c>.section-title</c> plus
///     <c>.main-container</c> from CityPlannerBuildingCostCalculator.razor.css, which its wonder counterpart
///     repeats verbatim.
/// </summary>
internal static class CalculatorCard
{
    public static View Build(string title, IEnumerable<View> children)
    {
        var body = new VerticalStackLayout {Spacing = 4, Padding = new Thickness(12, 0, 12, 12)};
        foreach (var child in children)
        {
            body.Add(child);
        }

        var tag = new Border
        {
            Style = ThemeResources.Style("FogCalculatorTag"),
            Content = new Label {Text = title, Style = ThemeResources.Style("FogCalculatorTagLabel")},
        };

        return new Border
        {
            Style = ThemeResources.Style("FogCard"),
            Content = new VerticalStackLayout {Spacing = 8, Children = {tag, body}},
        };
    }

    /// <summary>
    ///     A level selector. The website draws an outlined MudSelect with a floating label; a Picker shows the
    ///     same label as its title until something is picked, and then hands the choice to the platform's own
    ///     dropdown.
    /// </summary>
    public static Picker LevelPicker(string title)
    {
        return new Picker {Title = title, Style = ThemeResources.Style("FogPicker")};
    }
}
