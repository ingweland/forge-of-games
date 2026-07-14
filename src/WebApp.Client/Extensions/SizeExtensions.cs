using MudBlazor;

namespace Ingweland.Fog.WebApp.Client.Extensions;

public static class SizeExtensions
{
    public static string GetDescription(this Size size) => size switch
    {
        Size.Small => "small",
        Size.Medium => "medium",
        Size.Large => "large",
        _ => size.ToString().ToLowerInvariant(),
    };
}
