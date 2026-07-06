using Ingweland.Fog.Models.Hoh.Enums;

namespace Ingweland.Fog.Application.Core.Extensions;

public static class WonderIdExtensions
{
    public static CityId ToCity(this WonderId wonderId)
    {
        return wonderId switch
        {
            WonderId.China_ForbiddenCity or WonderId.China_GreatWall or WonderId.China_TerracottaArmy => CityId.China,
            WonderId.Egypt_AbuSimbel or WonderId.Egypt_CheopsPyramid or WonderId.Egypt_GreatSphinx => CityId.Egypt,
            WonderId.Mayas_ChichenItza => CityId.Mayas_ChichenItza,
            WonderId.Mayas_SayilPalace => CityId.Mayas_SayilPalace,
            WonderId.Mayas_Tikal => CityId.Mayas_Tikal,
            WonderId.Vikings_DragonshipEllida or WonderId.Vikings_Valhalla
                or WonderId.Vikings_Yggdrasil => CityId.Vikings,
            WonderId.Arabia_CityOfBrass => CityId.Arabia_CityOfBrass,
            WonderId.Arabia_NoriasOfHama => CityId.Arabia_NoriasOfHama,
            WonderId.Arabia_Petra => CityId.Arabia_Petra,
            WonderId.AncientEgyptEvent_AnubisTemple => CityId.AncientEgyptEvent,
            WonderId.Ithaka_PenelopesHearth => CityId.Ithaka,
            _ => CityId.Undefined,
        };
    }

    public static string GetImageFileName(this WonderId wonderId)
    {
        return $"banner_wonder_{wonderId}".ToLowerInvariant();
    }
}
