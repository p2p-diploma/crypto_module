using Microsoft.AspNetCore.Mvc;

namespace AppealService.Filters;

public class RoleAuthorizeAttribute : TypeFilterAttribute
{
    public RoleAuthorizeAttribute(string role)
        : base(typeof(RoleAuthorizationFilter))
    {
        Arguments = new object[] { role };
    }
}