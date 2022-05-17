global using static Crypto.Data.ObjectIdExtension;
using System.Reflection;
using Crypto.Application.Responses.ERC20;
using Crypto.Application.Utils;
using Crypto.Data.Configuration;
using Crypto.Data.Repositories;
using Crypto.Domain.Configuration;
using Crypto.Domain.Interfaces;
using Crypto.Domain.Models;
using Crypto.Server.Validators.Ethereum;
using FluentValidation.AspNetCore;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Models;
using MongoDB.Bson;
using MongoDB.Driver;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
#region MediatR, FluentValidation, Controllers
builder.Services.AddControllers().AddFluentValidation(fv => fv.RegisterValidatorsFromAssemblyContaining<RefundEtherFromP2PWalletValidator>())
    .ConfigureApiBehaviorOptions(opt => {
        opt.InvalidModelStateResponseFactory = context => new BadRequestObjectResult(context.ModelState.Values.First(q => q.Errors.Count > 0).Errors.First(er => !string.IsNullOrEmpty(er.ErrorMessage)).ErrorMessage);
    });
builder.Services.AddMediatR(typeof(Erc20WalletResponse).Assembly);
#endregion
#region Utils
builder.Services.AddTransient(opt =>
{
    var connections = opt.GetRequiredService<BlockchainConnections>();
    return new EthereumAccountManager(connections.Ganache, connections);
});
#endregion
#region Configuration
builder.Services.AddSingleton(new BlockchainConnections
{
    Ganache = builder.Configuration["BlockchainConnections:Ganache"], Kovan = builder.Configuration["BlockchainConnections:Kovan"],
    Rinkeby = builder.Configuration["BlockchainConnections:Rinkeby"], Goerly = builder.Configuration["BlockchainConnections:Goerly"],
    Ropsten = builder.Configuration["BlockchainConnections:Ropsten"]
});
builder.Services.AddSingleton(new SmartContractSettings { StandardERC20Address = builder.Configuration["SmartContractSettings:StandardERC20Address"] });
builder.Services.AddSingleton(new DatabaseSettings
{
    ConnectionString = builder.Configuration["DatabaseSettings:ConnectionString"],
    DatabaseName = builder.Configuration["DatabaseSettings:DatabaseName"],
    EthereumWalletsCollection = builder.Configuration["DatabaseSettings:EthereumWalletsCollection"],
    EthereumP2PWalletsCollection = builder.Configuration["DatabaseSettings:EthereumP2PWalletsCollection"],
});
#endregion
#region Data
builder.Services.AddSingleton<IMongoClient>(new MongoClient(builder.Configuration["DatabaseSettings:ConnectionString"]));
builder.Services.AddScoped<IWalletsRepository<EthereumWallet<ObjectId>, ObjectId>, EthereumWalletsRepository>();
builder.Services.AddScoped<IWalletsRepository<EthereumP2PWallet<ObjectId>, ObjectId>, P2PWalletsRepository>();
#endregion
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Version = "v1",
        Title = "Smart contracts interaction: Ethereum and ERC20 transfer API",
        Description = "API for transferring Ethereum and ERC20 via smart contracts: block sum, transfer and revert transfer"
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
app.MapControllers();
app.Run();