using Ingweland.Fog.Inn.Models.Hoh.Extensions;

namespace Ingweland.Fog.Inn.Models.Hoh;

public sealed partial class CommunicationDto
{
    public AllianceMembersResponse? AllianceMembersResponse =>
        RootContext.Messages.FindAndUnpackToList<AllianceMembersResponse>().FirstOrDefault();

    public AlliancePush? AlliancePush => RootContext.Messages.FindAndUnpackToList<AlliancePush>().FirstOrDefault();

    public AllianceRanksDTO AllianceRanks => Response.FindAndUnpack<AllianceRanksDTO>();

    public IList<AllianceDetailsDto> Alliances =>
        RootContext.Messages.FindAndUnpackToList<AllianceDetailsDto>("AllianceDTO");

    public BatchResponse BatchResponse => Response.FindAndUnpack<BatchResponse>();

    public IList<CityDTO> Cities => RootContext.Messages.FindAndUnpackToList<CityDTO>();

    public EquipmentPush? Equipment => RootContext.Messages.FindAndUnpackToList<EquipmentPush>().FirstOrDefault();

    public GameDesignResponse GameDesignResponse => Response.FindAndUnpack<GameDesignResponse>();

    public HeroBattleStatsResponse HeroBattleStatsResponse => Response.FindAndUnpack<HeroBattleStatsResponse>();

    public HeroFinishWaveResponse HeroFinishWaveResponse => Response.FindAndUnpack<HeroFinishWaveResponse>();

    public HeroPush HeroPush => RootContext.Messages.FindAndUnpack<HeroPush>();

    public HeroStartBattleResponse HeroStartBattleResponse => Response.FindAndUnpack<HeroStartBattleResponse>();

    public IList<HeroTreasureHuntAlliancePointsPush> HeroTreasureHuntAlliancePointsPushs =>
        RootContext.Messages.FindAndUnpackToList<HeroTreasureHuntAlliancePointsPush>();

    public IList<HeroTreasureHuntPlayerPointsPush> HeroTreasureHuntPlayerPointsPushs =>
        RootContext.Messages.FindAndUnpackToList<HeroTreasureHuntPlayerPointsPush>();

    public IList<InGameEventDto> InGameEvents =>
        RootContext.Messages.FindAndUnpackToList<InGameEventPush>().FirstOrDefault()?.Events ?? [];

    public IList<LeaderboardPush> Leaderboards => RootContext.Messages.FindAndUnpackToList<LeaderboardPush>();

    public LocaResponse LocaResponse => Response.FindAndUnpack<LocaResponse>();

    public OtherCityDTO OtherCity
    {
        get
        {
            try
            {
                return PackedMessages.FindAndUnpack<OtherCityDTO>();
            }
            catch
            {
                // ignore
            }

            return Response.FindAndUnpack<OtherCityDTO>();
        }
    }

    public PlayerRanksDTO PlayerRanks => Response.FindAndUnpack<PlayerRanksDTO>();

    public PvpBattleHistoryResponse PvpBattleHistoryResponse => Response.FindAndUnpack<PvpBattleHistoryResponse>();

    public PvpGetRankingResponse PvpGetRankingResponse => Response.FindAndUnpack<PvpGetRankingResponse>();

    public RelicPush? RelicPush => RootContext.Messages.FindAndUnpackToList<RelicPush>().FirstOrDefault();

    public ResearchStateDTO? ResearchState =>
        RootContext.Messages.FindAndUnpackToList<ResearchStateDTO>().FirstOrDefault();

    public IList<WoAAlliancePush> WoAAlliancePushs => RootContext.Messages.FindAndUnpackToList<WoAAlliancePush>();

    public WoADivisionPush? WoaDivisionPush =>
        RootContext.Messages.FindAndUnpackToList<WoADivisionPush>().FirstOrDefault();

    public IList<WoAHeroRosterPush> WoaHeroRosters => RootContext.Messages.FindAndUnpackToList<WoAHeroRosterPush>();

    public ReworkedWondersDTO? Wonders =>
        RootContext.Messages.FindAndUnpackToList<ReworkedWondersDTO>().FirstOrDefault();
}
