using Ingweland.Fog.Models.Hoh.Enums;

namespace Ingweland.Fog.Application.Client.Web.Extensions;

public static class HohUnitExtensions
{
    public static string GetTypeIconId(this UnitType unitType)
    {
        return unitType switch
        {
            UnitType.Cavalry => "icon_flat_unit_cavalry",
            UnitType.HeavyInfantry => "icon_flat_unit_heavyinfantry",
            UnitType.Infantry => "icon_flat_unit_infantry",
            UnitType.Ranged => "icon_flat_unit_ranged",
            UnitType.Siege => "icon_flat_unit_siege",
            UnitType.Gate => "icon_flat_unit_gate",
            UnitType.Spawner => "icon_flat_unit_spawner",
            UnitType.Ghost => "icon_flat_unit_ghost",
            UnitType.AllyNaya => "icon_flat_unit_allynaya",
            UnitType.AllyZhengYiSao => "icon_flat_unit_ally_zheng_yi_sao",
            _ => throw new ArgumentOutOfRangeException(nameof(unitType), unitType, null),
        };
    }

    public static string GetClassIconId(this HeroClassId classId)
    {
        return $"icon_class_{classId.ToString().ToLowerInvariant()}";
    }

    public static string GetColorIconId(this UnitColor unitColor, UnitColor iconColor = UnitColor.Undefined)
    {
        var finalIconColor = iconColor == UnitColor.Undefined ? "white" : iconColor.ToString().ToLowerInvariant();
        return $"icon_colour_{unitColor.ToString().ToLowerInvariant()}_{finalIconColor}";
    }

    public static string ToCssColor(this UnitColor color)
    {
        return color switch
        {
            UnitColor.Blue => "#479AFF",
            UnitColor.Green => "#35CF2E",
            UnitColor.Purple => "#7C3BE8",
            UnitColor.Red => "#FF4B32",
            UnitColor.Yellow => "#FFAA00",
            _ => "#c0c0c0",
        };
    }

    public static string ToCssColorVar(this UnitColor unitColor)
    {
        return unitColor switch
        {
            UnitColor.Blue => "var(--fog-units-blue-color)",
            UnitColor.Green => "var(--fog-units-green-color)",
            UnitColor.Purple => "var(--fog-units-purple-color)",
            UnitColor.Red => "var(--fog-units-red-color)",
            UnitColor.Yellow => "var(--fog-units-yellow-color)",
            _ => "var(--fog-units-neutral-color)",
        };
    }

    public static int ToStarCount(this HeroStarClass starClass)
    {
        return starClass switch
        {
            HeroStarClass.Star_2 => 2,
            HeroStarClass.Star_3 => 3,
            HeroStarClass.Star_4 => 4,
            HeroStarClass.Star_5 => 5,
            _ => 0,
        };
    }
}
