using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;

namespace LoyaltyApp.Services;

public sealed class JwtAuthenticationStateProvider : AuthenticationStateProvider
{
    private readonly ProtectedLocalStorage _storage;
    private readonly ILogger<JwtAuthenticationStateProvider> _log;

    private string JwtToken { get; set; }

    public JwtAuthenticationStateProvider(
        ProtectedLocalStorage storage,
        ILogger<JwtAuthenticationStateProvider> log)
    {
        _storage = storage;
        _log = log;
    }

    public async Task SetTokenAsync(string token)
    {
        JwtToken = token;
        if (!string.IsNullOrWhiteSpace(token))
        {
            await _storage.SetAsync("authToken", token);
        }
        else
        {
            await _storage.DeleteAsync("authToken");
        }
        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        if (JwtToken is null)
        {
            try
            {
                // JS interop not available during static prerendering
                var result = await _storage.GetAsync<string>("authToken");
                if (result.Success && !string.IsNullOrWhiteSpace(result.Value))
                {
                    JwtToken = result.Value;
                }
            }
            catch (CryptographicException)
            {
                await _storage.DeleteAsync("authToken");
            }
            catch (InvalidOperationException ex)
            {
                _log.LogWarning(ex, "JS interop недоступен во время пререндеринга.");
                return new AuthenticationState(new ClaimsPrincipal());
            }
        }

        var identity = new ClaimsIdentity();
        if (string.IsNullOrWhiteSpace(JwtToken))
        {
            return new AuthenticationState(new ClaimsPrincipal());
        }

        string logoutReason = null;

        try
        {
            var handler = new JwtSecurityTokenHandler();
            var jwt = handler.ReadJwtToken(JwtToken);
            if (jwt.ValidTo < DateTime.UtcNow)
            {
                await SetTokenAsync(null);
                logoutReason = "Сессия истекла. Пожалуйста, войдите заново.";
            }

            else
            {
                identity = new ClaimsIdentity(jwt.Claims, "jwt");
            }
        }
        catch (Exception ex)
        {
            _log.LogError(ex, "Некорректный JWT токен в хранилище.");
            await SetTokenAsync(null);
            logoutReason = "Ошибка авторизации. Пожалуйста, войдите заново.";
            JwtToken = null;
        }

        if (logoutReason != null)
        {
            await _storage.SetAsync("authLogoutReason", logoutReason);
        }

        var resultState = new AuthenticationState(new ClaimsPrincipal(identity));
        NotifyAuthenticationStateChanged(Task.FromResult(resultState));
        return resultState;
    }
}