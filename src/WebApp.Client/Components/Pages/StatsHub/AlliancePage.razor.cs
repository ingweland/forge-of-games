using Ingweland.Fog.Application.Client.Web.Analytics;
using Ingweland.Fog.Application.Client.Web.Analytics.Interfaces;
using Ingweland.Fog.Application.Client.Web.StatsHub.ViewModels;
using Ingweland.Fog.Dtos.Hoh.Stats;
using Microsoft.AspNetCore.Components;
using Refit;

namespace Ingweland.Fog.WebApp.Client.Components.Pages.StatsHub;

public partial class AlliancePage : StatsHubPageBase
{
    private AllianceProfileViewModel? _alliance;
    private IReadOnlyCollection<AllianceAthRankingViewModel>? _athRankings;
    private bool _athRankingsAreLoading;
    private CancellationTokenSource? _athRankingsCts;
    private bool _canShowChart;
    private Dictionary<string, object> _defaultAnalyticsParameters = [];
    private IReadOnlyCollection<StatsTimedIntValue>? _rankings;
    private bool _rankingsAreLoading;
    private CancellationTokenSource? _rankingsCts;
    private bool _showLastSeenOn;
    private IReadOnlyCollection<AllianceWoaRankingViewModel>? _woaRankings;
    private bool _woaRankingsAreLoading;
    private CancellationTokenSource? _woaRankingsCts;

    [Parameter]
    public required int AllianceId { get; set; }

    [Inject]
    public IAlliancePageAnalyticsService AnalyticsService { get; set; }

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();

        _alliance = await LoadWithPersistenceAsync(nameof(_alliance),
            () => StatsHubUiService.GetAllianceAsync(AllianceId));

        if (_alliance != null)
        {
            _defaultAnalyticsParameters = new Dictionary<string, object>
            {
                {AnalyticsParams.FOG_ALLIANCE_ID, _alliance.Alliance.Id},
                {AnalyticsParams.LOCATION, AnalyticsParams.Values.Locations.ALLIANCE_PROFILE},
            };
        }

        if (OperatingSystem.IsBrowser())
        {
            _canShowChart = true;
            IsInitialized = true;
        }
    }

    private async Task ToggleRankingChart(bool expanded)
    {
        AnalyticsService.TrackChartView(AnalyticsEvents.TOGGLE_CHART, _defaultAnalyticsParameters,
            AnalyticsParams.Values.Sources.ALLIANCE_RANKING_CHART, expanded);

        if (expanded)
        {
            await GetRankings();
        }
    }

    private async Task ToggleAthRankingsContainer(bool expanded)
    {
        AnalyticsService.TrackChartView(AnalyticsEvents.TOGGLE_VIEW, _defaultAnalyticsParameters,
            AnalyticsParams.Values.Sources.ALLIANCE_ATH_RANKINGS, expanded);

        if (expanded)
        {
            await GetAthRankings();
        }
    }

    private async Task ToggleWoaRankingsContainer(bool expanded)
    {
        AnalyticsService.TrackChartView(AnalyticsEvents.TOGGLE_VIEW, _defaultAnalyticsParameters,
            AnalyticsParams.Values.Sources.ALLIANCE_WOA_RANKINGS, expanded);

        if (expanded)
        {
            await GetWoaRankings();
        }
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
        _rankings = await StatsHubUiService.GetAllianceRankingsAsync(AllianceId, _rankingsCts.Token);
        _rankingsAreLoading = false;
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

        try
        {
            _athRankings = await StatsHubUiService.GetAllianceAthRankingsAsync(AllianceId);
            _athRankingsAreLoading = false;
        }
        catch (OperationCanceledException _)
        {
        }
        catch (ApiException apiEx) when (apiEx.InnerException is TaskCanceledException)
        {
            _athRankingsAreLoading = false;
        }
        catch (Exception e)
        {
            _athRankingsAreLoading = false;
            Console.Error.WriteLine(e);
        }
    }

    private async Task GetWoaRankings()
    {
        if (_woaRankings != null)
        {
            return;
        }

        if (_woaRankingsCts != null)
        {
            await _woaRankingsCts.CancelAsync();
        }

        _woaRankingsAreLoading = true;
        StateHasChanged();

        _woaRankingsCts = new CancellationTokenSource();

        try
        {
            _woaRankings = await StatsHubUiService.GetAllianceWoaRankingsAsync(AllianceId, _woaRankingsCts.Token);
            _woaRankingsAreLoading = false;
        }
        catch (OperationCanceledException _)
        {
        }
        catch (ApiException apiEx) when (apiEx.InnerException is TaskCanceledException)
        {
            _woaRankingsAreLoading = false;
        }
        catch (Exception e)
        {
            _woaRankingsAreLoading = false;
            Console.Error.WriteLine(e);
        }
    }
}
