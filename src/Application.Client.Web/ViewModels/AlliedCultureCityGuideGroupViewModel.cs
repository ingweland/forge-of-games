using Ingweland.Fog.Models.Hoh.Enums;

namespace Ingweland.Fog.Application.Client.Web.ViewModels;

public class AlliedCultureCityGuideGroupViewModel
{
    public required CityId CityId { get; init; }
    public required string CityName { get; init; }
    public required IReadOnlyCollection<AlliedCultureCityGuideViewModel> Guides { get; init; }
}
