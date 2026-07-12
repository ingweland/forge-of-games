using FluentResults;
using Ingweland.Fog.Application.Server.Errors;
using Ingweland.Fog.Application.Server.Interfaces;
using Ingweland.Fog.Application.Server.Services.Interfaces;
using Ingweland.Fog.Models.Fog.Entities;
using Ingweland.Fog.Models.Fog.Enums;
using Ingweland.Fog.Models.Hoh.Entities;
using Ingweland.Fog.Models.Hoh.Entities.Alliance;
using Ingweland.Fog.Models.Hoh.Entities.Ranking;
using Ingweland.Fog.Models.Hoh.Enums;
using Ingweland.Fog.Shared.Extensions;
using Ingweland.Fog.Shared.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Ingweland.Fog.Application.Server.Services;

public class FogPlayerService(IFogDbContext context, ILogger<FogPlayerService> logger) : IFogPlayerService
{
    private readonly SemaphoreSlim _upsertSemaphore = new(1, 1);

    public async Task UpdateStatusAsync(IEnumerable<int> playerIds, InGameEntityStatus status,
        CancellationToken cancellationToken)
    {
        var uniqueIds = playerIds.ToHashSet();
        var players = await context.Players.Where(x => uniqueIds.Contains(x.Id)).ToListAsync(cancellationToken);

        logger.LogDebug("Found {PlayerCount} players to update status", players.Count);

        foreach (var player in players)
        {
            DoUpdateStatusAsync(player, status, cancellationToken);
        }

        await context.SaveChangesAsync(cancellationToken);

        logger.LogDebug("Updated status to {Status} for {PlayerCount} players", status, players.Count);
    }

    public async Task UpdateStatusAsync(int playerId, InGameEntityStatus status, CancellationToken cancellationToken)
    {
        logger.LogDebug("Updating status to {Status} for player {PlayerId}", status, playerId);

        var player = await context.Players.FindAsync(playerId, cancellationToken);
        if (player == null)
        {
            logger.LogWarning("Could not update status because player with id {PlayerId} not found", playerId);
            return;
        }

        DoUpdateStatusAsync(player, status, cancellationToken);

        await context.SaveChangesAsync(cancellationToken);

        logger.LogDebug("Updated status to {Status} for player {PlayerId}", status, playerId);
    }

