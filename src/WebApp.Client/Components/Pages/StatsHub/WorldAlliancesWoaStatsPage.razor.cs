using Ingweland.Fog.Application.Client.Core.Localization;
using Ingweland.Fog.Application.Client.Web.Services.Hoh.Abstractions;
using Ingweland.Fog.Application.Client.Web.StatsHub.ViewModels;
using Ingweland.Fog.Application.Client.Web.ViewModels;
using Ingweland.Fog.Models.Fog;
using Ingweland.Fog.Models.Fog.Enums;
using Microsoft.AspNetCore.Components;

namespace Ingweland.Fog.WebApp.Client.Components.Pages.StatsHub;

public partial class WorldAlliancesWoaStatsPage : WorldStatsPageBase<AllianceViewModel>
{
    private bool _isLoading;
    private ICollection<AllianceViewModel>? _items;

    private IReadOnlyCollection<WoaPointsCategoryViewModel>? _pointsCategories;
    private WoaPointsCategory _selectedPointsCategory = WoaPointsCategory.Atlantis;

    [Inject]
    private ICommonUiService CommonUiService { get; set; }

    protected override string GetTitle()
    {
        return WorldId == "zz1"
            ? Loc[FogResource.StatsHub_Worlds_TopAllianceAthRankingListTitle, FogResource.StatsHub_Worlds_BetaWorld]
            : Loc[FogResource.StatsHub_Worlds_TopAllianceAthRankingListTitle, FogResource.StatsHub_Worlds_MainWorld];
    }

    protected override async ValueTask<PaginatedList<AllianceViewModel>> FetchDataAsync(int startIndex, int count,
        string? query = null, CancellationToken ct = default)
    {
        return await StatsHubUiService.GetAlliancesWoaRankingsAsync(WorldId, startIndex, count, _selectedPointsCategory,
            ct);
    }

    protected override void OnInitialized()
    {
        base.OnInitialized();

        IsInitialized = false;

        _pointsCategories =
            LoadWithPersistence(nameof(_pointsCategories), () => CommonUiService.GetWoaPointsCategories());

        IsInitialized = true;
    }

    protected override async Task OnParametersSetAsync()
    {
        await base.OnParametersSetAsync();

        if (Enum.TryParse<WoaPointsCategory>(Q, true, out var l))
        {
            _selectedPointsCategory = l;
        }
        else
        {
            _selectedPointsCategory = WoaPointsCategory.Atlantis;
        }
    }

    private void OnPointsCategoryChanged(WoaPointsCategory pointsCategory)
    {
        _selectedPointsCategory = pointsCategory;
        Search(pointsCategory.ToString());
    }
}
