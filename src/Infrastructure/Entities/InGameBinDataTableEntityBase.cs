using System.Runtime.Serialization;
using Ingweland.Fog.Shared.Utils;

namespace Ingweland.Fog.Infrastructure.Entities;

public abstract class InGameBinDataTableEntityBase : TableEntityBase
{
    private const int MaxSegmentsCount = 17;
    private const int SegmentSizeBytes = 60 * 1000;

    [IgnoreDataMember]
    public byte[] Data
    {
        get => CompressionUtils.Decompress(CombineSegments());
        set
        {
            var compressed = CompressionUtils.Compress(value);
            SplitIntoSegments(compressed);
        }
    }

    public required DateTime CollectedAt { get; init; }

    public byte[] CompressedData { get; set; } = null!;
    public byte[]? CompressedData1 { get; set; }
    public byte[]? CompressedData2 { get; set; }
    public byte[]? CompressedData3 { get; set; }
    public byte[]? CompressedData4 { get; set; }
    public byte[]? CompressedData5 { get; set; }
    public byte[]? CompressedData6 { get; set; }
    public byte[]? CompressedData7 { get; set; }
    public byte[]? CompressedData8 { get; set; }
    public byte[]? CompressedData9 { get; set; }
    public byte[]? CompressedData10 { get; set; }
    public byte[]? CompressedData11 { get; set; }
    public byte[]? CompressedData12 { get; set; }
    public byte[]? CompressedData13 { get; set; }
    public byte[]? CompressedData14 { get; set; }
    public byte[]? CompressedData15 { get; set; }
    public byte[]? CompressedData16 { get; set; }

    protected void SplitIntoSegments(byte[] compressed)
    {
        var chunks = compressed.Chunk(SegmentSizeBytes).ToArray();

        if (chunks.Length > MaxSegmentsCount)
        {
            throw new ArgumentOutOfRangeException(
                $"Data exceeded max allowed size in bytes: got {compressed.Length}, max {MaxSegmentsCount * SegmentSizeBytes}");
        }

        for (var i = 0; i < chunks.Length; i++)
        {
            switch (i)
            {
                case 0: CompressedData = chunks[i]; break;
                case 1: CompressedData1 = chunks[i]; break;
                case 2: CompressedData2 = chunks[i]; break;
                case 3: CompressedData3 = chunks[i]; break;
                case 4: CompressedData4 = chunks[i]; break;
                case 5: CompressedData5 = chunks[i]; break;
                case 6: CompressedData6 = chunks[i]; break;
                case 7: CompressedData7 = chunks[i]; break;
                case 8: CompressedData8 = chunks[i]; break;
                case 9: CompressedData9 = chunks[i]; break;
                case 10: CompressedData10 = chunks[i]; break;
                case 11: CompressedData11 = chunks[i]; break;
                case 12: CompressedData12 = chunks[i]; break;
                case 13: CompressedData13 = chunks[i]; break;
                case 14: CompressedData14 = chunks[i]; break;
                case 15: CompressedData15 = chunks[i]; break;
                case 16: CompressedData16 = chunks[i]; break;
            }
        }
    }

    private List<byte[]?> GetSegments()
    {
        return
        [
            CompressedData, CompressedData1, CompressedData2, CompressedData3, CompressedData4,
            CompressedData5, CompressedData6, CompressedData7, CompressedData8, CompressedData9,
            CompressedData10, CompressedData11, CompressedData12, CompressedData13, CompressedData14,
            CompressedData15, CompressedData16,
        ];
    }

    protected byte[] CombineSegments()
    {
        return GetSegments()
            .Where(segment => segment != null)
            .SelectMany(segment => segment!)
            .ToArray();
    }
}
