﻿using AppealService.Api.Config;
using AppealService.Api.Infrastructure;
using AppealService.Api.Models;
using Microsoft.Extensions.Options;

namespace AppealService.Api;

public class UsersApi : BaseHttpClientFactory
{
    private readonly ApiSettings _settings;
    public UsersApi(IOptions<ApiSettings> settings)
    {
        _settings = settings.Value;
        _builder = new(_settings.UsersAddress);
        _builder.AddToPath(_settings.UsersPath);
    }

    public async Task<User?> GetByEmail(string email, string? accessToken, CancellationToken token)
    {
        /*using var message = _builder.HttpMethod(HttpMethod.Get).AddToPath($"/{email}").HttpMessage;
        return await GetResponseAsync<User>(message, accessToken, token);*/
        return await Task.FromResult(new User
        {
            Email = email, FullName = "Test test"
        });
    }
}