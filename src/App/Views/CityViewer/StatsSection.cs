namespace Ingweland.Fog.App.Views.CityViewer;

/// <summary>
///     A titled panel section: the MAUI counterpart of the web's StatsSectionHeader plus MudCollapse. A thin
///     rule with the title tag hanging from it, then the body; tapping the tag collapses the body when
///     <see cref="IsCollapsible" /> is set.
/// </summary>
/// <remarks>
///     Built in code on purpose: with a XAML file, the content-property redirect to <see cref="Body" /> would
///     also capture the control's own root element.
/// </remarks>
[ContentProperty(nameof(Body))]
public class StatsSection : ContentView
{
    public static readonly BindableProperty TitleProperty = BindableProperty.Create(nameof(Title), typeof(string),
        typeof(StatsSection), string.Empty,
        propertyChanged: (bindable, _, newValue) => ((StatsSection) bindable)._titleLabel.Text = (string?) newValue);

    public static readonly BindableProperty IsCollapsibleProperty = BindableProperty.Create(nameof(IsCollapsible),
        typeof(bool), typeof(StatsSection), false);

    public static readonly BindableProperty BodyProperty = BindableProperty.Create(nameof(Body), typeof(View),
        typeof(StatsSection),
        propertyChanged: (bindable, _, newValue) => ((StatsSection) bindable)._bodyHost.Content = (View?) newValue);

    private readonly ContentView _bodyHost = new();
    private readonly Label _titleLabel;

    public StatsSection()
    {
        _titleLabel = new Label {Style = ThemeResources.Style("FogSectionTagLabel")};

        var tag = new Border {Style = ThemeResources.Style("FogSectionTag"), Content = _titleLabel};
        var tap = new TapGestureRecognizer();
        tap.Tapped += OnTagTapped;
        tag.GestureRecognizers.Add(tap);

        var rule = new ContentView {BackgroundColor = ThemeResources.Color("FogContainerColor")};
        rule.SetDynamicResource(HeightRequestProperty, "FogHairlineThickness");

        Padding = new Thickness(0, 0, 0, 16);
        Content = new VerticalStackLayout
        {
            Spacing = 6,
            Children = {new VerticalStackLayout {rule, tag}, _bodyHost},
        };
    }

    public string Title
    {
        get => (string) GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public bool IsCollapsible
    {
        get => (bool) GetValue(IsCollapsibleProperty);
        set => SetValue(IsCollapsibleProperty, value);
    }

    public View? Body
    {
        get => (View?) GetValue(BodyProperty);
        set => SetValue(BodyProperty, value);
    }

    private void OnTagTapped(object? sender, TappedEventArgs e)
    {
        if (IsCollapsible)
        {
            _bodyHost.IsVisible = !_bodyHost.IsVisible;
        }
    }
}
