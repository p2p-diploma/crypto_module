using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.IdentityModel.Tokens;

namespace Crypto.Server.Filters;

public class AuthorizationFilter : ActionFilterAttribute, IAuthorizationFilter
{
    private readonly string _role;
    private readonly string secretKey;
    public AuthorizationFilter(string role, IConfiguration configuration)
    {
        _role = role;
        secretKey = configuration["SecretKey"];
    }

    public void OnAuthorization(AuthorizationFilterContext context)
    {
        var isAuthorized = false;
        var access_token = context.HttpContext.Request.Cookies["jwt-access"];
        if (!string.IsNullOrEmpty(access_token))
        {
            var handler = new JwtSecurityTokenHandler();
            if (handler.CanReadToken(access_token))
            {
                var parameters = new TokenValidationParameters
                {
                    ValidateActor = false, ValidateIssuer = false, ValidateLifetime = false, ValidateAudience = false,
                    ValidateIssuerSigningKey = true, IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey))
                };
                SecurityToken? validatedToken;
                try { handler.ValidateToken(access_token, parameters, out validatedToken); } catch { validatedToken = null; }
                if (validatedToken != null)
                {
                    var user = validatedToken as JwtSecurityToken;
                    var role = user?.Claims.FirstOrDefault(x => x.Type == "role")?.Value;
                    if (role != null && role == _role)
                    {
                        var exp = user?.Claims.FirstOrDefault(x => x.Type == "exp")?.Value;
                        if (exp != null)
                        {
                            var leftExpiryTime = DateTimeOffset.FromUnixTimeSeconds(long.Parse(exp)).DateTime;
                            if (leftExpiryTime < DateTime.Now || leftExpiryTime - DateTime.Now > TimeSpan.FromMinutes(2))
                            {
                                isAuthorized = true;
                            }
                        }
                    }
                }
            }
        }
        if (isAuthorized == false)
            context.Result = new UnauthorizedResult();
    }
}