using Microsoft.AspNetCore.SignalR;
using StackExchange.Redis;

namespace ChatService.Hubs;

public class ChatHub : Hub
{
    private readonly IDatabase db;
    private readonly ILogger<ChatHub> _logger;
    public ChatHub(IConnectionMultiplexer redis, ILogger<ChatHub> logger)
    {
        _logger = logger;
        db = redis.GetDatabase();
    }

    public async Task Send(string message, string username)
    {
        var lotId = await db.StringGetAsync(Context.ConnectionId);
        _logger.LogInformation($"Found chat {lotId} from {Context.ConnectionId}");
        await Clients.Group(lotId).SendAsync("Send", message, username);
    }

    public async Task AddToChat(string lotId)
    {
        await db.StringSetAsync(Context.ConnectionId, lotId).ContinueWith(_ => Groups.AddToGroupAsync(Context.ConnectionId, lotId));
        _logger.LogInformation($"Adding {Context.ConnectionId} to chat {lotId}");
    }

    public async Task EndChat(string lotId)
    {
        await db.KeyDeleteAsync(Context.ConnectionId)
            .ContinueWith(_ => Groups.RemoveFromGroupAsync(Context.ConnectionId, lotId));
        _logger.LogInformation($"Removing {Context.ConnectionId} from chat {lotId}");
    }
}