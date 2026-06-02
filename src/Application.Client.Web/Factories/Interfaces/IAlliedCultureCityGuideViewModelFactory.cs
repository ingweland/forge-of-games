using Ingweland.Fog.Application.Client.Web.ViewModels;
using Ingweland.Fog.Dtos.Hoh;
using Ingweland.Fog.Dtos.Hoh.City;

namespace Ingweland.Fog.Application.Client.Web.Factories.Interfaces;

public interface IAlliedCultureCityGuideViewModelFactory
{
    AlliedCultureCityGuideViewModel Create(CommunityCityStrategyDto dto, WonderBasicDto wonderDto);
}
