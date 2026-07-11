using AutoMapper;
using Ingweland.Fog.Application.Server.Interfaces;
using Ingweland.Fog.Functions.Data;
using Ingweland.Fog.Models.Fog.Entities;
using Ingweland.Fog.Models.Hoh.Entities.Alliance;
using Ingweland.Fog.Models.Hoh.Entities.Woa;
using Ingweland.Fog.Models.Hoh.Enums;
using Ingweland.Fog.Shared.Constants;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Ingweland.Fog.Functions.Services;

public interface IAllianceWoaService
{
    Task RunAsync(WoaDivision? division, IReadOnlyCollection<WoaAlliance> woaAlliances,
        IReadOnlyCollection<HohAllianceExtended> alliances, string worldId,
        DateTime collectedAt);
}

public class AllianceWoaService(
    IFogDbContext context,
    IMapper mapper,
    IAllianceService allianceService,
    ILogger<AllianceWoaService> logger) : IAllianceWoaService
{
    public async Task RunAsync(WoaDivision? division, IReadOnlyCollection<WoaAlliance> woaAlliances,
        IReadOnlyCollection<HohAllianceExtended> alliances, string worldId,
        DateTime collectedAt)
    {
        if (woaAlliances.Count == 0 || division == null)
        {
            return;
        }

        var inGameEvent = await context.InGameEvents.FirstOrDefaultAsync(x =>
            x.WorldId == worldId && x.DefinitionId == EventDefinitionId.WoAEvent && x.EventId == division.EventId);
        if (inGameEvent == null)
        {
            logger.LogError("In-game event with key {WorldId}:{EventId} not found.", worldId, division.EventId);
            return;
        }

        var allianceAggregates = mapper.Map<List<AllianceAggregate>>(alliances, opt =>
        {
            opt.Items[ResolutionContextKeys.WORLD_ID] = worldId;
            opt.Items[ResolutionContextKeys.DATE] = collectedAt;
        });
        await allianceService.AddAsync(allianceAggregates);

        foreach (var allianceId in division.AllianceIds)
        {
            var alliance = await context.Alliances
                .Include(x =>
                    x.WoaRankings.Where(y => y.InGameEventId == inGameEvent.Id && y.DivisionId == division.DivisionId))
                .FirstOrDefaultAsync(x => x.WorldId == worldId && x.InGameAllianceId == allianceId);
            if (alliance == null)
            {
                logger.LogError("Alliance with key {WorldId}:{InGameAllianceId} not found.", worldId, allianceId);
                continue;
            }

            if (alliance.WoaRankings.Count > 1)
            {
                // this should never happen because it's enforced on db level, but let's double-check
                logger.LogError("Alliance with key {WorldId}:{InGameAllianceId} has more than one woa ranking.",
                    worldId, allianceId);
                continue;
            }

            var woaAlliance =
                woaAlliances.FirstOrDefault(x => x.AllianceId == allianceId && x.EventId == division.EventId);
            if (woaAlliance == null)
            {
                logger.LogError("Woa alliance with key {WorldId}:{InGameAllianceId} and event {EventId} not found.",
                    worldId, allianceId, division.EventId);
                continue;
            }

            if (!division.EloRatingByAllianceId.TryGetValue(allianceId, out var eloRating))
            {
                continue;
            }

            if (!division.EloDeltaByAllianceId.TryGetValue(allianceId, out var eloDelta))
            {
                continue;
            }

            if (!division.ExpectedVpShareByAllianceId.TryGetValue(allianceId, out var expectedVpShare))
            {
                continue;
            }

            var woaRanking = alliance.WoaRankings.FirstOrDefault();
            if (woaRanking == null)
            {
                woaRanking = new AllianceWoaRanking
                {
                    InGameEventId = inGameEvent.Id,
                    CollectedAt = collectedAt,
                    DivisionId = division.DivisionId,
                    EloRating = (int) Math.Round(eloRating),
                    EloDelta = (int) Math.Round(eloDelta),
                    ExpectedVictoryPointsShare = expectedVpShare,
                    VictoryPoints = (int) Math.Round(woaAlliance.VictoryPoints),
                };
                alliance.WoaRankings.Add(woaRanking);
            }
            else if (woaRanking.CollectedAt < collectedAt)
            {
                woaRanking.CollectedAt = collectedAt;
                woaRanking.EloRating = (int) Math.Round(eloRating);
                woaRanking.EloDelta = (int) Math.Round(eloDelta);
                woaRanking.ExpectedVictoryPointsShare = expectedVpShare;
                woaRanking.VictoryPoints = (int) Math.Round(woaAlliance.VictoryPoints);
            }
        }

        await context.SaveChangesAsync();
    }
}
