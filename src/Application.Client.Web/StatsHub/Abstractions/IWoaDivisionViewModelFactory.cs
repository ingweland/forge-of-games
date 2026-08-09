using Ingweland.Fog.Application.Client.Web.StatsHub.ViewModels;
using Ingweland.Fog.Dtos.Hoh;
using Ingweland.Fog.Dtos.Hoh.Stats;
using Ingweland.Fog.Models.Hoh.Enums;

namespace Ingweland.Fog.Application.Client.Web.StatsHub.Abstractions;

public interface IWoaDivisionViewModelFactory
{
    WoaDivisionViewModel Create(WoaDivisionDto dto, IReadOnlyDictionary<WoaTier, WoaTierDto> tiers);
}
