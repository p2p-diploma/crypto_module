using System.IdentityModel.Tokens.Jwt;

namespace Crypto.Server;

public static class TokenValidator
{
    public static bool AuthorizedAsUser(string token)
    {
        if (string.IsNullOrEmpty(token)) return false;
        var handler = new JwtSecurityTokenHandler();
        if (!handler.CanReadToken(token)) return false;
        var user = handler.ReadJwtToken(token);
        string? roles = user.Claims.FirstOrDefault(x => x.Type == "role")?.Value;
        return roles is "user";
    }
    
    public static bool AuthorizedAsAdmin(string token)
    {
        if (string.IsNullOrEmpty(token)) return false;
        var handler = new JwtSecurityTokenHandler();
        if (!handler.CanReadToken(token)) return false;
        var user = handler.ReadJwtToken(token);
        string? roles = user.Claims.FirstOrDefault(x => x.Type == "role")?.Value;
        return roles is "admin";
    }
}