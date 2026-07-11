using Ingweland.Fog.Dtos.Hoh;

namespace Ingweland.Fog.Application.Core.Services.Hoh.Abstractions;

public interface ICommonService
{
    Task<IReadOnlyCollection<AgeDto>> GetAgesAsync();

    Task<IReadOnlyCollection<ResourceDto>> GetResourcesAsync();

    Task<IReadOnlyCollection<PvpTierDto>> GetPvpTiersAsync();

    Task<IReadOnlyCollection<TreasureHuntLeagueDto>> GetTreasureHuntLeaguesAsync();

    Task<IReadOnlyCollection<WoaTierDto>> GetWoaTiersAsync();
}
