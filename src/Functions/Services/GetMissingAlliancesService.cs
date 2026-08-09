using AutoMapper;
using AutoMapper.QueryableExtensions;
using FluentResults;
using Ingweland.Fog.Application.Server.Interfaces;
using Ingweland.Fog.Application.Server.Services.Interfaces;
using Ingweland.Fog.Functions.Functions;
using Ingweland.Fog.Inn.Models.Hoh;
using Ingweland.Fog.InnSdk.Hoh.Abstractions;
using Ingweland.Fog.InnSdk.Hoh.Authentication.Models;
using Ingweland.Fog.InnSdk.Hoh.Errors;
using Ingweland.Fog.InnSdk.Hoh.Providers;
using Ingweland.Fog.Models.Fog.Entities;
using Ingweland.Fog.Models.Hoh.Entities.Alliance;
using Ingweland.Fog.Shared.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Ingweland.Fog.Functions.Services;

public interface IGetMissingAlliancesService
{
    Task RunAsync();
}

public class GetMissingAlliancesService(
    IFogDbContext context,
    IInnSdkClient innSdkClient,
    IMapper mapper,
    IGameWorldsProvider gameWorldsProvider,
    IAllianceUpdateOrchestrator allianceUpdateOrchestrator,
    ILogger<ManualTriggerFunction> logger) : IGetMissingAlliancesService
{
    private const int BLOCK_SIZE = 100;
    private const int JUMP_STEP = 5;
    private const int MISSES_BEFORE_JUMP = 2;
    private const int MAX_JUMP_PROBES = 2;
    private const int PROBE_DELAY_MS = 500;
    private const int MEMBER_UPDATE_DELAY_MS = 200;
    private const int MAX_CONSECUTIVE_FETCH_ERRORS = 10;
    private const int SCAN_FLOOR_ID = 1;

    private int _consecutiveFetchErrors;

    public async Task RunAsync()
    {
        var gw = gameWorldsProvider.GetGameWorlds().First(x => x.Id == "un1");
        var allianceIds = await context.Alliances.ProjectTo<AllianceKey>(mapper.ConfigurationProvider).ToListAsync();
        var knownIds = allianceIds.Where(x => x.WorldId == gw.Id).Select(x => x.InGameAllianceId).ToHashSet();

        _consecutiveFetchErrors = 0;

        for (var blockStart = GetBlockStart(knownIds.Max()); blockStart >= SCAN_FLOOR_ID; blockStart -= BLOCK_SIZE)
        {
            var scan = await ScanBlockAsync(gw, blockStart, knownIds);

            if (scan.Found.Count > 0)
            {
                await SaveAndUpdateMembersAsync(scan.Found, gw.Id);
            }

            logger.LogInformation(
                "==== Block {BlockStart}-{BlockEnd}:{WorldId} scanned: {ProbeCount} probes, {FoundCount} new alliances ====",
                blockStart, blockStart + BLOCK_SIZE - 1, gw.Id, scan.ProbeCount, scan.Found.Count);

            if (scan.Aborted)
            {
                logger.LogError("Aborting scan after {ErrorCount} consecutive failed requests",
                    _consecutiveFetchErrors);
                break;
            }
        }

        logger.LogInformation("DONE");
    }

    /// <summary>
    ///     Scans a single block of <see cref="BLOCK_SIZE" /> in-game ids forward, starting at <paramref name="blockStart" />.
    ///     In-game ids fill a block from its first id upward, so the scan steps one id at a time while alliances keep
    ///     turning up, and switches to <see cref="JUMP_STEP" /> strides once <see cref="MISSES_BEFORE_JUMP" /> consecutive
    ///     ids come back empty. After <see cref="MAX_JUMP_PROBES" /> strides in a row also come back empty the block is
    ///     treated as exhausted and the scan moves on. A hit at a stride target drops the scan back to single stepping
    ///     and restores the full stride allowance. The scan never crosses into the neighbouring block.
    /// </summary>
    private async Task<BlockScanResult> ScanBlockAsync(GameWorldConfig gw, int blockStart, HashSet<int> knownIds)
    {
        var blockEnd = blockStart + BLOCK_SIZE - 1;
        var found = new List<AllianceWithLeader>();
        var probeCount = 0;
        var consecutiveMisses = 0;

        for (var id = blockStart; id <= blockEnd;)
        {
            // Already ours: it exists, so keep stepping, but there is nothing to fetch.
            if (knownIds.Contains(id))
            {
                consecutiveMisses = 0;
                id++;
                continue;
            }

            await Task.Delay(PROBE_DELAY_MS);
            probeCount++;

            var probe = await GetAllianceAsync(gw, id);

            if (probe.IsSuccess)
            {
                logger.LogInformation(">>> Fetched alliance {AllianceId} for world {WorldId}", id, gw.Id);
                found.Add(probe.Value);
                _consecutiveFetchErrors = 0;
                consecutiveMisses = 0;
                id++;
                continue;
            }

            if (IsAllianceNotFound(probe))
            {
                _consecutiveFetchErrors = 0;
                consecutiveMisses++;

                // The first MISSES_BEFORE_JUMP misses are single steps, every miss after that is a stride. Once the
                // strides have used up MAX_JUMP_PROBES the rest of the block is taken to be empty.
                if (consecutiveMisses >= MISSES_BEFORE_JUMP + MAX_JUMP_PROBES)
                {
                    break;
                }

                id += consecutiveMisses < MISSES_BEFORE_JUMP ? 1 : JUMP_STEP;
                continue;
            }

            // Not a "no such alliance" answer but a failed request, so it says nothing about the block's occupancy.
            // Keep single stepping and leave the miss counter alone, otherwise a blip would cut the block short.
            logger.LogWarning("Failed to fetch alliance {AllianceId} for world {WorldId}: {Reasons}", id, gw.Id,
                string.Join("; ", probe.Errors.Select(x => x.Message)));

            if (++_consecutiveFetchErrors >= MAX_CONSECUTIVE_FETCH_ERRORS)
            {
                return new BlockScanResult(found, probeCount, true);
            }

            id++;
        }

        return new BlockScanResult(found, probeCount, false);
    }

    private async Task<Result<AllianceWithLeader>> GetAllianceAsync(GameWorldConfig gw, int allianceId)
    {
        try
        {
            return await innSdkClient.AllianceService.GetAllianceAsync(gw, allianceId);
        }
        catch (Exception e)
        {
            return Result.Fail<AllianceWithLeader>(new ExceptionalError(
                $"Unexpected failure while fetching alliance {allianceId} in world {gw.Id}", e));
        }
    }

    private static bool IsAllianceNotFound(Result<AllianceWithLeader> result)
    {
        return result.HasError<HohSoftError>(x => x.Error == SoftErrorType.AllianceNotFound);
    }

    private static int GetBlockStart(int allianceId)
    {
        return (allianceId - 1) / BLOCK_SIZE * BLOCK_SIZE + 1;
    }

    private async Task SaveAndUpdateMembersAsync(IReadOnlyCollection<AllianceWithLeader> alliances, string worldId)
    {
        await AddAlliancesAsync(alliances, worldId);
        var addedAlliances = await GetExistingAlliancesAsync(alliances.Select(x => x.Alliance.Id).ToHashSet(), worldId);
        foreach (var id in addedAlliances.Select(x => x.Id))
        {
            var delayTask = Task.Delay(MEMBER_UPDATE_DELAY_MS);
            var result = await allianceUpdateOrchestrator.UpdateMembersAsync(id, CancellationToken.None);
            result.LogIfFailed<GetMissingAlliancesService>();

            await delayTask;
        }
    }

    private async Task AddAlliancesAsync(IEnumerable<AllianceWithLeader> alliances, string worldId)
    {
        var today = DateTime.UtcNow.ToDateOnly();
        var now = DateTime.UtcNow;
        var unique = alliances
            .DistinctBy(p => p.Alliance.Id)
            .ToDictionary(p => p.Alliance.Id);
        logger.LogInformation("{ValidCount} valid alliances after filtering", unique.Count);
        var existingAlliances =
            await GetExistingAlliancesAsync(unique.Keys.ToHashSet(), worldId);
        var newAllianceKeys =
            unique.Keys.ToHashSet().Except(existingAlliances.Select(x => x.InGameAllianceId)).ToList();
        var newAlliances = newAllianceKeys.Select(k =>
        {
            var alliance = unique[k];
            return new Alliance
            {
                WorldId = worldId,
                InGameAllianceId = alliance.Alliance.Id,
                Name = alliance.Alliance.Name,
                BannerIconId = alliance.Alliance.Banner.IconId,
                BannerCrestId = alliance.Alliance.Banner.CrestId,
                BannerIconColorId = alliance.Alliance.Banner.IconColorId,
                BannerCrestColorId = alliance.Alliance.Banner.CrestColorId,
                Rank = alliance.Alliance.Rank,
                UpdatedAt = today,
                NameHistory = new List<AllianceNameHistoryEntry>
                    {new() {Name = alliance.Alliance.Name, ChangedAt = now}},
            };
        }).ToList();

        if (newAlliances.Count > 0)
        {
            context.Alliances.AddRange(newAlliances);
            await context.SaveChangesAsync();
        }

        logger.LogInformation("SaveChangesAsync completed, added {AddedAllianceCount} alliances", newAlliances.Count);
    }

    private async Task<IReadOnlyCollection<Alliance>> GetExistingAlliancesAsync(HashSet<int> inGameAllianceIds,
        string worldId)
    {
        var alliances = await context.Alliances
            .Where(p => inGameAllianceIds.Contains(p.InGameAllianceId))
            .ToListAsync();

        return alliances.Where(x => x.WorldId == worldId).ToList();
    }

    private sealed record BlockScanResult(
        IReadOnlyList<AllianceWithLeader> Found,
        int ProbeCount,
        bool Aborted);
}
