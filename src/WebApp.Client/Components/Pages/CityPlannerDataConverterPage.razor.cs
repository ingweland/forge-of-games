using Ingweland.Fog.Application.Client.Core.Localization;
using Ingweland.Fog.Application.Client.Web.CityPlanner.Abstractions;
using Ingweland.Fog.Application.Client.Web.Services.Abstractions;
using Ingweland.Fog.WebApp.Client.Components.Pages.Abstractions;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace Ingweland.Fog.WebApp.Client.Components.Pages;

public partial class CityPlannerDataConverterPage : FogPageBase
{
    private const string URL_FORMAT_ERROR_MESSAGE = "You must provide a valid url with 'sharedLayout' query value.";

    private string? _cityName;
    private string? _cityPlannerUrl;
    private bool _isConverting;
    private bool _success;

    [Inject]
    public ICityPlannerDataConverter Converter { get; set; }

    [Inject]
    private IDialogService DialogService { get; set; }

    [Inject]
    private ILogger<CityPlannerDataConverterPage> Logger { get; set; }

    [Inject]
    public IPersistenceService PersistenceService { get; set; }

    private IEnumerable<string> ValidateUrl(string src)
    {
        if (string.IsNullOrWhiteSpace(src))
        {
            yield return URL_FORMAT_ERROR_MESSAGE;
        }

        var hasError = false;
        try
        {
            var sharedLayout = Converter.ParseUrl(src);
            if (sharedLayout.IsFailed)
            {
                hasError = true;
            }
        }
        catch (Exception e)
        {
            Logger.LogError(e, "Error validating url.");
            hasError = true;
        }

        if (hasError)
        {
            yield return URL_FORMAT_ERROR_MESSAGE;
        }
    }

    private async Task Convert()
    {
        _isConverting = true;
        StateHasChanged();

        var sharedLayout = Converter.ParseUrl(_cityPlannerUrl!);
        var city = await Converter.ConvertAsync(sharedLayout.Value, _cityName!);
        if (city.IsSuccess)
        {
            try
            {
                await PersistenceService.SaveCity(city.Value);
                _ = await DialogService.ShowMessageBoxAsync(null, $"Successfully created city {_cityName}",
                    Loc[FogResource.Common_Ok]);
            }
            catch (Exception e)
            {
                _ = await DialogService.ShowMessageBoxAsync("Error saving city", e.Message, Loc[FogResource.Common_Ok]);
            }
        }
        else
        {
            var msg = string.Join("; ", city.Errors.Select(e => e.Message));
            _ = await DialogService.ShowMessageBoxAsync("Error converting data", msg, Loc[FogResource.Common_Ok]);
        }

        _isConverting = false;
        StateHasChanged();
    }
}
