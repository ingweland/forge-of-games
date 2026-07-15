using System.Collections;
using AutoMapper;
using Ingweland.Fog.Application.Client.Web.Caching.Interfaces;
using Ingweland.Fog.Application.Client.Web.Factories.Interfaces;
using Ingweland.Fog.Application.Client.Web.Models;
using Ingweland.Fog.Application.Client.Web.Services.Abstractions;
using Ingweland.Fog.Application.Client.Web.Services.Hoh.Abstractions;
using Ingweland.Fog.Application.Client.Web.StatsHub.Abstractions;
using Ingweland.Fog.Application.Client.Web.StatsHub.ViewModels;
using Ingweland.Fog.Application.Client.Web.ViewModels.Hoh.Battle;
using Ingweland.Fog.Application.Client.Web.ViewModels.Hoh.Units;
using Ingweland.Fog.Application.Core.Constants;
using Ingweland.Fog.Application.Core.Services.Hoh.Abstractions;
using Ingweland.Fog.Dtos.Hoh;
using Ingweland.Fog.Dtos.Hoh.Battle;
using Ingweland.Fog.Dtos.Hoh.Stats;
using Ingweland.Fog.Models.Fog;
using Ingweland.Fog.Models.Fog.Enums;
using Ingweland.Fog.Models.Hoh.Enums;
using Ingweland.Fog.Shared.Constants;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace Ingweland.Fog.Application.Client.Web.StatsHub;

public class StatsHubUiService : UiServiceBase, IStatsHubUiService
{
    private const string TOP_PLAYERS_KEY = "topPlayers";
    private const string TOP_ALLIANCES_KEY = "topAlliances";
    private const string TOP_EVENT_CITIES_KEY = "topEventCities";

    private readonly Lazy<Task<IReadOnlyDictionary<string, AgeDto>>> _ages;
    private readonly IAllianceAthRankingViewModelFactory _allianceAthRankingViewModelFactory;
    private readonly IAllianceWoaRankingViewModelFactory _allianceWoaRankingViewModelFactory;
    private readonly IBattleService _battleService;
    private readonly IBattleViewModelFactory _battleViewModelFactory;
    private readonly IPlayerCityPropertiesViewModelFactory _cityPropertiesViewModelFactory;
    private readonly ICommonService _commonService;
    private readonly ICommonUiService _commonUiService;
    private readonly IHohCoreDataCache _coreDataCache;
    private readonly IHeroProfileUiService _heroProfileUiService;
    private readonly IMapper _mapper;
    private readonly IMemoryCache _memoryCache;
    private readonly IPlayerAthRankingViewModelFactory _playerAthRankingViewModelFactory;
    private readonly IPlayerCityStrategyInfoViewModelFactory _playerCityStrategyInfoViewModelFactory;
    private readonly IStatsHubService _statsHubService;
    private readonly IStatsHubViewModelsFactory _statsHubViewModelsFactory;
    private readonly ITreasureHuntUiService _treasureHuntUiService;
    private readonly IWoaPlayerStatsViewModelFactory _woaPlayerStatsViewModelFactory;

    public StatsHubUiService(IStatsHubService statsHubService,
        ICommonService commonService,
        IStatsHubViewModelsFactory statsHubViewModelsFactory,
        ITreasureHuntUiService treasureHuntUiService,
        IBattleService battleService,
        IBattleViewModelFactory battleViewModelFactory,
        IHohCoreDataCache coreDataCache,
        IMapper mapper,
        IAllianceAthRankingViewModelFactory allianceAthRankingViewModelFactory,
        IAllianceWoaRankingViewModelFactory allianceWoaRankingViewModelFactory,
        ICommonUiService commonUiService,
        IPlayerCityPropertiesViewModelFactory cityPropertiesViewModelFactory,
        IPlayerCityStrategyInfoViewModelFactory playerCityStrategyInfoViewModelFactory,
        IPlayerAthRankingViewModelFactory playerAthRankingViewModelFactory,
        IWoaPlayerStatsViewModelFactory woaPlayerStatsViewModelFactory,
        IHeroProfileUiService heroProfileUiService,
        ILogger<StatsHubUiService> logger,
        IMemoryCache memoryCache) : base(logger)
    {
        _statsHubService = statsHubService;
        _commonService = commonService;
        _statsHubViewModelsFactory = statsHubViewModelsFactory;
        _treasureHuntUiService = treasureHuntUiService;
        _battleService = battleService;
        _battleViewModelFactory = battleViewModelFactory;
        _coreDataCache = coreDataCache;
        _mapper = mapper;
        _allianceAthRankingViewModelFactory = allianceAthRankingViewModelFactory;
        _allianceWoaRankingViewModelFactory = allianceWoaRankingViewModelFactory;
        _commonUiService = commonUiService;
        _cityPropertiesViewModelFactory = cityPropertiesViewModelFactory;
        _playerCityStrategyInfoViewModelFactory = playerCityStrategyInfoViewModelFactory;
        _memoryCache = memoryCache;
        _playerAthRankingViewModelFactory = playerAthRankingViewModelFactory;
        _woaPlayerStatsViewModelFactory = woaPlayerStatsViewModelFactory;
        _heroProfileUiService = heroProfileUiService;

        _ages = new Lazy<Task<IReadOnlyDictionary<string, AgeDto>>>(GetAgesAsync);
    }

