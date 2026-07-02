using Ingweland.Fog.Models.Hoh.Enums;

namespace Ingweland.Fog.Application.Core.Extensions;

public static class BattleTypeExtensions
{
    private static readonly Dictionary<BattleType, int> SortOrder = new()
    {
        {BattleType.Pvp, 1},
        {BattleType.Campaign, 2},
        {BattleType.TreasureHunt, 3},
        {BattleType.HistoricBattle, 4},
        {BattleType.TeslaStorm, 5},
        {BattleType.BattleEvent, 6},
    };

    public static BattleType ToBattleType(this string src)
    {
        if (string.IsNullOrWhiteSpace(src))
        {
            return BattleType.Undefined;
        }

        var lowerSrc = src.ToLowerInvariant();

        if (lowerSrc.StartsWith(nameof(RegionId.SiegeOfOrleans), StringComparison.InvariantCultureIgnoreCase) ||
            lowerSrc.StartsWith(nameof(RegionId.SpartasLastStand), StringComparison.InvariantCultureIgnoreCase) ||
            lowerSrc.StartsWith(nameof(RegionId.FallOfTroy), StringComparison.InvariantCultureIgnoreCase))
        {
            return BattleType.HistoricBattle;
        }

        if (lowerSrc.StartsWith("encounter_", StringComparison.InvariantCultureIgnoreCase))
        {
            return BattleType.TreasureHunt;
        }

        if (lowerSrc.StartsWith("teslastorm", StringComparison.InvariantCultureIgnoreCase))
        {
            return BattleType.TeslaStorm;
        }

        if (lowerSrc.StartsWith("pvp", StringComparison.InvariantCultureIgnoreCase))
        {
            return BattleType.Pvp;
        }

        if (lowerSrc.StartsWith(nameof(RegionId.AncientEgyptDungeon), StringComparison.InvariantCultureIgnoreCase) ||
            lowerSrc.StartsWith(nameof(RegionId.ScyllaDungeon), StringComparison.InvariantCultureIgnoreCase))
        {
            return BattleType.BattleEvent;
        }
        
        if (lowerSrc.StartsWith("elite_arena", StringComparison.InvariantCultureIgnoreCase))
        {
            return BattleType.EliteArena;
        }

        return BattleType.Campaign;
    }

    public static int GetSortOrder(this BattleType battleType)
    {
        return SortOrder.GetValueOrDefault(battleType, int.MaxValue);
    }
}
