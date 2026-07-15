using Ingweland.Fog.Models.Fog.Enums;

namespace Ingweland.Fog.Application.Client.Web.ViewModels;

public class WoaPointsCategoryViewModel
{
    public required WoaPointsCategory Category { get; init; }
    public required string IconUrl { get; init; }
    public required string Name { get; init; }
}
