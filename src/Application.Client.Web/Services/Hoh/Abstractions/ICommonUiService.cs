using Ingweland.Fog.Application.Client.Web.ViewModels;
using Ingweland.Fog.Application.Client.Web.ViewModels.Hoh;
using Ingweland.Fog.Dtos.Hoh;
using Ingweland.Fog.Models.Hoh.Enums;

namespace Ingweland.Fog.Application.Client.Web.Services.Hoh.Abstractions;

public interface ICommonUiService
{
    Task<IReadOnlyDictionary<string, AgeViewModel>> GetAgesAsync();
    Task<AgeViewModel?> GetAgeAsync(string ageId);
    Task<IReadOnlyDictionary<PvpTier, PvpTierDto>> GetPvpTiersAsync();
    Task<IReadOnlyDictionary<TreasureHuntLeague, TreasureHuntLeagueDto>> GetTreasureHuntLeaguesAsync();
    Task<IReadOnlyDictionary<WoaTier, WoaTierDto>> GetWoaTiersAsync();
    IReadOnlyCollection<WoaPointsCategoryViewModel> GetWoaPointsCategories();
}
