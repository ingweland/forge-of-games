using AutoMapper;
using Ingweland.Fog.Application.Server.Interfaces;
using Ingweland.Fog.Functions.Data;
using Ingweland.Fog.Models.Fog.Entities;
using Ingweland.Fog.Models.Hoh.Entities.Woa;
using Ingweland.Fog.Models.Hoh.Enums;
using Ingweland.Fog.Shared.Constants;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Ingweland.Fog.Functions.Services;

public interface IWoaPlayerStatsService
{
    Task RunAsync(IReadOnlyCollection<(DateTime CollectedAt, WoaPlayerStats Stats)> stats, string worldId);
}

public class WoaPlayerStatsService(
    IFogDbContext context,
    IMapper mapper,
    IPlayerService playerService,
    ILogger<WoaPlayerStatsService> logger) : IWoaPlayerStatsService
{
    private readonly List<InGameEventEntity> _events = [];

    public async Task RunAsync(IReadOnlyCollection<(DateTime CollectedAt, WoaPlayerStats Stats)> stats, string worldId)
    {
        if (stats.Count == 0)
        {
            return;
        }

        var uniqueStats = stats.OrderByDescending(x => x.CollectedAt)
            .DistinctBy(x => (x.Stats.Player.Id, x.Stats.EventId)).ToList();
        var minDate = uniqueStats.Last().CollectedAt;
        var players = mapper.Map<List<PlayerAggregate>>(
            uniqueStats.DistinctBy(x => x.Stats.Player.Id).Select(x => x.Stats.Player), opt =>
            {
                opt.Items[ResolutionContextKeys.WORLD_ID] = worldId;
                opt.Items[ResolutionContextKeys.DATE] = minDate;
            });
        await playerService.AddAsync(players);

        foreach (var s in uniqueStats)
        {
            var inGameEvent = _events.FirstOrDefault(x =>
                x.WorldId == worldId && x.DefinitionId == EventDefinitionId.WoAEvent && x.EventId == s.Stats.EventId);
            if (inGameEvent == null)
            {
                inGameEvent = await context.InGameEvents.FirstOrDefaultAsync(x =>
                    x.WorldId == worldId && x.DefinitionId == EventDefinitionId.WoAEvent &&
                    x.EventId == s.Stats.EventId);
                if (inGameEvent != null)
                {
                    _events.Add(inGameEvent);
                }
            }

            if (inGameEvent == null)
            {
                logger.LogError("In-game event with key {WorldId}:{EventId} not found.", worldId, s.Stats.EventId);
                continue;
            }

            var player = await context.Players
                .Include(x => x.WoaStats.Where(y => y.InGameEventId == inGameEvent.Id))
                .FirstOrDefaultAsync(x => x.WorldId == worldId && x.InGamePlayerId == s.Stats.Player.Id);
            if (player == null)
            {
                logger.LogError("Player with key {WorldId}:{InGamePlayerId} not found.", worldId, s.Stats.Player.Id);
                continue;
            }

            if (player.WoaStats.Count > 1)
            {
                // this should never happen because it's enforced on db level, but let's double-check
                logger.LogError("Player with key {WorldId}:{InGamePlayerId} has more than one woa stats item.",
                    worldId, s.Stats.Player.Id);
                continue;
            }

            var woaStatsItem = player.WoaStats.FirstOrDefault();
            if (woaStatsItem == null)
            {
                woaStatsItem = new WoaPlayerStatsEntity
                {
                    InGameEventId = inGameEvent.Id,
                    UpdatedAt = s.CollectedAt,
                    ContributionPoints = (int) s.Stats.ContributionPoints,
                    ContributionPointsGainedAt = s.Stats.ContributionPointsGainedAt,
                    HealingDone = s.Stats.HealingDone,
                    LostAttacks = s.Stats.LostAttacks,
                    RepairsStarted = s.Stats.RepairsStarted,
                    VictoryPoints = (int) Math.Round(s.Stats.VictoryPoints, MidpointRounding.AwayFromZero),
                    WonAttacks = s.Stats.WonAttacks,
                    WonDefenses = s.Stats.WonDefenses,
                };
                player.WoaStats.Add(woaStatsItem);
            }
            else if (woaStatsItem.UpdatedAt < s.CollectedAt)
            {
                woaStatsItem.UpdatedAt = s.CollectedAt;
                woaStatsItem.ContributionPoints = (int) s.Stats.ContributionPoints;
                woaStatsItem.ContributionPointsGainedAt = s.Stats.ContributionPointsGainedAt;
                woaStatsItem.HealingDone = s.Stats.HealingDone;
                woaStatsItem.LostAttacks = s.Stats.LostAttacks;
                woaStatsItem.RepairsStarted = s.Stats.RepairsStarted;
                woaStatsItem.VictoryPoints = (int) Math.Round(s.Stats.VictoryPoints, MidpointRounding.AwayFromZero);
                woaStatsItem.WonAttacks = s.Stats.WonAttacks;
                woaStatsItem.WonDefenses = s.Stats.WonDefenses;
            }
        }

        await context.SaveChangesAsync();
    }
}
