using Crypto.Configuration;
using Crypto.Data.Repositories;
using Crypto.Interfaces;
using Crypto.Models;
using Crypto.Services.Grpc;
using MongoDB.Bson;
using MongoDB.Driver;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddGrpc();
builder.Services.Configure<DatabaseSettings>(builder.Configuration.GetSection("DatabaseSettings"));
builder.Services.Configure<BlockchainConnections>(builder.Configuration.GetSection("BlockchainConnections"));
builder.Services.Configure<SmartContractSettings>(builder.Configuration.GetSection("SmartContractSettings"));
builder.Services.AddScoped<IWalletsRepository<EthereumWallet, ObjectId>, EthereumWalletsRepository>();
builder.Services.AddScoped(_ => new MongoClient(builder.Configuration["DatabaseSettings:ConnectionString"]));
var app = builder.Build();


// Configure the HTTP request pipeline.
app.MapGrpcService<CryptoWalletsGrpcService>();
app.MapGrpcService<TransferAgreementGrpcService>();
app.MapGet("/",
    () =>
        "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");

app.Run();