    public async Task<PlayerProfileViewModel?> GetPlayerProfileAsync(int playerId)
    {
        var player = await _statsHubService.GetPlayerProfileAsync(playerId);
        if (player == null)
        {
            return null;
        }

        var heroIds = player.Squads.Select(x => x.Hero.UnitId).ToHashSet();
        var heroes = await _coreDataCache.GetHeroes(heroIds);
        var treasureHuntDifficulties = await _treasureHuntUiService.GetDifficultiesAsync();
        var playerTreasureHuntDifficulty = player.TreasureHuntDifficulty != null
            ? treasureHuntDifficulties.FirstOrDefault(x => x.Difficulty == player.TreasureHuntDifficulty.Value)
            : null;
        var treasureHuntMaxPoints = playerTreasureHuntDifficulty != null
            ? _treasureHuntUiService.GetDifficultyMaxProgressPoints(playerTreasureHuntDifficulty.Difficulty)
            : 0;
        return _statsHubViewModelsFactory.CreatePlayerProfile(player, heroes, await _ages.Value,
            await _coreDataCache.GetBarracksByUnitMapAsync(),
            playerTreasureHuntDifficulty, treasureHuntMaxPoints, await _coreDataCache.GetRelicsAsync());
    }

    public async Task<PlayerViewModel?> GetPlayerAsync(int playerId, CancellationToken ct = default)
    {
        var player = await _statsHubService.GetPlayerAsync(playerId, ct);
        if (player == null)
        {
            return null;
        }

        var ages = await _ages.Value;
        return _mapper.Map<PlayerViewModel>(player, opt => { opt.Items[ResolutionContextKeys.AGES] = ages; });
    }

    public async Task<PaginatedList<PvpBattleViewModel>> GetPlayerBattlesAsync(PlayerViewModel player,
        int startIndex, int count,
        CancellationToken ct = default)
    {
        var result = await _statsHubService.GetPlayerBattlesAsync(player.Id, startIndex, count, ct);

        var heroIds = result.Items.SelectMany(b => b.WinnerUnits.Select(u => u.Hero!.UnitId))
            .Concat(result.Items.SelectMany(b => b.LoserUnits.Select(u => u.Hero!.UnitId)))
            .ToHashSet();
        var heroes = await _coreDataCache.GetHeroes(heroIds);
        var ages = await _ages.Value;
        var barracks = await _coreDataCache.GetBarracksByUnitMapAsync();
        var relics = await _coreDataCache.GetRelicsAsync();
        var newBattles = result.Items.Select(x =>
            _battleViewModelFactory.CreatePvpBattle(player, x, heroes, ages, barracks, relics)).ToList();
        return new PaginatedList<PvpBattleViewModel>(newBattles, result.StartIndex, result.TotalCount);
    }

    public async Task<AllianceProfileViewModel?> GetAllianceAsync(int allianceId)
    {
        var alliance = await _statsHubService.GetAllianceAsync(allianceId);
        if (alliance == null)
        {
            return null;
        }

        var treasureHuntDifficulties = await _treasureHuntUiService.GetDifficultiesAsync();
        var maxPoints = await _treasureHuntUiService.GetDifficultyMaxProgressPointsMapAsync();

        return _statsHubViewModelsFactory.CreateAlliance(alliance, await _ages.Value, treasureHuntDifficulties,
            maxPoints);
    }

    public async Task<IReadOnlyCollection<AllianceAthRankingViewModel>> GetAllianceAthRankingsAsync(int allianceId)
    {
        var leaguesTask = _commonUiService.GetTreasureHuntLeaguesAsync();
        var rankings = await _statsHubService.GetAllianceAthRankingsAsync(allianceId);
        var leagues = await leaguesTask;

        return rankings.OrderBy(x => x.StartedAt).Select(x => _allianceAthRankingViewModelFactory.Create(x,
            leagues.GetValueOrDefault(x.League,
                new TreasureHuntLeagueDto {League = TreasureHuntLeague.Undefined, Name = string.Empty}))).ToList();
    }

