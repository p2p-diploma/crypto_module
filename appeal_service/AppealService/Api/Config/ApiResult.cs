using System.Net;

namespace AppealService.Api.Config;

public record ApiResult(HttpStatusCode? StatusCode, string Message);