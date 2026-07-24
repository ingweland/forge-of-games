using System.Net.Http.Json;
using System.Text.RegularExpressions;
using Ingweland.Fog.InnSdk.Hoh.Authentication.Abstractions;
using Ingweland.Fog.InnSdk.Hoh.Authentication.Models;
using Ingweland.Fog.InnSdk.Hoh.Constants;
using Microsoft.Extensions.Logging;

namespace Ingweland.Fog.InnSdk.Hoh.Authentication;

public class WebAuthenticationService(
    IWebAuthPayloadFactory authPayloadFactory,
    HttpClient httpClient,
    IWebAuthenticationResponseHandler authenticationResponseHandler,
    IGameCredentialsManager credentialsManager,
    IGameConnectionManager connectionManager,
    ILogger<WebAuthenticationService> logger) : IWebAuthenticationService
{
    public async Task Authenticate(GameWorldConfig world)
    {
        logger.LogInformation("Authenticating on {WorldId}", world.Id);

        var credentials = await credentialsManager.GetAsync(world);
        if (credentials == null)
        {
            throw new AuthenticationException(AuthErrorCode.InvalidCredentials,
                $"Could not find credentials for {world.Id}");
        }

        var payload = authPayloadFactory.CreateForLogin(credentials.Username, credentials.Password);
        //login
        var url = string.Format(GameEndpoints.WebLoginUrl, world.SignInSubdomain);
        using var loginRequest = new HttpRequestMessage(HttpMethod.Post, url);
        loginRequest.Content = JsonContent.Create(payload);
        // The game now requires a Cookie header on the initial call.
        loginRequest.Headers.TryAddWithoutValidation("Cookie", "FoG");
        var response = await httpClient.SendAsync(loginRequest);
        try
        {
            response.EnsureSuccessStatusCode();
        }
        catch (Exception e)
        {
            throw new AuthenticationException(AuthErrorCode.UnexpectedError, "Could not sign in", e);
        }

        var loginResponse = await response.Content.ReadFromJsonAsync<LoginResponse>();

        //redirect
        var redirectedPage = await httpClient.GetStringAsync(loginResponse!.RedirectUrl);
        var pattern = @"const\s+clientVersion\s*=\s*""([^""]+)"";";
        var match = Regex.Match(redirectedPage, pattern);

        string clientVersion;
        if (match.Success)
        {
            clientVersion = match.Groups[1].Value;
        }
        else
        {
            throw new AuthenticationException(AuthErrorCode.ClientVersionNotFound, "Could not find client version");
        }

        //get auth token
        url = string.Format(GameEndpoints.AccountPlayUrl, world.Server);
        var accountRawResponse = await httpClient.PostAsJsonAsync(url, authPayloadFactory.CreateForPlay(clientVersion));
        try
        {
            accountRawResponse.EnsureSuccessStatusCode();
        }
        catch (Exception e)
        {
            throw new AuthenticationException(AuthErrorCode.UnexpectedError, "Could not get account play response", e);
        }

        var playResponse = await accountRawResponse.Content.ReadFromJsonAsync<AccountPlayResponse>();
        connectionManager.AddOrUpdate(
            authenticationResponseHandler.HandleResponse(playResponse!, world.Id, clientVersion));
    }
}
