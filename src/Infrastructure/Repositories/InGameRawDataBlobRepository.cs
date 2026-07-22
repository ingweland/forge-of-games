using System.Text.Json;
using Azure.Storage.Blobs;
using Ingweland.Fog.Application.Server.Interfaces.Hoh;
using Ingweland.Fog.Models.Fog.Entities;
using Ingweland.Fog.Shared.Utils;

namespace Ingweland.Fog.Infrastructure.Repositories;

public class InGameRawDataBlobRepository(string connectionString, string containerName)
    : IInGameRawDataBlobRepository
{
    private readonly Lazy<BlobContainerClient> _blobContainerClient =
        new(() => new BlobContainerClient(connectionString, containerName));

    public async Task<string> SaveAsync(InGameRawData data, string partitionKey)
    {
        var blobName = $"{partitionKey}/{Guid.NewGuid()}.json.gz";
        var compressed = CompressionUtils.CompressString(JsonSerializer.Serialize(data));
        var blobClient = _blobContainerClient.Value.GetBlobClient(blobName);
        await blobClient.UploadAsync(new BinaryData(compressed), false);
        return blobName;
    }
}
