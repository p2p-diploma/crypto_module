using Microsoft.AspNetCore.Mvc;

namespace Crypto.Server.Filters;

public class TokenAuthorizeAttribute : TypeFilterAttribute
{
    public TokenAuthorizeAttribute(string role)
        : base(typeof(AuthorizationFilter))
    {
        Arguments = new object[] { role };
    }
}