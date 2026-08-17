using System.Collections.Concurrent;
using Ingweland.Fog.Application.Client.Web.Factories.Interfaces;
using Ingweland.Fog.Application.Client.Web.Services.Abstractions;
using Ingweland.Fog.Application.Client.Web.ViewModels;
using Ingweland.Fog.Application.Core.Services.Hoh.Abstractions;
using Ingweland.Fog.Dtos.Hoh;
using Ingweland.Fog.Models.Fog.Entities;
using Ingweland.Fog.Models.Hoh.Enums;
using Microsoft.Extensions.Logging;

namespace Ingweland.Fog.Application.Client.Web.Services;

public class AlliedCultureCityGuidesUiService(
    IInGameEventService inGameEventService,
    ICityService cityService,
    IAlliedCultureCalendarItemViewModelFactory alliedCultureCalendarItemViewModelFactory,
    ICommunityCityStrategyService communityCityStrategyService,
    IFogSharingUiService fogSharingUiService,
    IPersistenceService persistenceService,
    IAlliedCultureCityGuideViewModelFactory alliedCultureCityGuideViewModelFactory,
    ILogger<AlliedCultureCityGuidesUiService> logger) : UiServiceBase(logger), IAlliedCultureCityGuidesUiService
{
    private readonly ConcurrentDictionary<string, Lazy<Task<IReadOnlyCollection<AlliedCultureCalendarItemViewModel>>>>
        _calendarItemsCache = new();

    public Task<IReadOnlyCollection<AlliedCultureCalendarItemViewModel>> GetCalendarAsync(string worldId,
        CancellationToken ct = default)
    {
        var lazy = _calendarItemsCache.GetOrAdd(worldId,
            key => new Lazy<Task<IReadOnlyCollection<AlliedCultureCalendarItemViewModel>>>(() =>
                LoadCalendarItemsAsync(key, ct)));

        return lazy.Value;
    }

    public async Task<IReadOnlyCollection<AlliedCultureCityGuideGroupViewModel>> GetGuidesAsync()
    {
        var guidesTask = ExecuteSafeAsync(() => communityCityStrategyService.GetStrategiesAsync(), []);
        var wondersTask = cityService.GetWonderBasicDataAsync();
        await Task.WhenAll(guidesTask, wondersTask);
        var guides = guidesTask.Result.Where(x => x.WonderId is not (null or WonderId.Undefined))
            .ToDictionary(x => x.WonderId!);
        var groups = new List<AlliedCultureCityGuideGroupViewModel>();
        foreach (var kvp in wondersTask.Result.OrderBy(x => x.Key))
        {
            var guideVms = new List<AlliedCultureCityGuideViewModel>();
            foreach (var w in kvp.Value)
            {
                if (!guides.TryGetValue(w.Id, out var guide))
                {
                    continue;
                }

                guideVms.Add(alliedCultureCityGuideViewModelFactory.Create(guide, w));
            }

            if (guideVms.Count > 0)
            {
                groups.Add(new AlliedCultureCityGuideGroupViewModel
                {
                    CityId = kvp.Key,
                    CityName = kvp.Value.First().CityName,
                    Guides = guideVms,
                });
            }
        }

        return groups;
    }

    public async Task<CityStrategy?> GetGuideAsync(string guideId)
    {
        var strategy = await persistenceService.LoadCityStrategy(guideId, true);
        if (strategy != null)
        {
            return strategy;
        }

        strategy = await fogSharingUiService.FetchCityStrategyAsync(guideId);
        if (strategy != null)
        {
            await persistenceService.SaveCommunityCityStrategy(guideId, strategy);
        }

        return strategy;
    }

    private Task<IReadOnlyCollection<AlliedCultureCalendarItemViewModel>> LoadCalendarItemsAsync(string worldId,
        CancellationToken ct)
    {
        return ExecuteSafeAsync<IReadOnlyCollection<AlliedCultureCalendarItemViewModel>>(
            async () =>
            {
                var events = await inGameEventService.Get(worldId, EventDefinitionId.EventCity, ct);
                var wonderIds = Enum.GetValues<WonderId>().Where(x => x != WonderId.Undefined).Select(x => x).ToList();
                var calendar = new List<AlliedCultureCalendarItemViewModel>();
                foreach (var inGameEventDto in GetActiveEvents(events))
                {
                    var wonderId =
                        wonderIds.FirstOrDefault(x => inGameEventDto.InGameDefinitionId.EndsWith(x.ToString()));
                    if (wonderId == WonderId.Undefined)
                    {
                        continue;
                    }

                    var wonder = await cityService.GetWonderAsync(wonderId);
                    if (wonder == null)
                    {
                        continue;
                    }

                    calendar.Add(alliedCultureCalendarItemViewModelFactory.Create(wonderId, wonder.WonderName,
                        inGameEventDto.StartAt, inGameEventDto.EndAt));
                }

                return calendar;
            },
            []);
    }

    private static IEnumerable<InGameEventDto> GetActiveEvents(IReadOnlyCollection<InGameEventDto> events)
    {
        var now = DateTime.UtcNow;
        return events.Where(x => x.EndAt > now).OrderBy(x => x.StartAt);
    }
}
