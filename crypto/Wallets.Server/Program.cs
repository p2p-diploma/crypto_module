global using static Crypto.Data.ObjectIdExtension;
using System.Reflection;
using Crypto.Application.Responses.ERC20;
using Crypto.Data.Configuration;
using Crypto.Data.Repositories;
using Crypto.Domain.Accounts;
using Crypto.Domain.Configuration;
using Crypto.Domain.Interfaces;
using FluentValidation.AspNetCore;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.HttpLogging;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Models;
using MongoDB.Bson;
using MongoDB.Driver;
using Wallets.Server.Filters;
using Wallets.Server.Validators;

var builder = WebApplication.CreateBuilder(args);
// Add services to the container.
#region MediatR, FluentValidation, Controllers
builder.Services.AddControllers(opts =>
    {
        opts.Filters.Add<ExceptionHandleFilter>();
    }).AddFluentValidation(fv => fv.RegisterValidatorsFromAssemblyContaining<CreateEthereumWalletValidator>())
    .ConfigureApiBehaviorOptions(opt => {
        opt.InvalidModelStateResponseFactory = context => new BadRequestObjectResult(context.ModelState.Values.First(q => q.Errors.Count > 0).Errors.First(er => !string.IsNullOrEmpty(er.ErrorMessage)).ErrorMessage);
    });
builder.Services.AddMediatR(typeof(Erc20WalletResponse).Assembly);
#endregion
#region Logging
builder.Logging.AddConsole();
builder.Services.AddHttpLogging(o =>
{
    o.LoggingFields = HttpLoggingFields.RequestPropertiesAndHeaders | HttpLoggingFields.RequestPath;
});
#endregion
#region Account Managers
builder.Services.AddTransient(opt =>
{
    var connections = opt.GetRequiredService<BlockchainConnections>();
    var logger = opt.GetRequiredService<ILogger<EthereumAccountManager>>();
    return new EthereumAccountManager(connections.Ganache, connections, logger);
});
builder.Services.AddTransient(opt =>
{
    var connections = opt.GetRequiredService<BlockchainConnections>();
    var logger = opt.GetRequiredService<ILogger<Erc20AccountManager>>();
    return new Erc20AccountManager(connections.Ganache, connections, logger);
});
#endregion
#region Configuration
builder.Services.AddSingleton(new BlockchainConnections
{
    Ganache = builder.Configuration["BlockchainConnections:Ganache"], Kovan = builder.Configuration["BlockchainConnections:Kovan"],
    Rinkeby = builder.Configuration["BlockchainConnections:Rinkeby"], Goerly = builder.Configuration["BlockchainConnections:Goerly"],
    Ropsten = builder.Configuration["BlockchainConnections:Ropsten"], TokenAddress = builder.Configuration["SmartContractSettings:TokenAddress"]
});
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
builder.Services.AddScoped<IEthereumWalletsRepository<ObjectId>, EthereumWalletsRepository>();
builder.Services.AddScoped<IEthereumP2PWalletsRepository<ObjectId>, EthereumP2PWalletsRepository>();
#endregion
//builder.Services.AddCors();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
#region Swagger
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
#endregion
var app = builder.Build();
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseHttpsRedirection();
app.UseHttpLogging();
//app.UseCors(b => b.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod().AllowCredentials());
app.UseAuthorization();
app.MapControllers();
app.Run();