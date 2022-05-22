using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Crypto.Server.Filters;

public class AuthorizationFilter : ActionFilterAttribute, IAuthorizationFilter
{
    private readonly string _role;
    private readonly string _refreshPath;
    public AuthorizationFilter(string role, IConfiguration config)
    {
        _role = role;
        _refreshPath = config["RefreshTokenPath"];
    }
    public void OnAuthorization(AuthorizationFilterContext context)
    {
        string? access_token = context.HttpContext.Request.Cookies["jwt-access"];

        if (string.IsNullOrEmpty(access_token)) { context.Result = new ChallengeResult(); return; }
        var handler = new JwtSecurityTokenHandler();
        if (!handler.CanReadToken(access_token)) { context.Result = new ChallengeResult(); return; }
        var user = handler.ReadJwtToken(access_token);
        
        string? role = user.Claims.FirstOrDefault(x => x.Type == "role")?.Value;
        if (role == null || role != _role) { context.Result = new ForbidResult(); return; }
        string? exp = user.Claims.FirstOrDefault(x => x.Type == "exp")?.Value;
        if (exp == null) { context.Result = new UnauthorizedResult(); return; }
        
        var tokenExpired = DateTime.Now - DateTime.Parse(exp);
        if (tokenExpired <= TimeSpan.FromMinutes(2))
        {
            using var client = new HttpClient();
            client.Send(new HttpRequestMessage(HttpMethod.Get, _refreshPath));
        }
    }
}