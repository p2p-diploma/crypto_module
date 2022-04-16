using System.ComponentModel.DataAnnotations;
using EthereumTransferGrpc.Configuration;
using EthereumTransferGrpc.Data;
using EthereumTransferGrpc.Documents;
using EthereumTransferGrpc.Protos.Wallets;
using Grpc.Core;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using Nethereum.KeyStore;
using static BCrypt.Net.BCrypt;
namespace EthereumTransferGrpc.Services.Grpc;

public class CryptoWalletsGrpcService : WalletProtoService.WalletProtoServiceBase
{
    private readonly ILogger<CryptoWalletsGrpcService> _logger;
    private readonly EthereumWalletsRepository _repository;
    private readonly EthereumAccountManager _accountManager;
    public CryptoWalletsGrpcService(ILogger<CryptoWalletsGrpcService> logger, EthereumWalletsRepository repository, IOptions<BlockchainConnections> options)
    {
        _logger = logger;
        _repository = repository;
        _accountManager = new(options.Value.Ganache, options);
    }

    public override async Task<CreateUserWalletResponse> CreateWallet(CreateUserWalletRequest request, ServerCallContext context)
    {
        if (string.IsNullOrEmpty(request.Password)) throw new RpcException(new(StatusCode.InvalidArgument, "Password is null or empty"));
        if (string.IsNullOrEmpty(request.Email) || !new EmailAddressAttribute().IsValid(request.Email)) 
            throw new RpcException(new(StatusCode.InvalidArgument, "User email is invalid"));
        if(await _repository.WalletExistsAsync(request.Email, context.CancellationToken)) 
            throw new RpcException(new(StatusCode.AlreadyExists, $"Wallet for user {request.Email} is already created"));
        
        _logger.LogDebug("User email and password are valid \n");
        var passwordHash = HashPassword(request.Password);
        _logger.LogDebug($"Hashed password: {passwordHash} \n");
        var keyStore = _accountManager.GenerateKeyStore(passwordHash, out string privateKey);
        _logger.LogDebug($"Ethereum keystore generated: encryption key = {passwordHash}, account address = {keyStore.Address} \n");
        EthereumWallet ethereumWallet = new()
        {
            Email = request.Email,
            Hash = passwordHash,
            KeyStore = keyStore,
            Id = ObjectId.GenerateNewId(DateTime.Now)
        };
        try
        {
            await _repository.CreateAsync(ethereumWallet, context.CancellationToken).ConfigureAwait(false);
            _logger.LogDebug("Ethereum wallet is successfully stored \n");
            context.GetHttpContext().Response.Cookies.Append("user_wallet_address", keyStore.Address);
            context.GetHttpContext().Response.Cookies.Append("user_email", request.Email);
            return new()
            {
                Address = keyStore.Address,
                PrivateKey = privateKey,
                Id = ethereumWallet.Id.ToString()
            };
        }
        catch (AggregateException e)
        {
            var errorMessage = "Failed to store ethereum wallet: " + e.InnerException?.Message;
            _logger.LogError(errorMessage);
            throw new RpcException(new(StatusCode.Internal, errorMessage));
        }
    }


    public override async Task<LoadUserWalletResponse> LoadWallet(LoadUserWalletRequest request, ServerCallContext context)
    {
        if (string.IsNullOrEmpty(request.Id)) throw new RpcException(new(StatusCode.InvalidArgument, "User id is empty or null"));
        var wallet = await _repository.FindByIdAsync(ObjectId.Parse(request.Id), context.CancellationToken);
        if (wallet.Email == string.Empty) 
            throw new RpcException(new(StatusCode.NotFound, $"Wallet for user id {request.Id} is not found"));
        var scryptService = new KeyStoreScryptService();
        var loadedAccount = _accountManager
            .LoadAccountFromKeyStore(scryptService.SerializeKeyStoreToJson(wallet.KeyStore), wallet.Hash);
        var balanceInEther = await _accountManager.GetAccountBalanceAsync(loadedAccount);
        return new LoadUserWalletResponse
        {
            Balance = (double) balanceInEther,
            Address = loadedAccount.Address,
            PrivateKey = loadedAccount.PrivateKey
        };
    }
}