using Ingweland.Fog.InnSdk.Hoh.Abstractions;
using Ingweland.Fog.InnSdk.Hoh.Services.Abstractions;

namespace Ingweland.Fog.InnSdk.Hoh;

public class InnSdkClient(
    Lazy<IStaticDataService> staticDataService,
    Lazy<IRankingsService> rankingsService,
    Lazy<IBattleService> battleService,
    Lazy<ICityService> cityService,
    Lazy<IPlayerService> playerService,
    Lazy<IAllianceService> allianceService,
    Lazy<IWoaService> woaService)
    : IInnSdkClient
{
    public ICityService CityService => cityService.Value;
    public IBattleService BattleService => battleService.Value;
    public IRankingsService RankingsService => rankingsService.Value;
    public IStaticDataService StaticDataService => staticDataService.Value;
    public IPlayerService PlayerService => playerService.Value;
    public IAllianceService AllianceService => allianceService.Value;
    public IWoaService WoaService => woaService.Value;
}
