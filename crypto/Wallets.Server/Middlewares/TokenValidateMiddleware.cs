using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace Wallets.Server.Middlewares;

public class TokenValidateMiddleware
{
    private readonly RequestDelegate _next;

    public TokenValidateMiddleware(RequestDelegate next)
    {
        _next = next;
    }
    
    public async Task InvokeAsync(HttpContext context)
    {
        var token = context.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
        if (AuthorizedAsUser(token) || AuthorizedAsAdmin(token))
            await _next(context);
        await context.ForbidAsync();
    }
}