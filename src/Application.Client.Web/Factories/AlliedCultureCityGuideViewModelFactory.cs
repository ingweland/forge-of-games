using AutoMapper;
using Ingweland.Fog.Application.Client.Web.Factories.Interfaces;
using Ingweland.Fog.Application.Client.Web.ViewModels;
using Ingweland.Fog.Application.Client.Web.ViewModels.Hoh.City;
using Ingweland.Fog.Dtos.Hoh;
using Ingweland.Fog.Dtos.Hoh.City;

namespace Ingweland.Fog.Application.Client.Web.Factories;

public class AlliedCultureCityGuideViewModelFactory(
    IMapper mapper) : IAlliedCultureCityGuideViewModelFactory
{
    public AlliedCultureCityGuideViewModel Create(CommunityCityStrategyDto dto, WonderBasicDto wonderDto)
    {
        return new AlliedCultureCityGuideViewModel
        {
            SharedDataId = dto.SharedDataId,
            Wonder = mapper.Map<WonderBasicViewModel>(wonderDto),
            CityId = dto.CityId,
        };
    }
}
