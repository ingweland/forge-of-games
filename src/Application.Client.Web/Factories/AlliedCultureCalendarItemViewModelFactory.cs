using Ingweland.Fog.Application.Client.Core.Localization;
using Ingweland.Fog.Application.Client.Web.Factories.Interfaces;
using Ingweland.Fog.Application.Client.Web.Providers.Interfaces;
using Ingweland.Fog.Application.Client.Web.ViewModels;
using Ingweland.Fog.Application.Core.Extensions;
using Ingweland.Fog.Models.Hoh.Enums;
using Microsoft.Extensions.Localization;

namespace Ingweland.Fog.Application.Client.Web.Factories;

public class AlliedCultureCalendarItemViewModelFactory(
    IAssetUrlProvider assetUrlProvider,
    IStringLocalizer<FogResource> loc)
    : IAlliedCultureCalendarItemViewModelFactory
{
    public AlliedCultureCalendarItemViewModel Create(WonderId wonderId, string wonderName, DateTime startAt,
        DateTime endAt, string? premiumHelpPagePath = null)
    {
        return new AlliedCultureCalendarItemViewModel
        {
            WonderId = wonderId,
            Name = premiumHelpPagePath == null
                ? wonderName
                : $"{wonderName} {loc[FogResource.Common_Premium]}",
            DatesFormatted = $"{startAt:d} - {endAt:d}",
            IconUrl = assetUrlProvider.GetHohIconUrl(wonderId.ToCity().GetIcon()),
            PremiumHelpPagePath = premiumHelpPagePath,
        };
    }
}
