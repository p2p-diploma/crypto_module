using Crypto.Application.ERC20;
using Crypto.Application.Ethereum;
using Crypto.Data.Configuration;
using Crypto.Data.Repositories;
using Crypto.Domain.Configuration;
using Crypto.Domain.Interfaces;
using Crypto.Domain.Models;
using Crypto.Server.Validators.ERC20;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers()
    .AddFluentValidation(opt => opt.RegisterValidatorsFromAssemblyContaining<Program>())
    .ConfigureApiBehaviorOptions(opt => {
        opt.InvalidModelStateResponseFactory = context => new BadRequestObjectResult(context.ModelState.Values.First(q => q.Errors.Count > 0).Errors.First(er => !string.IsNullOrEmpty(er.ErrorMessage)).ErrorMessage);
});
#region Configuration
builder.Services.Configure<BlockchainConnections>(builder.Configuration.GetSection("BlockchainConnections"));
builder.Services.Configure<SmartContractSettings>(builder.Configuration.GetSection("SmartContractSettings"));
builder.Services.Configure<DatabaseSettings>(builder.Configuration.GetSection("DatabaseSettings"));
#endregion
#region Data
builder.Services.AddSingleton<IMongoClient>(new MongoClient(builder.Configuration["DatabaseSettings:ConnectionString"]));
builder.Services.AddScoped<IWalletsRepository<EthereumWallet<ObjectId>, ObjectId>>(opt =>
{
    var settings = opt.GetRequiredService<IOptions<DatabaseSettings>>();
    var client = opt.GetRequiredService<IMongoClient>();
    return new EthereumWalletsRepository(settings.Value, client);
});
#endregion
#region Ethereum
builder.Services.AddTransient(opt =>
{
    var connections = opt.GetRequiredService<IOptions<BlockchainConnections>>().Value;
    return new EthereumAccountManager(connections.Ganache, connections);
});
builder.Services.AddScoped<EthereumWalletService>();
builder.Services.AddScoped(opt =>
{
    var settings = opt.GetRequiredService<IOptions<SmartContractSettings>>().Value;
    var repository = opt.GetRequiredService<IWalletsRepository<EthereumWallet<ObjectId>, ObjectId>>();
    var manager = opt.GetRequiredService<EthereumAccountManager>();
    return new EthereumTransferService(settings, repository, manager);
});
builder.Services.AddScoped(opt =>
{
    var settings = opt.GetRequiredService<IOptions<SmartContractSettings>>().Value;
    var repository = opt.GetRequiredService<IWalletsRepository<EthereumWallet<ObjectId>, ObjectId>>();
    var manager = opt.GetRequiredService<EthereumAccountManager>();
    return new ERC20WalletService(settings, repository, manager);
});
#endregion
#region ERC20
builder.Services.AddScoped(opt =>
{
    var settings = opt.GetRequiredService<IOptions<SmartContractSettings>>().Value;
    var repository = opt.GetRequiredService<IWalletsRepository<EthereumWallet<ObjectId>, ObjectId>>();
    var manager = opt.GetRequiredService<EthereumAccountManager>();
    return new ERC20TransferService(settings, repository, manager);
});
#endregion
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.MapControllers();
app.Run();