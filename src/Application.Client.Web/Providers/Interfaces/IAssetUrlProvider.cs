using Ingweland.Fog.Models.Hoh.Enums;

namespace Ingweland.Fog.Application.Client.Web.Providers.Interfaces;

public interface IAssetUrlProvider
{
    string GetHohEquipmentIconUrl(EquipmentSet equipmentSet, EquipmentSlotType slot);
    string GetHohHeroAbilityIconUrl(string heroAbilityId);
    string GetHohIconUrl(string assetId, string extension);
    string GetHohIconUrl(string assetId);
    string GetHohImageUrl(string assetId, string extension);
    string GetIconUrl(string assetId);
    string GetIconUrl(string assetId, string extension);
    string GetHohImageUrl(string assetId);
    string GetHohPlayerAvatarUrl(string avatarId);
    string GetHohTechnologyImageUrl(string assetId, string extension);
    string GetHohTechnologyImageUrl(string assetId);
    string GetHohUnitImageUrl(string assetId);
    string GetHohUnitPortraitUrl(string assetId, string extension);
    string GetHohUnitPortraitUrl(string assetId);
    string GetHohUnitStatIconUrl(UnitStatType unitStatType);
    string GetHohStatAttributeIconUrl(StatAttribute statAttribute);
    string GetHohUnitVideoUrl(string assetId, string extension);
    string GetHohUnitVideoUrl(string assetId);
    (string Image, string Meta) GetHohIconAtlasUrl();
    string GetFontUrl(string font, string locale);
    string GetImageUrl(string filename);
    string GetHohEquipmentSetIconUrl(EquipmentSet equipmentSet);
    string GetHohRelicIconUrl(string relicId);
    string GetHohTreasureHuntLeagueIconUrl(TreasureHuntLeague league);
    string GetHohWoaLeagueIconUrl(WoaTier tier);
}
