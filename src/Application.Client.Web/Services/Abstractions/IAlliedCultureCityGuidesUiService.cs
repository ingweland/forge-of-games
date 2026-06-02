using Ingweland.Fog.Application.Client.Web.ViewModels;
using Ingweland.Fog.Models.Fog.Entities;

namespace Ingweland.Fog.Application.Client.Web.Services.Abstractions;

public interface IAlliedCultureCityGuidesUiService
{
    Task<IReadOnlyCollection<AlliedCultureCalendarItemViewModel>> GetCalendarAsync(string worldId,
        CancellationToken ct = default);

    Task<IReadOnlyCollection<AlliedCultureCityGuideGroupViewModel>> GetGuidesAsync();

    Task<CityStrategy?> GetGuideAsync(string guideId);
}
