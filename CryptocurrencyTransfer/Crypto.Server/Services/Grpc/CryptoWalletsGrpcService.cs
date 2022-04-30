using System.ComponentModel.DataAnnotations;
using Crypto.Configuration;
using Crypto.Interfaces;
using Crypto.Models;
using Crypto.Protos.Wallets;
using Crypto.Services.Ethereum;
using Grpc.Core;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using Nethereum.KeyStore;
using Nethereum.Web3.Accounts;
using static BCrypt.Net.BCrypt;

namespace Crypto.Services.Grpc;
public class CryptoWalletsGrpcService : WalletProtoService.WalletProtoServiceBase
{
    private readonly ILogger<CryptoWalletsGrpcService> _logger;
    private readonly IWalletsRepository<EthereumWallet, ObjectId> _repository;
    private readonly EthereumAccountManager _accountManager;
    public CryptoWalletsGrpcService(ILogger<CryptoWalletsGrpcService> logger, IWalletsRepository<EthereumWallet, ObjectId> repository, IOptions<BlockchainConnections> options)
    {
        _logger = logger;
        _repository = repository;
        _accountManager = new(options.Value.Ganache, options);
    }

    public override async Task<CreateUserWalletResponse> CreateWallet(CreateUserWalletRequest request, ServerCallContext context)
    {
        if (string.IsNullOrEmpty(request.Password))
        {
            context.GetHttpContext().Response.StatusCode = StatusCodes.Status400BadRequest;
            throw new RpcException(new(StatusCode.InvalidArgument, "Password is null or empty"));
        }
        if (string.IsNullOrEmpty(request.Email) || !new EmailAddressAttribute().IsValid(request.Email))
        {
            context.GetHttpContext().Response.StatusCode = StatusCodes.Status400BadRequest;
            throw new RpcException(new(StatusCode.InvalidArgument, "User email is invalid"));
        }
        if (await _repository.ExistsAsync(request.Email, context.CancellationToken))
        {
            context.GetHttpContext().Response.StatusCode = StatusCodes.Status400BadRequest;
            throw new RpcException(new(StatusCode.AlreadyExists, $"Wallet for user {request.Email} is already created"));
        }
        
        _logger.LogDebug("User email and password are valid \n");
        var passwordHash = HashPassword(request.Password);
        _logger.LogDebug($"Hashed password: {passwordHash} \n");
        var keyStore = EthereumAccountManager.GenerateKeyStore(passwordHash, out string privateKey);
        _logger.LogDebug($"Ethereum keystore generated: encryption key = {passwordHash}, account address = {keyStore.Address} \n");
        
        EthereumWallet ethereumWallet = new()
        { Email = request.Email, Hash = passwordHash, KeyStore = keyStore, Id = ObjectId.GenerateNewId(DateTime.Now) };
        try
        {
            await _repository.CreateAsync(ethereumWallet, context.CancellationToken).ConfigureAwait(false);
            _logger.LogDebug("Ethereum wallet is successfully stored \n");
            context.GetHttpContext().Response.Cookies.Append("user_ethereum_wallet_address", keyStore.Address);
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
            _logger.LogError("Failed to store ethereum wallet: " + e.InnerException?.Message);
            throw new RpcException(new(StatusCode.Internal, errorMessage));
        }
    }

    public override async Task<CreateUserWalletResponse> LoadWallet(LoadUserWalletRequest request, ServerCallContext context)
    {
        if (string.IsNullOrEmpty(request.Password))
        {
            context.GetHttpContext().Response.StatusCode = StatusCodes.Status400BadRequest;
            throw new RpcException(new(StatusCode.InvalidArgument, "Password is null or empty"));
        }
        if (string.IsNullOrEmpty(request.Email) || !new EmailAddressAttribute().IsValid(request.Email))
        {
            context.GetHttpContext().Response.StatusCode = StatusCodes.Status400BadRequest;
            throw new RpcException(new(StatusCode.InvalidArgument, "User email is invalid"));
        }
        if (await _repository.ExistsAsync(request.Email, context.CancellationToken))
        {
            context.GetHttpContext().Response.StatusCode = StatusCodes.Status400BadRequest;
            throw new RpcException(new(StatusCode.AlreadyExists, $"Wallet for user {request.Email} is already created"));
        }
        if (string.IsNullOrEmpty(request.PrivateKey))
        {
            context.GetHttpContext().Response.StatusCode = StatusCodes.Status400BadRequest;
            throw new RpcException(new(StatusCode.InvalidArgument, "Private key is either null or empty"));
        }
        
        var loadedAccount = new Account(request.PrivateKey, _accountManager.ChainId);
        if (string.IsNullOrWhiteSpace(loadedAccount.Address))
            throw new RpcException(new(StatusCode.InvalidArgument, "Specified private key is invalid: can't load account"));
        _logger.LogDebug("User email and password are valid \n");
        var passwordHash = HashPassword(request.Password);
        _logger.LogDebug($"Hashed password: {passwordHash} \n");
        var keyStore = EthereumAccountManager.GenerateKeyStore(passwordHash, request.PrivateKey);
        _logger.LogDebug($"Ethereum keystore generated: encryption key = {passwordHash}, account address = {keyStore.Address} \n");
        EthereumWallet ethereumWallet = new()
            { Email = request.Email, Hash = passwordHash, KeyStore = keyStore, Id = ObjectId.GenerateNewId(DateTime.Now) };
        try
        {
            await _repository.CreateAsync(ethereumWallet, context.CancellationToken).ConfigureAwait(false);
            _logger.LogDebug("Ethereum wallet is successfully stored \n");
            context.GetHttpContext().Response.Cookies.Append("user_ethereum_wallet_address", keyStore.Address);
            context.GetHttpContext().Response.Cookies.Append("user_email", request.Email);
            return new()
            {
                Address = keyStore.Address,
                PrivateKey = request.PrivateKey,
                Id = ethereumWallet.Id.ToString()
            };
        }
        catch (AggregateException e)
        {
            var errorMessage = "Failed to store ethereum wallet: " + e.InnerException?.Message;
            _logger.LogError("Failed to store ethereum wallet: " + e.InnerException?.Message);
            throw new RpcException(new(StatusCode.Internal, errorMessage));
        }
    }
    public override async Task<GetUserWalletResponse> GetWallet(GetUserWalletRequest request, ServerCallContext context)
    {
        if (!ObjectId.TryParse(request.Id, out ObjectId id))
        {
            context.GetHttpContext().Response.StatusCode = StatusCodes.Status400BadRequest;
            throw new RpcException(new(StatusCode.InvalidArgument, "Wallet id is invalid"));
        }
        var wallet = await _repository.FindByIdAsync(id, context.CancellationToken);
        if (wallet.Email == string.Empty)
        {
            context.GetHttpContext().Response.StatusCode = StatusCodes.Status404NotFound;
            throw new RpcException(new(StatusCode.NotFound, $"Wallet for id {id} is not found"));
        }
        var scryptService = new KeyStoreScryptService();
        var loadedAccount = _accountManager.LoadAccountFromKeyStore(scryptService.SerializeKeyStoreToJson(wallet.KeyStore), wallet.Hash);
        var balanceInEther = await _accountManager.GetAccountBalanceInEtherAsync(loadedAccount);
        return new GetUserWalletResponse
        {
            Balance = (double) balanceInEther,
            Address = loadedAccount.Address,
            PrivateKey = loadedAccount.PrivateKey
        };
    }
    
}