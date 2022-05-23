using Microsoft.AspNetCore.SignalR;
using StackExchange.Redis;

namespace ChatService.Hubs;

public class ChatHub : Hub
{
    private readonly IDatabase db;
    public ChatHub(IConnectionMultiplexer redis) => db = redis.GetDatabase();

    public async Task Send(string message, string username)
    {
        var lotId = await db.StringGetAsync(Context.ConnectionId);
        await Clients.Group(lotId).SendAsync("Send", message, username);
    }

    public async Task AddToChat(string lotId)
    {
        await db.StringSetAsync(Context.ConnectionId, lotId).ContinueWith(_ => Groups.AddToGroupAsync(Context.ConnectionId, lotId));
    }

    public async Task EndChat(string lotId)
    {
        await db.KeyDeleteAsync(Context.ConnectionId)
            .ContinueWith(_ => Groups.RemoveFromGroupAsync(Context.ConnectionId, lotId));
    }
}