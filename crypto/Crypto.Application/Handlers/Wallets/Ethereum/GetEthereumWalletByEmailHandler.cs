using Crypto.Application.Queries.Ethereum;
using Crypto.Application.Responses.Ethereum;
using Crypto.Application.Utils;
using Crypto.Domain.Exceptions;
using Crypto.Domain.Interfaces;
using Crypto.Domain.Models;
using MediatR;
using MongoDB.Bson;
using Nethereum.KeyStore;

namespace Crypto.Application.Handlers.Wallets.Ethereum;

public class GetEthereumWalletByEmailHandler : IRequestHandler<GetEthereumWalletByEmailQuery, EthereumWalletResponse>
{
    private readonly IWalletsRepository<EthereumWallet<ObjectId>, ObjectId> _repository;
    private readonly EthereumAccountManager _accountManager;

    public GetEthereumWalletByEmailHandler(IWalletsRepository<EthereumWallet<ObjectId>, ObjectId> repository, EthereumAccountManager accountManager)
    {
        _repository = repository;
        _accountManager = accountManager;
    }

    public async Task<EthereumWalletResponse> Handle(GetEthereumWalletByEmailQuery request, CancellationToken cancellationToken)
    {
        var wallet = await _repository.FindOneAsync(w => w.Email == request.Email, cancellationToken);
        if (wallet.Id == ObjectId.Empty)
            throw new AccountNotFoundException($"Wallet with email {request.Email} is not found");
        var scryptService = new KeyStoreScryptService();
        var loadedAccount = _accountManager.LoadAccountFromKeyStore(scryptService.SerializeKeyStoreToJson(wallet.KeyStore), wallet.Hash);
        var balanceInEther = await _accountManager.GetAccountBalanceInEtherAsync(loadedAccount);
        return new(balanceInEther, loadedAccount.Address, loadedAccount.PrivateKey);
    }
}