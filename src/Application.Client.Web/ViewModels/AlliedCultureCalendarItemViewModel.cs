using Ingweland.Fog.Models.Hoh.Enums;

namespace Ingweland.Fog.Application.Client.Web.ViewModels;

public class AlliedCultureCalendarItemViewModel
{
    public required string DatesFormatted { get; init; }
    public string? IconUrl { get; init; }
    public required string Name { get; init; }
    public required WonderId WonderId { get; init; }
}
