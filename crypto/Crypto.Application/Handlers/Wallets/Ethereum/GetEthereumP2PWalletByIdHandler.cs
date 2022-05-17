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

public class GetEthereumP2PWalletByIdHandler : IRequestHandler<GetEthereumP2PWalletByIdQuery, EthereumP2PWalletResponse>
{
    private readonly IWalletsRepository<EthereumP2PWallet<ObjectId>, ObjectId> _repository;
    private readonly EthereumAccountManager _accountManager;

    public GetEthereumP2PWalletByIdHandler(IWalletsRepository<EthereumP2PWallet<ObjectId>, ObjectId> repository, EthereumAccountManager accountManager)
    {
        _repository = repository;
        _accountManager = accountManager;
    }

    public async Task<EthereumP2PWalletResponse> Handle(GetEthereumP2PWalletByIdQuery request, CancellationToken cancellationToken)
    {
        var parsedId = ObjectId.Parse(request.WalletId);
        var wallet = await _repository.FindOneAsync(w => w.UserWalletId == parsedId, cancellationToken);
        if (wallet.Id == ObjectId.Empty)
            throw new AccountNotFoundException($"P2P wallet with id {request.WalletId} is not found");
        var scryptService = new KeyStoreScryptService();
        var loadedAccount = _accountManager.LoadAccountFromKeyStore(scryptService.SerializeKeyStoreToJson(wallet.KeyStore), wallet.Hash);
        var balanceInEther = await _accountManager.GetAccountBalanceInEtherAsync(loadedAccount);
        return new(loadedAccount.Address, balanceInEther);
    }
}