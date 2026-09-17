using System.Collections.ObjectModel;
using AutoMapper;
using Ingweland.Fog.Inn.Models.Hoh;
using Ingweland.Fog.Inn.Models.Hoh.Extensions;
using Ingweland.Fog.Models.Hoh.Entities.Units;
using Ingweland.Fog.Models.Hoh.Enums;
using Ingweland.Fog.Shared.Helpers;

namespace Ingweland.Fog.HohCoreDataParserSdk.Converters;

public class
    HeroUnitStatFormulaDefinitionConverter : ITypeConverter<HeroUnitStatFormulaDefinitionDTO, UnitStatFormulaData>
{
    public UnitStatFormulaData Convert(HeroUnitStatFormulaDefinitionDTO source, UnitStatFormulaData destination,
        ResolutionContext context)
    {
        var rarityFactors = new Dictionary<string, UnitStatFormulaFactors>();
        //rarityFactors = source.RarityUnits.ToDictionary(dto => dto.RarityId, dto => dto.Factors);
        return new UnitStatFormulaData
        {
            Type = HohStringParser.ParseEnumFromString<UnitStatFormulaType>(source.Id),
            BaseFactor = source.Unit.Normal.ToFloat(),
            RarityFactors =
                new ReadOnlyDictionary<string, UnitStatFormulaFactors>(
                    new Dictionary<string, UnitStatFormulaFactors>()),
        };
    }
}
