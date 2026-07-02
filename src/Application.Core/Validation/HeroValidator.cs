using Ingweland.Fog.Application.Core.Interfaces;

namespace Ingweland.Fog.Application.Core.Validation;

public class HeroValidator : IHeroValidator
{
    public bool IsValidHero(string queryString)
    {
        // Some units are located in a Hero slot. However, they are not regular player's heroes.
        if (queryString == "unit.Unit_SpartasLastStand_Leonidas_1" ||
            queryString.Contains("Unit_FallOfTroy_Barricade") || queryString.Contains("Unit_FallOfTroy_Gate") ||
            queryString.Contains("Unit_Anubis_Boss") || queryString.Contains("Unit_Scylla_Boss") || 
            queryString.Contains("Unit_ScyllaMaw_Boss"))
        {
            return false;
        }

        return true;
    }
}
