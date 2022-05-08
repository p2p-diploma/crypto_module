using System.Reflection;
using Crypto.Application.ERC20;
using Crypto.Application.Ethereum;
using Crypto.Data.Configuration;
using Crypto.Data.Repositories;
using Crypto.Domain.Configuration;
using Crypto.Domain.Interfaces;
using Crypto.Domain.Models;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using MongoDB.Bson;
using MongoDB.Driver;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers().AddFluentValidation(opt => opt.RegisterValidatorsFromAssemblyContaining<Program>())
    .ConfigureApiBehaviorOptions(opt => {
        opt.InvalidModelStateResponseFactory = context => new BadRequestObjectResult(context.ModelState.Values.First(q => q.Errors.Count > 0).Errors.First(er => !string.IsNullOrEmpty(er.ErrorMessage)).ErrorMessage);
    });
builder.Services.Configure<BlockchainConnections>(builder.Configuration.GetSection("BlockchainConnections"));
builder.Services.Configure<SmartContractSettings>(builder.Configuration.GetSection("SmartContractSettings"));
builder.Services.Configure<DatabaseSettings>(builder.Configuration.GetSection("DatabaseSettings"));
builder.Services.AddSingleton<IMongoClient>(new MongoClient(builder.Configuration["DatabaseSettings:ConnectionString"]));
builder.Services.AddScoped<IWalletsRepository<EthereumWallet<ObjectId>, ObjectId>>(opt =>
{
    var settings = opt.GetRequiredService<IOptions<DatabaseSettings>>();
    var client = opt.GetRequiredService<IMongoClient>();
    return new EthereumWalletsRepository(settings.Value, client);
});
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
    return new ERC20WalletService(settings, repository, manager);
});
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Version = "v1",
        Title = "Crypto wallets: Ethereum and ERC20 token wallets API",
        Description = "API for managing Ethereum and ERC20 wallets: create, load existing ones and get information about wallets"
    });
    var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();