using EthereumTransferGrpc.Configuration;
using EthereumTransferGrpc.Data;
using EthereumTransferGrpc.Services;
using EthereumTransferGrpc.Services.Grpc;
using MongoDB.Driver;

var builder = WebApplication.CreateBuilder(args);

// Additional configuration is required to successfully run gRPC on macOS.
// For instructions on how to configure Kestrel and gRPC clients on macOS, visit https://go.microsoft.com/fwlink/?linkid=2099682

// Add services to the container.
builder.Services.AddGrpc();
builder.Services.Configure<DatabaseSettings>(builder.Configuration.GetSection("DatabaseSettings"));
builder.Services.Configure<BlockchainConnections>(builder.Configuration.GetSection("BlockchainConnections"));
builder.Services.Configure<SmartContractSettings>(builder.Configuration.GetSection("SmartContractSettings"));
builder.Services.AddScoped<EthereumWalletsRepository>();
builder.Services.AddScoped(_ => new MongoClient(builder.Configuration["DatabaseSettings:ConnectionString"]));
var app = builder.Build();

// Configure the HTTP request pipeline.
app.MapGrpcService<CryptoWalletsGrpcService>();
app.MapGrpcService<TransferAgreementGrpcService>();
app.MapGet("/",
    () =>
        "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");

app.Run();