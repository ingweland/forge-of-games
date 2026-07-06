using Ingweland.Fog.Models.Hoh.Enums;
using SkiaSharp;

namespace Ingweland.Fog.Application.Client.Web.CityPlanner.Rendering;

public class DefaultCityMapEntityStyle : ICityMapEntityStyle
{
    private static readonly SKColor CultureColor = SKColor.Parse("#A697E8");

    private static readonly SKColor UnchangedCultureColor =
        SKColor.Parse("#dddddd");

    private static readonly SKColor BuffOverflowColor = SKColor.Parse("#6F659C");

    private static readonly SKColor UnchangedBuffOverflowColor =
        SKColor.Parse("#cfcfcf");

    private readonly IDictionary<BuildingType, SKPaint> _buildingTypePaints =
        new Dictionary<BuildingType, SKPaint>();

    public SKPaint BuffOverflowPaint { get; } = new()
    {
        Color = BuffOverflowColor,
        IsAntialias = true,
        Style = SKPaintStyle.Fill,
    };

    public SKPaint UnchangedBuffOverflowPaint { get; } = new()
    {
        Color = UnchangedBuffOverflowColor,
        IsAntialias = true,
        Style = SKPaintStyle.Fill,
    };

    public SKPaint UnchangedCultureFillPaint { get; } = new()
    {
        Color = UnchangedCultureColor,
        IsAntialias = true,
        Style = SKPaintStyle.Fill,
    };

    public SKPaint UnchangedNameTextPaint { get; } = new()
    {
        Color = SKColor.Parse("#999999"),
        IsAntialias = true,
    };

    public SKPaint UnchangedBuildingPaint { get; } = new()
    {
        Color = SKColor.Parse("#f0f0f0"),
        IsAntialias = false,
        Style = SKPaintStyle.Fill,
    };

    public SKPaint GetPaint(BuildingType buildingType)
    {
        if (!_buildingTypePaints.TryGetValue(buildingType, out var paint))
        {
            paint = new SKPaint
            {
                Color = GetBuildingColor(buildingType),
                IsAntialias = false,
                Style = SKPaintStyle.Fill,
            };
            _buildingTypePaints.Add(buildingType, paint);
        }

        return paint!;
    }

    public SKPaint CultureFillPaint { get; } = new()
    {
        Color = CultureColor,
        IsAntialias = true,
        Style = SKPaintStyle.Fill,
    };

    public int NameDefaultTextSize { get; } = 16;
    public int ProductionTimeDefaultTextSize { get; } = 16;

    public SKPaint NegativeSelectionFillPaint { get; } = new()
    {
        Color = SKColors.OrangeRed,
        IsAntialias = false,
        Style = SKPaintStyle.Fill,
    };

    public SKPaint PositiveSelectionFillPaint { get; } = new()
    {
        Color = SKColors.Green,
        IsAntialias = false,
        Style = SKPaintStyle.Fill,
    };

    public SKPaint LockedFillPaint { get; } = new()
    {
        Color = SKColor.Parse("#48000000"),
        IsAntialias = false,
        Style = SKPaintStyle.Fill,
    };

    public SKPaint CustomizationFillPaint { get; } = new()
    {
        Color = SKColor.Parse("#E895CE"),
        IsAntialias = true,
        Style = SKPaintStyle.Fill,
    };

    public SKPaint NegativeSelectionStrokePaint { get; } = new()
    {
        Color = SKColors.DarkRed,
        StrokeWidth = 2,
        IsAntialias = true,
        Style = SKPaintStyle.Stroke,
    };

    public SKPaint PositiveSelectionStrokePaint { get; } = new()
    {
        Color = SKColors.DarkGreen,
        StrokeWidth = 2,
        IsAntialias = true,
        Style = SKPaintStyle.Stroke,
    };

    public SKPaint DefaultFillPaint { get; } = new()
    {
        Color = SKColor.Parse("#9B97A8"),
        IsAntialias = false,
        Style = SKPaintStyle.Fill,
    };

    public SKPaint DefaultStrokePaint { get; } = new()
    {
        Color = SKColor.Parse("#686945"),
        StrokeWidth = 0.5f,
        IsAntialias = true,
        Style = SKPaintStyle.Stroke,
    };

    public SKPaint OverflowPaint { get; } = new()
    {
        Color = SKColor.Parse("#24000000"),
        IsAntialias = false,
        Style = SKPaintStyle.Fill,
    };

    public SKPaint StateIconPaint { get; } = new()
    {
        Color = SKColor.Parse("#34495E"),
        IsAntialias = false,
        Style = SKPaintStyle.Fill,
    };

    public SKPaint LockIconPaint { get; } = new()
    {
        Color = SKColor.Parse("#e0e0e0"),
        IsAntialias = false,
        Style = SKPaintStyle.Fill,
    };

    public SKPaint OverflowStrokePaint { get; } = new()
    {
        Color = SKColor.Parse("#c5c5c5"),
        StrokeWidth = 2,
        IsAntialias = true,
        Style = SKPaintStyle.Stroke,
        PathEffect = SKPathEffect.CreateDash([5, 8], 0),
    };

    public SKPaint NameTextPaint { get; } = new()
    {
        Color = SKColors.Black,
        IsAntialias = true,
    };

    public SKColor GetBuffBackgroundColor(float cultureValue, bool isUnchanged = false)
    {
        if (!isUnchanged)
        {
            return cultureValue <= 1.0 ? SKColors.White : CultureColor;
        }

        return cultureValue <= 1.0 ? SKColor.Parse("#f8f8f8") : UnchangedCultureColor;
    }

    public SKPaint GetBuffForegroundPaint(float cultureValue, bool isUnchanged = false)
    {
        if (!isUnchanged)
        {
            return cultureValue <= 1.0 ? CultureFillPaint : BuffOverflowPaint;
        }

        return cultureValue <= 1.0 ? UnchangedCultureFillPaint : UnchangedBuffOverflowPaint;
    }

    private static SKColor GetBuildingColor(BuildingType buildingType)
    {
        switch (buildingType)
        {
            case BuildingType.CultureSite:
            case BuildingType.RitualSite:
            case BuildingType.Irrigation:
            case BuildingType.PresetIrrigation:
                return SKColor.Parse("#A697E8");

            case BuildingType.Home:
                return SKColor.Parse("#95BAE8");

            case BuildingType.Farm:
            case BuildingType.Aviary:
            case BuildingType.Beehive:
            case BuildingType.RiceFarm:
            case BuildingType.CamelFarm:
            case BuildingType.Plantation:
                return SKColor.Parse("#CFE895");

            case BuildingType.Workshop:
                return SKColor.Parse("#E8DF95");

            case BuildingType.Barracks:
            case BuildingType.Quarry:
            case BuildingType.GoldMine:
            case BuildingType.PapyrusField:
            case BuildingType.FishingPier:
            case BuildingType.Pier:
                return SKColor.Parse("#E8B995");

            default:
                return SKColor.Parse("#9B97A8");
        }
    }
}
