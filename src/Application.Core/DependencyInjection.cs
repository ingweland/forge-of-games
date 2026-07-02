using Ingweland.Fog.Application.Core.Calculators;
using Ingweland.Fog.Application.Core.Calculators.Interfaces;
using Ingweland.Fog.Application.Core.CityPlanner;
using Ingweland.Fog.Application.Core.CityPlanner.Abstractions;
using Ingweland.Fog.Application.Core.CityPlanner.Stats;
using Ingweland.Fog.Application.Core.CityPlanner.Stats.BuildingTypedStats;
using Ingweland.Fog.Application.Core.Factories;
using Ingweland.Fog.Application.Core.Factories.Interfaces;
using Ingweland.Fog.Application.Core.Formatters;
using Ingweland.Fog.Application.Core.Formatters.Interfaces;
using Ingweland.Fog.Application.Core.Interfaces;
using Ingweland.Fog.Application.Core.Repository;
using Ingweland.Fog.Application.Core.Repository.Abstractions;
using Ingweland.Fog.Application.Core.Services;
using Ingweland.Fog.Application.Core.Services.Hoh;
using Ingweland.Fog.Application.Core.Services.Hoh.Abstractions;
using Ingweland.Fog.Application.Core.Utils;
using Ingweland.Fog.Application.Core.Validation;
using Microsoft.Extensions.DependencyInjection;

namespace Ingweland.Fog.Application.Core;

public static class DependencyInjection
{
    public static void AddApplicationCoreServices(this IServiceCollection services)
    {
        services.AddAutoMapper(typeof(DependencyInjection).Assembly);

        services.AddSingleton<ICityCalculators, CityCalculators>();
        services.AddSingleton<ITimeFormatters, TimeFormatters>();
        services.AddSingleton<ICityExpansionsHasher, CityExpansionsHasher>();
        services.AddSingleton<IUnitStatCalculators, UnitStatCalculators>();
        services.AddSingleton<IUnitPowerCalculator, UnitPowerCalculator>();
        services.AddSingleton<IEquipmentStatsCalculators, EquipmentStatsCalculators>();
        services.AddSingleton<IHohCoreDataRepository, HohCoreDataRepository>();
        services.AddSingleton<IHohGameLocalizationDataRepository, HohGameLocalizationDataRepository>();
        services.AddSingleton<IHohDataCacheKeyFactory, HohDataCacheKeyFactory>();
        services.AddSingleton<InitCityConfigs>();
        services.AddSingleton<IHeroValidator, HeroValidator>();

        services.AddScoped<ICityPlannerDataService, CityPlannerDataService>();
        services.AddScoped<ICityMapStateCoreFactory, CityMapStateCoreFactory>();
        services.AddScoped<ICityStatsProcessorFactory, CityStatsProcessorFactory>();
        services.AddScoped<ICityStatsCalculator, CityStatsCalculator>();
        services.AddScoped<IHohCitySnapshotFactory, HohCitySnapshotFactory>();
        services.AddScoped<ICityMapEntityFactory, CityMapEntityFactory>();
        services.AddScoped<IMapAreaFactory, MapAreaFactory>();
        services.AddScoped<IProductionStatsProcessorFactory, ProductionStatsProcessorFactory>();
        services.AddScoped<ICityMapEntityStatsFactory, CityMapEntityStatsFactory>();
        services.AddScoped<IBattleDefinitionIdFactory, BattleDefinitionIdFactory>();
        services.AddScoped<IHohCityFactory, HohCityFactory>();
        services.AddScoped<IProductInfoFactory, ProductInfoFactory>();
        services.AddScoped<ICityService, CityService>();
        services.AddScoped<IBuildingTypeDtoFactory, BuildingTypeDtoFactory>();
        services.AddScoped<IBuildingGroupDtoFactory, BuildingGroupDtoFactory>();
        services.AddScoped<IWonderDtoFactory, WonderDtoFactory>();
        services.AddScoped<ICityPlannerDataFactory, CityPlannerDataFactory>();
        services.AddScoped<IHohGameLocalizationService, HohGameLocalizationService>();
        services.AddScoped<IUnitDtoFactory, UnitDtoFactory>();
        services.AddScoped<IRegionDtoFactory, RegionDtoFactory>();
        services.AddScoped<ICampaignService, CampaignService>();
        services.AddScoped<IHeroBasicDtoFactory, HeroBasicDtoFactory>();
        services.AddScoped<IHeroDtoFactory, HeroDtoFactory>();
        services.AddScoped<IHeroAbilityDtoFactory, HeroAbilityDtoFactory>();
        services.AddScoped<IUnitService, UnitService>();
        services.AddScoped<ITreasureHuntStageDtoFactory, TreasureHuntStageDtoFactory>();
        services.AddScoped<ITreasureHuntService, TreasureHuntService>();
        services.AddScoped<IResearchService, ResearchService>();
        services.AddScoped<IRelicDtoFactory, RelicDtoFactory>();
        services.AddScoped<IRelicCoreDataService, RelicCoreDataService>();
        services.AddScoped<ICommonService, CommonService>();
        services.AddScoped<IResourceDtoFactory, ResourceDtoFactory>();
        services.AddScoped<ICommandCenterService, CommandCenterService>();
        services.AddScoped<IEquipmentDataDtoFactory, EquipmentDataDtoFactory>();
        services.AddScoped<IEquipmentService, EquipmentService>();
    }
}
