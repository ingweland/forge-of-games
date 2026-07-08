using Ingweland.Fog.Application.Client.Web.EquipmentConfigurator.Abstractions;
using Ingweland.Fog.Application.Client.Web.Models;
using Ingweland.Fog.Application.Client.Web.Services.Abstractions;
using Ingweland.Fog.Application.Client.Web.Settings;
using Ingweland.Fog.Application.Core.Services.Hoh.Abstractions;
using Ingweland.Fog.Application.Server.Settings;
using Ingweland.Fog.InnSdk.Hoh.Authentication.Models;
using Ingweland.Fog.WebApp.Apis;
using Ingweland.Fog.WebApp.Client.Services;
using Ingweland.Fog.WebApp.Client.Services.Abstractions;
using Ingweland.Fog.WebApp.Services;
using Ingweland.Fog.WebApp.Startup;
using Ingweland.Fog.WebApp.Startup.Interfaces;

namespace Ingweland.Fog.WebApp;

internal static class DependencyInjection
{
    public static void AddWebAppServices(this IHostApplicationBuilder builder)
    {
        var services = builder.Services;

        services.AddAutoMapper(typeof(DependencyInjection).Assembly);

        services.AddSingleton<IStartupTask, PreloadDataStartupTask>();
        services.AddSingleton<IProtobufResponseFactory, ProtobufResponseFactory>();
        services.AddSingleton<IClientLocaleService, DummyClientLocaleService>();

        services.AddHostedService<StartupTaskHostedService>();

        services.AddScoped<IPersistenceService, DummyPersistenceService>();
        services.AddScoped<IInGameStartupDataService, DummyInGameStartupDataService>();
        services.AddScoped<IJSInteropService, DummyJSInteropService>();
        services.AddScoped<ICityPlannerSharingService, DummyCityPlannerSharingService>();
        services.AddScoped<ICommandCenterSharingService, DummyCommandCenterSharingService>();
        services.AddScoped<ILocalStorageBackupService, DummyLocalStorageBackupService>();
        services.AddScoped<IFogSharingService, DummyFogSharingService>();
        services.AddScoped<IEquipmentProfilePersistenceService, DummyEquipmentProfilePersistenceService>();
        services.AddScoped<ISharedImageUploaderService, DummySharedImageUploaderService>();
        services.AddScoped<IAdSenseService, DummyAdSenseService>();

        services.AddScoped<CityPlannerNavigationState>();
        services.AddScoped<IMainMenuService, MainMenuService>();
        services.AddScoped<IPageMetadataService, PageMetadataService>();
        services.AddScoped<AppBarService>();
        services.AddScoped<CityStrategyNavigationState>();
    }

    public static void AddWebAppSettings(this IHostApplicationBuilder builder)
    {
        if (!builder.Environment.IsDevelopment())
        {
            var connectionString = builder.Configuration.GetConnectionString("AppConfiguration") ??
                throw new InvalidOperationException("The Connection string  `AppConfiguration` was not found.");

            var refreshInterval = builder.Environment.IsStaging() ? TimeSpan.FromMinutes(2) : TimeSpan.FromMinutes(10);
            builder.Configuration.AddAzureAppConfiguration(options =>
            {
                options.Connect(connectionString)
                    .Select($"{ResourceSettings.CONFIGURATION_PROPERTY_NAME}:*")
                    .Select($"{MaintenanceModeSettings.CONFIGURATION_PROPERTY_NAME}:*")
                    .Select($"{DefaultDomainRestrictionSettings.CONFIGURATION_PROPERTY_NAME}:*")
                    .Select("Logging:LogLevel:*")
                    .ConfigureRefresh(refreshOptions =>
                    {
                        refreshOptions
                            .Register("RefreshSentinel", true)
                            .SetRefreshInterval(refreshInterval);
                    });
            });

            builder.Services.AddAzureAppConfiguration();
        }

        builder.Services.Configure<AssetsSettings>(
            builder.Configuration.GetSection(AssetsSettings.CONFIGURATION_PROPERTY_NAME));

        builder.Services.Configure<ResourceSettings>(
            builder.Configuration.GetSection(ResourceSettings.CONFIGURATION_PROPERTY_NAME));

        builder.Services.Configure<StorageSettings>(
            builder.Configuration.GetSection(StorageSettings.CONFIGURATION_PROPERTY_NAME));

        builder.Services.Configure<HohServerCredentials>(
            builder.Configuration.GetSection(HohServerCredentials.CONFIGURATION_PROPERTY_NAME));

        builder.Services.Configure<MaintenanceModeSettings>(
            builder.Configuration.GetSection(MaintenanceModeSettings.CONFIGURATION_PROPERTY_NAME));
        
        builder.Services.Configure<DefaultDomainRestrictionSettings>(
            builder.Configuration.GetSection(DefaultDomainRestrictionSettings.CONFIGURATION_PROPERTY_NAME));
    }
}
