using Microsoft.AspNetCore.Mvc;

namespace Wallets.Server.Filters;

public class JwtAuthorizeAttribute : TypeFilterAttribute
{
    public JwtAuthorizeAttribute(string role)
        : base(typeof(CookieJwtAuthorizationFilter))
    {
        Arguments = new object[] { role };
    }
}