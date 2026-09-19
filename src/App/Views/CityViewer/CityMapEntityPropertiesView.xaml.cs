using Ingweland.Fog.App.ViewModels.CityViewer;
using Ingweland.Fog.Application.Client.Core.Localization;
using Ingweland.Fog.Application.Client.Web.ViewModels.Hoh.City;

namespace Ingweland.Fog.App.Views.CityViewer;

public partial class CityMapEntityPropertiesView : ContentView
{
    public CityMapEntityPropertiesView()
    {
        InitializeComponent();
    }

    protected override void OnBindingContextChanged()
    {
        base.OnBindingContextChanged();

        var production = (BindingContext as CityMapEntityPanelModel)?.Building.ProductionComponent;
        if (production == null)
        {
            StatsTable.Clear(ProductsTable);
            StatsTable.Clear(ProductionCostTable);
            return;
        }

        FillProductsTable(production);
        ProductionValuesTable.Fill(ProductionCostTable, production.Cost,
            ((CityMapEntityPanelModel) BindingContext).StorageIconUrl);
    }

    // ProductionComponent.razor's table: [selection] | reward icons | 1h | 24hrs, rewards stacked per product.
    private void FillProductsTable(ProductionComponentViewModel production)
    {
        var header = new List<View?>();
        if (production.CanSelectProduct)
        {
            header.Add(null);
        }

        header.Add(null);
        header.Add(StatsTable.HeaderText(FogResource.Common_1h));
        header.Add(StatsTable.HeaderText(FogResource.Common_24hrs));

        var rows = production.Products.Select(product =>
        {
            var row = new List<View?>();
            if (production.CanSelectProduct)
            {
                var dot = StatsTable.SelectionDot(product.IsSelected);
                dot.Margin = new Thickness(12, 2, 0, 2);
                row.Add(dot);
            }

            row.Add(StatsTable.Stack(product.Rewards.Select(x => StatsTable.Icon(x.IconUrl, 18))));
            row.Add(StatsTable.Stack(product.Rewards.Select(x => RewardLabel(x.OneHourProduction))));
            row.Add(StatsTable.Stack(product.Rewards.Select(x => RewardLabel(x.OneDayProduction))));
            return row;
        });

        StatsTable.Fill(ProductsTable, header, rows, new Thickness(12, 2));
    }

    // As tall as the 18px icons so each value lines up with its resource.
    private static View RewardLabel(string text)
    {
        var label = StatsTable.Text(text, "FogProductCellLabel");
        label.HeightRequest = 18;
        return label;
    }
}
