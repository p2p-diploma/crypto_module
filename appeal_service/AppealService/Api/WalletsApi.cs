using System.Net;
using System.Net.Http.Headers;
using AppealService.Api.Config;
using AppealService.Api.Infrastructure;
using AppealService.Api.Models;
using Microsoft.Extensions.Options;

namespace AppealService.Api;

public class WalletsApi : BaseHttpClientFactory
{
    private readonly ApiSettings _settings;
    public WalletsApi(IHttpClientFactory factory, IOptions<ApiSettings> settings) : base(factory)
    {
        _settings = settings.Value;
        _builder = new(_settings.WalletsPath);
    }

    public async Task<ApiResult> FreezeWallets(string accessToken, string sellerEmail, CancellationToken token)
    {
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        try
        {
            var sellerWallet = await GetResponseAsync<Wallet>(_builder.HttpMethod(HttpMethod.Get)
                .AddToPath($"/email/{sellerEmail}")
                .AddQueryString("includeBalance", "false").HttpMessage, token);
            
           var freezeRequest = _builder.SetPath($"/freeze/{sellerWallet.Id}").HttpMethod(HttpMethod.Put).HttpMessage;
           var message = await GetResponseStringAsync(freezeRequest, token);
           return new ApiResult(HttpStatusCode.OK, message);
        }
        catch (HttpRequestException e)
        {
            return new ApiResult(e.StatusCode, e.Message);
        }
    }
}