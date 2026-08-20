using System.Diagnostics;
using FluentResults;
using Ingweland.Fog.Application.Server.Interfaces;
using Ingweland.Fog.Application.Server.Services.Interfaces;
using Ingweland.Fog.Models.Fog.Entities;
using Ingweland.Fog.Models.Fog.Enums;
using Ingweland.Fog.Models.Hoh.Constants;
using Ingweland.Fog.Shared.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Ingweland.Fog.Functions.Services;

public interface ITopHeroInsightsProcessor
{
    Task RunAsync();
}

public class TopHeroInsightsProcessor(
    IFogDbContext context,
    IHeroInsightsService heroInsightsService,
    ILogger<TopHeroInsightsProcessor> logger) : ITopHeroInsightsProcessor
{
    private static readonly IReadOnlySet<string> Ages = new HashSet<string>
    {
        AgeIds.BRONZE_AGE,
        AgeIds.MINOAN_ERA,
        AgeIds.CLASSIC_GREECE,
        AgeIds.EARLY_ROME,
        AgeIds.ROMAN_EMPIRE,
        AgeIds.BYZANTINE_ERA,
        AgeIds.AGE_OF_THE_FRANKS,
        AgeIds.FEUDAL_AGE,
        AgeIds.IBERIAN_ERA,
        AgeIds.KINGDOM_OF_SICILY,
        AgeIds.HIGH_MIDDLE_AGE,
        AgeIds.EARLY_GOTHIC_ERA,
        AgeIds.LATE_GOTHIC_ERA,
    };

    private static readonly IReadOnlyCollection<HeroLevelRange> LevelRanges =
        [new(0, 59), new(60, 79), new(80, 99), new(100, 119), new(120, int.MaxValue)];

    public async Task RunAsync()
    {
        var results = new List<Result>();
        results.Add(await Process(HeroInsightsMode.Top));

        foreach (var age in Ages)
        {
            results.Add(await Process(HeroInsightsMode.Top, age));
        }

        results.Add(await Process(HeroInsightsMode.MostPopular));
        foreach (var age in Ages)
        {
            results.Add(await Process(HeroInsightsMode.MostPopular, age));
        }

        foreach (var levelRange in LevelRanges)
        {
            results.Add(await Process(HeroInsightsMode.MostPopular, null, levelRange.From, levelRange.To));
        }

        foreach (var age in Ages)
        {
            foreach (var levelRange in LevelRanges)
            {
                results.Add(await Process(HeroInsightsMode.MostPopular, age, levelRange.From, levelRange.To));
            }
        }

        results.Add(await Process(HeroInsightsMode.PlayersTop100));
        results.Add(await Process(HeroInsightsMode.PlayersTop500));
        results.Add(await Process(HeroInsightsMode.PlayersTop1000));
        results.Add(await Process(HeroInsightsMode.PlayersTop5000));
        results.Add(await Process(HeroInsightsMode.PlayersTop10000));

        var finalResult = results.Merge();
        if (finalResult.IsFailed)
        {
            throw new Exception("There were errors while processing top hero insights. Check inner logs for details.");
        }
    }

    private async Task<Result> Process(HeroInsightsMode mode, string? age = null, int? from = null, int? to = null)
    {
        var today = DateTime.UtcNow.ToDateOnly();
        var existingInsights = await context.TopHeroInsights.FirstOrDefaultAsync(x =>
            x.AgeId == age && x.Mode == mode && x.CreatedAt == today && x.FromLevel == from &&
            x.ToLevel == to);

        if (existingInsights != null)
        {
            logger.LogInformation("Skipping: mode - {mode}, age - {age}, from = {from}, to = {to}.", mode, age, from,
                to);
            return Result.Ok();
        }

        var sw = new Stopwatch();
        logger.LogInformation(
            "Processing: mode - {mode}, age - {age}, from = {from}, to = {to}.", mode, age, from, to);
        sw.Restart();
        var insightsResult = await heroInsightsService.GetAsync(mode, age, from, to, CancellationToken.None);
        sw.Stop();
        insightsResult.LogIfFailed<TopHeroInsightsProcessor>();
        if (insightsResult.IsFailed)
        {
            return insightsResult.ToResult();
        }

        context.TopHeroInsights.Add(new TopHeroInsightsEntity
        {
            Mode = mode,
            AgeId = age,
            CreatedAt = today,
            FromLevel = from,
            ToLevel = to,
            Heroes = insightsResult.Value.ToHashSet(),
        });

        await context.SaveChangesAsync();
        logger.LogInformation(
            "Processed: mode - {mode}, age - {age}, from = {from}, to = {to}. Elapsed: {elapsed}", mode, age,
            from, to, sw.Elapsed);

        return Result.Ok();
    }
}
