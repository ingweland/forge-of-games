using System.Net;
using Ingweland.Fog.InnSdk.Hoh.Abstractions;
using Ingweland.Fog.InnSdk.Hoh.Authentication;
using Ingweland.Fog.InnSdk.Hoh.Authentication.Abstractions;
using Ingweland.Fog.InnSdk.Hoh.Authentication.Models;
using Ingweland.Fog.InnSdk.Hoh.Factories;
using Ingweland.Fog.InnSdk.Hoh.Factories.Interfaces;
using Ingweland.Fog.InnSdk.Hoh.Net;
using Ingweland.Fog.InnSdk.Hoh.Net.Abstractions;
using Ingweland.Fog.InnSdk.Hoh.Services;
using Ingweland.Fog.InnSdk.Hoh.Services.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Polly;
using AuthenticationManager = Ingweland.Fog.InnSdk.Hoh.Authentication.AuthenticationManager;

namespace Ingweland.Fog.InnSdk.Hoh;

public static class DependencyInjection
{
    public static void AddInnSdkServices(this IServiceCollection services)
    {
        services.AddAutoMapper(typeof(DependencyInjection).Assembly);

        services.AddSingleton<IWebAuthenticationResponseHandler, WebAuthenticationResponseHandler>();
        services.AddSingleton<IWebAuthPayloadFactory, WebAuthPayloadFactory>();
        services.AddSingleton<ILocalizationRequestPayloadFactory, LocalizationRequestPayloadFactory>();
        services.AddSingleton<IGameDesignRequestPayloadFactory, GameDesignRequestPayloadFactory>();
        services.AddSingleton<IPlayerRankingRequestPayloadFactory, PlayerRankingRequestPayloadFactory>();
        services.AddSingleton<IAllianceRankingRequestPayloadFactory, AllianceRankingRequestPayloadFactory>();
        services.AddSingleton<IAuthenticationManager, AuthenticationManager>();

        services.TryAddSingleton<IGameConnectionManager, GameConnectionManager>();
        services.TryAddScoped<IGameCredentialsManager, DefaultGameCredentialsManager>();

        services.AddScoped<IStaticDataService, StaticDataService>();
        services.AddScoped<IRankingsService, RankingsService>();
        services.AddScoped<IDataParsingService, DataParsingService>();
        services.AddScoped<IBattleService, BattleService>();
        services.AddScoped<ICityService, CityService>();
        services.AddScoped<IPlayerService, PlayerService>();
        services.AddScoped<IAllianceService, AllianceService>();
        services.AddScoped<IWoaService, WoaService>();

        services.AddScoped<Lazy<IStaticDataService>>(sp =>
            new Lazy<IStaticDataService>(sp.GetRequiredService<IStaticDataService>));
        services.AddScoped<Lazy<IRankingsService>>(sp =>
            new Lazy<IRankingsService>(sp.GetRequiredService<IRankingsService>));
        services.AddScoped<Lazy<IBattleService>>(sp =>
            new Lazy<IBattleService>(sp.GetRequiredService<IBattleService>));
        services.AddScoped<Lazy<ICityService>>(sp =>
            new Lazy<ICityService>(sp.GetRequiredService<ICityService>));
        services.AddScoped<Lazy<IPlayerService>>(sp =>
            new Lazy<IPlayerService>(sp.GetRequiredService<IPlayerService>));
        services.AddScoped<Lazy<IAllianceService>>(sp =>
            new Lazy<IAllianceService>(sp.GetRequiredService<IAllianceService>));
        services.AddScoped<Lazy<IWoaService>>(sp =>
            new Lazy<IWoaService>(sp.GetRequiredService<IWoaService>));

        services.AddHttpClient<IWebAuthenticationService, WebAuthenticationService>();

        services.AddHttpClient<IGameApiClient, GameApiClient>()
            .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
            {
                AutomaticDecompression = DecompressionMethods.GZip,
            })
            .AddSdkHttpClientResilienceHandler()
            .AddHttpMessageHandler<AuthenticationHandler>()
            .AddHttpMessageHandler<RequiredHeadersHandler>()
            .AddHttpMessageHandler<HttpContextDependencyHandler>();

        services.AddScoped<IInnSdkClient, InnSdkClient>();

        services.AddTransient<AuthenticationHandler>();
        services.AddTransient<RequiredHeadersHandler>();
        services.AddTransient<HttpContextDependencyHandler>();
    }

    private static IHttpClientBuilder AddSdkHttpClientResilienceHandler(this IHttpClientBuilder builder)
    {
        builder.AddStandardResilienceHandler(options =>
        {
            options.Retry.ShouldHandle = args =>
            {
                if (args.Outcome.Exception is HttpRequestException)
                {
                    return PredicateResult.True();
                }

                if (args.Outcome.Result is HttpResponseMessage response)
                {
                    return ValueTask.FromResult(!response.IsSuccessStatusCode);
                }

                //TODO handle SoftErrorType.REQUEST_SESSION_EXPIRED
                return ValueTask.FromResult(false);
            };
            options.Retry.Delay = TimeSpan.FromSeconds(3);
            options.Retry.MaxRetryAttempts = 1;
            options.Retry.OnRetry = args =>
            {
                var request = args.Context.GetRequestMessage();
                if (request != null)
                {
                    if (request.Options.TryGetValue(new HttpRequestOptionsKey<GameWorldConfig>(nameof(GameWorldConfig)),
                            out var gameWorld) &&
                        request.Options.TryGetValue(
                            new HttpRequestOptionsKey<IGameConnectionManager>(nameof(IGameConnectionManager)),
                            out var gameConnectionManager))
                    {
                        gameConnectionManager.Remove(gameWorld.Id);
                    }
                }

                return ValueTask.CompletedTask;
            };
        });
        
        return builder;
    }
}