    public Task<IReadOnlyCollection<PlayerAthRankingViewModel>> GetPlayerAthRankingsAsync(int playerId,
        CancellationToken ct = default)
    {
        return ExecuteSafeAsync<IReadOnlyCollection<PlayerAthRankingViewModel>>(
            async () =>
            {
                var rankings = await _statsHubService.GetPlayerAthRankingsAsync(playerId, ct);
                return rankings.OrderBy(x => x.StartedAt).Select(x => _playerAthRankingViewModelFactory.Create(x))
                    .ToList();
            },
            []);
    }

    public async Task<IReadOnlyCollection<PvpRankingViewModel>> GetPlayerPvpRankingsAsync(int playerId)
    {
        var rankings = await _statsHubService.GetPlayerPvpRankingsAsync(playerId);

        return _mapper.Map<IReadOnlyCollection<PvpRankingViewModel>>(rankings.OrderBy(x => x.CollectedAt));
    }

    public async Task<PaginatedList<AllianceViewModel>> GetAllianceStatsAsync(string worldId, int startIndex,
        int pageSize,
        string? allianceName = null, CancellationToken ct = default)
    {
        var result =
            await _statsHubService.GetAlliancesAsync(worldId, startIndex, pageSize, allianceName, ct);
        return _statsHubViewModelsFactory.CreateAlliances(result);
    }

    public async Task<PaginatedList<PlayerViewModel>> GetPlayerStatsAsync(string worldId, int startIndex, int pageSize,
        string? playerName = null, CancellationToken ct = default)
    {
        var result =
            await _statsHubService.GetPlayersAsync(worldId, startIndex, pageSize, playerName, ct);
        return _statsHubViewModelsFactory.CreatePlayers(result, await _ages.Value);
    }

    public async Task<IReadOnlyList<BattleSummaryViewModel>> SearchBattles(
        BattleSearchRequest request, CancellationToken ct = default)
    {
        var result = await _battleService.SearchBattlesAsync(request, ct);

        return await _battleViewModelFactory.CreateBattleSummaryViewModels(result.Battles, request.BattleType);
    }

    public async Task<PaginatedList<AllianceViewModel>> GetAlliancesAthRankingsAsync(string worldId, int startIndex,
        int pageSize, TreasureHuntLeague league, CancellationToken ct = default)
    {
        var result = await _statsHubService.GetAlliancesAthRankingsAsync(worldId, startIndex, pageSize, league, ct);
        return _statsHubViewModelsFactory.CreateAlliances(result);
    }

    public async Task<PaginatedList<AllianceViewModel>> GetAlliancesWoaRankingsAsync(string worldId, int startIndex,
        int pageSize, WoaPointsCategory pointsCategory,
        CancellationToken ct = default)
    {
        var result =
            await _statsHubService.GetAlliancesWoaRankingsAsync(worldId, startIndex, pageSize, pointsCategory, ct);
        return _statsHubViewModelsFactory.CreateAlliances(result);
    }

    public async Task<PaginatedList<PlayerViewModel>> GetEventCityRankingsAsync(string worldId,
        CancellationToken ct = default)
    {
        return await ExecuteSafeAsync(
            async () =>
            {
                var result = await _statsHubService.GetEventCityRankingsAsync(worldId, ct);
                return _statsHubViewModelsFactory.CreatePlayers(result, await _ages.Value);
            },
            PaginatedList<PlayerViewModel>.Empty);
    }

    public async Task<IReadOnlyCollection<PlayerViewModel>> GetTopEventCitiesAsync(string worldId,
        CancellationToken ct = default)
    {
        return await GetOrCreateAsync($"{TOP_EVENT_CITIES_KEY}:{worldId}", () => ExecuteSafeAsync(
            async () =>
            {
                var players = await GetEventCityRankingsAsync(worldId, ct);
                return players.Items.Take(FogConstants.DEFAULT_STATS_PAGE_SIZE).ToList();
            },
            []));
    }

    public async Task<IReadOnlyCollection<AllianceViewModel>> GetTopAlliancesAsync(string worldId,
        CancellationToken ct = default)
    {
        return await GetOrCreateAsync($"{TOP_ALLIANCES_KEY}:{worldId}", () => ExecuteSafeAsync(
            async () =>
            {
                var result = await _statsHubService.GetTopAlliancesAsync(worldId, ct);
                return _statsHubViewModelsFactory.CreateAlliances(result).ToList();
            },
            []));
    }

    public async Task<IReadOnlyCollection<AllianceViewModel>> GetTopAlliancesAthRankingsAsync(string worldId,
        TreasureHuntLeague league, CancellationToken ct = default)
    {
        return await ExecuteSafeAsync(
            async () =>
            {
                var result = await _statsHubService.GetAlliancesAthRankingsAsync(worldId, 0,
                    FogConstants.DEFAULT_STATS_PAGE_SIZE, league, ct);
                return _statsHubViewModelsFactory.CreateAlliances(result.Items);
            },
            []);
    }

