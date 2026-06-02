using Ingweland.Fog.Application.Client.Web.Analytics;
using Ingweland.Fog.Application.Client.Web.Analytics.Interfaces;
using Ingweland.Fog.Application.Client.Web.Models;
using Ingweland.Fog.Application.Client.Web.Services.Abstractions;
using Ingweland.Fog.Application.Client.Web.ViewModels;
using Ingweland.Fog.Application.Core.Helpers;
using Ingweland.Fog.Models.Hoh.Enums;
using Ingweland.Fog.WebApp.Client.Components.Pages.Abstractions;
using Microsoft.AspNetCore.Components;

namespace Ingweland.Fog.WebApp.Client.Components.Pages;

public partial class AlliedCultureCityGuides : FogPageBase
{
    private IReadOnlyCollection<AlliedCultureCalendarItemViewModel> _calendar = [];
    private bool _dataIsLoading;
    private IReadOnlyCollection<AlliedCultureCityGuideGroupViewModel> _guides = [];

    [Inject]
    private IAlliedCultureCityGuidesUiService AlliedCultureCityGuidesUiService { get; set; }

    [Inject]
    private ICityStrategyAnalyticsService AnalyticsService { get; set; }

    [Inject]
    public CityStrategyNavigationState CityStrategyNavigationState { get; set; }

    [Inject]
    private NavigationManager NavigationManager { get; set; }

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();

        if (!OperatingSystem.IsBrowser())
        {
            return;
        }

        _ = GetDataAsync();

        AnalyticsService.TrackEvent(AnalyticsEvents.OPEN_ALLIED_CULTURE_CITY_GUIDES_PAGE);
    }

    private async Task GetDataAsync()
    {
        _dataIsLoading = true;
        StateHasChanged();

        var cTask = AlliedCultureCityGuidesUiService.GetCalendarAsync("un1");
        var gTask = AlliedCultureCityGuidesUiService.GetGuidesAsync();

        await Task.WhenAll(cTask, gTask);

        _calendar = cTask.Result;
        _guides = gTask.Result;

        _dataIsLoading = false;
        StateHasChanged();
    }

    private async Task OpenCalendarItem(WonderId wonderId)
    {
        var s = _guides.SelectMany(x => x.Guides).FirstOrDefault(x => x.Wonder.Id == wonderId);
        if (s == null)
        {
            return;
        }

        await OpenCommunityStrategy(s);
    }

    private async Task OpenCommunityStrategy(AlliedCultureCityGuideViewModel communityStrategy)
    {
        _dataIsLoading = true;
        StateHasChanged();

        var eventParams = new Dictionary<string, object>
        {
            {AnalyticsParams.CITY_ID, communityStrategy.CityId.ToString()},
            {AnalyticsParams.SHARE_ID, communityStrategy.SharedDataId},
        };

        AnalyticsService.TrackEvent(AnalyticsEvents.OPEN_COMMUNITY_CITY_STRATEGY_INIT, eventParams);

        var strategy = await AlliedCultureCityGuidesUiService.GetGuideAsync(communityStrategy.SharedDataId);
        if (strategy != null)
        {
            CityStrategyNavigationState.Data = new CityStrategyNavigationState.CityStrategyNavigationStateData
            {
                Strategy = strategy,
                IsRemote = true,
                IsCommunity = true,
            };

            _dataIsLoading = false;

            AnalyticsService.TrackEvent(AnalyticsEvents.OPEN_COMMUNITY_CITY_STRATEGY_SUCCESS, eventParams);

            NavigationManager.NavigateTo(FogUrlBuilder.PageRoutes.CITY_STRATEGY_VIEWER_PATH);
        }
        else
        {
            AnalyticsService.TrackEvent(AnalyticsEvents.OPEN_COMMUNITY_CITY_STRATEGY_ERROR, eventParams);
        }

        _dataIsLoading = false;
        StateHasChanged();
    }
}
