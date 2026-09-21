using Ingweland.Fog.Application.Client.Core.Localization;
using Ingweland.Fog.Application.Client.Web.Services.Hoh.Abstractions;
using Ingweland.Fog.Application.Client.Web.ViewModels.Hoh.City;
using Ingweland.Fog.Models.Hoh.Enums;

namespace Ingweland.Fog.App.Views.CityViewer;

/// <summary>
///     What it costs to take the selected building from one level to another: the MAUI counterpart of
///     CityPlannerBuildingCostCalculator. It sits under the building's properties, where the website's mobile
///     viewer puts it inside CityMapEntityPropertiesDialog.
/// </summary>
internal sealed class BuildingCostCalculatorView : ContentView
{
    private readonly ICityUiService _cityUiService;
    private readonly Grid _costTable;
    private readonly Picker _fromPicker;
    private readonly Label _subjectLabel;
    private readonly Picker _toPicker;
    private readonly IToolsUiService _toolsUiService;

    private CancellationTokenSource _cts = new();
    private bool _hasContent;
    private bool _isHostVisible = true;
    private IReadOnlyList<LevelOption> _fromLevels = [];
    private BuildingGroupViewModel? _group;

    // Set while a picker is being refilled: assigning ItemsSource or SelectedIndex raises
    // SelectedIndexChanged, and the website's selects only ever report what the user picked.
    private bool _isRefilling;

    private IReadOnlyList<LevelOption> _toLevels = [];

    public BuildingCostCalculatorView(ICityUiService cityUiService, IToolsUiService toolsUiService)
    {
        _cityUiService = cityUiService;
        _toolsUiService = toolsUiService;

        _subjectLabel = new Label {Style = ThemeResources.Style("FogCalculatorSubjectLabel")};
        _fromPicker = CalculatorCard.LevelPicker(FogResource.Tools_CurrentLevel);
        _toPicker = CalculatorCard.LevelPicker(FogResource.Tools_TargetLevel);
        _costTable = new Grid();

        _fromPicker.SelectedIndexChanged += OnFromLevelChanged;
        _toPicker.SelectedIndexChanged += OnToLevelChanged;

        Content = CalculatorCard.Build(FogResource.Tools_BuildingCost_Name,
            [_subjectLabel, _fromPicker, _toPicker, _costTable]);
        // Nothing to cost yet, so nothing to take up room in the panel.
        ApplyVisibility();
    }

    /// <summary>
    ///     Loads the group the building belongs to and starts from its current level, as the website hands the
    ///     component its Building.Group and Building.Level.
    /// </summary>
    public async Task ShowAsync(CityId cityId, BuildingGroup buildingGroup, string buildingName, int fromLevel)
    {
        await _cts.CancelAsync();
        _cts = new CancellationTokenSource();
        var token = _cts.Token;

        Reset();
        _subjectLabel.Text = buildingName;

        if (buildingGroup == BuildingGroup.Undefined)
        {
            return;
        }

        BuildingGroupViewModel? group;
        try
        {
            group = await _cityUiService.GetBuildingGroupAsync(cityId, buildingGroup, token);
        }
        catch (Exception)
        {
            // The website swallows this as well: without the group there is nothing to cost.
            group = null;
        }

        // A later selection has taken over; leave everything it has already set up alone.
        if (token.IsCancellationRequested)
        {
            return;
        }

        _group = group;

        var levels = BuildableLevels();
        if (levels.Count == 0)
        {
            Reset();
            return;
        }

        // The level below the first buildable one, so a building that is not up yet can be costed from
        // nothing. BuildingLevelSpecs.ZeroLevel when that lands on zero, as the website does.
        var groundLevel = levels[0].Level - 1;
        _fromLevels =
        [
            new LevelOption(groundLevel == 0
                ? BuildingLevelSpecs.ZeroLevel
                : new BuildingLevelSpecs {Level = groundLevel, CanBeConstructed = false, CanBeUpgradedTo = false}),
            ..levels.Select(x => new LevelOption(x)),
        ];

        var index = IndexOfLevel(_fromLevels, fromLevel);
        Fill(_fromPicker, _fromLevels, index < 0 ? 0 : index);
        RefreshTargetLevels(false);
        CalculateCost();
        _hasContent = true;
        ApplyVisibility();
    }