    public async Task UpsertPlayerAsync(PlayerProfile profile, string worldId)
    {
        logger.LogDebug("Upserting player {PlayerId} from world {WorldId}", profile.Player.Id, worldId);
        await _upsertSemaphore.WaitAsync();
        try
        {
            logger.LogDebug("Acquired semaphore for player {PlayerId} from world {WorldId}",
                profile.Player.Id, worldId);
            await DoUpsertPlayer(profile, worldId);
            await context.SaveChangesAsync();
            logger.LogDebug("Successfully upserted player {PlayerId} from world {WorldId}",
                profile.Player.Id, worldId);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error upserting player {PlayerId} from world {WorldId}: {ErrorMessage}",
                profile.Player.Id, worldId, ex.Message);
            throw;
        }
        finally
        {
            _upsertSemaphore.Release();
            logger.LogDebug("Released semaphore for player {PlayerId} from world {WorldId}",
                profile.Player.Id, worldId);
        }
    }

    public async Task<Result<bool>> AddPlayerAsync(string worldId, PlayerRank playerRank)
    {
        logger.LogDebug("Starting {m} for player {PlayerId} from world {WorldId}", nameof(AddPlayerAsync),
            playerRank.Id, worldId);
        await _upsertSemaphore.WaitAsync();
        try
        {
            var existingPlayer = await context.Players
                .FirstOrDefaultAsync(x => x.InGamePlayerId == playerRank.Id && x.WorldId == worldId);

            if (existingPlayer != null)
            {
                return Result.Ok(false);
            }

            var newPlayer = new Player
            {
                InGamePlayerId = playerRank.Id,
                WorldId = worldId,
                Age = HohStringParser.GetConcreteId(playerRank.Age),
                Name = playerRank.Name,
                AvatarId = playerRank.AvatarId,
            };
            context.Players.Add(newPlayer);
            await context.SaveChangesAsync();

            logger.LogDebug("Successfully added player {PlayerId} from world {WorldId}", playerRank.Id, worldId);
            return Result.Ok(true);
        }
        catch (Exception ex)
        {
            return Result.Fail(new PlayerAdditionError(worldId, playerRank.Id, ex));
        }
        finally
        {
            logger.LogDebug("Releasing semaphore for player {PlayerId} from world {WorldId}", playerRank.Id, worldId);
            _upsertSemaphore.Release();
        }
    }

    public async Task<Result<Player>> UpsertPlayerAsync(string worldId, HohPlayer player, int rankingPoints,
        DateTime lastOnline)
    {
        logger.LogDebug("Starting UpsertPlayerAsync for player {PlayerId} from world {WorldId}", player.Id, worldId);
        await _upsertSemaphore.WaitAsync();
        var now = DateTime.UtcNow;
        try
        {
            var modifiedPlayer = await AddOrUpdatePlayerAsync(worldId, player, null, rankingPoints, now);
            modifiedPlayer.LastSeenOnline = lastOnline;

            await context.SaveChangesAsync();

            logger.LogDebug("Successfully upserted player {PlayerId} from world {WorldId}", player.Id, worldId);
            return Result.Ok(modifiedPlayer);
        }
        catch (Exception ex)
        {
            return Result.Fail(new PlayerUpsertionError(worldId, player.Id, ex));
        }
        finally
        {
            _upsertSemaphore.Release();
            logger.LogDebug("Released semaphore for player {PlayerId} from world {WorldId}", player.Id, worldId);
        }
    }

    public async Task<Result<IReadOnlyCollection<(AllianceMember AllianceMember, Player Player)>>> UpsertPlayersAsync(
        string worldId, IReadOnlyCollection<AllianceMember> allianceMembers)
    {
        logger.LogDebug("Starting bulk upsert of {MemberCount} players from world {WorldId}", allianceMembers.Count,
            worldId);

        var upsertedPlayers = new List<(AllianceMember AllianceMember, Player Player)>();
        foreach (var member in allianceMembers)
        {
            logger.LogDebug("Upserting player {PlayerId} from alliance member", member.Player.Id);
            var playerResult =
                await UpsertPlayerAsync(worldId, member.Player, member.RankingPoints, member.LastSeenOnline);
            if (playerResult.IsFailed)
            {
                return Result.Fail<IReadOnlyCollection<(AllianceMember AllianceMember, Player Player)>>(playerResult
                    .Errors);
            }

            upsertedPlayers.Add((member, playerResult.Value));
        }

        logger.LogDebug("Successfully upserted {PlayerCount} players from world {WorldId}", upsertedPlayers.Count,
            worldId);
        return Result.Ok<IReadOnlyCollection<(AllianceMember AllianceMember, Player Player)>>(upsertedPlayers);
    }

    private void DoUpdateStatusAsync(Player player, InGameEntityStatus status, CancellationToken cancellationToken)
    {
        logger.LogDebug("Updating status for player {PlayerId} to {Status}", player.Id, status);
        player.Status = status;
        if (status == InGameEntityStatus.Missing)
        {
            logger.LogDebug("Clearing alliance membership for missing player {PlayerId}", player.Id);
            player.AllianceMembership = null;
        }
    }

    private void UpsertSquads(Player player, IReadOnlyCollection<ProfileSquad> squads, DateOnly collectedAt)
    {
        logger.LogDebug("Upserting {SquadCount} squads for player {PlayerId}", squads.Count, player.InGamePlayerId);

        player.Squads.Clear();
        foreach (var squad in squads)
        {
            logger.LogDebug("Adding squad unit {UnitId} for player {PlayerId}", squad.Hero.UnitId,
                player.InGamePlayerId);
            player.Squads.Add(new ProfileSquadEntity
            {
                UnitId = squad.Hero.UnitId,
                Level = squad.Hero.Level,
                AscensionLevel = squad.Hero.AscensionLevel,
                AbilityLevel = squad.Hero.AbilityLevel,
                AwakeningLevel = squad.Hero.AwakeningLevel,
                CollectedAt = collectedAt,
                Data = new ProfileSquadDataEntity
                {
                    Hero = squad.Hero,
                    SupportUnit = squad.SupportUnit,
                },
                Age = player.Age,
            });
        }

        logger.LogDebug("Upserting squads completed for player {PlayerId}.", player.InGamePlayerId);
    }

    private void AddHeroes(Player player, IReadOnlyCollection<ProfileSquad> squads)
    {
        logger.LogDebug("Adding {SquadCount} heroes for player {PlayerId}", squads.Count, player.InGamePlayerId);

        var existing = player.Heroes.Select(x => x.UnitId).ToHashSet();
        var newHeroes = squads.Select(x => x.Hero.UnitId).ToHashSet().Except(existing);
        foreach (var h in newHeroes)
        {
            player.Heroes.Add(new PlayerHeroEntity {UnitId = h});
        }
    }

    private async Task<Player> AddOrUpdatePlayerAsync(string worldId, HohPlayer player, int? rank, int rankingPoints,
        DateTime now)
    {
        var today = now.ToDateOnly();

        logger.LogDebug("Getting existing player {PlayerId} from world {WorldId}",
            player.Id, worldId);
        var existingPlayer = await context.Players
            .Include(p =>
                p.Rankings.Where(pr => pr.Type == PlayerRankingType.TotalHeroPower && pr.CollectedAt == today))
            .Include(p => p.NameHistory)
            .Include(p => p.AgeHistory)
            .Include(p => p.AllianceHistory)
            .Include(p => p.Squads)
            .Include(p => p.AllianceMembership)
            .Include(p => p.PvpRankings2.Where(x => x.CollectedAt == today))
            .Include(p => p.Heroes)
            .AsSplitQuery()
            .FirstOrDefaultAsync(x => x.InGamePlayerId == player.Id && x.WorldId == worldId);

        Player modifiedPlayer;
        if (existingPlayer != null)
        {
            logger.LogDebug("Updating existing player {PlayerId}", existingPlayer.Id);
            existingPlayer.Name = player.Name;
            existingPlayer.Age = player.Age;
            existingPlayer.Status = InGameEntityStatus.Active;
            modifiedPlayer = existingPlayer;
        }
        else
        {
            logger.LogDebug("Creating new player {PlayerId} from world {WorldId}", player.Id, worldId);
            var newPlayer = new Player
            {
                WorldId = worldId,
                InGamePlayerId = player.Id,
                Name = player.Name,
                Age = player.Age,
                Status = InGameEntityStatus.Active,
            };

            modifiedPlayer = newPlayer;
            context.Players.Add(newPlayer);
        }

        modifiedPlayer.UpdatedAt = today;
        modifiedPlayer.AvatarId = player.AvatarId;
        if (rank.HasValue)
        {
            modifiedPlayer.Rank = rank;
        }

        modifiedPlayer.RankingPoints = rankingPoints;

        if (modifiedPlayer.NameHistory.All(x => x.Name != player.Name))
        {
            logger.LogDebug("Adding name {PlayerName} to history for player {PlayerId}",
                player.Name, player.Id);
            modifiedPlayer.NameHistory.Add(new PlayerNameHistoryEntry {Name = player.Name});
        }

        if (modifiedPlayer.AgeHistory.OrderByDescending(x => x.ChangedAt).FirstOrDefault()?.Age != player.Age)
        {
            logger.LogDebug("Adding age {PlayerAge} to history for player {PlayerId}",
                player.Age, player.Id);
            modifiedPlayer.AgeHistory.Add(new PlayerAgeHistoryEntry {Age = player.Age, ChangedAt = now});
        }

        var existingRanking =
            modifiedPlayer.Rankings.FirstOrDefault(x =>
                x.Type == PlayerRankingType.TotalHeroPower && x.CollectedAt == today);
        if (existingRanking != null)
        {
            logger.LogDebug("Updating existing ranking for player {PlayerId}: Rank {Rank}, Points {Points}",
                player.Id, rank, rankingPoints);
            existingRanking.Points = rankingPoints;
            existingRanking.Rank = rank ?? 0;
            existingRanking.CollectedAt = today;
        }
        else
        {
            logger.LogDebug("Adding new ranking for player {PlayerId}: Rank {Rank}, Points {Points}",
                player.Id, rank, rankingPoints);
            modifiedPlayer.Rankings.Add(new PlayerRanking
            {
                Points = rankingPoints,
                Rank = rank ?? 0,
                CollectedAt = today,
                Type = PlayerRankingType.TotalHeroPower,
            });
        }

        return modifiedPlayer;
    }

    private async Task DoUpsertPlayer(PlayerProfile profile, string worldId)
    {
        var now = DateTime.UtcNow;
        var today = now.ToDateOnly();
        logger.LogDebug("Starting DoUpsertPlayer for {PlayerId} from world {WorldId}, date: {Today}",
            profile.Player.Id, worldId, today);

        Alliance? alliance = null;
        if (profile.Alliance != null)
        {
            logger.LogDebug("Upserting alliance {AllianceId} for player {PlayerId}",
                profile.Alliance.Id, profile.Player.Id);
            alliance = await context.Alliances.FirstOrDefaultAsync(x =>
                x.WorldId == worldId && x.InGameAllianceId == profile.Alliance.Id);
            if (alliance == null)
            {
                throw new Exception($"Could not find alliance: WorldId={worldId}, InGameAllianceId={profile.Alliance.Id
                }");
            }
        }

        var modifiedPlayer =
            await AddOrUpdatePlayerAsync(worldId, profile.Player, profile.Rank, profile.RankingPoints, now);
        modifiedPlayer.ProfileUpdatedAt = today;

        if (alliance != null)
        {
            logger.LogDebug("Setting alliance {AllianceId} for player {PlayerId}", alliance.Id, profile.Player.Id);
            if (modifiedPlayer.AllianceMembership == null ||
                modifiedPlayer.AllianceMembership.AllianceId != alliance.Id)
            {
                modifiedPlayer.AllianceMembership = new AllianceMemberEntity
                {
                    Alliance = alliance,
                };
            }

            if (modifiedPlayer.AllianceHistory.All(a => a.Id != alliance.Id))
            {
                modifiedPlayer.AllianceHistory.Add(alliance);
                logger.LogDebug("Added alliance {AllianceId} to history for player {PlayerId}",
                    alliance.Id, profile.Player.Id);
            }
        }
        else
        {
            logger.LogDebug("Clearing alliance for player {PlayerId}", profile.Player.Id);
            modifiedPlayer.AllianceMembership = null;
        }

        modifiedPlayer.TreasureHuntDifficulty = profile.TreasureHuntDifficulty;
        var pvpTier = modifiedPlayer.PvpRankings2.FirstOrDefault(x => x.CollectedAt == today);
        if (pvpTier == null)
        {
            logger.LogDebug("Adding new PVP tier {PvpTier} for player {PlayerId}", profile.PvpTier, profile.Player.Id);
            modifiedPlayer.PvpRankings2.Add(new PvpRanking2
            {
                CollectedAt = today,
                Tier = profile.PvpTier,
            });
        }
        else if (pvpTier.Tier != profile.PvpTier)
        {
            logger.LogDebug("Updating PVP tier for player {PlayerId} from {OldPvpTier} to {NewPvpTier}",
                profile.Player.Id, pvpTier.Tier, profile.PvpTier);
            pvpTier.Tier = profile.PvpTier;
        }

        UpsertSquads(modifiedPlayer, profile.Squads, today);
        AddHeroes(modifiedPlayer, profile.Squads);

        logger.LogDebug("Saving changes for player {PlayerId} from world {WorldId}",
            profile.Player.Id, worldId);
    }
}
