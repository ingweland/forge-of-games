using Ingweland.Fog.Application.Client.Web.StatsHub.ViewModels;
using Microsoft.AspNetCore.Components;

namespace Ingweland.Fog.WebApp.Client.Components.Pages.StatsHub;

public partial class WoaDivisionPage : StatsHubPageBase
{
    private WoaDivisionViewModel? _division;

    [Parameter]
    public required int DivisionId { get; set; }

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();

        _division = await LoadWithPersistenceAsync(nameof(_division),
            () => StatsHubUiService.GetWoaDivisionAsync(DivisionId));

        if (OperatingSystem.IsBrowser())
        {
            IsInitialized = true;
        }
    }
}
