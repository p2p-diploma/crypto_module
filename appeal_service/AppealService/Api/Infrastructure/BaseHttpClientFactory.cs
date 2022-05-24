using System.Net;

namespace AppealService.Api.Infrastructure;

public abstract class BaseHttpClientFactory
{
    protected HttpRequestBuilder _builder;

    public BaseHttpClientFactory(IConfiguration config)
    {
        _builder = new HttpRequestBuilder(config["ApiSettings:BaseAddress"]);
    }

    public virtual async Task<T?> GetResponseAsync<T>(HttpRequestMessage request, string? accessToken, CancellationToken token) where T : class
    {
        var cookies = new CookieContainer();
        var baseAddress = new Uri(_builder.BaseAddress);
        using var handler = new HttpClientHandler { CookieContainer = cookies };
        cookies.Add(baseAddress, new Cookie("jwt-access", accessToken));
        using var client = new HttpClient(handler){ BaseAddress = baseAddress };
        using var response = await client.SendAsync(request, token);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<T>(cancellationToken: token);
    }

    public virtual async Task<string> GetResponseStringAsync(HttpRequestMessage request, string? accessToken, CancellationToken token)
    {
        var cookies = new CookieContainer();
        var baseAddress = new Uri(_builder.BaseAddress);
        using var handler = new HttpClientHandler { CookieContainer = cookies };
        cookies.Add(baseAddress, new Cookie("jwt-access", accessToken));
        using var client = new HttpClient(handler){ BaseAddress = baseAddress };
        using var response = await client.SendAsync(request, token);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync(token);
    }
}