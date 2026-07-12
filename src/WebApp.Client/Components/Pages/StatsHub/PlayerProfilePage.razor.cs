using Ingweland.Fog.Application.Client.Web.Analytics;
using Ingweland.Fog.Application.Client.Web.Analytics.Interfaces;
using Ingweland.Fog.Application.Client.Web.CityStrategyBuilder.Abstractions;
using Ingweland.Fog.Application.Client.Web.Models;
using Ingweland.Fog.Application.Client.Web.Providers.Interfaces;
using Ingweland.Fog.Application.Client.Web.Services.Abstractions;
using Ingweland.Fog.Application.Client.Web.Services.Hoh.Abstractions;
using Ingweland.Fog.Application.Client.Web.StatsHub.ViewModels;
using Ingweland.Fog.Application.Client.Web.ViewModels.Hoh.Battle;
using Ingweland.Fog.Application.Client.Web.ViewModels.Hoh.Units;
using Ingweland.Fog.Application.Core.Helpers;
using Ingweland.Fog.Application.Core.Services.Hoh.Abstractions;
using Ingweland.Fog.Dtos.Hoh;
using Ingweland.Fog.Dtos.Hoh.Stats;
using Ingweland.Fog.Models.Fog.Entities;
using Ingweland.Fog.Models.Hoh.Enums;
using Ingweland.Fog.Shared.Extensions;
using Ingweland.Fog.WebApp.Client.Components.Elements.StatsHub;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using Refit;
using Syncfusion.Blazor.Charts;

namespace Ingweland.Fog.WebApp.Client.Components.Pages.StatsHub;

public partial class PlayerProfilePage : StatsHubPageBase, IAsyncDisposable
{
    private readonly CancellationTokenSource _inGameEventCts = new();
    private IReadOnlyCollection<PlayerAthRankingViewModel>? _athRankings;
    private bool _athRankingsAreLoading;
    private CancellationTokenSource? _athRankingsCts;
    private CancellationTokenSource _battleStatsCts = new();
    private bool _canShowChart;
    private CancellationTokenSource _cityFetchCts = new();
    private PlayerCityPropertiesViewModel? _cityProperties;
    private bool _cityPropertiesAreLoading;
    private CancellationTokenSource? _cityPropertiesCts;
    private DateTime? _citySnapshotDate = DateTime.Today;
    private Dictionary<string, object> _defaultAnalyticsParameters = [];
    private bool _fetchingCity;
    private IReadOnlyCollection<HeroBasicViewModel>? _heroes;
    private bool _heroesAreLoading;
    private CancellationTokenSource? _heroesCts;
    private bool _isDisposed;
    private DateTime _maxPvpRankingsChartDate = DateTime.Today.AddDays(5);
    private PvpTier _maxPvpTier = PvpTier.PvP_Tier_Overlord_1;
    private DateTime _minPvpRankingsChartDate = DateTime.Today.AddDays(-5);
    private PvpTier _minPvpTier = PvpTier.Undefined;
    private PlayerProfileViewModel? _player;
    private IReadOnlyCollection<PvpRankingViewModel>? _pvpRankings;
    private bool _pvpRankingsAreLoading;
    private CancellationTokenSource? _pvpRankingsCts;
    private IReadOnlyDictionary<PvpTier, PvpTierDto> _pvpTiers = new Dictionary<PvpTier, PvpTierDto>();
    private IReadOnlyCollection<StatsTimedIntValue>? _rankings;
    private bool _rankingsAreLoading;
    private CancellationTokenSource? _rankingsCts;
    private CancellationTokenSource? _woaRankingsCts;
    private IReadOnlyCollection<WoaPlayerStatsViewModel>? _woaStats;
    private bool _woaStatsAreLoading;

    [Inject]
    public IPlayerProfilePageAnalyticsService AnalyticsService { get; set; }

    [Inject]
    private IAssetUrlProvider AssetUrlProvider { get; set; }

    [Inject]
    private IBattleUiService BattleUiService { get; set; }

    [Inject]
    private CityPlannerNavigationState CityPlannerNavigationState { get; set; }

