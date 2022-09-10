using Crypto.Domain.Exceptions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Wallets.Server.Filters;

public class ExceptionHandleFilter : IExceptionFilter
{
    public void OnException(ExceptionContext context)
    {
        context.Result = context.Exception switch
        {
            ArgumentException e => new BadRequestObjectResult(e.Message),
            AccountBalanceException e => new BadRequestObjectResult(e.Message),
            NotFoundException => new NoContentResult(),
            AccountLockedException e => new BadRequestObjectResult(e.Message),
            _ => new StatusCodeResult(500)
        };
    }
}