using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace AppealService.Filters;

public class RoleAuthorizationFilter : ActionFilterAttribute, IAuthorizationFilter
{
    private readonly string _role;
    public RoleAuthorizationFilter(string role)
    {
        _role = role;
    }
    public void OnAuthorization(AuthorizationFilterContext context)
    {
        var token = context.HttpContext.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
        if (!AuthorizedAs(_role, token))
            context.Result = new ChallengeResult();
    }
    private bool AuthorizedAs(string role, string token)
    {
        if (string.IsNullOrEmpty(token)) return false;
        var handler = new JwtSecurityTokenHandler();
        if (!handler.CanReadToken(token)) return false;
        var user = handler.ReadJwtToken(token);
        string? roles = user.Claims.FirstOrDefault(x => x.Type == "role")?.Value;
        return roles != null && roles == role;
    }
}