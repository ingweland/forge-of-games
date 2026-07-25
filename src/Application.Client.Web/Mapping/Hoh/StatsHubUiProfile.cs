using AutoMapper;
using Ingweland.Fog.Application.Client.Web.Extensions;
using Ingweland.Fog.Application.Client.Web.Mapping.Hoh.Converters;
using Ingweland.Fog.Application.Client.Web.StatsHub.ViewModels;
using Ingweland.Fog.Application.Client.Web.ViewModels.Hoh.Battle;
using Ingweland.Fog.Dtos.Hoh;
using Ingweland.Fog.Dtos.Hoh.Stats;
using Ingweland.Fog.Models.Fog;
using Ingweland.Fog.Models.Hoh.Entities.Alliance;
using Ingweland.Fog.Models.Hoh.Enums;
using Ingweland.Fog.Shared.Constants;
using Ingweland.Fog.Shared.Extensions;
using Ingweland.Fog.Shared.Formatters;

namespace Ingweland.Fog.Application.Client.Web.Mapping.Hoh;

public class StatsHubUiProfile : Profile
{
    public StatsHubUiProfile()
    {
        CreateMap<PlayerDto, PlayerViewModel>()
            .ForMember(dest => dest.Rank, opt => opt.MapFrom(src => src.Rank == 0 ? "-" : src.Rank.ToString()))
            .ForMember(dest => dest.RankingPoints,
                opt => opt.MapFrom(src => src.RankingPoints == 0 ? "-" : src.RankingPoints.ToString()))
            .ForMember(dest => dest.RankingPointsFormatted,
                opt => opt.MapFrom(src =>
                    src.RankingPoints == 0 ? "-" : NumberFormatter.FormatCompactNumber(src.RankingPoints)))
            .ForMember(dest => dest.Age, opt =>
                opt.MapFrom((src, _, _, context) =>
                {
                    var ages = context.Items.GetRequiredItem<IReadOnlyDictionary<string, AgeDto>>(ResolutionContextKeys
                        .AGES);
                    return ages.TryGetValue(src.Age, out var age) ? age.Name : src.Age;
                }))
            .ForMember(dest => dest.AgeColor, opt =>
                opt.MapFrom((src, _, _, context) =>
                {
                    var ages = context.Items.GetRequiredItem<IReadOnlyDictionary<string, AgeDto>>(ResolutionContextKeys
                        .AGES);
                    ages.TryGetValue(src.Age, out var age);
                    return age.ToCssColor();
                }))
            .ForMember(dest => dest.AvatarUrl,
                opt => opt.ConvertUsing<PlayerAvatarIdToUrlConverter, int>(src => src.AvatarId))
            .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => src.UpdatedAt.ToString("d")))
            .ForMember(dest => dest.IsStale,
                opt => opt.MapFrom(src => src.UpdatedAt < DateOnly.FromDateTime(DateTime.UtcNow).AddDays(-1)));
        CreateMap<PaginatedList<PlayerDto>, PaginatedList<PlayerViewModel>>();

        CreateMap<AllianceMemberDto, AllianceMemberViewModel>()
            .ForMember(dest => dest.RankingPointsFormatted,
                opt => opt.MapFrom(src =>
                    src.RankingPoints == 0 ? "-" : NumberFormatter.FormatCompactNumber(src.RankingPoints)))
            .ForMember(dest => dest.Age, opt =>
                opt.MapFrom((src, _, _, context) =>
                {
                    var ages = context.Items.GetRequiredItem<IReadOnlyDictionary<string, AgeDto>>(ResolutionContextKeys
                        .AGES);
                    return ages.TryGetValue(src.Age, out var age) ? age.Name : src.Age;
                }))
            .ForMember(dest => dest.AgeColor, opt =>
                opt.MapFrom((src, _, _, context) =>
                {
                    var ages = context.Items.GetRequiredItem<IReadOnlyDictionary<string, AgeDto>>(ResolutionContextKeys
                        .AGES);
                    ages.TryGetValue(src.Age, out var age);
                    return age.ToCssColor();
                }))
            .ForMember(dest => dest.AvatarUrl,
                opt => opt.ConvertUsing<PlayerAvatarIdToUrlConverter, int>(src => src.AvatarId))
            .ForMember(dest => dest.JoinedOn, opt =>
            {
                opt.PreCondition(src => src.JoinedAt.HasValue);
                opt.MapFrom(src => src.JoinedAt!.Value.ToLocalTime().ToString("d"));
            })
            .ForMember(dest => dest.LastSeenOn, opt =>
            {
                opt.PreCondition(src => src.LastSeenAt.HasValue);
                opt.MapFrom(src => src.LastSeenAt!.Value.ToLocalTime().ToString("d"));
            })
            .ForMember(dest => dest.IsStale,
                opt => opt.MapFrom(src => src.UpdatedAt < DateOnly.FromDateTime(DateTime.UtcNow).AddDays(-1)))
            .ForMember(dest => dest.RoleIconUrl,
                opt => opt.ConvertUsing<AllianceMemberRoleToIconUrlConverter, AllianceMemberRole>(src => src.Role))
            .ForMember(dest => dest.TreasureHuntDifficulty, opt => opt.MapFrom((src, _, _, context) =>
            {
                var treasureHuntDifficulties =
                    context.Items.GetRequiredItem<IReadOnlyDictionary<int, TreasureHuntDifficultyBasicViewModel>>(
                        ResolutionContextKeys.TREASURE_HUNT_DIFFICULTY_VMS);
                return treasureHuntDifficulties!.GetValueOrDefault(src.TreasureHuntDifficulty, null);
            }))
            .ForMember(dest => dest.TreasureHuntMaxPoints, opt =>
                opt.MapFrom((src, _, _, context) =>
                {
                    var maxPointsMap =
                        context.Items.GetRequiredItem<IReadOnlyDictionary<int, int>>(ResolutionContextKeys
                            .TREASURE_HUNT_DIFFICULTY_POINTS_MAP);
                    return maxPointsMap.GetValueOrDefault(src.TreasureHuntDifficulty, 0);
                }));

        CreateMap<AllianceDto, AllianceViewModel>()
            .ForMember(dest => dest.RankingPointsFormatted,
                opt => opt.MapFrom(src => NumberFormatter.FormatCompactNumber(src.RankingPoints)))
            .ForMember(dest => dest.Banner,
                opt => opt.ConvertUsing<AllianceBannerViewModelConverter, AllianceBanner>(src => src.Banner))
            .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => src.UpdatedAt.ToString("d")))
            .ForMember(dest => dest.IsStale,
                opt => opt.MapFrom(src => src.UpdatedAt < DateOnly.FromDateTime(DateTime.UtcNow).AddDays(-1)));
        CreateMap<PaginatedList<AllianceDto>, PaginatedList<AllianceViewModel>>();
        CreateMap<AllianceProfileDto, AllianceProfileViewModel>();

        CreateMap<PvpRankingDto, PvpRankingViewModel>()
            .ForMember(dest => dest.CollectedAt,
                opt => opt.MapFrom(src => src.CollectedAt.ToDateTime(TimeOnly.MinValue)));

        CreateMap<PvpEliteRankingDto, PvpEliteRankingViewModel>()
            .ForMember(dest => dest.CollectedAt,
                opt => opt.MapFrom(src => src.CollectedAt.ToDateTime(TimeOnly.MinValue)));
    }
}
