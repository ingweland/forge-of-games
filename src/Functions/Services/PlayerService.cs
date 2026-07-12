using AutoMapper;
using AutoMapper.QueryableExtensions;
using Ingweland.Fog.Application.Server.Interfaces;
using Ingweland.Fog.Application.Server.Services.Hoh.Abstractions;
using Ingweland.Fog.Application.Server.Services.Interfaces;
using Ingweland.Fog.Functions.Data;
using Ingweland.Fog.Models.Fog.Entities;
using Ingweland.Fog.Models.Fog.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Player = Ingweland.Fog.Models.Fog.Entities.Player;

namespace Ingweland.Fog.Functions.Services;

public interface IPlayerService
{
    Task AddAsync(IEnumerable<PlayerAggregate> playerAggregates);
    Task<IReadOnlySet<int>> AddMissingPlayersAsync(IReadOnlySet<int> inGamePlayerIds, string worldId);
}

public class PlayerService(
    IFogDbContext context,
    IMapper mapper,
    IFogPlayerService playerService,
    IFogAllianceService fogAllianceService,
    IInGamePlayerService inGamePlayerService,
    ILogger<PlayerRankingService> logger) : IPlayerService
{
    public async Task AddAsync(IEnumerable<PlayerAggregate> playerAggregates)
    {
        var unique = playerAggregates
            .Where(p => p.CanBeConvertedToPlayer())
            .OrderByDescending(p => p.CollectedAt) // we need this to correctly set UpdateAt on the player
            .DistinctBy(p => p.Key)
            .ToDictionary(p => p.Key);
        logger.LogInformation("Filtered aggregates to {UniqueCount} unique items.", unique.Count);
        var existingPlayerKeys = await GetExistingPlayersAsync(unique.Keys.Select(pk => pk.InGamePlayerId).ToHashSet());
        logger.LogInformation("Found {ExistingCount} existing players.", existingPlayerKeys.Count);
        var newPlayerKeys = unique.Keys.ToHashSet().Except(existingPlayerKeys).ToList();
        logger.LogInformation("Identified {NewCount} new players.", newPlayerKeys.Count);
        var newPlayersList = newPlayerKeys.Select(k =>
        {
            var playerAggregate = unique[k];
            return new Player
            {
                WorldId = playerAggregate.WorldId,
                InGamePlayerId = playerAggregate.InGamePlayerId,
                Name = playerAggregate.Name!,
                Age = playerAggregate.Age!,
                AvatarId = playerAggregate.AvatarId ?? 0,
                UpdatedAt = DateOnly.FromDateTime(playerAggregate.CollectedAt),
                TreasureHuntDifficulty = playerAggregate.TreasureHuntDifficulty,
                Status = InGameEntityStatus.Active,
            };
        }).ToList();
        context.Players.AddRange(newPlayersList);
        await context.SaveChangesAsync();
        logger.LogInformation("Saved {NewPlayersCount} new players.", newPlayersList.Count);
    }

    public async Task<IReadOnlySet<int>> AddMissingPlayersAsync(IReadOnlySet<int> inGamePlayerIds, string worldId)
    {
        var existingPlayerKeys = await GetExistingPlayersAsync(inGamePlayerIds.ToHashSet());
        var missingPlayerKeys =
            inGamePlayerIds.Except(existingPlayerKeys.Where(x => x.WorldId == worldId).Select(p => p.InGamePlayerId))
                .Select(x => new PlayerKey(worldId, x)).ToList();
        var failedPlayerIds = new HashSet<int>();
        logger.LogInformation("Missing player count: {Count}:", missingPlayerKeys.Count);
        foreach (var playerKey in missingPlayerKeys)
        {
            logger.LogDebug("Processing player {@Player}", playerKey);
            var delayTask = Task.Delay(200);
            var profile = await inGamePlayerService.FetchProfile(playerKey);
            if (profile.IsSuccess)
            {
                try
                {
                    if (profile.Value.Alliance != null)
                    {
                        await fogAllianceService.UpsertAlliance(profile.Value.Alliance, worldId,
                            DateTime.UtcNow);
                    }

                    await playerService.UpsertPlayerAsync(profile.Value, worldId);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error upserting alliance/player {@Player}", playerKey);
                    failedPlayerIds.Add(playerKey.InGamePlayerId);
                }
            }
            else
            {
                failedPlayerIds.Add(playerKey.InGamePlayerId);
            }

            await delayTask;
        }

        logger.LogInformation("Done processing missing players. Failed players count: {Count}:", failedPlayerIds.Count);
        return failedPlayerIds;
    }

    private async Task<HashSet<PlayerKey>> GetExistingPlayersAsync(HashSet<int> inGamePlayerIds)
    {
        logger.LogInformation("Querying existing players for {IdCount} in-game player IDs.", inGamePlayerIds.Count);
        var existing = await context.Players
            .Where(p => inGamePlayerIds.Contains(p.InGamePlayerId))
            .ProjectTo<PlayerKey>(mapper.ConfigurationProvider)
            .ToHashSetAsync();
        logger.LogInformation("Query returned {ExistingCount} existing players.", existing.Count);
        return existing;
    }
}
