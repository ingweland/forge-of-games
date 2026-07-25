using AutoMapper;
using Ingweland.Fog.Dtos.Hoh.Battle;
using Ingweland.Fog.Dtos.Hoh.Stats;
using Ingweland.Fog.Models.Fog.Entities;
using Ingweland.Fog.Models.Fog.Enums;
using Ingweland.Fog.Models.Hoh.Entities.Alliance;
using Ingweland.Fog.Shared.Extensions;

namespace Ingweland.Fog.Application.Server.Mapping.Hoh;

public class StatsMappingProfile : Profile
{
    public StatsMappingProfile()
    {
        CreateMap<Player, PlayerKey>();
        CreateMap<Player, PlayerKeyExtended>();
        CreateMap<PlayerRanking, PlayerRanking>();
        CreateMap<Player, Player>()
            .ForMember(dest => dest.Rankings, opt => opt.Ignore());
        CreateMap<Player, PlayerDto>()
            .ForMember(dest => dest.AllianceId, opt =>
            {
                opt.PreCondition(x => x.AllianceMembership != null);
                opt.MapFrom(x => x.AllianceMembership!.AllianceId);
            })
            .ForMember(dest => dest.AllianceName, opt =>
            {
                opt.PreCondition(x => x.AllianceMembership != null);
                opt.MapFrom(x => x.AllianceMembership!.Alliance.Name);
            });
        CreateMap<PlayerRanking, PlayerKey>();
        CreateMap<PlayerRanking, PlayerDto>()
            .ForMember(dest => dest.RankingPoints, opt => opt.MapFrom(x => x.Points))
            .ForMember(dest => dest.Rank, opt => opt.MapFrom(x => x.Rank))
            .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(x => x.CollectedAt))
            .ForMember(dest => dest.Age, opt => opt.MapFrom(x => x.Player.Age))
            .ForMember(dest => dest.Name, opt => opt.MapFrom(x => x.Player.Name))
            .ForMember(dest => dest.WorldId, opt => opt.MapFrom(x => x.Player.WorldId))
            .ForMember(dest => dest.AvatarId, opt => opt.MapFrom(x => x.Player.AvatarId))
            .ForMember(dest => dest.Id, opt => opt.MapFrom(x => x.Player.Id))
            .ForMember(dest => dest.AllianceId, opt =>
            {
                opt.PreCondition(x => x.Player.AllianceMembership != null);
                opt.MapFrom(x => x.Player.AllianceMembership!.AllianceId);
            })
            .ForMember(dest => dest.AllianceName, opt =>
            {
                opt.PreCondition(x => x.Player.AllianceMembership != null);
                opt.MapFrom(x => x.Player.AllianceMembership!.Alliance.Name);
            });
        CreateMap<PlayerRanking, StatsTimedIntValue>()
            .ForMember(dest => dest.Value, opt => opt.MapFrom(src => src.Points))
            .ForMember(dest => dest.Date, opt => opt.MapFrom(src => src.CollectedAt.ToDateTime(TimeOnly.MinValue)));

        CreateMap<Alliance, AllianceKey>();
        CreateMap<AllianceRanking, AllianceRanking>();
        CreateMap<Alliance, Alliance>()
            .ForMember(dest => dest.Rankings, opt => opt.Ignore());
        CreateMap<Alliance, AllianceDto>()
            .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => src.Status == InGameEntityStatus.Missing))
            .ForMember(dest => dest.UpdatedAt,
                opt => opt.MapFrom(src =>
                    src.UpdatedAt > src.MembersUpdatedAt.ToDateOnly()
                        ? src.UpdatedAt
                        : src.MembersUpdatedAt.ToDateOnly()))
            .ForMember(dest => dest.Banner, opt => opt.MapFrom(x => x));
        CreateMap<AllianceRanking, StatsTimedIntValue>()
            .ForMember(dest => dest.Value, opt => opt.MapFrom(src => src.Points))
            .ForMember(dest => dest.Date, opt => opt.MapFrom(src => src.CollectedAt.ToDateTime(TimeOnly.MinValue)));

        CreateMap<PvpBattle, BattleKey>();

        CreateMap<ProfileSquadEntity, ProfileSquadDto>()
            .ForMember(dest => dest.Hero, opt => opt.MapFrom(x => x.Data.Hero))
            .ForMember(dest => dest.SupportUnit, opt => opt.MapFrom(x => x.Data.SupportUnit));

        CreateMap<EquipmentInsightsEntity, EquipmentInsightsDto>();
        CreateMap<RelicInsightsEntity, RelicInsightsDto>();
        CreateMap<PvpRanking2, PvpRankingDto>();
        CreateMap<PvpEliteRanking, PvpEliteRankingDto>();
        CreateMap<Alliance, AllianceBanner>()
            .ForMember(dest => dest.IconId, opt => opt.MapFrom(x => x.BannerIconId))
            .ForMember(dest => dest.CrestId, opt => opt.MapFrom(x => x.BannerCrestId))
            .ForMember(dest => dest.IconColorId, opt => opt.MapFrom(x => x.BannerIconColorId))
            .ForMember(dest => dest.CrestColorId, opt => opt.MapFrom(x => x.BannerCrestColorId));
    }
}
