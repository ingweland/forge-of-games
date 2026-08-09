namespace Ingweland.Fog.Application.Client.Web.StatsHub.ViewModels;

public class WoaDivisionViewModel
{
    public required IReadOnlyCollection<WoaDivisionAllianceViewModel> Alliances { get; init; }
    public required string EventLabel { get; init; }
}