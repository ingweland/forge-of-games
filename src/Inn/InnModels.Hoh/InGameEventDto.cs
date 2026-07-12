using Google.Protobuf;

namespace Ingweland.Fog.Inn.Models.Hoh;

public sealed partial class InGameEventDto
{
    public T? GetState<T>(string key) where T : IMessage<T>, new()
    {
        return !ComponentStates.TryGetValue(key, out var packedState) ? default : packedState.Unpack<T>();
    }

    public T? GetState<T>(params string[] keys) where T : IMessage<T>, new()
    {
        foreach (var key in keys)
        {
            var state = GetState<T>(key);
            if (state != null)
            {
                return state;
            }
        }

        return default;
    }
}
