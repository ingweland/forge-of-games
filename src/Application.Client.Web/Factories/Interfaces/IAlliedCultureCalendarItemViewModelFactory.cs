using Ingweland.Fog.Application.Client.Web.ViewModels;
using Ingweland.Fog.Models.Hoh.Enums;

namespace Ingweland.Fog.Application.Client.Web.Factories.Interfaces;

public interface IAlliedCultureCalendarItemViewModelFactory
{
    AlliedCultureCalendarItemViewModel Create(WonderId wonderId, string wonderName, DateTime startAt,
        DateTime endAt);
}
