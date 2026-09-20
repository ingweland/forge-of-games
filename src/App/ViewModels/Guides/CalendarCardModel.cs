using Ingweland.Fog.Application.Client.Web.ViewModels;

namespace Ingweland.Fog.App.ViewModels.Guides;

/// <summary>
///     A calendar entry together with the guide its card opens, which the website's page looks up on the
///     click instead (AlliedCultureCityGuides.razor.cs, OpenCalendarItem). Guide is null when the wonder
///     has no guide.
/// </summary>
public record CalendarCardModel(AlliedCultureCalendarItemViewModel Item, AlliedCultureCityGuideViewModel? Guide);
