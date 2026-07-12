using Ingweland.Fog.Application.Client.Web.StatsHub.ViewModels;
using Ingweland.Fog.Dtos.Hoh.Stats;

namespace Ingweland.Fog.Application.Client.Web.StatsHub.Abstractions;

public interface IWoaPlayerStatsViewModelFactory
{
    WoaPlayerStatsViewModel Create(WoaPlayerStatsDto dto);
}
