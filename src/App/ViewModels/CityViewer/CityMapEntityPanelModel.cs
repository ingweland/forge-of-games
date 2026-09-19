using Ingweland.Fog.Application.Client.Core.Localization;
using Ingweland.Fog.Application.Client.Web.CityPlanner.Stats;

namespace Ingweland.Fog.App.ViewModels.CityViewer;

/// <summary>
///     What CityMapEntityPropertiesComponent.razor works out inline (read-only mode), on top of the shared
///     view model.
/// </summary>
public class CityMapEntityPanelModel
{
    public CityMapEntityPanelModel(CityMapEntityViewModel building, string storageIconUrl)
    {
        Building = building;
        StorageIconUrl = storageIconUrl;
        Title = $"{building.Name} {FogResource.Hoh_Lvl} {building.Level}";
        AgeColor = CssColor.Parse(building.Age?.Color);
        AgeName = building.Age?.Name ?? string.Empty;
        LockButtonText = building.IsLocked
            ? FogResource.CityPlanner_CityMapEntity_Unlock
            : FogResource.CityPlanner_CityMapEntity_Lock;
        HasProduction = building.ProductionComponent != null;
        HasProductionCost = building.ProductionComponent?.Cost.Count > 0;
    }

    public Color? AgeColor { get; }
    public string AgeName { get; }
    public CityMapEntityViewModel Building { get; }
    public bool HasProduction { get; }
    public bool HasProductionCost { get; }
    public string LockButtonText { get; }
    public string StorageIconUrl { get; }
    public string Title { get; }
}