    public async Task<IReadOnlyCollection<PlayerViewModel>> GetTopPlayersAsync(string worldId,
        CancellationToken ct = default)
    {
        return await GetOrCreateAsync($"{TOP_PLAYERS_KEY}:{worldId}", () => ExecuteSafeAsync(
            async () =>
            {
                var result = await _statsHubService.GetTopPlayersAsync(worldId, ct);
                return _statsHubViewModelsFactory.CreatePlayers(result, await _ages.Value).ToList();
            },
            []));
    }

    public Task<IReadOnlyCollection<StatsTimedIntValue>> GetAllianceRankingsAsync(int allianceId,
        CancellationToken ct = default)
    {
        return ExecuteSafeAsync(
            () => _statsHubService.GetAllianceRankingsAsync(allianceId, ct),
            []);
    }

    public Task<IReadOnlyCollection<StatsTimedIntValue>> GetPlayerRankingsAsync(int playerId,
        CancellationToken ct = default)
    {
        return ExecuteSafeAsync(
            () => _statsHubService.GetPlayerRankingsAsync(playerId, ct),
            []);
    }

    public async Task<PlayerCityPropertiesViewModel?> GetPlayerCityPropertiesAsync(int playerId,
        CancellationToken ct = default)
    {
        var result = await ExecuteSafeAsync(
            () => _statsHubService.GetPlayerProductionCapacityAsync(playerId, ct),
            null);

        return result != null ? _cityPropertiesViewModelFactory.Create(result) : null;
    }

    public async Task<IReadOnlyCollection<AllianceWoaRankingViewModel>> GetAllianceWoaRankingsAsync(int allianceId,
        CancellationToken ct = default)
    {
        var tiersTask = _commonUiService.GetWoaTiersAsync();
        var rankings = await _statsHubService.GetAllianceWoaRankingsAsync(allianceId, ct);
        var tiers = await tiersTask;

        return rankings.OrderBy(x => x.StartedAt).Select(x => _allianceWoaRankingViewModelFactory.Create(x,
            tiers.GetValueOrDefault(x.Tier, WoaTierDto.Default))).ToList();
    }

    public Task<IReadOnlyCollection<WoaPlayerStatsViewModel>> GetWoaPlayerStatsAsync(int playerId,
        CancellationToken ct = default)
    {
        return ExecuteSafeAsync<IReadOnlyCollection<WoaPlayerStatsViewModel>>(
            async () =>
            {
                var stats = await _statsHubService.GetWoaPlayerStatsAsync(playerId, ct);
                return stats.OrderBy(x => x.StartedAt).Select(x => _woaPlayerStatsViewModelFactory.Create(x)).ToList();
            },
            []);
    }

    public Task<IReadOnlyCollection<HeroBasicViewModel>> GetPlayerHeroesAsync(int playerId, CancellationToken ct)
    {
        return ExecuteSafeAsync<IReadOnlyCollection<HeroBasicViewModel>>(
            async () =>
            {
                var playerHeroes = await _statsHubService.GetPlayerHeroesAsync(playerId, ct);
                if (playerHeroes.Count == 0)
                {
                    return [];
                }

                var heroes = await _heroProfileUiService.GetHeroes(HeroFilterRequest.Empty, playerHeroes.ToHashSet());
                return heroes.OrderBy(x => x.Name).ToList();
            },
            []);
    }

    public async Task<IReadOnlyCollection<AllianceViewModel>> GetTopAlliancesWoaRankingsAsync(string worldId,
        WoaPointsCategory pointsCategory, CancellationToken ct = default)
    {
        return await ExecuteSafeAsync(
            async () =>
            {
                var result = await _statsHubService.GetAlliancesWoaRankingsAsync(worldId, 0,
                    FogConstants.DEFAULT_STATS_PAGE_SIZE, pointsCategory, ct);
                return _statsHubViewModelsFactory.CreateAlliances(result.Items);
            },
            []);
    }

    private async Task<T> GetOrCreateAsync<T>(string key, Func<Task<T>> factory) where T : ICollection, new()
    {
        return (await _memoryCache.GetOrCreateAsync(key, async entry =>
        {
            var result = await factory();

            if (result.Count == 0)
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMilliseconds(1);
                return [];
            }

            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1);
            return result;
        }))!;
    }

    private async Task<IReadOnlyDictionary<string, AgeDto>> GetAgesAsync()
    {
        return (await _commonService.GetAgesAsync()).ToDictionary(a => a.Id);
    }
}
