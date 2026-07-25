using Ingweland.Fog.Models.Hoh.Enums;

namespace Ingweland.Fog.Application.Client.Web.StatsHub.ViewModels;

public class PvpEliteRankingViewModel
{
    public required DateTime CollectedAt { get; set; }
    public required EliteArenaTier Tier { get; set; }
}
