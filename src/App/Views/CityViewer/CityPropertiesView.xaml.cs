using Ingweland.Fog.App.ViewModels.CityViewer;
using Ingweland.Fog.Application.Client.Core.Localization;

namespace Ingweland.Fog.App.Views.CityViewer;

public partial class CityPropertiesView : ContentView
{
    public CityPropertiesView()
    {
        InitializeComponent();
    }

    protected override void OnBindingContextChanged()
    {
        base.OnBindingContextChanged();

        if (BindingContext is not CityPropertiesPanelModel model)
        {
            StatsTable.Clear(ProductionTable);
            StatsTable.Clear(ProductionCostTable);
            StatsTable.Clear(AreaTable);
            return;
        }

        ProductionValuesTable.Fill(ProductionTable, model.Properties.Production.Products, model.StorageIconUrl);
        ProductionValuesTable.Fill(ProductionCostTable, model.Properties.Production.Costs, model.StorageIconUrl);
        StatsTable.Fill(AreaTable,
            new View?[]
            {
                StatsTable.HeaderText(FogResource.Hoh_Building), StatsTable.HeaderText(FogResource.CityPlanner_Count),
                StatsTable.HeaderText(FogResource.CityPlanner_Area),
            },
            model.Properties.Areas.AreasByGroup.Select(x => new View?[]
            {
                StatsTable.Text(x.GroupName), StatsTable.Text(x.Count), StatsTable.Text(x.Area),
            }),
            new Thickness(6, 2), endAlignFirstColumn: true);
    }
}
