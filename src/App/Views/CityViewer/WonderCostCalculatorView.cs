using Ingweland.Fog.Application.Client.Core.Localization;
using Ingweland.Fog.Application.Client.Web.Services.Hoh.Abstractions;
using Ingweland.Fog.Dtos.Hoh.City;

namespace Ingweland.Fog.App.Views.CityViewer;

/// <summary>
///     What it costs to take the city's wonder from one level to another: the MAUI counterpart of
///     CityPlannerWonderCostCalculator. It sits under the city properties, where both of the website's viewers
///     put it.
/// </summary>
internal sealed class WonderCostCalculatorView : ContentView
{
    // The website's own ceiling; the game's wonders do not reach it.
    private const int MAX_LEVEL = 50;

    private readonly WrapList _costList;
    private readonly Picker _fromPicker;
    private readonly Label _subjectLabel;
    private readonly Picker _toPicker;
    private readonly IToolsUiService _toolsUiService;

    private bool _hasContent;
    private bool _isHostVisible = true;

    // Set while a picker is being refilled, as in BuildingCostCalculatorView.
    private bool _isRefilling;

    private WonderDto? _wonder;

    public WonderCostCalculatorView(IToolsUiService toolsUiService)
    {
        _toolsUiService = toolsUiService;

        _subjectLabel = new Label {Style = ThemeResources.Style("FogCalculatorSubjectLabel")};
        _fromPicker = CalculatorCard.LevelPicker(FogResource.Tools_CurrentLevel);
        _toPicker = CalculatorCard.LevelPicker(FogResource.Tools_TargetLevel);
        // .cost-items-container, tightened to the spacing the panel's own chip rows use: the website has
        // room to spread these out and a phone panel has not.
        _costList = new WrapList {ColumnSpacing = 4, RowSpacing = 2};

        _fromPicker.ItemsSource = Enumerable.Range(0, MAX_LEVEL + 1).ToList();
        _fromPicker.SelectedIndexChanged += OnFromLevelChanged;
        _toPicker.SelectedIndexChanged += OnToLevelChanged;

        Content = CalculatorCard.Build(FogResource.Tools_WonderCost_Name,
            [_subjectLabel, _fromPicker, _toPicker, _costList]);
        // Nothing to cost yet, so nothing to take up room in the panel.
        ApplyVisibility();
    }

    /// <summary>
    ///     Starts from the wonder's current level, as the website hands the component its CityWonderLevel.
    /// </summary>
    public void Show(WonderDto? wonder, int fromLevel)
    {
        _wonder = wonder;
        if (wonder == null)
        {
            Clear();
            return;
        }

        _subjectLabel.Text = wonder.WonderName;
        _isRefilling = true;
        _fromPicker.SelectedIndex = Math.Clamp(fromLevel, 0, MAX_LEVEL);
        _isRefilling = false;

        RefreshTargetLevels();
        CalculateCost();
        _hasContent = true;
        ApplyVisibility();
    }

    /// <summary>
    ///     No wonder in this city, so nothing to cost.
    /// </summary>
    public void Clear()
    {
        _wonder = null;
        _costList.Clear();
        _hasContent = false;
        ApplyVisibility();
    }

    /// <summary>
    ///     Whether the card this calculator hangs under is itself showing. The panel shows one card at a time
    ///     on a narrow window, and a calculator belongs to its card rather than to the panel.
    /// </summary>
    public bool IsHostVisible
    {
        get => _isHostVisible;
        set
        {
            _isHostVisible = value;
            ApplyVisibility();
        }
    }

    private void ApplyVisibility()
    {
        IsVisible = _isHostVisible && _hasContent;
    }

    private void OnFromLevelChanged(object? sender, EventArgs e)
    {
        if (_isRefilling)
        {
            return;
        }

        RefreshTargetLevels();
        CalculateCost();
    }

    private void OnToLevelChanged(object? sender, EventArgs e)
    {
        if (!_isRefilling)
        {
            CalculateCost();
        }
    }

    /// <summary>
    ///     The levels above the current one, keeping the chosen target where it is still one of them.
    /// </summary>
    private void RefreshTargetLevels()
    {
        var fromLevel = FromLevel;
        var previousTarget = _toPicker.SelectedItem as int?;
        var levels = Enumerable.Range(fromLevel + 1, MAX_LEVEL - fromLevel).ToList();

        _isRefilling = true;
        _toPicker.ItemsSource = levels;
        _toPicker.SelectedIndex = levels.Count == 0
            ? -1
            : Math.Max(0, levels.IndexOf(previousTarget ?? levels[0]));
        _isRefilling = false;

        // The website hides both the selector and the costs once the ceiling is reached.
        _toPicker.IsVisible = levels.Count > 0;
    }

    private void CalculateCost()
    {
        _costList.Clear();

        if (_wonder == null || _toPicker.SelectedItem is not int toLevel)
        {
            return;
        }

        foreach (var cost in _toolsUiService.CalculateWonderLevelsCost(_wonder, FromLevel, toLevel))
        {
            _costList.Add(new IconLabelChip {BindingContext = cost});
        }
    }

    private int FromLevel => _fromPicker.SelectedIndex < 0 ? 0 : _fromPicker.SelectedIndex;
}
