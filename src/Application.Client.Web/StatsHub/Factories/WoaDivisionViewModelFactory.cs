using Ingweland.Fog.Application.Client.Web.StatsHub.Abstractions;
using Ingweland.Fog.Application.Client.Web.StatsHub.ViewModels;
using Ingweland.Fog.Dtos.Hoh;
using Ingweland.Fog.Dtos.Hoh.Stats;
using Ingweland.Fog.Models.Hoh.Enums;

namespace Ingweland.Fog.Application.Client.Web.StatsHub.Factories;

public class WoaDivisionViewModelFactory(
    IStatsHubViewModelsFactory statsHubViewModelsFactory,
    IAllianceWoaRankingViewModelFactory allianceWoaRankingViewModelFactory) : IWoaDivisionViewModelFactory
{
    public WoaDivisionViewModel Create(WoaDivisionDto dto, IReadOnlyDictionary<WoaTier, WoaTierDto> tiers)
    {
        var alliances = statsHubViewModelsFactory
            .CreateAlliances(dto.Alliances.Select(x => x.Alliance).ToList())
            .ToDictionary(x => x.Id);

        var items = new List<WoaDivisionAllianceViewModel>();
        foreach (var item in dto.Alliances)
        {
            if (!alliances.TryGetValue(item.Alliance.Id, out var alliance))
            {
                continue;
            }

            items.Add(new WoaDivisionAllianceViewModel
            {
                Alliance = alliance,
                Ranking = allianceWoaRankingViewModelFactory.Create(item.Ranking,
                    tiers.GetValueOrDefault(item.Ranking.Tier, WoaTierDto.Default)),
            });
        }

        return new WoaDivisionViewModel
        {
            EventLabel = $"{dto.StartedAt:d} - {dto.EndedAt:d}",
            Alliances = items,
        };
    }
}
