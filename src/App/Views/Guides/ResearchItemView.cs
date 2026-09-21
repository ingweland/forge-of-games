using Ingweland.Fog.App.ViewModels.CityViewer;
using Ingweland.Fog.App.Views.CityViewer;
using Ingweland.Fog.Application.Client.Core.Localization;
using Ingweland.Fog.Application.Client.Web.Services.Hoh.Abstractions;
using Ingweland.Fog.Application.Client.Web.ViewModels.Hoh.Research;
using Ingweland.Fog.Application.Core.Extensions;
using Ingweland.Fog.Models.Fog.Entities;
using Ingweland.Fog.Models.Hoh.Enums;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Layouts;

namespace Ingweland.Fog.App.Views.Guides;

/// <summary>
///     A research timeline item: what the guide researches at this point, and what it costs. The total sits at
///     the top, then one collapsible section per age holding that age's technologies, each tinted by whether an
///     earlier item already opened it or this one targets it.
/// </summary>
/// <remarks>
///     The website has no separate viewer component for this item - its builder component takes an IsReadOnly
///     flag and drops the click handler, and that is the whole difference. So it is here: nothing is tappable
///     but the age headers, and a technology's state is applied once and read once rather than watched, which
///     is what TechnologyComponent's state subscription is for over there.
/// </remarks>
internal sealed class ResearchItemView : ContentView
{
    // .items-container's and .item-row's gaps, and the chip itself. The website reflows all three at 676px and
    // again at 768px; this follows CityGuidePage's own wide/narrow split instead, so there is one width to
    // reason about rather than three.
    private const double WIDE_SPACING = 16;
    private const double NARROW_SPACING = 6;
    private const double NARROW_CHIP_WIDTH = 85;
    private const double WIDE_CHIP_HEIGHT = 46;

    private readonly List<AgeSection> _sections = [];

    private bool _isWide;

    public ResearchItemView(CityStrategyResearchTimelineItem item, CityId cityId,
        IResearchCalculatorService calculator, bool isWide)
    {
        _isWide = isWide;

        // .component-root: white, and without the border the markdown items' card has.
        BackgroundColor = ThemeResources.Color("FogSurfaceColor");
        Content = new ActivityIndicator
        {
            IsRunning = true,
            Color = ThemeResources.Color("FogPrimaryColor"),
            HorizontalOptions = LayoutOptions.Center,
            VerticalOptions = LayoutOptions.Center,
        };

        _ = LoadAsync(item, cityId, calculator);
    }

    /// <summary>
    ///     Redraws in the other layout when the window crosses CityGuidePage's width. Only a section that has
    ///     been opened has anything built to redraw.
    /// </summary>
    public void SetWide(bool isWide)
    {
        if (isWide == _isWide)
        {
            return;
        }

        _isWide = isWide;
        foreach (var section in _sections)
        {
            section.Refresh(_isWide);
        }
    }

    /// <summary>
    ///     ResearchTimelineItemComponent.OnParametersSetAsync, and fire-and-forget as AssetImage loads its own
    ///     image. Keeping the wait here leaves ShowSelectedItem synchronous, so a second arrow tap replaces
    ///     this view rather than racing it: one dropped mid-load just finishes into itself, off the page.
    /// </summary>
    private async Task LoadAsync(CityStrategyResearchTimelineItem item, CityId cityId,
        IResearchCalculatorService calculator)
    {
        try
        {
            // Mapped here as the web component maps it before calling: the service caches the technologies
            // under the id it is handed but reads them back under the default technology city when it totals
            // the cost, so the two have to be the same value.
            var ages = await calculator.InitializeAsync(cityId.ToDefaultTechnologyCity());

            // What the guide's earlier research items already opened, then what this one adds. Both pull their
            // ancestors in with them, and the target pass runs second so that it can leave the open ones out.
            calculator.SetOpenTechnologiesWithAncestors(item.OpenedTechnologies);
            calculator.SetTargetTechnologiesWithAncestors(item.Technologies);

            Content = Build(ages, await calculator.CalculateCost(), item);
        }
        catch (Exception e)
        {
            IPlatformApplication.Current?.Services.GetService<ILogger<ResearchItemView>>()
                ?.LogError(e, "Could not show the research timeline item {ItemId}", item.Id);

            Content = new Label
            {
                Text = e.Message,
                Style = ThemeResources.Style("FogTextLabel"),
                Margin = 24,
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center,
                HorizontalTextAlignment = TextAlignment.Center,
            };
        }
    }

