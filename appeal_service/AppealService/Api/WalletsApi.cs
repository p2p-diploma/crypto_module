using System.Net;
using AppealService.Api.Config;
using AppealService.Api.Infrastructure;
using AppealService.Api.Models;
using Microsoft.Extensions.Options;

namespace AppealService.Api;

public class WalletsApi : BaseHttpClientFactory
{
    private readonly ApiSettings _settings;
    public WalletsApi(IOptions<ApiSettings> settings, IConfiguration conf) : base(conf)
    {
        _settings = settings.Value;
        _builder = new(_settings.WalletsPath);
    }

    public async Task<ApiResult> FreezeWallets(string? accessToken, string sellerEmail, CancellationToken token)
    {
        try
        {
            var walletMessage = _builder.HttpMethod(HttpMethod.Get)
                .AddToPath($"/email/{sellerEmail}").AddQueryString("includeBalance", "false").HttpMessage;
            var sellerWallet = await GetResponseAsync<Wallet>(walletMessage, accessToken, token);
            
           var freezeRequest = _builder.SetPath($"/freeze/{sellerWallet.Id}").HttpMethod(HttpMethod.Put).HttpMessage;
           var message = await GetResponseStringAsync(freezeRequest, accessToken, token);
           return new ApiResult(HttpStatusCode.OK, message);
        }
        catch (HttpRequestException e)
        {
            return new ApiResult(e.StatusCode, e.Message);
        }
    }
}