    /// <summary>
    ///     No building selected, so nothing to cost.
    /// </summary>
    public void Clear()
    {
        _ = _cts.CancelAsync();
        Reset();
    }

    private void Reset()
    {
        _group = null;
        _fromLevels = [];
        _toLevels = [];
        Fill(_fromPicker, _fromLevels, -1);
        Fill(_toPicker, _toLevels, -1);
        StatsTable.Clear(_costTable);
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

    private List<BuildingLevelSpecs> BuildableLevels()
    {
        return _group?.Buildings
            .Where(b => b.ConstructionComponent != null || b.UpgradeComponent != null)
            .Select(b => b.LevelSpecs)
            .OrderBy(x => x.Level)
            .ToList() ?? [];
    }

    private void OnFromLevelChanged(object? sender, EventArgs e)
    {
        if (_isRefilling)
        {
            return;
        }

        RefreshTargetLevels(true);
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
    private void RefreshTargetLevels(bool keepSelection)
    {
        var fromLevel = Selected(_fromPicker, _fromLevels)?.Specs.Level ?? 0;
        var previousTarget = keepSelection ? Selected(_toPicker, _toLevels)?.Specs.Level : null;

        _toLevels = BuildableLevels().Where(x => x.Level > fromLevel).Select(x => new LevelOption(x)).ToList();

        var index = previousTarget == null ? -1 : IndexOfLevel(_toLevels, previousTarget.Value);
        Fill(_toPicker, _toLevels, index < 0 ? 0 : index);
        _toPicker.IsVisible = _toLevels.Count > 0;
    }

    private void CalculateCost()
    {
        var from = Selected(_fromPicker, _fromLevels);
        var to = Selected(_toPicker, _toLevels);
        if (_group == null || from == null || to == null)
        {
            StatsTable.Clear(_costTable);
            return;
        }

        var costs = _toolsUiService.CalculateBuildingMultiLevelCost(_group, from.Specs.Level, to.Specs.Level);
        StatsTable.Fill(_costTable,
            new View?[]
            {
                StatsTable.HeaderText(FogResource.Hoh_Resource),
                StatsTable.HeaderText(FogResource.Hoh_Building_Construction),
                StatsTable.HeaderText(FogResource.Hoh_Building_Upgrade),
            },
            costs.Costs.Select(item => new View?[]
            {
                StatsTable.Icon(item.IconUrl, 18), StatsTable.Text(item.ConstructionCost),
                StatsTable.Text(item.UpgradeCost),
            }),
            new Thickness(6, 2));
    }

    private void Fill(Picker picker, IReadOnlyList<LevelOption> options, int selectedIndex)
    {
        _isRefilling = true;
        picker.ItemsSource = options.ToList();
        picker.SelectedIndex = options.Count == 0 ? -1 : selectedIndex;
        _isRefilling = false;
    }

    private static int IndexOfLevel(IReadOnlyList<LevelOption> options, int level)
    {
        for (var i = 0; i < options.Count; i++)
        {
            if (options[i].Specs.Level == level)
            {
                return i;
            }
        }

        return -1;
    }

    private static LevelOption? Selected(Picker picker, IReadOnlyList<LevelOption> options)
    {
        return picker.SelectedIndex >= 0 && picker.SelectedIndex < options.Count ? options[picker.SelectedIndex] : null;
    }

    /// <summary>
    ///     One line of a level picker. A Picker draws its items as plain text, so the level chip, age chip and
    ///     construct/upgrade icons of the website's BuildingLevelSelectItemComponent come down to the level and
    ///     its age; which of the two costs applies is still the cost table's two columns.
    /// </summary>
    private sealed record LevelOption(BuildingLevelSpecs Specs)
    {
        public override string ToString()
        {
            return string.IsNullOrEmpty(Specs.AgeName)
                ? Specs.Level.ToString()
                : $"{Specs.Level} - {Specs.AgeName}";
        }
    }
}
