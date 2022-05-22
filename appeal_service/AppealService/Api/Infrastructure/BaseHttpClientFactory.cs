using System.Text.Json;

namespace AppealService.Api.Infrastructure;

public abstract class BaseHttpClientFactory
{
    private readonly IHttpClientFactory _factory;
    protected HttpRequestBuilder _builder;
    public BaseHttpClientFactory(IHttpClientFactory factory)
    {
        _factory = factory;
    }
    public HttpClient Client => _factory.CreateClient();

    public virtual async Task<T?> GetResponseAsync<T>(HttpRequestMessage request, CancellationToken token) where T : class
    {
        using var client = Client;
        using var response = await client.SendAsync(request, token);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<T>(cancellationToken: token);
    }

    public virtual async Task<string> GetResponseStringAsync(HttpRequestMessage request, CancellationToken token)
    {
        using var client = Client;
        using var response = await client.SendAsync(request, token);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync(token);
    }
}