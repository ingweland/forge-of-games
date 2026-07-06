using Ingweland.Fog.Models.Hoh.Constants;
using Ingweland.Fog.Models.Hoh.Enums;

namespace Ingweland.Fog.Application.Core.Extensions;

public static class CityIdExtensions
{
    private static readonly List<WonderId> MayaWonders =
        [WonderId.Mayas_Tikal, WonderId.Mayas_ChichenItza, WonderId.Mayas_SayilPalace];

    private static readonly List<WonderId> ChineseWonders =
        [WonderId.China_ForbiddenCity, WonderId.China_GreatWall, WonderId.China_TerracottaArmy];

    private static readonly List<WonderId> EgyptianWonders =
        [WonderId.Egypt_AbuSimbel, WonderId.Egypt_CheopsPyramid, WonderId.Egypt_GreatSphinx];

    private static readonly List<WonderId> VikingsWonders =
        [WonderId.Vikings_Valhalla, WonderId.Vikings_Yggdrasil, WonderId.Vikings_DragonshipEllida];

    private static readonly List<WonderId> ArabicWonders =
        [WonderId.Arabia_Petra, WonderId.Arabia_CityOfBrass, WonderId.Arabia_NoriasOfHama];

    private static readonly List<WonderId> AncientEgyptWonders =
        [WonderId.AncientEgyptEvent_AnubisTemple];

    private static readonly List<WonderId> IthakaWonders =
        [WonderId.Ithaka_PenelopesHearth];

    public static IReadOnlyCollection<WonderId> GetWonders(this CityId cityId)
    {
        return cityId switch
        {
            CityId.Mayas_Tikal => MayaWonders,
            CityId.Mayas_ChichenItza => MayaWonders,
            CityId.Mayas_SayilPalace => MayaWonders,
            CityId.China => ChineseWonders,
            CityId.Egypt => EgyptianWonders,
            CityId.Vikings => VikingsWonders,
            CityId.Arabia_CityOfBrass => ArabicWonders,
            CityId.Arabia_NoriasOfHama => ArabicWonders,
            CityId.Arabia_Petra => ArabicWonders,
            CityId.AncientEgyptEvent => AncientEgyptWonders,
            CityId.Ithaka => IthakaWonders,
            _ => Array.Empty<WonderId>(),
        };
    }

    public static string ToDefaultAge(this CityId cityId)
    {
        return cityId switch
        {
            CityId.Mayas_Tikal => AgeIds.MAYAS,
            CityId.Mayas_ChichenItza => AgeIds.MAYAS,
            CityId.Mayas_SayilPalace => AgeIds.MAYAS,
            CityId.China => AgeIds.CHINA,
            CityId.Vikings => AgeIds.VIKINGS,
            CityId.Egypt => AgeIds.EGYPT,
            CityId.Arabia_CityOfBrass => AgeIds.ARABIA,
            CityId.Arabia_NoriasOfHama => AgeIds.ARABIA,
            CityId.Arabia_Petra => AgeIds.ARABIA,
            CityId.AncientEgyptEvent => AgeIds.ANCIENT_EGYPT_EVENT,
            CityId.Ithaka => AgeIds.ITHAKA,
            _ => AgeIds.BRONZE_AGE,
        };
    }

    public static CityId ToDefaultTechnologyCity(this CityId cityId)
    {
        return cityId switch
        {
            CityId.Mayas_Tikal or CityId.Mayas_ChichenItza or CityId.Mayas_SayilPalace => CityId.Mayas_ChichenItza,
            CityId.Arabia_CityOfBrass or CityId.Arabia_NoriasOfHama or CityId.Arabia_Petra => CityId.Arabia_Petra,
            CityId.Ithaka => CityId.Ithaka,
            _ => cityId,
        };
    }

    public static string GetIcon(this CityId cityId)
    {
        return cityId switch
        {
            CityId.Mayas_Tikal => "icon_city_crest_maya",
            CityId.Mayas_ChichenItza => "icon_city_crest_maya",
            CityId.Mayas_SayilPalace => "icon_city_crest_maya",
            CityId.China => "icon_city_crest_china",
            CityId.Vikings => "icon_city_crest_vikings",
            CityId.Egypt => "icon_city_crest_egypt",
            CityId.Arabia_CityOfBrass => "icon_city_crest_arabia",
            CityId.Arabia_NoriasOfHama => "icon_city_crest_arabia",
            CityId.Arabia_Petra => "icon_city_crest_arabia",
            CityId.AncientEgyptEvent => "icon_city_crest_ancient_egypt",
            CityId.Ithaka => "icon_city_crest_ithaka",
            _ => string.Empty,
        };
    }

    public static string ToInGameId(this CityId cityId)
    {
        return $"city.City_{cityId}";
    }
}