    private View Build(IReadOnlyCollection<AgeTechnologiesViewModel> ages, ResearchCostViewModel cost,
        CityStrategyResearchTimelineItem item)
    {
        OpenStartingAge(ages, item);

        // .content-container.
        var content = new VerticalStackLayout {Spacing = 6, Padding = new Thickness(0, 12, 0, 0)};
        if (cost.Cost.Count > 0)
        {
            content.Add(new Label
            {
                Text = FogResource.Hoh_Cost,
                Style = ThemeResources.Style("FogResearchCostTitle"),
            });
            content.Add(BuildCost(cost));
        }

        content.Add(BuildAges(ages));

        return new ScrollView {Content = content};
    }

    /// <summary>
    ///     .cost-items-container. The website's ResourceWithValue at Size.Small is an icon over its value in a
    ///     bordered box, which is what IconLabelTile already draws, over the same view model.
    /// </summary>
    private static View BuildCost(ResearchCostViewModel cost)
    {
        var items = new WrapList {ColumnSpacing = 10, RowSpacing = 10, JustifyContent = FlexJustify.Center};
        foreach (var resource in cost.Cost)
        {
            items.Add(new IconLabelTile {BindingContext = resource});
        }

        return new Border
        {
            Style = ThemeResources.Style("FogCard"),
            Padding = 12,
            Margin = new Thickness(12, 0),
            Content = items,
        };
    }

    // .fog-container.vertical-layout. The border clips the first age header's top corners to its own shape.
    private View BuildAges(IEnumerable<AgeTechnologiesViewModel> ages)
    {
        var stack = new VerticalStackLayout();
        foreach (var age in ages)
        {
            var section = new AgeSection(age, _isWide);
            _sections.Add(section);
            stack.Add(section.Header);
            stack.Add(section.Body);
        }

        return new Border {Style = ThemeResources.Style("FogCard"), Content = stack};
    }

    /// <summary>
    ///     Which section starts open, from ResearchTimelineItemComponent: the age this item's work is in,
    ///     which is the first still holding a technology that is neither opened nor targeted. An item that has
    ///     selected nothing at all has no work anywhere yet and opens at the start of the tree; one that
    ///     completes the tree has none left and opens at its end.
    /// </summary>
    private static void OpenStartingAge(IReadOnlyCollection<AgeTechnologiesViewModel> ages,
        CityStrategyResearchTimelineItem item)
    {
        if (ages.Count == 0)
        {
            return;
        }

        var age = item.Technologies.Count == 0 && item.OpenedTechnologies.Count == 0
            ? ages.First()
            : ages.FirstOrDefault(HasUnresearched) ?? ages.Last();

        age.IsListOpen = true;
    }

    private static bool HasUnresearched(AgeTechnologiesViewModel age)
    {
        return age.Technologies.Any(x => x.State is not (ResearchCalculatorTechnologyState.Open
            or ResearchCalculatorTechnologyState.SelectedOpen or ResearchCalculatorTechnologyState.Target
            or ResearchCalculatorTechnologyState.SelectedTarget));
    }

    /// <summary>
    ///     One row of the tree: .item-row, spread across the width when narrow and packed to the middle with a
    ///     gap between when wide.
    /// </summary>
    /// <remarks>
    ///     A FlexLayout because those are the two CSS rules as they stand, and because it brings flex-shrink
    ///     with them. A row is one column of the technology tree, and a long one over a narrow phone is wider
    ///     than the screen: the website's chips narrow to fit, where a grid of equal columns would let chips
    ///     that ask for a fixed width overlap their neighbours.
    /// </remarks>
    private static View BuildRow(IEnumerable<ResearchCalculatorTechnologyViewModel> technologies, bool isWide)
    {
        var row = new FlexLayout {JustifyContent = isWide ? FlexJustify.Center : FlexJustify.SpaceEvenly};
        foreach (var tech in technologies)
        {
            var chip = BuildChip(tech, isWide);
            // FlexLayout has no gap, and half of one on either side of every chip comes to the same thing:
            // the outermost halves fall outside a row that is centred anyway.
            chip.Margin = isWide ? new Thickness(WIDE_SPACING / 2, 0) : default;
            row.Add(chip);
        }

        return row;
    }

