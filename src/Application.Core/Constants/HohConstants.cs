namespace Ingweland.Fog.Application.Core.Constants;

public static class HohConstants
{
    public const int MAX_TEAM_MEMBERS = 5;
    public const float RESEARCH_TO_PLAYER_RANKING_POINTS_FACTOR = 0.25f;
    public const float PLAYER_TOTAL_TO_ALLIANCE_RANKING_POINTS_FACTOR = 0.1f;
    public const int WONDER_MAX_LEVEL = 50;

    public static readonly int[] CapitalPremiumExpansionCost =
        [190, 290, 490, 590, 690, 890, 990, 1500, 2000, 2500, 3000, 4000, 5000, 6000, 7000, 8000, 9000, 10000];

    public static readonly int[] WoaBracketTierThresholds = [0, 1000, 1800, 2500, 3200, 4000];
}
