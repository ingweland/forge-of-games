using System.Net;
using FluentResults;
using Ingweland.Fog.Application.Core;
using Ingweland.Fog.Application.Core.Interfaces;
using Ingweland.Fog.Application.Core.Services;
using Ingweland.Fog.Application.Core.Services.Hoh.Abstractions;
using Ingweland.Fog.Application.Server.Behaviors;
using Ingweland.Fog.Application.Server.Caching;
using Ingweland.Fog.Application.Server.Factories;
using Ingweland.Fog.Application.Server.Factories.Interfaces;
using Ingweland.Fog.Application.Server.Logging;
using Ingweland.Fog.Application.Server.PlayerCity;
using Ingweland.Fog.Application.Server.PlayerCity.Abstractions;
using Ingweland.Fog.Application.Server.Providers;
using Ingweland.Fog.Application.Server.Services;
using Ingweland.Fog.Application.Server.Services.Hoh;
using Ingweland.Fog.Application.Server.Services.Hoh.Abstractions;
using Ingweland.Fog.Application.Server.Services.Interfaces;
using Ingweland.Fog.Application.Server.StatsHub.Factories;
using Ingweland.Fog.InnSdk.Hoh.Providers;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Polly;

namespace Ingweland.Fog.Application.Server;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddApplicationCoreServices();

        services.AddLazyCache();

        services.AddAutoMapper(typeof(DependencyInjection).Assembly);

        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly);
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(CachingBehavior<,>));
        });

        services.AddSingleton<InGameRawDataTablePartitionKeyProvider>();
        services.AddSingleton<InGameBinDataTablePartitionKeyProvider>();
        services.AddSingleton<IGameWorldsProvider, GameWorldsProvider>();
        services.AddSingleton<IFailedPlayerCityFetchesCache, FailedPlayerCityFetchesCache>();
        services.AddSingleton<ICacheKeyFactory, CacheKeyFactory>();
        services.AddSingleton<IHohDataCache, HohDataCache>();
        services.AddSingleton<IHohDataCacheClearingService, HohDataCacheClearingService>();
        services.AddScoped<IInGameStartupDataProcessingService, InGameStartupDataProcessingService>();
        services.AddScoped<IBarracksProfileFactory, BarracksProfileFactory>();
        services.AddScoped<ICommandCenterProfileFactory, CommandCenterProfileFactory>();
        services.AddScoped<IPlayerProfileDtoFactory, PlayerProfileDtoFactory>();
        services.AddScoped<IAllianceProfileDtoFactory, AllianceProfileDtoFactory>();
        services.AddScoped<IStatsHubService, StatsHubService>();
        services.AddScoped<IBattleService, BattleService>();
        services.AddScoped<IBattleSearchResultFactory, BattleSearchResultFactory>();
        services.AddScoped<IBattleStatsDtoFactory, BattleStatsDtoFactory>();
        services.AddScoped<IBattleQueryService, BattleQueryService>();
        services.AddScoped<IUnitBattleDtoFactory, UnitBattleDtoFactory>();
        services.AddScoped<IInGameDataParsingService, InGameDataParsingService>();
        services.AddScoped<IPlayerCityService, PlayerCityService>();
        services.AddScoped<ICityPlannerService, CityPlannerService>();
        services.AddScoped<IHohCityCreationService, HohCityCreationService>();
        services.AddScoped<IPlayerBattlesFactory, PlayerBattlesFactory>();
        services.AddScoped<IInGamePlayerService, InGamePlayerService>();
        services.AddScoped<IFogPlayerService, FogPlayerService>();
        services.AddScoped<IInGameAllianceService, InGameAllianceService>();
        services.AddScoped<IFogAllianceService, FogAllianceService>();
        services.AddScoped<IAllianceUpdateOrchestrator, AllianceUpdateOrchestrator>();
        services.AddScoped<IHeroInsightsService, HeroInsightsService>();
        services.AddScoped<IEquipmentInsightsService, EquipmentInsightsService>();
        services.AddScoped<IRelicService, RelicService>();
        services.AddScoped<IAllianceAthRankingDtoFactory, AllianceAthRankingDtoFactory>();
        services.AddScoped<IAllianceWoaRankingDtoFactory, AllianceWoaRankingDtoFactory>();
        services.AddScoped<IFogRankingService, FogRankingService>();
        services.AddScoped<IRankingUpdateOrchestrator, RankingUpdateOrchestrator>();
        services.AddScoped<IFogCommonService, FogCommonService>();
        services.AddScoped<IInGameEventService, InGameEventService>();
        services.AddScoped<IWonderRankingDtoFactory, WonderRankingDtoFactory>();
        services.AddScoped<IPlayerCityStrategyInfoDtoFactory, PlayerCityStrategyInfoDtoFactory>();
        services.AddScoped<ICommunityCityStrategyService, CommunityCityStrategyService>();
        services.AddScoped<IPlayerAthRankingDtoFactory, PlayerAthRankingDtoFactory>();
        services.AddScoped<IHeroAbilityService, HeroAbilityService>();

        services.AddTransient<IResultLogger, ResultLogger>();

        services.AddHttpClient<IWikipediaService, WikipediaService>(client =>
            {
                client.DefaultRequestHeaders.UserAgent.ParseAdd("ForgeOfGames/1.0");
            })
            .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
            {
                AutomaticDecompression = DecompressionMethods.GZip,
            })
            .AddStandardResilienceHandler(options =>
            {
                options.Retry.BackoffType = DelayBackoffType.Exponential;
                options.Retry.MaxRetryAttempts = 3;
            });

        return services;
    }
}
