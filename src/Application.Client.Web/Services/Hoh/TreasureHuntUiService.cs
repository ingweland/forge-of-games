using System.Collections.ObjectModel;
using AutoMapper;
using Ingweland.Fog.Application.Client.Web.Factories.Interfaces;
using Ingweland.Fog.Application.Client.Web.Services.Hoh.Abstractions;
using Ingweland.Fog.Application.Client.Web.ViewModels.Hoh.Battle;
using Ingweland.Fog.Application.Core.Services.Hoh.Abstractions;

namespace Ingweland.Fog.Application.Client.Web.Services.Hoh;

public class TreasureHuntUiService(
    ITreasureHuntService treasureHuntService,
    ITreasureHuntStageViewModelFactory treasureHuntStageViewModelFactory,
    IMapper mapper)
    : ITreasureHuntUiService
{
    private readonly Dictionary<(int difficulty, int stageIndex), TreasureHuntStageViewModel> _stages = new();
    private IReadOnlyCollection<TreasureHuntDifficultyBasicViewModel>? _difficulties;
    private IReadOnlyDictionary<int, int>? _difficultyMaxProgressPointsMap;
    private IReadOnlyDictionary<(int difficulty, int stage), ReadOnlyDictionary<int, int>>? _treasureHuntEncounterMap;

    public int GetDifficultyMaxProgressPoints(int difficulty)
    {
        return (10 + 5 * difficulty) * 80;
    }

    public async Task<IReadOnlyCollection<TreasureHuntDifficultyBasicViewModel>> GetDifficultiesAsync()
    {
        if (_difficulties != null)
        {
            return _difficulties;
        }

        var difficulties = await treasureHuntService.GetDifficultiesAsync();
        _difficulties =
            mapper.Map<IReadOnlyCollection<TreasureHuntDifficultyBasicViewModel>>(
                difficulties.OrderBy(x => x.Difficulty));
        return _difficulties;
    }

    public async Task<TreasureHuntStageViewModel?> GetStageAsync(int difficulty, int stageIndex)
    {
        var key = (difficulty, stageIndex);
        if (_stages.TryGetValue(key, out var cachedStage))
        {
            return cachedStage;
        }

        var stage = await treasureHuntService.GetStageAsync(difficulty, stageIndex);
        if (stage == null)
        {
            return null;
        }

        var stageViewModel = treasureHuntStageViewModelFactory.Create(stage);
        _stages[key] = stageViewModel;
        return stageViewModel;
    }

    public async Task<IReadOnlyDictionary<(int difficulty, int stage), ReadOnlyDictionary<int, int>>>
        GetBattleEncounterToIndexMapAsync()
    {
        if (_treasureHuntEncounterMap != null)
        {
            return _treasureHuntEncounterMap;
        }

        var encounters = await treasureHuntService.GetTreasureHuntEncountersBasicDataAsync();
        _treasureHuntEncounterMap = encounters
            .GroupBy(src => (src.Difficulty, src.Stage))
            .ToDictionary(g => g.Key, g => g.ToDictionary(src => src.Encounter, src => src.Index).AsReadOnly())
            .AsReadOnly();

        return _treasureHuntEncounterMap;
    }

    public async Task<IReadOnlyDictionary<int, int>> GetDifficultyMaxProgressPointsMapAsync()
    {
        if (_difficultyMaxProgressPointsMap != null)
        {
            return _difficultyMaxProgressPointsMap;
        }

        _ = await GetDifficultiesAsync();

        _difficultyMaxProgressPointsMap =
            _difficulties!.ToDictionary(x => x.Difficulty, x => GetDifficultyMaxProgressPoints(x.Difficulty));

        return _difficultyMaxProgressPointsMap;
    }
}
