using Ingweland.Fog.Shared.Utils;

namespace Ingweland.Fog.Functions.Validators;

public class EndpointValidator
{
    private static readonly IReadOnlyDictionary<string, HashSet<string>> GameEndpointsToCollectionCategoryIdsMap =
        new Dictionary<string, HashSet<string>>
        {
            {"game/wakeup", ["alliance", "leaderboards", "woa", "heroes"]},
            {"game/ranking/player", ["leaderboards"]},
            {"game/ranking/alliance", ["leaderboards"]},
            {"game/pvp/get-battle-history", ["pvpBattles"]},
            {"game/battle/hero/stats", ["battleStats"]},
            {"game/battle/hero/complete-wave", ["battles"]},
            {"game/battle/hero/start", ["battles"]},
            {"game/woa/get-player-statistics", ["woa"]},
            {"game/startup", ["heroes"]},
        };

    public bool ValidateEndpoint(string responseUrl, IEnumerable<string> collectionCategoryIds, out string errorMessage)
    {
        errorMessage = string.Empty;
        var path = UriUtils.GetPath(responseUrl);
        if (!GameEndpointsToCollectionCategoryIdsMap.TryGetValue(path, out var allowedCategoryIds))
        {
            errorMessage = $"Path {path} not mapped.";
            return false;
        }

        foreach (var categoryId in collectionCategoryIds)
        {
            if (!allowedCategoryIds.Contains(categoryId))
            {
                errorMessage = $"Received data does not match collection category id {categoryId}.";
                return false;
            }
        }

        return true;
    }
}
