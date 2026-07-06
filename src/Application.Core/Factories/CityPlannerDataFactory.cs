using AutoMapper;
using Ingweland.Fog.Application.Core.Extensions;
using Ingweland.Fog.Application.Core.Factories.Interfaces;
using Ingweland.Fog.Application.Core.Services.Hoh.Abstractions;
using Ingweland.Fog.Dtos.Hoh;
using Ingweland.Fog.Dtos.Hoh.City;
using Ingweland.Fog.Dtos.Hoh.CityPlanner;
using Ingweland.Fog.Models.Hoh.Entities;
using Ingweland.Fog.Models.Hoh.Entities.City;
using Ingweland.Fog.Models.Hoh.Enums;

namespace Ingweland.Fog.Application.Core.Factories;

public class CityPlannerDataFactory(
    IMapper mapper,
    IWonderDtoFactory wonderDtoFactory,
    IHohGameLocalizationService gameLocalizationService) : ICityPlannerDataFactory
{
    public CityPlannerDataDto Create(CityDefinition cityDefinition, IReadOnlyCollection<Expansion> expansions,
        IReadOnlyCollection<BuildingDto> buildings, IEnumerable<BuildingCustomization> customizations,
        IReadOnlyCollection<Age> ages, IReadOnlyCollection<Wonder> wonders,
        IReadOnlyCollection<ExpansionCosts> expansionCosts)
    {
        var wonderIds = cityDefinition.Id.GetWonders();
        return new CityPlannerDataDto
        {
            City = cityDefinition,
            Expansions = expansions,
            Buildings = buildings,
            BuildingCustomizations = mapper.Map<IReadOnlyList<BuildingCustomizationDto>>(customizations),
            Ages = mapper.Map<IReadOnlyList<AgeDto>>(ages),
            Wonders = wonders.Where(w => wonderIds.Contains(w.Id)).Select(wonderDtoFactory.Create).ToList(),
            NewCityDialogItems = CreateDialogItems(wonders),
            ExpansionCosts = expansionCosts,
        };
    }

    private List<NewCityDialogItemDto> CreateDialogItems(IReadOnlyCollection<Wonder> wonders)
    {
        var mayaWonderIds = CityId.Mayas_Tikal.GetWonders();
        var arabiaWonderIds = CityId.Arabia_Petra.GetWonders();
        var ancientEgyptWonderIds = CityId.AncientEgyptEvent.GetWonders();
        var ithakaWonderIds = CityId.Ithaka.GetWonders();
        return
        [
            new NewCityDialogItemDto
            {
                CityId = CityId.Capital,
                CityName = gameLocalizationService.GetCityName(CityId.Capital),
            },

            new NewCityDialogItemDto
            {
                CityId = CityId.Mayas_ChichenItza,
                CityName = gameLocalizationService.GetCityName(CityId.Mayas_ChichenItza),
                Wonders = wonders.Where(w => mayaWonderIds.Contains(w.Id)).Select(wonderDtoFactory.Create)
                    .ToList(),
            },

            new NewCityDialogItemDto
            {
                CityId = CityId.China,
                CityName = gameLocalizationService.GetCityName(CityId.China),
                Wonders = wonders.Where(w => w.CityId == CityId.China).Select(wonderDtoFactory.Create).ToList(),
            },

            new NewCityDialogItemDto
            {
                CityId = CityId.Vikings,
                CityName = gameLocalizationService.GetCityName(CityId.Vikings),
                Wonders = wonders.Where(w => w.CityId == CityId.Vikings).Select(wonderDtoFactory.Create).ToList(),
            },

            new NewCityDialogItemDto
            {
                CityId = CityId.Egypt,
                CityName = gameLocalizationService.GetCityName(CityId.Egypt),
                Wonders = wonders.Where(w => w.CityId == CityId.Egypt).Select(wonderDtoFactory.Create).ToList(),
            },

            new NewCityDialogItemDto
            {
                CityId = CityId.Arabia_Petra,
                CityName = gameLocalizationService.GetCityName(CityId.Arabia_Petra),
                Wonders = wonders.Where(w => arabiaWonderIds.Contains(w.Id)).Select(wonderDtoFactory.Create)
                    .ToList(),
            },
            new NewCityDialogItemDto
            {
                CityId = CityId.AncientEgyptEvent,
                CityName = gameLocalizationService.GetCityName(CityId.AncientEgyptEvent),
                Wonders = wonders.Where(w => ancientEgyptWonderIds.Contains(w.Id)).Select(wonderDtoFactory.Create)
                    .ToList(),
            },
            new NewCityDialogItemDto
            {
                CityId = CityId.Ithaka,
                CityName = gameLocalizationService.GetCityName(CityId.Ithaka),
                Wonders = wonders.Where(w => ithakaWonderIds.Contains(w.Id)).Select(wonderDtoFactory.Create)
                    .ToList(),
            },
        ];
    }
}
