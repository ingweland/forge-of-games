using Ingweland.Fog.Application.Client.Web.ViewModels.Hoh.City;
using Ingweland.Fog.Models.Hoh.Enums;

namespace Ingweland.Fog.Application.Client.Web.ViewModels;

public class AlliedCultureCityGuideViewModel
{
    public required CityId CityId { get; init; }
    public required string SharedDataId { get; init; }
    public required WonderBasicViewModel Wonder { get; init; }
}
