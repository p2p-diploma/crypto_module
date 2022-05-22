﻿using Microsoft.AspNetCore.Mvc;

namespace Wallets.Server.Filters;

public class RoleAuthorizeAttribute : TypeFilterAttribute
{
    public RoleAuthorizeAttribute(string role)
        : base(typeof(RoleAuthorizationFilter))
    {
        Arguments = new object[] { role };
    }
}