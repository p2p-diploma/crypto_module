using AppealService.Api.Config;
using AppealService.Api.Infrastructure;
using AppealService.Api.Models;
using Microsoft.Extensions.Options;

namespace AppealService.Api;

public class TransactionsApi : BaseHttpClientFactory
{
    private readonly ApiSettings _settings;
    public TransactionsApi(IOptions<ApiSettings> settings)
    {
        _settings = settings.Value;
        _builder = new(_settings.TradesAddress);
        _builder.AddToPath(_settings.TradesPath);
    }

    public async Task<TransactionResponse?> GetById(string transactionId, string accessToken, CancellationToken token)
    {
        using var message = _builder.AddToPath($"/{transactionId}").HttpMethod(HttpMethod.Get).HttpMessage;
        return await GetResponseAsync<TransactionResponse>(message, accessToken, token);
    }
}