    [Inject]
    public CityStrategyNavigationState CityStrategyNavigationState { get; set; }

    [Inject]
    private ICityStrategyUiService CityStrategyUiService { get; set; }

    [Inject]
    private ICommonUiService CommonUiService { get; set; }

    [Inject]
    private IDialogService DialogService { get; set; }

    [Inject]
    private IInGameEventUiService InGameEventUiService { get; set; }

    [Inject]
    private ILogger<PlayerProfilePage> Logger { get; set; }

    [Inject]
    private IPersistenceService PersistenceService { get; set; }

    [Inject]
    private IStatsHubService StatsHubService { get; set; }

    public async ValueTask DisposeAsync()
    {
        await DisposeAsyncCore();
        GC.SuppressFinalize(this);
    }

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();

        if (OperatingSystem.IsBrowser())
        {
            _canShowChart = true;
            IsInitialized = true;
        }
    }

    protected override async Task OnParametersSetAsync()
    {
        if (_isDisposed)
        {
            return;
        }

        await base.OnParametersSetAsync();

        if (_player == null || _player.Player.Id != PlayerId)
        {
            IsInitialized = false;
            _player = await StatsHubUiService.GetPlayerProfileAsync(PlayerId);
            _citySnapshotDate = _player!.CitySnapshotDays.Last();

            _defaultAnalyticsParameters = new Dictionary<string, object>
            {
                {AnalyticsParams.FOG_PLAYER_ID, _player!.Player.Id},
                {AnalyticsParams.LOCATION, AnalyticsParams.Values.Locations.PLAYER_PROFILE},
            };

            IsInitialized = true;
        }
    }

    private string GetIconString(string icon)
    {
        return $"<image width=\"100%\" height=\"100%\" xlink:href=\"{icon}\" preserveAspectRatio=\"xMidYMid meet\"/>";
    }

    protected virtual async ValueTask DisposeAsyncCore()
    {
        if (_isDisposed)
        {
            return;
        }

        _isDisposed = true;

        await _cityFetchCts.CancelAsync();
        _cityFetchCts.Dispose();

        await _battleStatsCts.CancelAsync();
        _battleStatsCts.Dispose();

        await _inGameEventCts.CancelAsync();
        _inGameEventCts.Dispose();

        if (_cityPropertiesCts != null)
        {
            await _cityPropertiesCts.CancelAsync();
        }
    }

    private void OnPlayerClicked(int playerId)
    {
        AnalyticsService.TrackEvent(AnalyticsEvents.NAVIGATE_PLAYER_PROFILE, _defaultAnalyticsParameters,
            new Dictionary<string, object> {{AnalyticsParams.SOURCE, AnalyticsParams.Values.Sources.PVP_BATTLE}});

        NavigationManager.NavigateTo(FogUrlBuilder.PageRoutes.Player(playerId));
    }

    private static DialogOptions GetDefaultDialogOptions()
    {
        return new DialogOptions
        {
            MaxWidth = MaxWidth.Medium,
            FullWidth = true,
            BackgroundClass = "dialog-blur-bg",
            Position = DialogPosition.TopCenter,
            CloseButton = true,
            CloseOnEscapeKey = true,
            NoHeader = true,
        };
    }

    private async Task OpenBattleStats(int? battleStatsId)
    {
        await _battleStatsCts.CancelAsync();
        _battleStatsCts.Dispose();
        if (battleStatsId == null)
        {
            return;
        }

        AnalyticsService.TrackEvent(AnalyticsEvents.VIEW_BATTLE_STATS, _defaultAnalyticsParameters,
            new Dictionary<string, object> {{AnalyticsParams.FOG_BATTLE_ID, battleStatsId}});

        _battleStatsCts = new CancellationTokenSource();
        var stats = await BattleUiService.GetBattleStatsAsync(battleStatsId.Value, _battleStatsCts.Token);

        if (_isDisposed)
        {
            return;
        }

        var options = GetDefaultDialogOptions();

        var parameters = new DialogParameters<BattleStatsDialog> {{d => d.Stats, stats}};
        await DialogService.ShowAsync<BattleStatsDialog>(null, parameters, options);
    }

    private async Task HandleCityOperation(Func<HohCity, Task> cityHandler, Action onFailure)
    {
        if (_isDisposed)
        {
            return;
        }

        await _cityFetchCts.CancelAsync();
        _cityFetchCts.Dispose();

        if (_isDisposed)
        {
            return;
        }

        _fetchingCity = true;
        _cityFetchCts = new CancellationTokenSource();

        try
        {
            var date = _citySnapshotDate == DateTime.Today ? null : _citySnapshotDate?.ToDateOnly();
            var city = await StatsHubService.GetPlayerCityAsync(_player!.Player.Id, date);
            if (_isDisposed)
            {
                return;
            }

            if (city == null)
            {
                onFailure.Invoke();
                return;
            }

            await cityHandler(city);
        }
        catch (Exception e)
        {
            onFailure.Invoke();
            Logger.LogError(e, "Error while fetching city data");
        }

        if (_isDisposed)
        {
            return;
        }

        _fetchingCity = false;
    }

    private async Task VisitCity()
    {
        var parameters = new Dictionary<string, object>
        {
            {AnalyticsParams.CITY_ID, CityId.Capital.ToString()},
        };
        AnalyticsService.TrackEvent(AnalyticsEvents.VISIT_CITY_INIT, _defaultAnalyticsParameters, parameters);
        await HandleCityOperation(city =>
            {
                AnalyticsService.TrackEvent(AnalyticsEvents.VISIT_CITY_SUCCESS, _defaultAnalyticsParameters,
                    parameters);

                CityPlannerNavigationState.Data = new CityPlannerNavigationState.CityPlannerNavigationStateData
                {
                    City = city,
                    IsRemote = true,
                };
                NavigationManager.NavigateTo(FogUrlBuilder.PageRoutes.CITY_VIEWER_PATH);
                return Task.CompletedTask;
            },
            () => AnalyticsService.TrackEvent(AnalyticsEvents.VISIT_CITY_ERROR, _defaultAnalyticsParameters,
                parameters));
    }

    private async Task ShowCityStats()
    {
        var parameters = new Dictionary<string, object>
        {
            {AnalyticsParams.CITY_ID, CityId.Capital.ToString()},
        };

        AnalyticsService.TrackEvent(AnalyticsEvents.VIEW_CITY_STATS_INIT, _defaultAnalyticsParameters, parameters);

        await HandleCityOperation(async city =>
            {
                AnalyticsService.TrackEvent(AnalyticsEvents.VIEW_CITY_STATS_SUCCESS, _defaultAnalyticsParameters,
                    parameters);

                city.Id = Guid.NewGuid().ToString("N");
                await PersistenceService.SaveTempCities([city]);
                NavigationManager.NavigateTo(FogUrlBuilder.PageRoutes.CITIES_STATS_PATH);
            },
            () => AnalyticsService.TrackEvent(AnalyticsEvents.VIEW_CITY_STATS_ERROR, _defaultAnalyticsParameters,
                parameters));
    }

    private async Task ToggleRankingChart(bool expanded)
    {
        AnalyticsService.TrackChartView(AnalyticsEvents.TOGGLE_CHART, _defaultAnalyticsParameters,
            AnalyticsParams.Values.Sources.PLAYER_RANKING_CHART, expanded);

        if (expanded)
        {
            await GetRankings();
        }
    }

    private async Task ToggleProductionCapacityView(bool expanded)
    {
        AnalyticsService.TrackChartView(AnalyticsEvents.TOGGLE_VIEW, _defaultAnalyticsParameters,
            AnalyticsParams.Values.Sources.PLAYER_PRODUCTION_CAPACITY_VIEW, expanded);

        if (expanded)
        {
            await GetCityProperties();
        }
    }

    private async Task GetCityProperties()
    {
        if (_cityProperties != null)
        {
            return;
        }

        if (_cityPropertiesCts != null)
        {
            await _cityPropertiesCts.CancelAsync();
        }

        _cityPropertiesAreLoading = true;
        _fetchingCity = true;
        StateHasChanged();

        _cityPropertiesCts = new CancellationTokenSource();
        _cityProperties =
            await StatsHubUiService.GetPlayerCityPropertiesAsync(PlayerId, _cityPropertiesCts.Token);
        _cityPropertiesAreLoading = false;
        _fetchingCity = false;
    }

    private async Task GetRankings()
    {
        if (_rankings != null)
        {
            return;
        }

        if (_rankingsCts != null)
        {
            await _rankingsCts.CancelAsync();
        }

        _rankingsAreLoading = true;
        StateHasChanged();

        _rankingsCts = new CancellationTokenSource();
        _rankings = await StatsHubUiService.GetPlayerRankingsAsync(PlayerId, _rankingsCts.Token);
        _rankingsAreLoading = false;
    }

    private async Task TogglePvpChart(bool expanded)
    {
        AnalyticsService.TrackChartView(AnalyticsEvents.TOGGLE_CHART, _defaultAnalyticsParameters,
            AnalyticsParams.Values.Sources.PLAYER_PVP_RANKING_CHART, expanded);

        await GetPvpTiers();
        await GetPvpRankings();
    }

    private async Task GetPvpTiers()
    {
        if (_pvpTiers.Count > 0)
        {
            return;
        }

        _pvpTiers = await CommonUiService.GetPvpTiersAsync();
    }

    private async Task OpenBattleSquadProfile(HeroProfileViewModel squad)
    {
        var options = GetDefaultDialogOptions();

        var parameters = new DialogParameters<ProfileSquadDialog> {{d => d.HeroProfile, squad}};
        await DialogService.ShowAsync<ProfileSquadDialog>(null, parameters, options);
    }

    private void NavigateToBattlesScreen()
    {
        NavigationManager.NavigateTo(FogUrlBuilder.PageRoutes.PlayerBattles(PlayerId));
    }

    private void OnPlayerInfoAllianceClicked(int allianceId)
    {
        AnalyticsService.TrackAllianceNavigation(AnalyticsEvents.NAVIGATE_ALLIANCE, _defaultAnalyticsParameters,
            AnalyticsParams.Values.Sources.PLAYER_INFO_COMPONENT, allianceId);
    }

    private void OnAllianceClicked(int allianceId)
    {
        AnalyticsService.TrackAllianceNavigation(AnalyticsEvents.NAVIGATE_ALLIANCE, _defaultAnalyticsParameters,
            AnalyticsParams.Values.Sources.ALLIANCE_LIST, allianceId);
    }

    private async Task OnProfileSquadClicked(HeroProfileBasicViewModel profile)
    {
        AnalyticsService.TrackSquadProfileView(AnalyticsEvents.VIEW_SQUAD_PROFILE, _defaultAnalyticsParameters,
            AnalyticsParams.Values.Sources.TOP_HEROES, profile.HeroUnitId);

        var fullProfile = await BattleUiService.CreateHeroProfile(profile);
        await OpenBattleSquadProfile(fullProfile);
    }

    private async Task OnPvpBattleSquadClicked(BattleSquadBasicViewModel squad)
    {
        AnalyticsService.TrackSquadProfileView(AnalyticsEvents.VIEW_SQUAD_PROFILE, _defaultAnalyticsParameters,
            AnalyticsParams.Values.Sources.PVP_BATTLE, squad.HeroUnitId);

        var fullSquad = await BattleUiService.CreateHeroProfile(squad);
        await OpenBattleSquadProfile(fullSquad);
    }

    private async Task GetPvpRankings()
    {
        if (_pvpRankings != null)
        {
            return;
        }

        if (_pvpRankingsCts != null)
        {
            await _pvpRankingsCts.CancelAsync();
        }

        _pvpRankingsAreLoading = true;
        StateHasChanged();

        _pvpRankingsCts = new CancellationTokenSource();

        try
        {
            _pvpRankings = await StatsHubUiService.GetPlayerPvpRankingsAsync(PlayerId);
            _minPvpRankingsChartDate =
                _pvpRankings.MinBy(x => x.CollectedAt)?.CollectedAt.AddDays(-1) ?? DateTime.Today;
            _maxPvpRankingsChartDate = _pvpRankings.MaxBy(x => x.CollectedAt)?.CollectedAt.AddDays(1) ?? DateTime.Today;
            _minPvpTier = _pvpRankings.MinBy(x => x.Tier)?.Tier - 1 ?? PvpTier.Undefined;
            _maxPvpTier = _pvpRankings.MaxBy(x => x.Tier)?.Tier + 1 ?? PvpTier.PvP_Tier_Overlord_1;
            _pvpRankingsAreLoading = false;
        }
        catch (OperationCanceledException _)
        {
        }
        catch (ApiException apiEx) when (apiEx.InnerException is TaskCanceledException)
        {
            _pvpRankingsAreLoading = false;
        }
        catch (Exception e)
        {
            _pvpRankingsAreLoading = false;
            Console.Error.WriteLine(e);
        }
    }

    public void AxisLabelEvent(AxisLabelRenderEventArgs args)
    {
        if (args.Axis.Name == "PrimaryYAxis")
        {
            if (Enum.TryParse<PvpTier>(args.Text, out var value) && _pvpTiers.TryGetValue(value, out var pvpTier))
            {
                if (pvpTier.Tier is > PvpTier.Undefined and <= PvpTier.PvP_Tier_Overlord_1)
                {
                    args.Text = pvpTier.Name;
                }
                else
                {
                    args.Text = "";
                }
            }
            else
            {
                args.Text = "";
            }
        }
    }

    private async Task ToggleAthRankingsContainer(bool expanded)
    {
        AnalyticsService.TrackChartView(AnalyticsEvents.TOGGLE_VIEW, _defaultAnalyticsParameters,
            AnalyticsParams.Values.Sources.PLAYER_ATH_RANKINGS, expanded);

        if (expanded)
        {
            await GetAthRankings();
        }
    }

    private async Task ToggleHeroesContainer(bool expanded)
    {
        AnalyticsService.TrackChartView(AnalyticsEvents.TOGGLE_VIEW, _defaultAnalyticsParameters,
            AnalyticsParams.Values.Sources.PLAYER_HEROES, expanded);

        if (expanded)
        {
            await GetHeroesAsync();
        }
    }

    private async Task GetAthRankings()
    {
        if (_athRankings != null)
        {
            return;
        }

        if (_athRankingsCts != null)
        {
            await _athRankingsCts.CancelAsync();
        }

        _athRankingsAreLoading = true;
        StateHasChanged();

        _athRankingsCts = new CancellationTokenSource();

        _athRankings = await StatsHubUiService.GetPlayerAthRankingsAsync(PlayerId);
        _athRankingsAreLoading = false;
    }

    private async Task GetHeroesAsync()
    {
        if (_heroes != null)
        {
            return;
        }

        if (_heroesCts != null)
        {
            await _heroesCts.CancelAsync();
        }

        _heroesAreLoading = true;
        StateHasChanged();

        _heroesCts = new CancellationTokenSource();

        _heroes = await StatsHubUiService.GetPlayerHeroesAsync(PlayerId, _heroesCts.Token);
        _heroesAreLoading = false;
    }

    private async Task ToggleWoaStatsContainer(bool expanded)
    {
        AnalyticsService.TrackChartView(AnalyticsEvents.TOGGLE_VIEW, _defaultAnalyticsParameters,
            AnalyticsParams.Values.Sources.WOA_PLAYER_STATS, expanded);

        if (expanded)
        {
            await GetWoaStats();
        }
    }

    private async Task GetWoaStats()
    {
        if (_woaStats != null)
        {
            return;
        }

        if (_woaRankingsCts != null)
        {
            await _woaRankingsCts.CancelAsync();
        }

        _woaStatsAreLoading = true;
        StateHasChanged();

        _woaRankingsCts = new CancellationTokenSource();

        _woaStats = await StatsHubUiService.GetWoaPlayerStatsAsync(PlayerId, _woaRankingsCts.Token);
        _woaStatsAreLoading = false;
    }
}