    /// <summary>
    ///     TechnologyComponent: the technology's icon and its name, on the background its state gives it.
    ///     Narrow is a fixed-width card with the name under the icon, wide the 676px form, a pill of its own
    ///     width.
    /// </summary>
    private static View BuildChip(ResearchCalculatorTechnologyViewModel tech, bool isWide)
    {
        // An AssetImage, so the icon is downloaded once and read from disk after that.
        var icon = StatsTable.Icon(tech.IconUrl, isWide ? 32 : 28);
        var label = new Label
        {
            Text = tech.Name,
            Style = ThemeResources.Style(isWide ? "FogTechChipWideLabel" : "FogTechChipLabel"),
        };

        View content;
        if (isWide)
        {
            content = new HorizontalStackLayout {Spacing = 4, Children = {icon, label}};
        }
        else
        {
            // A grid rather than a stack for .label's flex: 1 - the chips in a row all take the height of the
            // tallest, and the name centres itself in whatever is left under the icon.
            icon.HorizontalOptions = LayoutOptions.Center;
            var stack = new Grid {RowSpacing = 4};
            stack.RowDefinitions.Add(new RowDefinition(GridLength.Auto));
            stack.RowDefinitions.Add(new RowDefinition(GridLength.Star));
            stack.Add(icon);
            stack.Add(label, 0, 1);
            content = stack;
        }

        return new Border
        {
            Style = ThemeResources.Style("FogTechChip"),
            BackgroundColor = BackgroundFor(tech.State),
            WidthRequest = isWide ? -1 : NARROW_CHIP_WIDTH,
            HeightRequest = isWide ? WIDE_CHIP_HEIGHT : -1,
            Content = content,
        };
    }

    /// <summary>
    ///     TechnologyComponent.GetBgColor. A guide only ever shows two of these: Open, for what an earlier
    ///     research item already researched, and Target, for what this one adds. The selected pair belongs to
    ///     the website's research calculator page, whose service this shares.
    /// </summary>
    private static Color BackgroundFor(ResearchCalculatorTechnologyState state)
    {
        return ThemeResources.Color(state switch
        {
            ResearchCalculatorTechnologyState.Open => "FogOpenTechColor",
            ResearchCalculatorTechnologyState.SelectedOpen => "FogSelectedOpenTechColor",
            ResearchCalculatorTechnologyState.Target => "FogTargetTechColor",
            ResearchCalculatorTechnologyState.SelectedTarget => "FogSelectedTargetTechColor",
            _ => "FogSurfaceColor",
        });
    }

    /// <summary>
    ///     One age: the colored bar naming it, and that age's technologies under it. Tapping the bar toggles
    ///     the body, where the website animates a MudCollapse.
    /// </summary>
    /// <remarks>
    ///     The body is built the first time it is shown. The website builds every age up front and lets the
    ///     browser pace the hundred-odd images that come with them; AssetImage starts every download it is
    ///     given at once, so here only an open age pays for its icons.
    /// </remarks>
    private sealed class AgeSection
    {
        private readonly AgeTechnologiesViewModel _age;

        private bool _isWide;

        public AgeSection(AgeTechnologiesViewModel age, bool isWide)
        {
            _age = age;
            _isWide = isWide;

            Header = new Label
            {
                Text = age.AgeName,
                Style = ThemeResources.Style("FogAgeHeaderLabel"),
                // The factory hands the age's color down as the CSS one the website writes into its style tag.
                BackgroundColor = CssColor.Parse(age.AgeColor) ?? ThemeResources.Color("FogContainerColor"),
            };

            var tap = new TapGestureRecognizer();
            tap.Tapped += OnHeaderTapped;
            Header.GestureRecognizers.Add(tap);

            Body = new ContentView();
            SetOpen(age.IsListOpen);
        }

        public ContentView Body { get; }

        public Label Header { get; }

        public void Refresh(bool isWide)
        {
            _isWide = isWide;
            if (Body.Content != null)
            {
                Body.Content = BuildBody();
            }
        }

        private void OnHeaderTapped(object? sender, TappedEventArgs e)
        {
            SetOpen(!Body.IsVisible);
        }

        private void SetOpen(bool isOpen)
        {
            if (isOpen)
            {
                Body.Content ??= BuildBody();
            }

            Body.IsVisible = isOpen;
        }

        // .items-container: one row per horizontal index, each ordered down the tree.
        private View BuildBody()
        {
            var rows = new VerticalStackLayout
            {
                Spacing = _isWide ? WIDE_SPACING : NARROW_SPACING,
                Padding = 6,
                Margin = new Thickness(0, 0, 0, 12),
            };

            foreach (var group in _age.Technologies.GroupBy(x => x.HorizontalIndex).OrderBy(g => g.Key))
            {
                rows.Add(BuildRow(group.OrderBy(x => x.VerticalIndex), _isWide));
            }

            return rows;
        }
    }
}
