using Ingweland.Fog.Application.Server.Settings;
using Ingweland.Fog.Functions.Services;
using Ingweland.Fog.Functions.Services.Interfaces;
using Ingweland.Fog.Functions.Services.Orchestration;
using Ingweland.Fog.Functions.Validators;
using Ingweland.Fog.InnSdk.Hoh.Authentication.Models;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using BattleService = Ingweland.Fog.Functions.Services.BattleService;

namespace Ingweland.Fog.Functions;

public static class DependencyInjection
{
    public static IServiceCollection AddFunctionsServices(this IServiceCollection services)
    {
        services.AddSingleton<EndpointValidator>();
        services.AddSingleton<HohHelperResponseDtoValidator>();
        services.AddSingleton<PayloadValidator>();
        services.AddSingleton<WorldValidator>();
        services.AddSingleton<SubmissionIdValidator>();

        services.AddTransient<DatabaseWarmUpService>();

        services.AddScoped<IPvpRankingService, PvpRankingService>();
        services.AddScoped<IPlayerRankingService, PlayerRankingService>();
        services.AddScoped<IPlayerService, PlayerService>();
        services.AddScoped<IPlayerAgeHistoryService, PlayerAgeHistoryService>();
        services.AddScoped<IPlayerNameHistoryService, PlayerNameHistoryService>();
        services.AddScoped<IAllianceRankingService, AllianceRankingService>();
        services.AddScoped<IAllianceService, AllianceService>();
        services.AddScoped<IAllianceNameHistoryService, AllianceNameHistoryService>();
        services.AddScoped<IAllianceMembersService, AllianceMembersService>();
        services.AddScoped<IPvpBattleService, PvpBattleService>();
        services.AddScoped<IBattleService, BattleService>();
        services.AddScoped<IBattleStatsService, BattleStatsService>();
        services.AddScoped<IPvpBattlesBulkUpdater, PvpBattlesBulkUpdater>();
        services.AddScoped<IPlayersUpdateManager, PlayersUpdateManager>();
        services.AddScoped<ITopPlayersUpdateManager, TopPlayersUpdateManager>();
        services.AddScoped<IPlayerCityFetcher, PlayerCityFetcher>();
        services.AddScoped<ITopPlayersCityFetcher, TopPlayersCityFetcher>();
        services.AddScoped<ITopAllianceMemberProfilesUpdateManager, TopAllianceMemberProfilesUpdateManager>();
        services.AddScoped<ICultureUsageRatioUpdater, CultureUsageRatioUpdater>();
        services.AddScoped<IPlayerUpdater, PlayerUpdater>();
        services.AddScoped<ITopAllianceMemberUpdateManager, TopAllianceMemberUpdateManager>();
        services.AddScoped<ITopHeroInsightsProcessor, TopHeroInsightsProcessor>();
        services.AddScoped<IAllianceMembersUpdateManager, AllianceMembersUpdateManager>();
        services.AddScoped<IBattleTimelineService, BattleTimelineService>();
        services.AddScoped<IPlayerSquadsAnalyzer, PlayerSquadsAnalyzer>();
        services.AddScoped<IDatabaseCleanupService, DatabaseCleanupService>();
        services.AddScoped<IInGameEventsFetcher, InGameEventsFetcher>();
        services.AddScoped<IAllianceAthService, AllianceAthService>();
        services.AddScoped<IHohCoreDataFetcher, HohCoreDataFetcher>();
        services.AddScoped<IGetMissingAlliancesService, GetMissingAlliancesService>();
        services.AddScoped<IMissingPlayersVerificator, MissingPlayersVerificator>();
        services.AddScoped<IAlliancesUpdateManager, AlliancesUpdateManager>();
        services.AddScoped<ITopAlliancesUpdateManager, TopAlliancesUpdateManager>();
        services.AddScoped<IHeroAttributeFeaturesParser, HeroAttributeFeaturesParser>();
        services.AddScoped<IPlayerCitySnapshotStatsUpdater, PlayerCitySnapshotStatsUpdater>();
        services.AddScoped<IEventCityWonderRankingFetcher, EventCityWonderRankingFetcher>();
        services.AddScoped<IEventCityFetcher, EventCityFetcher>();
        services.AddScoped<IEventCityStrategyFactory, EventCityStrategyFactory>();
        services.AddScoped<IPlayerAthService, PlayerAthService>();
        services.AddScoped<IAllianceWoaService, AllianceWoaService>();

        services.AddScoped<HohHelperResponseDtoToTablePkConverter>();

        return services;
    }

    public static void AddConfigurations(this FunctionsApplicationBuilder builder)
    {
        builder.Configuration.AddUserSecrets<Program>(optional: true);
        var customEnvironment = builder.Configuration.GetValue<string>("CustomEnvironment");
        if (customEnvironment != "Development")
        {
            var connectionString = builder.Configuration.GetConnectionString("AppConfiguration") ??
                throw new InvalidOperationException("The Connection string  `AppConfiguration` was not found.");

            builder.Configuration.AddAzureAppConfiguration(options =>
            {
                // we do not set up autorefresh because functions run for a short period
                options.Connect(connectionString);
            });
        }

        IConfigurationBuilder configBuilder = builder.Configuration;
        var dynamicProvider = new DynamicConfigurationProvider();
        configBuilder.Add(new DynamicConfigurationSource(dynamicProvider));
        builder.Services.AddSingleton<IDynamicConfigurationProvider>(dynamicProvider);

        builder.Services.Configure<HohServerCredentials>(
            builder.Configuration.GetSection(HohServerCredentials.CONFIGURATION_PROPERTY_NAME));
        builder.Services.Configure<StorageSettings>(
            builder.Configuration.GetSection(StorageSettings.CONFIGURATION_PROPERTY_NAME));
        builder.Services.Configure<ResourceSettings>(
            builder.Configuration.GetSection(ResourceSettings.CONFIGURATION_PROPERTY_NAME));
    }
}
