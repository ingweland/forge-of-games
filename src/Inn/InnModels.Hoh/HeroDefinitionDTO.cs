namespace Ingweland.Fog.Inn.Models.Hoh;

public sealed partial class HeroDefinitionDTO
{
    public string ClassId => PackedComponents.Where(any => any.Is(GameDesignReference.Descriptor))
        .Select(any =>
        {
            var gameDesignReference = any.Unpack<GameDesignReference>();
            return gameDesignReference.Type.EndsWith("HeroClassDefinitionDTO") ? gameDesignReference.Id : null;
        }).Single(s => s != null)!;
    
    public string AwakeningId => PackedComponents.Where(any => any.Is(GameDesignReference.Descriptor))
        .Select(any =>
        {
            var gameDesignReference = any.Unpack<GameDesignReference>();
            return gameDesignReference.Type.EndsWith("HeroAwakeningComponentDTO") ? gameDesignReference.Id : null;
        }).Single(s => s != null)!;
    
    public string AbilityId => PackedComponents.Where(any => any.Is(GameDesignReference.Descriptor))
        .Select(any =>
        {
            var gameDesignReference = any.Unpack<GameDesignReference>();
            return gameDesignReference.Type.EndsWith("HeroBattleAbilityComponentDTO") ? gameDesignReference.Id : null;
        }).Single(s => s != null)!;

    public HeroProgressionComponentDTO ProgressionComponent => PackedComponents
        .Single(any => any.Is(HeroProgressionComponentDTO.Descriptor)).Unpack<HeroProgressionComponentDTO>();
}
