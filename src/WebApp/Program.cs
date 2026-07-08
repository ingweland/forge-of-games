using System.Text.Json.Serialization;
using Azure.Monitor.OpenTelemetry.AspNetCore;
using FluentResults;
using Ingweland.Fog.Application.Client.Web;
using Ingweland.Fog.Application.Core.Helpers;
using Ingweland.Fog.Application.Server;
using Ingweland.Fog.Infrastructure;
using Ingweland.Fog.InnSdk.Hoh;
using Ingweland.Fog.Shared;
using Ingweland.Fog.Shared.Localization;
using Ingweland.Fog.WebApp;
using Ingweland.Fog.WebApp.Apis;
using Ingweland.Fog.WebApp.Components;
using Ingweland.Fog.WebApp.Constants;
using Ingweland.Fog.WebApp.Middleware;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.AspNetCore.ResponseCompression;
using MudBlazor.Services;
using _Imports = Ingweland.Fog.WebApp.Client._Imports;

var builder = WebApplication.CreateBuilder(args);
builder.AddWebAppSettings();
builder.Services.AddOpenApi();
builder.Services.AddRazorComponents()
    .AddInteractiveWebAssemblyComponents();
builder.Services.AddMudServices();
builder.Services.AddApplicationServices();
builder.AddWebAppServices();
builder.Services.AddInfrastructureServices();
builder.Services.AddInfrastructureDbContext(builder.Configuration);
builder.Services.AddSharedServices();
builder.Services.AddInnSdkServices();
builder.Services.AddWebAppApplicationServices();
builder.Services.AddLocalization();
builder.Services.AddResponseCompression(options =>
{
    options.Providers.Clear();
    options.Providers.Add<GzipCompressionProvider>();
    options.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(["application/x-protobuf"]);
    options.EnableForHttps = true;
});
builder.Services.AddCors(options =>
{
    options.AddPolicy(PolicyNames.CORS_IN_GAME_DATA_IMPORT_POLICY,
        policy => { policy.WithOrigins("*").WithMethods("POST", "OPTIONS").AllowAnyHeader(); });
});
builder.Services.Configure<JsonOptions>(options =>
{
    options.SerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault;
});
builder.Services.AddOpenTelemetry().UseAzureMonitor();
var app = builder.Build();

var resultLogger = app.Services.GetRequiredService<IResultLogger>();
Result.Setup(cfg => { cfg.Logger = resultLogger; });

app.UseMiddleware<MaintenanceModeMiddleware>();
app.UseMiddleware<DefaultDomainRestrictionMiddleware>();
app.Use(async (context, next) =>
{
    //TODO: add redirection from HELP_HERO_PLAYGROUNDS_PATH
    if (context.Request.Path.ToString().Contains(FogUrlBuilder.PageRoutes.COMMAND_CENTER_HERO_PLAYGROUNDS_PATH,
            StringComparison.OrdinalIgnoreCase))
    {
        context.Response.Redirect(FogUrlBuilder.PageRoutes.BASE_HEROES_PATH, true);
    }
    else
    {
        await next();
    }
});

app.UseResponseCompression();

if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
    app.MapOpenApi();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();

    app.UseAzureAppConfiguration();
}

app.UseHttpsRedirection();

app.UseAntiforgery();

app.UseCors();

var localizationOptions = new RequestLocalizationOptions()
    .SetDefaultCulture(HohSupportedCultures.DefaultCulture)
    .AddSupportedCultures(HohSupportedCultures.AllCultures.ToArray())
    .AddSupportedUICultures(HohSupportedCultures.AllCultures.ToArray());

app.UseRequestLocalization(localizationOptions);

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(_Imports).Assembly);

app.MapHohApi();
app.MapStatsApi();
app.MapFogApi();

app.Run();
