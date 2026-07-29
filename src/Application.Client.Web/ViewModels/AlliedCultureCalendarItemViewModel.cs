using Ingweland.Fog.Models.Hoh.Enums;

namespace Ingweland.Fog.Application.Client.Web.ViewModels;

public class AlliedCultureCalendarItemViewModel
{
    public required string DatesFormatted { get; init; }
    public string? IconUrl { get; init; }
    public required string Name { get; init; }

    /// <summary>
    ///     When set, this is a premium item: clicking it opens the given help page instead of a community strategy.
    /// </summary>
    public string? PremiumHelpPagePath { get; init; }

    public required WonderId WonderId { get; init; }
}
