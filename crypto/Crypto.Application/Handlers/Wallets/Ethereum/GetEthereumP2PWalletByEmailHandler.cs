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

public class GetEthereumP2PWalletByEmailHandler : IRequestHandler<GetEthereumP2PWalletByEmailQuery, EthereumP2PWalletResponse>
{
    private readonly IWalletsRepository<EthereumP2PWallet<ObjectId>, ObjectId> _repository;
    private readonly EthereumAccountManager _accountManager;

    public GetEthereumP2PWalletByEmailHandler(IWalletsRepository<EthereumP2PWallet<ObjectId>, ObjectId> repository, EthereumAccountManager accountManager)
    {
        _repository = repository;
        _accountManager = accountManager;
    }

    public async Task<EthereumP2PWalletResponse> Handle(GetEthereumP2PWalletByEmailQuery request, CancellationToken cancellationToken)
    {
        var wallet = await _repository.FindOneAsync(w => w.Email == request.Email, cancellationToken);
        if (wallet.Id == ObjectId.Empty)
            throw new AccountNotFoundException($"P2P wallet with email {request.Email} is not found");
        var scryptService = new KeyStoreScryptService();
        var loadedAccount = _accountManager.LoadAccountFromKeyStore(scryptService.SerializeKeyStoreToJson(wallet.KeyStore), wallet.Hash);
        var balanceInEther = await _accountManager.GetAccountBalanceInEtherAsync(loadedAccount);
        return new(loadedAccount.Address, balanceInEther);
    }
}