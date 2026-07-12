using Ingweland.Fog.Application.Core.Repository.Abstractions;
using Ingweland.Fog.Application.Server.Interfaces;
using Ingweland.Fog.Models.Fog.Entities;
using Microsoft.EntityFrameworkCore;

namespace Ingweland.Fog.Functions.Services;

public interface IPlayerHeroesService
{
    Task RunAsync(IReadOnlyDictionary<int, HashSet<string>> units,
        IReadOnlyDictionary<int, HashSet<string>> heroes, string worldId);
}

public class PlayerHeroesService(
    IFogDbContext context,
    IPlayerService playerService,
    IHohCoreDataRepository hohCoreDataRepository) : IPlayerHeroesService
{
    public async Task RunAsync(IReadOnlyDictionary<int, HashSet<string>> units,
        IReadOnlyDictionary<int, HashSet<string>> heroes, string worldId)
    {
        if (units.Count == 0 && heroes.Count == 0)
        {
            return;
        }

        var inGamePlayerIds = units.Keys.Union(heroes.Keys).ToHashSet();
        var failedPlayerIds = await playerService.AddMissingPlayersAsync(inGamePlayerIds, worldId);
        var targetPlayerIds = inGamePlayerIds.Except(failedPlayerIds).ToHashSet();
        var players = await GetExistingPlayersAsync(targetPlayerIds, worldId);
        var heroesToUnitIds = (await hohCoreDataRepository.GetHeroesAsync()).ToDictionary(x => x.Id, x => x.UnitId);
        foreach (var player in players)
        {
            var unitIds = units.GetValueOrDefault(player.InGamePlayerId, []);
            var heroIds = heroes.GetValueOrDefault(player.InGamePlayerId, []);
            foreach (var heroId in heroIds)
            {
                if (heroesToUnitIds.TryGetValue(heroId, out var unitId))
                {
                    unitIds.Add(unitId);
                }
            }

            var existingUnits = player.Heroes.Select(x => x.UnitId).ToHashSet();
            var newUnits = unitIds.Except(existingUnits);
            foreach (var unitId in newUnits)
            {
                player.Heroes.Add(new PlayerHeroEntity {UnitId = unitId});
            }
        }

        await context.SaveChangesAsync();
    }

    private async Task<IReadOnlyCollection<Player>> GetExistingPlayersAsync(IReadOnlySet<int> inGamePlayerIds,
        string worldId)
    {
        return await context.Players
            .Include(x => x.Heroes)
            .Where(p => inGamePlayerIds.Contains(p.InGamePlayerId) && p.WorldId == worldId)
            .ToListAsync();
    }
}
