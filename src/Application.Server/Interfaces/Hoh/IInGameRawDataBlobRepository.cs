using Ingweland.Fog.Models.Fog.Entities;

namespace Ingweland.Fog.Application.Server.Interfaces.Hoh;

public interface IInGameRawDataBlobRepository
{
    Task<string> SaveAsync(InGameRawData data, string partitionKey);
}
