using Microsoft.Maui.Layouts;

namespace Ingweland.Fog.App.Views.CityViewer;

/// <summary>
///     A wrapping row of items, like CSS <c>flex-wrap: wrap</c> with column and row gaps, hidden while empty.
///     FlexLayout has no gap property, so every item gets a trailing margin that this layout's own negative
///     margin cancels at the edges. Hiding it while empty stops that negative margin from pulling the next
///     element up. Don't set <see cref="View.Margin" /> on it.
/// </summary>
public class WrapList : FlexLayout
{
    public static readonly BindableProperty ColumnSpacingProperty = BindableProperty.Create(nameof(ColumnSpacing),
        typeof(double), typeof(WrapList), 12.0,
        propertyChanged: (bindable, _, _) => ((WrapList) bindable).ApplySpacing());

    public static readonly BindableProperty RowSpacingProperty = BindableProperty.Create(nameof(RowSpacing),
        typeof(double), typeof(WrapList), 12.0,
        propertyChanged: (bindable, _, _) => ((WrapList) bindable).ApplySpacing());

    public WrapList()
    {
        Wrap = FlexWrap.Wrap;
        // Not Center: MAUI's flex engine offsets a centered item by its whole top-minus-bottom margin rather than
        // half of it, which would lift these items by half the row spacing. A list's items share one height anyway.
        AlignItems = FlexAlignItems.Start;
        AlignContent = FlexAlignContent.Start;
        IsVisible = false;
        ApplySpacing();
    }

    public double ColumnSpacing
    {
        get => (double) GetValue(ColumnSpacingProperty);
        set => SetValue(ColumnSpacingProperty, value);
    }

    public double RowSpacing
    {
        get => (double) GetValue(RowSpacingProperty);
        set => SetValue(RowSpacingProperty, value);
    }

    private Thickness ItemMargin => new(0, 0, ColumnSpacing, RowSpacing);

    protected override void OnChildAdded(Element child)
    {
        base.OnChildAdded(child);

        if (child is View view)
        {
            view.Margin = ItemMargin;
            SetShrink((BindableObject) view, 0);
        }

        IsVisible = true;
    }

    protected override void OnChildRemoved(Element child, int oldLogicalIndex)
    {
        base.OnChildRemoved(child, oldLogicalIndex);
        // Excludes the removed child explicitly rather than relying on when Children is updated.
        IsVisible = Children.Any(x => !ReferenceEquals(x, child));
    }

    private void ApplySpacing()
    {
        Margin = new Thickness(0, 0, -ColumnSpacing, -RowSpacing);
        foreach (var view in Children.OfType<View>())
        {
            view.Margin = ItemMargin;
        }
    }
}
