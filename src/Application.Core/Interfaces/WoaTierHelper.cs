using Ingweland.Fog.Models.Hoh.Enums;

namespace Ingweland.Fog.Application.Core.Interfaces;

public interface IWoaTierHelper
{
    WoaTier GetTier(int points);
}
