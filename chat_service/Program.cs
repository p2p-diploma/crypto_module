using ChatService.Hubs;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

#region Chat provider
builder.Services.AddSignalR();
#endregion
#region Redis
ConnectionMultiplexer.Connect(builder.Configuration["ConnectionStrings:Redis"]).GetDatabase().StringSet("a", "b");
builder.Services.AddSingleton<IConnectionMultiplexer>(ConnectionMultiplexer.Connect(builder.Configuration["ConnectionStrings:Redis"]));
#endregion
var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseHttpsRedirection();
app.UseCors(b => b.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod().AllowCredentials());
app.UseAuthorization();
app.MapControllers();
app.MapHub<ChatHub>("/chat");
app.Run();