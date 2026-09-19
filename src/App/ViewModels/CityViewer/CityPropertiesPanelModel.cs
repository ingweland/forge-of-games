using Ingweland.Fog.Application.Client.Core.Localization;
using Ingweland.Fog.Application.Client.Web.CityPlanner.Stats;
using Ingweland.Fog.Application.Client.Web.ViewModels.Hoh;
using Ingweland.Fog.Application.Core.Extensions;

namespace Ingweland.Fog.App.ViewModels.CityViewer;

/// <summary>
///     What CityPropertiesComponent.razor works out inline, on top of the shared view model.
/// </summary>
public class CityPropertiesPanelModel
{
    public CityPropertiesPanelModel(CityPlannerCityPropertiesViewModel properties, string? wonderName,
        string storageIconUrl, string totalAreaIconUrl)
    {
        Properties = properties;
        StorageIconUrl = storageIconUrl;
        AgeColor = CssColor.Parse(properties.Age.Color);

        HasWonder = wonderName != null;
        WonderLine = HasWonder ? $"{wonderName} {FogResource.Hoh_Lvl} {properties.WonderLevel}" : string.Empty;
        HasWonderCost = properties.WonderCost != null;
        WonderCostTitle = $"{FogResource.Hoh_WorldWonder} {properties.WonderNextLevelRangeLabel}";
        HasWonderBonus = properties.WonderBonus != null;

        HasProductionCosts = properties.Production.Costs.Count > 0;
        ShowHappiness = properties.CityId.HasHappiness();
        HasPremiumExpansions = properties.Areas.PremiumExpansionCount != null;
        AreaTiles =
        [
            new IconLabelItemViewModel {IconUrl = totalAreaIconUrl, Label = properties.Areas.TotalArea},
            ..properties.Areas.AreasByType,
        ];
    }

    public Color? AgeColor { get; }
    public IReadOnlyList<IconLabelItemViewModel> AreaTiles { get; }
    public bool HasPremiumExpansions { get; }
    public bool HasProductionCosts { get; }
    public bool HasWonder { get; }
    public bool HasWonderBonus { get; }
    public bool HasWonderCost { get; }
    public CityPlannerCityPropertiesViewModel Properties { get; }
    public bool ShowHappiness { get; }
    public string StorageIconUrl { get; }
    public string WonderCostTitle { get; }
    public string WonderLine { get; }
}
