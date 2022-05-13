using Crypto.Application.Queries.Ethereum;
using Crypto.Application.Responses.Ethereum;
using Crypto.Application.Utils;
using Crypto.Domain.Exceptions;
using Crypto.Domain.Interfaces;
using Crypto.Domain.Models;
using MediatR;
using MongoDB.Bson;
using Nethereum.KeyStore;

namespace Crypto.Application.Handlers.Wallets;

public class GetEthereumWalletHandler : IRequestHandler<GetEthereumWalletQuery, EthereumWalletResponse>
{
    private readonly IWalletsRepository<EthereumWallet<ObjectId>, ObjectId> _repository;
    private readonly EthereumAccountManager _accountManager;

    public GetEthereumWalletHandler(IWalletsRepository<EthereumWallet<ObjectId>, ObjectId> repository, EthereumAccountManager accountManager)
    {
        _repository = repository;
        _accountManager = accountManager;
    }

    public async Task<EthereumWalletResponse> Handle(GetEthereumWalletQuery request, CancellationToken token)
    {
        var parsedId = ObjectId.Parse(request.Id);
        var wallet = await _repository.FindOneAsync(w => w.Id == parsedId, token);
        if (wallet.Email == string.Empty)
            throw new AccountNotFoundException($"Wallet with id {request.Id} is not found");
        var scryptService = new KeyStoreScryptService();
        var loadedAccount = _accountManager.LoadAccountFromKeyStore(scryptService.SerializeKeyStoreToJson(wallet.KeyStore), wallet.Hash);
        var balanceInEther = await _accountManager.GetAccountBalanceInEtherAsync(loadedAccount);
        return new EthereumWalletResponse(balanceInEther, loadedAccount.Address, loadedAccount.PrivateKey);
    }
}