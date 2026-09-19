using Ingweland.Fog.Application.Client.Core.Localization;
using Ingweland.Fog.Application.Client.Web.CityPlanner.Stats;

namespace Ingweland.Fog.App.Views.CityViewer;

/// <summary>
///     The table of CityProductionStatsComponent: resource | default (storage icon) | 1h | 24hrs.
/// </summary>
internal static class ProductionValuesTable
{
    public static void Fill(Grid table, IEnumerable<TimedProductionValuesViewModel> items, string storageIconUrl)
    {
        StatsTable.Fill(table,
            new View?[]
            {
                null, StatsTable.Icon(storageIconUrl, 16), StatsTable.HeaderText(FogResource.Common_1h),
                StatsTable.HeaderText(FogResource.Common_24hrs),
            },
            items.Select(x => new View?[]
            {
                StatsTable.Icon(x.IconUrl, 16), StatsTable.Text(x.Default), StatsTable.Text(x.OneHour),
                StatsTable.Text(x.OneDay),
            }),
            new Thickness(6, 2));
    }
}
