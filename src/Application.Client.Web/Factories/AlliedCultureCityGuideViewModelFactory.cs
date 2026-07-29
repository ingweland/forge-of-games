using AutoMapper;
using Ingweland.Fog.Application.Client.Core.Localization;
using Ingweland.Fog.Application.Client.Web.Factories.Interfaces;
using Ingweland.Fog.Application.Client.Web.ViewModels;
using Ingweland.Fog.Application.Client.Web.ViewModels.Hoh.City;
using Ingweland.Fog.Dtos.Hoh;
using Ingweland.Fog.Dtos.Hoh.City;
using Microsoft.Extensions.Localization;

namespace Ingweland.Fog.Application.Client.Web.Factories;

public class AlliedCultureCityGuideViewModelFactory(
    IMapper mapper,
    IStringLocalizer<FogResource> loc) : IAlliedCultureCityGuideViewModelFactory
{
    public AlliedCultureCityGuideViewModel Create(CommunityCityStrategyDto dto, WonderBasicDto wonderDto,
        string? premiumHelpPagePath = null)
    {
        var wonder = mapper.Map<WonderBasicViewModel>(wonderDto);
        return new AlliedCultureCityGuideViewModel
        {
            SharedDataId = dto.SharedDataId,
            Wonder = wonder,
            CityId = dto.CityId,
            DisplayName = premiumHelpPagePath == null
                ? wonder.Name
                : $"{wonder.Name} {loc[FogResource.Common_Premium]}",
            PremiumHelpPagePath = premiumHelpPagePath,
        };
    }
}
