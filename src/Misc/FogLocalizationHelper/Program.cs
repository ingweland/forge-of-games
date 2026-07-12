using Ingweland.Fog.Inn.Models.Hoh;
using Ingweland.Fog.Shared.Localization;
using Newtonsoft.Json;

var searchKeys = new HashSet<string>
{
    "Base.CampaignPanel.Campaign",
    "Base.QuestlinesPanel.EncounterOfRegion",
    "Base.HeroPanel.Ability",
    "Base.HeroPanel.AwakeningSuccessful",
    "Base.BuildingTypes.barracks_Name",
    "Base.ShopPopup.Heroes",
    "Base.BattleStatsPanel.Hero",
    "Base.HeroPanel.LVL",
    "Base.HeroFilterPopover.Level",
    "Base.HeroFilterPopover.AbilityLevel",
    "Base.Buildings.default_Name",
    "Base.TreasureHunt.Header",
    "Base.UnlockableFeatures.Wonders",
    "Base.WorldWondersInventionInformationPopup.WonderRewardLabel",
    "Base.Generic.MainCity",
    "Base.Generic.Resource",
    "Base.WorldWondersDetailPanel.WonderHeader",
    "Base.ResearchTreePanel.Header",
    "Base.BuildingContextPanel.Customization",
    "Base.BuildingContextPanel.ProductionLabel",
    "Base.Generic.Cancel",
    "Base.BuildingContextPanel.InfoTab",
    "Base.CampaignPanel.Battle",
    "Base.UnlockableFeatures.Pvp",
    "Base.HistoricBattlePanel.HistoricBattle",
    "Base.TeslaStormsPanel.TeslaStorms",
    "Base.CampaignPanel.Difficulty",
    "Base.TreasureHuntDifficultiesPanel.Stage",
    "Base.AllianceMemberContextPopover.Visit",
    "Base.Resources.coins_Name",
    "Base.Resources.food_Name",
    "Base.Generic.Good",
    "Base.BuildPanel.ExpansionCategory",
    "Base.ExpansionPopup.PremiumHeader",
    "Base.HeroPanel.BaseStats",
    "Base.HeroPanel.Equipment",
    "Base.EquipmentFilterPopover.SelectMainAttributes",
    "Base.EquipmentFilterPopover.SelectSubAttributes",
    "SoftError.AllianceCity.PROCESS_SLOT_NOT_UNLOCKED",
    "Base.EquipmentSets.equipment_set.Knight_Name",
    "Base.EquipmentFilterPopover.RarityAndEnhancementLevel",
    "Base.UnitStats.unit_stat.Attack_Percent_Abbrev",
    "Base.UnitStats.unit_stat.Attack_Abbrev",
    "Base.UnitStats.unit_stat.Defense_Percent_Abbrev",
    "Base.UnitStats.unit_stat.Defense_Abbrev",
    "Base.UnitStats.unit_stat.MaxHitPoints_Abbrev",
    "Base.UnitStats.unit_stat.MaxHitPoints_Percent_Abbrev",
    "Base.UnitStats.unit_stat.BaseDamage_Percent_Abbrev",
    "Base.UnitStats.unit_stat.CritDamage_Abbrev",
    "Base.UnitStats.unit_stat.InitialFocusInSecondsBonus_Abbrev",
    "Base.UnitStats.unit_stat.CritChance_Abbrev",
    "Base.UnitStats.unit_stat.AttackSpeed_Abbrev",
    "Base.HeroPanel.Relics",
    "Base.TreasureHuntRankingPanel.TreasureHuntRanking",
    "Base.PvpPanel.Ranking",
    "Base.TreasureHuntRankingPanel.TreasureHuntLeague",
    "Base.LeaderboardEventPanel.TaskInfo.EventCity_Name",
    "Base.Subscriptions.PRODUCTION_DURATION_Name",
    "Base.Technologies.Technology_Egypt_Storage_Name",
    "Base.BuildingGroups.premiumCulture_Name",
    "Base.BuildingGroups.premiumHome_Name",
    "Base.BuildingGroups.premiumFarm_Name",
    "Base.RewardPanel.CityUnlocked",
    "Base.Technologies.Technology_BronzeAge_EventCities_Name",
    "Base.WorldWondersOverviewPanel.ActiveWondersInAlliedCulture",
    "Base.EquipmentFilterPopover.HideEquipped",
    "Base.HeroPanel.Equipped",
    "Base.EquipmentLevelUpPanel.EquippedHint",
    "Base.BattleEventPanel.BattleEvent",
    "Base.IngameEvents.BattleEvent_AncientEgyptEvent_AnubisDungeon_Name",
    "Base.EventDungeonInfoPopup.BattleEvent_AncientEgyptEvent_AnubisDungeon.BossHeader",
    "Base.UnlockableFeatures.EventCities",
    "Base.HarborShopPanel.Atlantis.ButtonLabel",
    "Base.WoA.Header",
    "Base.WoARankingPanel.Contributed",
    "Base.WoARankingPanel.HealingDone",
    "Base.WoARankingPanel.RepairsStarted",
    "Base.WoA.VictoryPointsTitle",
    "Base.WoARankingPanel.AttacksStarted",
    "Tutorial.JoystickBattle.LostOverlayTitle",
    "Base.PvpFightPanel.Attack",
};

var result = new Dictionary<string, List<Translations>>();
foreach (var localeCode in HohSupportedCultures.AllCultures)
{
    var filePath = GetInputFilePath(localeCode);

    using var localizationFile = File.OpenRead(filePath);
    var data = CommunicationDto.Parser.ParseFrom(localizationFile).LocaResponse.Entries
        .ToDictionary(entry => entry.Key, entry => entry.Values);

    var translations = searchKeys.Select(searchKey => new Translations {Key = searchKey, Strings = data[searchKey]})
        .ToList();

    result.Add(localeCode, translations);
}

File.WriteAllText("translations.json", JsonConvert.SerializeObject(result, Formatting.Indented));

static string GetInputFilePath(string localeCode)
{
    var dir = @"D:\Temp\";
    var fileName = $"loca_{localeCode}.bin";
    return $"{dir}{fileName}";
}

internal class Translations
{
    public string Key { get; set; }
    public IList<string> Strings { get; set; }
}
