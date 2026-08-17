using Ingweland.Fog.Application.Client.Web.Factories.Interfaces;
using Ingweland.Fog.Application.Client.Web.Providers.Interfaces;
using Ingweland.Fog.Application.Client.Web.ViewModels;
using Ingweland.Fog.Application.Core.Extensions;
using Ingweland.Fog.Models.Hoh.Enums;

namespace Ingweland.Fog.Application.Client.Web.Factories;

public class AlliedCultureCalendarItemViewModelFactory(IAssetUrlProvider assetUrlProvider)
    : IAlliedCultureCalendarItemViewModelFactory
{
    public AlliedCultureCalendarItemViewModel Create(WonderId wonderId, string wonderName, DateTime startAt,
        DateTime endAt)
    {
        return new AlliedCultureCalendarItemViewModel
        {
            WonderId = wonderId,
            Name = wonderName,
            DatesFormatted = $"{startAt:d} - {endAt:d}",
            IconUrl = assetUrlProvider.GetHohIconUrl(wonderId.ToCity().GetIcon()),
        };
    }
}
