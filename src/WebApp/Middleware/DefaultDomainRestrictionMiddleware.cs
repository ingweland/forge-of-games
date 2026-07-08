using System.Net;
using System.Net.Sockets;
using Ingweland.Fog.Application.Server.Settings;
using Microsoft.Extensions.Options;

namespace Ingweland.Fog.WebApp.Middleware;

public class DefaultDomainRestrictionMiddleware : IDisposable
{
    private const string BLOCKED_WEBSITE = ".azurewebsites.net";
    private const string MAIN_WEBSITE = "https://forgeofgames.com";
    private readonly RequestDelegate _next;
    private readonly IDisposable? _optionsChangeToken;
    private List<IPNetwork> _allowedNetworks;
    private bool _disposed;

    private DefaultDomainRestrictionSettings _settings;

    public DefaultDomainRestrictionMiddleware(RequestDelegate next,
        IOptionsMonitor<DefaultDomainRestrictionSettings> optionsMonitor)
    {
        _next = next;
        _settings = optionsMonitor.CurrentValue;
        _allowedNetworks = ParseAllowedNetworks(_settings.AllowedIPs);
        _optionsChangeToken = optionsMonitor.OnChange((options, _) =>
        {
            if (!_disposed)
            {
                _settings = options;
                _allowedNetworks = ParseAllowedNetworks(options.AllowedIPs);
            }
        });
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        _optionsChangeToken?.Dispose();
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var isDefaultDomain = context.Request.Host.Host.EndsWith(BLOCKED_WEBSITE, StringComparison.OrdinalIgnoreCase);

        if (_settings.Enabled && isDefaultDomain)
        {
            var clientIP = context.Connection.RemoteIpAddress;

            // Dual-stack sockets can report an IPv4 client as an IPv4-mapped IPv6
            // address (::ffff:203.0.113.10), which won't match an IPv4 CIDR entry
            // unless it's normalized back first.
            if (clientIP is {IsIPv4MappedToIPv6: true})
            {
                clientIP = clientIP.MapToIPv4();
            }

            if (clientIP != null && _allowedNetworks.Any(network => network.Contains(clientIP)))
            {
                await _next(context);
                return;
            }

            context.Response.Redirect(MAIN_WEBSITE, true);
            return;
        }

        await _next(context);
    }

    private static List<IPNetwork> ParseAllowedNetworks(IEnumerable<string> entries)
    {
        var networks = new List<IPNetwork>();

        foreach (var entry in entries)
        {
            if (TryParseEntry(entry, out var network))
            {
                networks.Add(network);
            }
        }

        return networks;
    }

    private static bool TryParseEntry(string entry, out IPNetwork network)
    {
        // Plain IP, no "/" -> treat it as an exact match (a /32 or /128 network)
        if (!entry.Contains('/') && IPAddress.TryParse(entry, out var ip))
        {
            var prefixLength = ip.AddressFamily == AddressFamily.InterNetworkV6 ? 128 : 32;
            network = new IPNetwork(ip, prefixLength);
            return true;
        }

        // Otherwise expect CIDR notation, e.g. 198.51.100.0/24
        return IPNetwork.TryParse(entry, out network);
    }
}
