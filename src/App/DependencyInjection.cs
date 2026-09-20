using System.Text.Json;
using System.Text.Json.Serialization;
using Ingweland.Fog.App.Repositories;
using Ingweland.Fog.App.Services;
using Ingweland.Fog.App.Services.Abstractions;
using Ingweland.Fog.App.Views;
using Ingweland.Fog.Application.Client.Web;
using Ingweland.Fog.Application.Client.Web.Analytics.Interfaces;
using Ingweland.Fog.Application.Client.Web.CityPlanner.Abstractions;
using Ingweland.Fog.Application.Client.Web.CityPlanner.Rendering;
using Ingweland.Fog.Application.Client.Web.EquipmentConfigurator.Abstractions;
using Ingweland.Fog.Application.Client.Web.Services.Abstractions;
using Ingweland.Fog.Application.Client.Web.Services.Hoh.Abstractions;
using Ingweland.Fog.Application.Client.Web.Settings;
using Ingweland.Fog.Application.Core.Repository.Abstractions;
using Ingweland.Fog.Application.Core.Services.Hoh.Abstractions;
using Ingweland.Fog.Shared;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Polly;
using Refit;

namespace Ingweland.Fog.App;

public static class DependencyInjection
{
    private const string API_BASE_URL_KEY = "Fog:ApiBaseUrl";

    public static void AddAppServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSharedServices();
        services.AddLocalization();
        services.AddWebAppApplicationServices();

        services.Configure<AssetsSettings>(configuration.GetSection(AssetsSettings.CONFIGURATION_PROPERTY_NAME));

        // Platform seams: the same set src/WebApp fills with Dummy* services to run the client
        // application layer outside the browser.
        services.AddSingleton<IHohDataProvider, FileSystemHohDataProvider>();
        services.AddSingleton<IHohLocalizationDataProvider, FileSystemHohLocalizationDataProvider>();
        services.AddSingleton<IHohDataInitializationService, HohDataInitializationService>();
        services.AddScoped<IPersistenceService, NotSupportedPersistenceService>();
        services.AddScoped<IJSInteropService, NotSupportedJsInteropService>();
        services.AddScoped<IEquipmentProfilePersistenceService, NotSupportedEquipmentProfilePersistenceService>();
        services.Replace(ServiceDescriptor.Singleton<IAnalyticsService, NoOpAnalyticsService>());

        // Assets are downloaded once and then read from disk (AssetCacheHandler): the city planner's own downloads,
        // i.e. its font and the icon atlas image and JSON (the shared layer registers these typed clients; this adds
        // the handler to them), and images shown in the UI (AssetImage uses the named client).
        services.AddSingleton<AssetFileCache>();
        services.AddTransient<AssetCacheHandler>();
        services.AddHttpClient<ITypefaceProvider, TypefaceProvider>().AddHttpMessageHandler<AssetCacheHandler>();
        services.AddHttpClient<IProductionRenderer, ProductionRenderer>().AddHttpMessageHandler<AssetCacheHandler>();
        services.AddHttpClient(AssetCacheHandler.HTTP_CLIENT_NAME).AddHttpMessageHandler<AssetCacheHandler>();

        var apiBaseUrl = configuration[API_BASE_URL_KEY] ??
            throw new InvalidOperationException($"`{API_BASE_URL_KEY}` is missing from appsettings.json.");
        var refitSettings = new RefitSettings
        {
            ContentSerializer = new SystemTextJsonContentSerializer(GetDefaultJsonSerializerOptions()),
        };
        AddRefitJsonApiClient<IHohDataService>(services, apiBaseUrl, refitSettings);
        AddRefitJsonApiClient<IInGameEventService>(services, apiBaseUrl, refitSettings);
        AddRefitJsonApiClient<IFogSharingService>(services, apiBaseUrl, refitSettings, "api");
        AddRefitJsonApiClient<ICommunityCityStrategyService>(services, apiBaseUrl, refitSettings, "api");

        services.AddTransient<CityViewerHomePage>();
        services.AddTransient<AboutPage>();
        services.AddTransient<CityViewerPage>();
        services.AddTransient<AlliedCultureCityGuidesPage>();
        services.AddTransient<CityGuidePage>();
    }

    // Same client setup as WebApp.Client/DependencyInjection.cs, with an absolute origin in place of
    // the WASM host's base address.
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
