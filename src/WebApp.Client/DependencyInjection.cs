using System.Text.Json;
using System.Text.Json.Serialization;
using Ingweland.Fog.Application.Client.Web.EquipmentConfigurator.Abstractions;
using Ingweland.Fog.Application.Client.Web.Models;
using Ingweland.Fog.Application.Client.Web.Services.Abstractions;
using Ingweland.Fog.Application.Client.Web.Settings;
using Ingweland.Fog.Application.Core.Repository.Abstractions;
using Ingweland.Fog.Application.Core.Services;
using Ingweland.Fog.Application.Core.Services.Hoh.Abstractions;
using Ingweland.Fog.Shared.Helpers;
using Ingweland.Fog.WebApp.Client.Net;
using Ingweland.Fog.WebApp.Client.Repositories;
using Ingweland.Fog.WebApp.Client.Repositories.Abstractions;
using Ingweland.Fog.WebApp.Client.Services;
using Ingweland.Fog.WebApp.Client.Services.Abstractions;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Polly;
using Refit;
using IHohDataService = Ingweland.Fog.Application.Client.Web.Services.Hoh.Abstractions.IHohDataService;

namespace Ingweland.Fog.WebApp.Client;

internal static class DependencyInjection
{
    public static void AddWebAppClientServices(this IServiceCollection services, string baseAddress)
    {
        services.AddAutoMapper(typeof(DependencyInjection).Assembly);

        services.AddSingleton<IHohDataProvider, IndexedDbHohDataProvider>();
        services.AddSingleton<IHohLocalizationDataProvider, IndexedDbHohLocalizationDataProvider>();
        services.AddSingleton<IClientLocaleService, ClientLocaleService>();
        services.AddSingleton<IHohDataInitializationService, HohDataInitializationService>();
        services.AddSingleton<IFogLocalDbContext, FogLocalDbContext>();

        services.AddScoped<CityPlannerNavigationState>();
        services.AddScoped<IJSInteropService, JSInteropService>();
        services.AddScoped<IClipboardService, ClipboardService>();
        services.AddScoped<IPersistenceService, PersistenceService>();
        services.AddScoped<IMainMenuService, MainMenuService>();
        services.AddScoped<IPageMetadataService, PageMetadataService>();
        services.AddScoped<ILocalStorageBackupService, LocalStorageBackupService>();
        services.AddScoped<IEquipmentProfilePersistenceService, EquipmentProfilePersistenceService>();
        services.AddScoped<AppBarService>();
        services.AddScoped<CityStrategyNavigationState>();
        services.AddScoped<IAdSenseService, AdSenseService>();

        var refitSettings = new RefitSettings
        {
            ContentSerializer = new ProtobufHttpContentSerializer(new ProtobufSerializer()),
        };

        var refitJsonSettings = new RefitSettings
        {
            ContentSerializer = new SystemTextJsonContentSerializer(GetDefaultJsonSerializerOptions()),
        };
        AddRefitJsonApiClient<ICommandCenterSharingService>(services, baseAddress, refitJsonSettings);
        AddRefitJsonApiClient<IInGameStartupDataService>(services, baseAddress, refitJsonSettings);
        AddRefitJsonApiClient<ICityPlannerSharingService>(services, baseAddress, refitJsonSettings);
        AddRefitJsonApiClient<IStatsHubService>(services, baseAddress, refitJsonSettings);
        AddRefitJsonApiClient<IWikipediaService>(services, baseAddress, refitJsonSettings);
        AddRefitJsonApiClient<IBattleService>(services, baseAddress, refitJsonSettings);
        AddRefitJsonApiClient<ICityPlannerService>(services, baseAddress, refitJsonSettings);
        AddRefitJsonApiClient<IEquipmentInsightsService>(services, baseAddress, refitJsonSettings);
        AddRefitJsonApiClient<IRelicService>(services, baseAddress, refitJsonSettings);
        AddRefitJsonApiClient<IFogCommonService>(services, baseAddress, refitJsonSettings);
        AddRefitJsonApiClient<IInGameEventService>(services, baseAddress, refitJsonSettings);
        AddRefitJsonApiClient<IFogSharingService>(services, baseAddress, refitJsonSettings, "api");
        AddRefitJsonApiClient<ICommunityCityStrategyService>(services, baseAddress, refitJsonSettings, "api");
        AddRefitJsonApiClient<ISharedImageUploaderService>(services, baseAddress, refitJsonSettings, "api");
        AddRefitJsonApiClient<IHohDataService>(services, baseAddress, refitJsonSettings);
        AddRefitJsonApiClient<IHeroAbilityService>(services, baseAddress, refitJsonSettings);
    }

    private static void AddRefitProtobufApiClient<T>(IServiceCollection services, string baseAddress,
        RefitSettings settings)
        where T : class
    {
        services
            .AddRefitClient<T>(settings)
            .ConfigureHttpClient(client =>
            {
                client.BaseAddress = new Uri($"{baseAddress}api/hoh");
                client.DefaultRequestHeaders.Add("Accept", "application/x-protobuf");
                client.DefaultRequestHeaders.Add("Accept-Encoding", "gzip");
            })
            .AddStandardResilienceHandler(options =>
            {
                options.Retry.BackoffType = DelayBackoffType.Exponential;
                options.Retry.MaxRetryAttempts = 3;
                options.AttemptTimeout.Timeout = TimeSpan.FromSeconds(30);
                options.TotalRequestTimeout.Timeout = TimeSpan.FromSeconds(120);
                options.CircuitBreaker.SamplingDuration = TimeSpan.FromSeconds(60);
            });
    }

    private static void AddRefitJsonApiClient<T>(IServiceCollection services, string baseAddress,
        RefitSettings settings, string group = "api/hoh")
        where T : class
    {
        services
            .AddRefitClient<T>(settings)
            .ConfigureHttpClient(client => { client.BaseAddress = new Uri($"{baseAddress}{group}"); })
            .AddStandardResilienceHandler(options =>
            {
                options.Retry.BackoffType = DelayBackoffType.Exponential;
                options.Retry.MaxRetryAttempts = 3;
                options.AttemptTimeout.Timeout = TimeSpan.FromSeconds(15);
                options.TotalRequestTimeout.Timeout = TimeSpan.FromSeconds(60);
            });
    }

    public static void AddWebAppClientSettings(this WebAssemblyHostBuilder builder)
    {
        builder.Services.Configure<AssetsSettings>(
            builder.Configuration.GetSection("AssetsSettings"));
    }

    private static JsonSerializerOptions GetDefaultJsonSerializerOptions()
    {
        var jsonSerializerOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault,
        };
        jsonSerializerOptions.Converters.Add(new ObjectToInferredTypesConverter());

        return jsonSerializerOptions;
    }
}
