using Microsoft.Maui.Controls.Shapes;

namespace Ingweland.Fog.App.Views.CityViewer;

/// <summary>
///     Fills a <see cref="Grid" /> as a table styled like the web's city planner stats tables: a tinted header
///     row, zebra rows, and content-sized columns with the last one taking the remaining width.
/// </summary>
/// <remarks>
///     Built in code because MAUI cannot share column widths between separately templated rows.
/// </remarks>
internal static class StatsTable
{
    /// <param name="cellMargin">Applied to every cell that doesn't set its own margin.</param>
    /// <param name="endAlignFirstColumn">Right-aligns the first column, header included.</param>
    public static void Fill(Grid grid, IReadOnlyList<View?> header, IEnumerable<IReadOnlyList<View?>> rows,
        Thickness cellMargin, bool endAlignFirstColumn = false)
    {
        Clear(grid);
        grid.RowSpacing = 0;
        grid.ColumnSpacing = 0;

        for (var i = 0; i < header.Count; i++)
        {
            grid.ColumnDefinitions.Add(
                new ColumnDefinition(i == header.Count - 1 ? GridLength.Star : GridLength.Auto));
        }

        AddRow(grid, header, ThemeResources.Color("FogSurface2Color"), cellMargin, endAlignFirstColumn);

        var zebra = ThemeResources.Color("FogZebraColor");
        var dataRow = 0;
        foreach (var row in rows)
        {
            dataRow++;
            // Same as the web's `tbody tr:nth-child(even)`.
            AddRow(grid, row, dataRow % 2 == 0 ? zebra : null, cellMargin, endAlignFirstColumn);
        }
    }

    public static void Clear(Grid grid)
    {
        grid.Children.Clear();
        grid.RowDefinitions.Clear();
        grid.ColumnDefinitions.Clear();
    }

    public static Label Text(string text, string styleKey = "FogTableCellLabel")
    {
        return new Label {Text = text, Style = ThemeResources.Style(styleKey)};
    }

    public static Label HeaderText(string text)
    {
        return Text(text, "FogTableHeaderLabel");
    }

    public static Image Icon(string url, double size)
    {
        return new AssetImage
        {
            Url = url,
            WidthRequest = size,
            HeightRequest = size,
            Aspect = Aspect.AspectFit,
            VerticalOptions = LayoutOptions.Center,
        };
    }

    public static VerticalStackLayout Stack(IEnumerable<View> views)
    {
        var stack = new VerticalStackLayout {Spacing = 1};
        foreach (var view in views)
        {
            stack.Add(view);
        }

        return stack;
    }

    /// <summary>
    ///     A read-only radio mark. The web shows a disabled radio button for the selected product; a real
    ///     RadioButton would bring its 44px minimum size and platform template along.
    /// </summary>
    public static View SelectionDot(bool isSelected)
    {
        var brush = new SolidColorBrush(ThemeResources.Color("FogTextColor"));
        var dot = new Grid
        {
            WidthRequest = 14,
            HeightRequest = 14,
            VerticalOptions = LayoutOptions.Center,
            Opacity = 0.6,
        };
        dot.Add(new Ellipse {Stroke = brush, StrokeThickness = 1.5, WidthRequest = 14, HeightRequest = 14});
        if (isSelected)
        {
            dot.Add(new Ellipse
            {
                Fill = brush,
                WidthRequest = 7,
                HeightRequest = 7,
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center,
            });
        }

        return dot;
    }

    private static void AddRow(Grid grid, IReadOnlyList<View?> cells, Color? background, Thickness cellMargin,
        bool endAlignFirstColumn)
    {
        var rowIndex = grid.RowDefinitions.Count;
        grid.RowDefinitions.Add(new RowDefinition(GridLength.Auto));

        if (background != null)
        {
            var fill = new ContentView {BackgroundColor = background};
            grid.Add(fill, 0, rowIndex);
            Grid.SetColumnSpan(fill, grid.ColumnDefinitions.Count);
        }

        for (var column = 0; column < cells.Count; column++)
        {
            var cell = cells[column];
            if (cell == null)
            {
                continue;
            }

            if (cell.Margin == default)
            {
                cell.Margin = cellMargin;
            }

            if (column == 0 && endAlignFirstColumn)
            {
                cell.HorizontalOptions = LayoutOptions.End;
            }

            grid.Add(cell, column, rowIndex);
        }
    }
}
