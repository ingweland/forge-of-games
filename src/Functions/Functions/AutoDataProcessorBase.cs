using AutoMapper;
using Ingweland.Fog.Application.Server.Interfaces.Hoh;
using Ingweland.Fog.Application.Server.Providers;
using Ingweland.Fog.Application.Server.Services.Hoh.Abstractions;
using Ingweland.Fog.Functions.Data;
using Ingweland.Fog.InnSdk.Hoh.Providers;
using Ingweland.Fog.Models.Fog.Entities;
using Ingweland.Fog.Models.Hoh.Entities;
using Ingweland.Fog.Models.Hoh.Entities.Alliance;
using Ingweland.Fog.Models.Hoh.Entities.Ranking;
using Ingweland.Fog.Models.Hoh.Enums;
using Ingweland.Fog.Shared.Constants;
using Microsoft.Extensions.Logging;

namespace Ingweland.Fog.Functions.Functions;

public abstract class AutoDataProcessorBase(
    IGameWorldsProvider gameWorldsProvider,
    IInGameRawDataTableRepository inGameRawDataTableRepository,
    IInGameDataParsingService inGameDataParsingService,
    InGameRawDataTablePartitionKeyProvider inGameRawDataTablePartitionKeyProvider,
    IMapper mapper,
    ILogger<AutoDataProcessorBase> logger,
    DatabaseWarmUpService databaseWarmUpService) : FunctionBase(gameWorldsProvider, inGameRawDataTableRepository,
    inGameDataParsingService, inGameRawDataTablePartitionKeyProvider, logger)
{
    private const int WAKEUP_BATCH_SIZE = 100;

    private static readonly HashSet<PlayerRankingType> PlayerRankingTypes =
        [PlayerRankingType.ResearchPoints, PlayerRankingType.TotalHeroPower];

    private static readonly HashSet<AllianceRankingType> AllianceRankingTypes = [AllianceRankingType.MemberTotal];

    protected bool HasMoreWakeupData { get; private set; }

    protected async
        Task<(List<PlayerAggregate> PlayerAggregates, List<AllianceAggregate> AllianceAggregates,
            List<(DateTime CollectedAt, AllianceKey AllianceKey, IReadOnlyCollection<AllianceMember>)>
            ConfirmedAllianceMembers)> PrepareData(int wakeupPage = -1)
    {
        var date = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(-1);
        logger.LogInformation("{activity} started for date {date}", nameof(AutoDataProcessorBase), date);
        await databaseWarmUpService.WarmUpDatabaseIfRequiredAsync();

        var playerAggregates = new List<PlayerAggregate>(32000);
        var allianceAggregates = new List<AllianceAggregate>(16000);
        var allConfirmedAllianceMembers =
            new List<(DateTime CollectedAt, AllianceKey AllianceKey, IReadOnlyCollection<AllianceMember>)>();
        foreach (var gameWorld in GameWorldsProvider.GetGameWorlds())
        {
            logger.LogInformation("Processing game world {gameWorldId}", gameWorld.Id);
            var allianceWakeups =
                await GetWakeupsAsync(InGameRawDataTablePartitionKeyProvider.Alliance(gameWorld.Id, date), wakeupPage);
            logger.LogInformation("{count} alliance wakeups retrieved for game world {gameWorldId}",
                allianceWakeups.Count, gameWorld.Id);
            var alliances = allianceWakeups
                .Where(t => t.Wakeup.Alliance != null)
                .Select(t => (t.CollectedAt, Alliance: t.Wakeup.Alliance!))
                .ToList();
            logger.LogInformation("{count} alliances retrieved for game world {gameWorldId}",
                alliances.Count, gameWorld.Id);

            var leaderboardParticipants = allianceWakeups
                .SelectMany(t =>
                    t.Wakeup.Leaderboards.SelectMany(x => x.Participants.Select(y => (t.CollectedAt, Participant: y))))
                .ToList();
            var leaderboardParticipantAlliances = leaderboardParticipants
                .Select(t => (t.CollectedAt, t.Participant.Alliance))
                .Where(t => t.Alliance != null)
                .ToList();
            alliances = alliances.Concat(leaderboardParticipantAlliances!).ToList();
            logger.LogInformation("{count} alliances retrieved from leaderboards for game world {gameWorldId}",
                leaderboardParticipantAlliances.Count, gameWorld.Id);

            var alliancesMembers = allianceWakeups
                .Where(t => t.Wakeup.AllianceWithMembers != null)
                .SelectMany(t => t.Wakeup.AllianceWithMembers!.Members.Select(m =>
                    (t.CollectedAt, t.Wakeup.AllianceWithMembers.AllianceId, m.Player)))
                .ToList();
            logger.LogInformation("{count} alliance members retrieved for game world {gameWorldId}",
                alliancesMembers.Count, gameWorld.Id);
            var confirmedAllianceMembers = allianceWakeups
                .Where(t => t.Wakeup.AllianceWithMembers != null)
                .Select(t => (t.CollectedAt,
                    new AllianceKey(gameWorld.Id, t.Wakeup.AllianceWithMembers!.AllianceId),
                    t.Wakeup.AllianceWithMembers!.Members))
                .ToList();
            logger.LogInformation("{count} confirmed alliance members retrieved for game world {gameWorldId}",
                confirmedAllianceMembers.Count, gameWorld.Id);
            allConfirmedAllianceMembers.AddRange(confirmedAllianceMembers);

            var pvpBattles = await GetPvpBattles(gameWorld.Id, date);
            logger.LogInformation("{count} pvp battles retrieved for game world {gameWorldId}",
                pvpBattles.Count, gameWorld.Id);

            foreach (var playerRankingType in PlayerRankingTypes)
            {
                var playerRankings = await GetPlayerRanking(gameWorld.Id, date, playerRankingType);
                logger.LogInformation("{count} player rankings retrieved for game world {gameWorldId}",
                    playerRankings.Count, gameWorld.Id);
                foreach (var t in playerRankings)
                {
                    playerAggregates.Add(mapper.Map<PlayerAggregate>(t.PlayerRank, opt =>
                    {
                        opt.Items[ResolutionContextKeys.PLAYER_RANKING_TYPE] = playerRankingType;
                        opt.Items[ResolutionContextKeys.WORLD_ID] = gameWorld.Id;
                        opt.Items[ResolutionContextKeys.DATE] = t.CollectedAt;
                    }));
                }
            }

            foreach (var allianceRankingType in AllianceRankingTypes)
            {
                var allianceRankings = await GetAllianceRanking(gameWorld.Id, date, allianceRankingType);
                logger.LogInformation("{count} alliance rankings retrieved for game world {gameWorldId}",
                    allianceRankings.Count, gameWorld.Id);

                foreach (var t in allianceRankings)
                {
                    playerAggregates.Add(mapper.Map<PlayerAggregate>(t.AllianceRank.Leader, opt =>
                    {
                        opt.Items[ResolutionContextKeys.WORLD_ID] = gameWorld.Id;
                        opt.Items[ResolutionContextKeys.DATE] = t.CollectedAt;
                    }));
                }

                foreach (var t in allianceRankings)
                {
                    allianceAggregates.Add(mapper.Map<AllianceAggregate>(t.AllianceRank, opt =>
                    {
                        opt.Items[ResolutionContextKeys.ALLIANCE_RANKING_TYPE] = allianceRankingType;
                        opt.Items[ResolutionContextKeys.WORLD_ID] = gameWorld.Id;
                        opt.Items[ResolutionContextKeys.DATE] = t.CollectedAt;
                    }));
                }
            }

            foreach (var t in alliancesMembers)
            {
                playerAggregates.Add(mapper.Map<PlayerAggregate>(t,
                    opt => { opt.Items[ResolutionContextKeys.WORLD_ID] = gameWorld.Id; }));
            }

            foreach (var t in pvpBattles)
            {
                playerAggregates.Add(mapper.Map<PlayerAggregate>(t.PvpBattle.Winner, opt =>
                {
                    opt.Items[ResolutionContextKeys.WORLD_ID] = gameWorld.Id;
                    opt.Items[ResolutionContextKeys.DATE] = date.ToDateTime(TimeOnly.MinValue);
                }));
                playerAggregates.Add(mapper.Map<PlayerAggregate>(t.PvpBattle.Loser, opt =>
                {
                    opt.Items[ResolutionContextKeys.WORLD_ID] = gameWorld.Id;
                    opt.Items[ResolutionContextKeys.DATE] = date.ToDateTime(TimeOnly.MinValue);
                }));
            }

            foreach (var t in alliances)
            {
                allianceAggregates.Add(mapper.Map<AllianceAggregate>(t.Alliance, opt =>
                {
                    opt.Items[ResolutionContextKeys.WORLD_ID] = gameWorld.Id;
                    opt.Items[ResolutionContextKeys.DATE] = t.CollectedAt;
                }));
            }

            foreach (var t in leaderboardParticipants)
            {
                playerAggregates.Add(mapper.Map<PlayerAggregate>(t.Participant, opt =>
                {
                    opt.Items[ResolutionContextKeys.WORLD_ID] = gameWorld.Id;
                    opt.Items[ResolutionContextKeys.DATE] = t.CollectedAt;
                }));
            }

            logger.LogInformation("Completed processing game world {gameWorldId}", gameWorld.Id);
        }

        logger.LogInformation("Total player aggregates: {count}, Total alliance aggregates: {count}",
            playerAggregates.Count, allianceAggregates.Count);

        return (playerAggregates, allianceAggregates, allConfirmedAllianceMembers);
    }

    private async Task<List<(DateTime CollectedAt, Wakeup Wakeup)>> GetWakeupsAsync(string partitionKey,
        int wakeupPage = -1)
    {
        var rawData = await ExecuteSafeAsync(async () =>
        {
            if (wakeupPage >= 0)
            {
                var result = await InGameRawDataTableRepository.GetAsync(partitionKey, WAKEUP_BATCH_SIZE * wakeupPage,
                    WAKEUP_BATCH_SIZE);
                if (result.Count >= WAKEUP_BATCH_SIZE)
                {
                    HasMoreWakeupData = true;
                }

                return result;
            }

            return await InGameRawDataTableRepository.GetAllAsync(partitionKey);
        }, "", []);
        var alliances = new List<(DateTime CollectedAt, Wakeup Wakeup)>();
        foreach (var rd in rawData)
        {
            try
            {
                alliances.Add((rd.CollectedAt, InGameDataParsingService.ParseWakeup(rd.Base64Data)));
            }
            catch (Exception e)
            {
                logger.LogError(e, "Error parsing wakeup raw data collected on {CollectedAt}", rd.CollectedAt);
            }
        }

        return alliances;
    }

    private async Task<List<(DateTime CollectedAt, PlayerRank PlayerRank)>> GetPlayerRanking(string worldId,
        DateOnly date, PlayerRankingType playerRankingType)
    {
        var playerRankingRawData = await ExecuteSafeAsync(
            () => InGameRawDataTableRepository.GetAllAsync(
                InGameRawDataTablePartitionKeyProvider.PlayerRankings(worldId, date, playerRankingType)),
            $"Error getting player raw data for world {worldId} on {date} for playerRankingType {playerRankingType}",
            []);
        var rankings = new List<(DateTime CollectedAt, PlayerRank PlayerRank)>();
        foreach (var rawData in playerRankingRawData)
        {
            var ranksResult = InGameDataParsingService.ParsePlayerRanking(rawData.Base64Data);
            if (ranksResult.IsFailed)
            {
                ranksResult.Log<AutoDataProcessorBase>(LogLevel.Error);
                logger.LogError(null, "Error parsing player raw data collected on {date}", rawData.CollectedAt);
            }

            rankings.AddRange(ranksResult.Value.Top100.Select(pr => (rawData.CollectedAt, pr)));
            rankings.AddRange(ranksResult.Value.SurroundingRanking.Select(pr => (rawData.CollectedAt, pr)));
        }

        return rankings;
    }

    private async Task<List<(DateTime CollectedAt, AllianceRank AllianceRank)>> GetAllianceRanking(string worldId,
        DateOnly date, AllianceRankingType allianceRankingType)
    {
        var allianceRankingRawData = await ExecuteSafeAsync(
            () => InGameRawDataTableRepository.GetAllAsync(
                InGameRawDataTablePartitionKeyProvider.AllianceRankings(worldId, date, allianceRankingType)), "", []);
        var rankings = new List<(DateTime CollectedAt, AllianceRank AllianceRank)>();
        foreach (var rawData in allianceRankingRawData)
        {
            try
            {
                var ranks = InGameDataParsingService.ParseAllianceRankings(rawData.Base64Data);
                rankings.AddRange(ranks.Top100.Select(pr => (rawData.CollectedAt, pr)));
                rankings.AddRange(ranks.SurroundingRanking.Select(pr => (rawData.CollectedAt, pr)));
            }
            catch (Exception e)
            {
                logger.LogError(e,
                    "Error parsing alliance raw data collected on {date} for allianceRankingType {AllianceRankingType}",
                    rawData.CollectedAt, allianceRankingType);
            }
        }

        return rankings;
    }

    private async Task<List<(DateTime CollectedAt, PvpRank PvpRank)>> GetPvpRanking(string worldId, DateOnly date)
    {
        var pvpRankingRawData = await ExecuteSafeAsync(
            () => InGameRawDataTableRepository.GetAllAsync(
                InGameRawDataTablePartitionKeyProvider.PvpRankings(worldId, date)), "", []);
        var rankings = new List<(DateTime CollectedAt, PvpRank PvpRank)>();
        foreach (var rawData in pvpRankingRawData)
        {
            try
            {
                rankings.AddRange(InGameDataParsingService.ParsePvpRanking(rawData.Base64Data)
                    .Select(pr => (rawData.CollectedAt, pr)));
            }
            catch (Exception e)
            {
                logger.LogError(e, "Error parsing pvp raw data collected on {date}", rawData.CollectedAt);
            }
        }

        return rankings;
    }
}
