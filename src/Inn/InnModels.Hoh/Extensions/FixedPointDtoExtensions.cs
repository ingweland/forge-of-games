namespace Ingweland.Fog.Inn.Models.Hoh.Extensions;

public static class FixedPointDtoExtensions
{
    private const float ONE = 65536f;

    /// <summary>
    ///     Converts the game's 16.16 fixed-point value to a float. An absent value is 0.
    /// </summary>
    public static float ToFloat(this FixedPointDTO? value)
    {
        return value?.RawValue / ONE ?? 0f;
    }
}
