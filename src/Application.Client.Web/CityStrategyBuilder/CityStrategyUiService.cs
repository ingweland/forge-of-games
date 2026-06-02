using Ingweland.Fog.Application.Client.Web.CityStrategyBuilder.Abstractions;
using Ingweland.Fog.Application.Client.Web.Services.Abstractions;
using Ingweland.Fog.Application.Core.CityPlanner;
using Ingweland.Fog.Application.Core.Constants;
using Ingweland.Fog.Models.Fog.Entities;
using Microsoft.Extensions.Logging;

namespace Ingweland.Fog.Application.Client.Web.CityStrategyBuilder;

public class CityStrategyUiService(
    ICityStrategyFactory cityStrategyFactory,
    ILogger<CityStrategyUiService> logger) : UiServiceBase(logger), ICityStrategyUiService
{
    public CityStrategy CreateCityStrategy(NewCityRequest newCityRequest)
    {
        return cityStrategyFactory.Create(newCityRequest, FogConstants.CITY_PLANNER_VERSION);
    }
}
