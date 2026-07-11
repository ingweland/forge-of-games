using Ingweland.Fog.Application.Client.Web.Providers.Interfaces;
using Ingweland.Fog.Application.Client.Web.Settings;
using Ingweland.Fog.Models.Hoh.Enums;
using Microsoft.Extensions.Options;

namespace Ingweland.Fog.Application.Client.Web.Providers;

public class AssetUrlProvider(IOptionsSnapshot<AssetsSettings> assetsSettings) : IAssetUrlProvider
{
    public string GetHohIconUrl(string assetId, string extension)
    {
        return GetAssetUrl(assetsSettings.Value.HohIconsPath, $"{assetId}{extension}");
    }

    public string GetHohIconUrl(string assetId)
    {
        return GetAssetUrl(assetsSettings.Value.HohIconsPath, $"{assetId}.png");
    }

    public string GetHohPlayerAvatarUrl(string avatarId)
    {
        return GetAssetUrl(assetsSettings.Value.HohPlayerAvatarsPath, $"{avatarId}.png");
    }

    public string GetHohTechnologyImageUrl(string assetId, string extension)
    {
        return GetAssetUrl(assetsSettings.Value.HohImagesBasePath, "technologies", $"{assetId}{extension}");
    }

    public string GetHohTechnologyImageUrl(string assetId)
    {
        return GetAssetUrl(assetsSettings.Value.HohImagesBasePath, "technologies", $"{assetId}.png");
    }

    public string GetHohImageUrl(string assetId, string extension)
    {
        return GetAssetUrl(assetsSettings.Value.HohImagesBasePath, $"{assetId}{extension}");
    }

    public string GetIconUrl(string assetId)
    {
        return GetAssetUrl(assetsSettings.Value.ImagesBasePath, "icons", $"{assetId}.png");
    }

    public string GetIconUrl(string assetId, string extension)
    {
        return GetAssetUrl(assetsSettings.Value.ImagesBasePath, "icons", $"{assetId}{extension}");
    }

    public string GetHohImageUrl(string assetId)
    {
        return GetAssetUrl(assetsSettings.Value.HohImagesBasePath, $"{assetId}.png");
    }

    public string GetImageUrl(string filename)
    {
        return GetAssetUrl(assetsSettings.Value.ImagesBasePath, filename);
    }

    public string GetHohUnitPortraitUrl(string assetId, string extension)
    {
        return GetAssetUrl(assetsSettings.Value.HohUnitImagesPath, $"{assetId}{extension}");
    }

    public string GetHohUnitPortraitUrl(string assetId)
    {
        return GetAssetUrl(assetsSettings.Value.HohUnitImagesPath, $"{assetId}.png");
    }

    public string GetHohUnitImageUrl(string assetId)
    {
        return GetAssetUrl(assetsSettings.Value.HohUnitImagesPath, $"{assetId}_fullbody.png");
    }

    public string GetHohStatAttributeIconUrl(StatAttribute statAttribute)
    {
        var suffixLenght = "Bonus".Length;
        return statAttribute switch
        {
            StatAttribute.AttackBonus or StatAttribute.BaseDamageBonus or StatAttribute.DefenseBonus
                or StatAttribute.MaxHitPointsBonus => GetAssetUrl(assetsSettings.Value.HohIconsPath,
                    $"icon_unit_stat_{statAttribute.ToString()[..^suffixLenght]}_percent.png"),
            _ => GetAssetUrl(assetsSettings.Value.HohIconsPath, $"icon_unit_stat_{statAttribute}.png"),
        };
    }

    public string GetHohUnitVideoUrl(string assetId, string extension)
    {
        return GetAssetUrl(assetsSettings.Value.HohUnitVideosPath, $"{assetId}{extension}");
    }

    public string GetHohUnitVideoUrl(string assetId)
    {
        return GetAssetUrl(assetsSettings.Value.HohUnitVideosPath, $"{assetId}.mp4");
    }

    public (string Image, string Meta) GetHohIconAtlasUrl()
    {
        var fileName = $"hoh-icons-{assetsSettings.Value.HohIconAtlasHash}";
        return (GetAssetUrl(assetsSettings.Value.HohIconsPath, $"{fileName}.png"),
            GetAssetUrl(assetsSettings.Value.HohIconsPath, $"{fileName}.json"));
    }

    public string GetHohUnitStatIconUrl(UnitStatType unitStatType)
    {
        return GetAssetUrl(assetsSettings.Value.HohIconsPath, $"icon_unit_stat_{unitStatType}.png");
    }

    public string GetHohRelicIconUrl(string relicId)
    {
        return GetAssetUrl(assetsSettings.Value.HohIconsPath, $"full_relic_{relicId}.png");
    }

    public string GetHohTreasureHuntLeagueIconUrl(TreasureHuntLeague league)
    {
        return GetAssetUrl(assetsSettings.Value.HohIconsPath, $"icon_ath_tier_0{(int) league + 1}.png");
    }

    public string GetHohWoaLeagueIconUrl(WoaTier tier)
    {
        return GetAssetUrl(assetsSettings.Value.HohIconsPath, $"icon_woa_tier_{(int) tier}.png");
    }

    public string GetHohHeroAbilityIconUrl(string heroAbilityId)
    {
        var iconId = heroAbilityId.Replace('.', '_').ToLowerInvariant();
        return GetAssetUrl(assetsSettings.Value.HohIconsPath, $"icon_{iconId}.png");
    }

    public string GetFontUrl(string font, string locale)
    {
        var filename = locale switch
        {
            "ja-JP" => "NotoSansJP-Regular.ttf",
            "zh-TW" => "NotoSansTC-Regular.ttf",
            "ko-KR" => "NotoSansKR-Regular.ttf",
            _ => $"{font}.ttf",
        };
        var basePath = assetsSettings.Value.BaseUrl.TrimEnd('/');
        return string.Join("/", new[] {basePath, assetsSettings.Value.Fonts.Trim('/'), filename});
    }

    public string GetHohEquipmentSetIconUrl(EquipmentSet equipmentSet)
    {
        return GetAssetUrl(assetsSettings.Value.HohIconsPath,
            $"icon_equipmentset_{equipmentSet.ToString().ToLowerInvariant()}.png");
    }

    public string GetHohEquipmentIconUrl(EquipmentSet equipmentSet, EquipmentSlotType slot)
    {
        return GetAssetUrl(assetsSettings.Value.HohIconsPath,
            $"icon_equipment_{equipmentSet.ToString().ToLowerInvariant()}_{slot.ToString().ToLowerInvariant()}.png");
    }

    private string GetAssetUrl(params string[] pathElements)
    {
        var cleanElements = pathElements
            .Where(x => !string.IsNullOrEmpty(x))
            .Select(x => x.Trim('/'));

        var basePath = assetsSettings.Value.BaseUrl.TrimEnd('/');

        var fullPath = string.Join("/",
            new[] {basePath, assetsSettings.Value.Version}
                .Concat(cleanElements));

        return fullPath;
    }
}
