using Microsoft.AspNetCore.Mvc;

namespace Wallets.Server.Filters;

public class TokenAuthorizeAttribute : TypeFilterAttribute
{
    public TokenAuthorizeAttribute(string role)
        : base(typeof(AuthorizationFilter))
    {
        Arguments = new object[] { role };
    }
}