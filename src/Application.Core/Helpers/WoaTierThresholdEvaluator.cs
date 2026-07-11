using Ingweland.Fog.Application.Core.Constants;
using Ingweland.Fog.Application.Core.Interfaces;
using Ingweland.Fog.Models.Hoh.Enums;

namespace Ingweland.Fog.Application.Core.Helpers;

public class WoaTierHelper : IWoaTierHelper
{
    public WoaTier GetTier(int points)
    {
        if (points < 0)
        {
            return WoaTier.Undefined;
        }

        var thresholds = HohConstants.WoaBracketTierThresholds;
        for (var i = thresholds.Length - 1; i >= 0; i--)
        {
            if (points >= thresholds[i])
            {
                return (WoaTier) i;
            }
        }

        return WoaTier.Undefined;
    }
}
