using Ingweland.Fog.Application.Client.Core.Localization;
using Ingweland.Fog.Application.Client.Web.Services.Hoh.Abstractions;
using Ingweland.Fog.Application.Client.Web.StatsHub.ViewModels;
using Ingweland.Fog.Application.Client.Web.ViewModels;
using Ingweland.Fog.Models.Fog;
using Ingweland.Fog.Models.Fog.Enums;
using Microsoft.AspNetCore.Components;

namespace Ingweland.Fog.WebApp.Client.Components.Pages.StatsHub;

public partial class WorldPlayersWoaRankingsPage : WorldStatsPageBase<PlayerViewModel>
{
    private WoaPlayerStatsCategory _selectedStatsCategory = WoaPlayerStatsCategory.VictoryPoints;

    private IReadOnlyCollection<WoaPlayerStatsCategoryViewModel>? _statsCategories;

    [Inject]
    private ICommonUiService CommonUiService { get; set; }

    protected override string GetTitle()
    {
        return WorldId == "zz1"
            ? Loc[FogResource.StatsHub_Worlds_TopWoaPlayersListTitle, FogResource.StatsHub_Worlds_BetaWorld]
            : Loc[FogResource.StatsHub_Worlds_TopWoaPlayersListTitle, FogResource.StatsHub_Worlds_MainWorld];
    }

    protected override async ValueTask<PaginatedList<PlayerViewModel>> FetchDataAsync(int startIndex, int count,
        string? query = null, CancellationToken ct = default)
    {
        return await StatsHubUiService.GetWoaPlayerRankingsAsync(WorldId, startIndex, count, _selectedStatsCategory,
            ct);
    }

    protected override void OnInitialized()
    {
        base.OnInitialized();

        IsInitialized = false;

        _statsCategories =
            LoadWithPersistence(nameof(_statsCategories), () => CommonUiService.GetWoaPlayerStatsCategories());

        IsInitialized = true;
    }

    protected override async Task OnParametersSetAsync()
    {
        await base.OnParametersSetAsync();

        if (Enum.TryParse<WoaPlayerStatsCategory>(Q, true, out var c))
        {
            _selectedStatsCategory = c;
        }
        else
        {
            _selectedStatsCategory = WoaPlayerStatsCategory.VictoryPoints;
        }
    }

    private void OnStatsCategoryChanged(WoaPlayerStatsCategory statsCategory)
    {
        _selectedStatsCategory = statsCategory;
        Search(statsCategory.ToString());
    }
}
