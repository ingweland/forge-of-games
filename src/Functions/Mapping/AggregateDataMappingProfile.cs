using AutoMapper;
using Ingweland.Fog.Functions.Data;
using Ingweland.Fog.Models.Fog.Entities;
using Ingweland.Fog.Models.Hoh.Entities;
using Ingweland.Fog.Models.Hoh.Entities.Alliance;
using Ingweland.Fog.Models.Hoh.Entities.Ranking;
using Ingweland.Fog.Models.Hoh.Enums;
using Ingweland.Fog.Shared.Constants;
using Ingweland.Fog.Shared.Extensions;
using Ingweland.Fog.Shared.Helpers;

namespace Ingweland.Fog.Functions.Mapping;

public class AggregateDataMappingProfile : Profile
{
    public AggregateDataMappingProfile()
    {
        // to aggregate
        CreateMap<PlayerRank, PlayerAggregate>()
            .ForMember(dest => dest.InGamePlayerId, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.Age, opt => opt.MapFrom(src => HohStringParser.GetConcreteId(src.Age)))
            .ForMember(dest => dest.RankingPoints, opt => opt.MapFrom(src => src.Points))
            .ForMember(dest => dest.AllianceName, opt => opt.MapFrom(src => src.AllianceName ?? string.Empty))
            .ForMember(dest => dest.PlayerRankingType, opt =>
                opt.MapFrom((_, _, _, context) =>
                    context.Items.GetRequiredItem<PlayerRankingType>(ResolutionContextKeys.PLAYER_RANKING_TYPE)))
            .ForMember(dest => dest.CollectedAt, opt =>
                opt.MapFrom((_, _, _, context) => context.Items.GetRequiredItem<DateTime>(ResolutionContextKeys.DATE)))
            .ForMember(dest => dest.WorldId, opt =>
                opt.MapFrom((_, _, _, context) =>
                    context.Items.GetRequiredItem<string>(ResolutionContextKeys.WORLD_ID)));

        CreateMap<PvpRank, PlayerAggregate>()
            .ForMember(dest => dest.InGamePlayerId, opt => opt.MapFrom(src => src.Player.Id))
            .ForMember(dest => dest.Age, opt => opt.MapFrom(src => HohStringParser.GetConcreteId(src.Player.Age)))
            .ForMember(dest => dest.AvatarId, opt => opt.MapFrom(src => src.Player.AvatarId))
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Player.Name))
            .ForMember(dest => dest.AllianceName,
                opt => opt.MapFrom(src => src.Alliance == null ? string.Empty : src.Alliance.Name))
            .ForMember(dest => dest.InGameAllianceId, opt =>
            {
                opt.PreCondition(src => src.Alliance != null);
                opt.MapFrom(src => src.Alliance!.Id);
            })
            .ForMember(dest => dest.PvpRank, opt => opt.MapFrom(src => src.Rank))
            .ForMember(dest => dest.PvpRankingPoints, opt => opt.MapFrom(src => src.Points))
            .ForMember(dest => dest.CollectedAt, opt =>
                opt.MapFrom((_, _, _, context) => context.Items.GetRequiredItem<DateTime>(ResolutionContextKeys.DATE)))
            .ForMember(dest => dest.WorldId, opt =>
                opt.MapFrom((_, _, _, context) =>
                    context.Items.GetRequiredItem<string>(ResolutionContextKeys.WORLD_ID)));

        CreateMap<HohPlayer, PlayerAggregate>()
            .ForMember(dest => dest.InGamePlayerId, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.Age, opt => opt.MapFrom(src => HohStringParser.GetConcreteId(src.Age)))
            .ForMember(dest => dest.AvatarId, opt => opt.MapFrom(src => src.AvatarId))
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
            .ForMember(dest => dest.CollectedAt, opt =>
                opt.MapFrom((_, _, _, context) => context.Items.GetRequiredItem<DateTime>(ResolutionContextKeys.DATE)))
            .ForMember(dest => dest.WorldId, opt =>
                opt.MapFrom((_, _, _, context) =>
                    context.Items.GetRequiredItem<string>(ResolutionContextKeys.WORLD_ID)));

        CreateMap<(DateTime CollectedAt, int AllianceId, HohPlayer Player), PlayerAggregate>()
            .IncludeMembers(src => src.Player)
            .ForMember(dest => dest.InGameAllianceId, opt => opt.MapFrom(src => src.AllianceId))
            .ForMember(dest => dest.CollectedAt, opt => opt.MapFrom(src => src.CollectedAt));

        CreateMap<AllianceRank, AllianceAggregate>()
            .ForMember(dest => dest.InGameAllianceId, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.RankingPoints, opt => opt.MapFrom(src => src.Points))
            .ForMember(dest => dest.LeaderInGameId, opt => opt.MapFrom(src => src.Leader.Id))
            .ForMember(dest => dest.AllianceRankingType, opt =>
                opt.MapFrom((_, _, _, context) =>
                    context.Items.GetRequiredItem<AllianceRankingType>(ResolutionContextKeys.ALLIANCE_RANKING_TYPE)))
            .ForMember(dest => dest.CollectedAt, opt =>
                opt.MapFrom((_, _, _, context) => context.Items.GetRequiredItem<DateTime>(ResolutionContextKeys.DATE)))
            .ForMember(dest => dest.WorldId, opt =>
                opt.MapFrom((_, _, _, context) =>
                    context.Items.GetRequiredItem<string>(ResolutionContextKeys.WORLD_ID)));

        CreateMap<HohAlliance, AllianceAggregate>()
            .ForMember(dest => dest.InGameAllianceId, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.CollectedAt, opt =>
                opt.MapFrom((_, _, _, context) => context.Items.GetRequiredItem<DateTime>(ResolutionContextKeys.DATE)))
            .ForMember(dest => dest.WorldId, opt =>
                opt.MapFrom((_, _, _, context) =>
                    context.Items.GetRequiredItem<string>(ResolutionContextKeys.WORLD_ID)));

        CreateMap<HohAllianceExtended, AllianceAggregate>()
            .ForMember(dest => dest.InGameAllianceId, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.Rank, opt => opt.MapFrom(src => src.Rank))
            .ForMember(dest => dest.CollectedAt, opt =>
                opt.MapFrom((_, _, _, context) => context.Items.GetRequiredItem<DateTime>(ResolutionContextKeys.DATE)))
            .ForMember(dest => dest.WorldId, opt =>
                opt.MapFrom((_, _, _, context) =>
                    context.Items.GetRequiredItem<string>(ResolutionContextKeys.WORLD_ID)));

        CreateMap<PvpRank, AllianceAggregate>()
            .ForMember(dest => dest.InGameAllianceId, opt => opt.MapFrom(src => src.Alliance!.Id))
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Alliance!.Name))
            .ForMember(dest => dest.Banner, opt => opt.MapFrom(src => src.Alliance!.Banner))
            .ForMember(dest => dest.MemberInGameId, opt => opt.MapFrom(src => src.Player.Id))
            .ForMember(dest => dest.CollectedAt, opt =>
                opt.MapFrom((_, _, _, context) => context.Items.GetRequiredItem<DateTime>(ResolutionContextKeys.DATE)))
            .ForMember(dest => dest.WorldId, opt =>
                opt.MapFrom((_, _, _, context) =>
                    context.Items.GetRequiredItem<string>(ResolutionContextKeys.WORLD_ID)));

        CreateMap<PlayerProfile, PlayerAggregate>()
            .ForMember(dest => dest.InGamePlayerId, opt => opt.MapFrom(src => src.Player.Id))
            .ForMember(dest => dest.Age, opt => opt.MapFrom(src => HohStringParser.GetConcreteId(src.Player.Age)))
            .ForMember(dest => dest.AvatarId, opt => opt.MapFrom(src => src.Player.AvatarId))
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Player.Name))
            .ForMember(dest => dest.Rank, opt => opt.MapFrom(src => src.Rank))
            .ForMember(dest => dest.RankingPoints, opt => opt.MapFrom(src => src.RankingPoints))
            .ForMember(dest => dest.ProfileSquads, opt => opt.MapFrom(src => src.Squads))
            .ForMember(dest => dest.InGameAllianceId, opt =>
            {
                opt.PreCondition(x => x.Alliance != null);
                opt.MapFrom(src => src.Alliance!.Id);
            })
            .ForMember(dest => dest.AllianceName, opt =>
            {
                opt.PreCondition(x => x.Alliance != null);
                opt.MapFrom(src => src.Alliance!.Name);
            })
            .ForMember(dest => dest.PlayerRankingType, opt =>
                opt.MapFrom((_, _, _, context) =>
                    context.Items.GetRequiredItem<PlayerRankingType>(ResolutionContextKeys.PLAYER_RANKING_TYPE)))
            .ForMember(dest => dest.CollectedAt, opt =>
                opt.MapFrom((_, _, _, context) => context.Items.GetRequiredItem<DateTime>(ResolutionContextKeys.DATE)))
            .ForMember(dest => dest.WorldId, opt =>
                opt.MapFrom((_, _, _, context) =>
                    context.Items.GetRequiredItem<string>(ResolutionContextKeys.WORLD_ID)));

        CreateMap<(HohPlayer Player, HohAlliance? Alliance), PlayerAggregate>()
            .ForMember(dest => dest.InGamePlayerId, opt => opt.MapFrom(src => src.Player.Id))
            .ForMember(dest => dest.Age, opt => opt.MapFrom(src => HohStringParser.GetConcreteId(src.Player.Age)))
            .ForMember(dest => dest.AvatarId, opt => opt.MapFrom(src => src.Player.AvatarId))
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Player.Name))
            .ForMember(dest => dest.InGameAllianceId, opt =>
            {
                opt.PreCondition(x => x.Alliance != null);
                opt.MapFrom(src => src.Alliance!.Id);
            })
            .ForMember(dest => dest.AllianceName, opt =>
            {
                opt.PreCondition(x => x.Alliance != null);
                opt.MapFrom(src => src.Alliance!.Name);
            })
            .ForMember(dest => dest.CollectedAt, opt =>
                opt.MapFrom((_, _, _, context) => context.Items.GetRequiredItem<DateTime>(ResolutionContextKeys.DATE)))
            .ForMember(dest => dest.WorldId, opt =>
                opt.MapFrom((_, _, _, context) =>
                    context.Items.GetRequiredItem<string>(ResolutionContextKeys.WORLD_ID)));

        // from aggregate

        CreateMap<PlayerAggregate, PlayerRanking>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.PlayerId, opt => opt.Ignore())
            .ForMember(dest => dest.Points, opt => opt.MapFrom(src => src.RankingPoints))
            .ForMember(dest => dest.Rank, opt => opt.MapFrom(src => src.Rank))
            .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.PlayerRankingType))
            .ForMember(dest => dest.CollectedAt, opt => opt.MapFrom(src => DateOnly.FromDateTime(src.CollectedAt)));

        CreateMap<AllianceAggregate, AllianceRanking>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.AllianceId, opt => opt.Ignore())
            .ForMember(dest => dest.Points, opt => opt.MapFrom(src => src.RankingPoints))
            .ForMember(dest => dest.Rank, opt => opt.MapFrom(src => src.Rank))
            .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.AllianceRankingType))
            .ForMember(dest => dest.CollectedAt, opt => opt.MapFrom(src => DateOnly.FromDateTime(src.CollectedAt)));

        CreateMap<ProfileSquad, ProfileSquadEntity>()
            .ForMember(dest => dest.UnitId, opt => opt.MapFrom(src => src.Hero.UnitId))
            .ForMember(dest => dest.Level, opt => opt.MapFrom(src => src.Hero.Level))
            .ForMember(dest => dest.AscensionLevel, opt => opt.MapFrom(src => src.Hero.AscensionLevel))
            .ForMember(dest => dest.AbilityLevel, opt => opt.MapFrom(src => src.Hero.AbilityLevel))
            .ForMember(dest => dest.AwakeningLevel, opt => opt.MapFrom(src => src.Hero.AwakeningLevel))
            .ForMember(dest => dest.Data, opt => opt.MapFrom(src => new ProfileSquadDataEntity
            {
                Hero = src.Hero,
                SupportUnit = src.SupportUnit,
            }))
            .ForMember(dest => dest.Age, opt =>
                opt.MapFrom((_, _, _, context) => context.Items.GetRequiredItem<string>(ResolutionContextKeys.AGE)))
            .ForMember(dest => dest.CollectedAt, opt =>
                opt.MapFrom((_, _, _, context) => context.Items.GetRequiredItem<DateOnly>(ResolutionContextKeys.DATE)));
    }
}
