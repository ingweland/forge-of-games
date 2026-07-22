using Ingweland.Fog.Models.Fog.Enums;

namespace Ingweland.Fog.Application.Client.Web.ViewModels;

public class WoaPlayerStatsCategoryViewModel
{
    public required WoaPlayerStatsCategory Category { get; init; }
    public required string Name { get; init; }
}
