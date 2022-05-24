using Crypto.Application.Handlers.Base;
using Crypto.Application.Queries.Ethereum;
using Crypto.Application.Responses.Ethereum;
using Crypto.Domain.Accounts;
using Crypto.Domain.Exceptions;
using Crypto.Domain.Interfaces;
using Crypto.Domain.Models;
using MongoDB.Bson;
using Nethereum.KeyStore;

namespace Crypto.Application.Handlers.Wallets.Ethereum;

public class GetEthereumP2PWalletByEmailHandler 
    : EthereumP2PWalletBaseHandler<GetEthereumP2PWalletByEmailQuery, EthereumP2PWalletWithIdResponse>
{
    private readonly EthereumAccountManager _accountManager;
    public GetEthereumP2PWalletByEmailHandler(IEthereumP2PWalletsRepository<ObjectId> repository, EthereumAccountManager accountManager) 
        : base(repository)
    {
        _accountManager = accountManager;
    }
    public override async Task<EthereumP2PWalletWithIdResponse> Handle(GetEthereumP2PWalletByEmailQuery request, CancellationToken cancellationToken)
    {
        var wallet = await _repository.FindOneAndProjectAsync(w => w.Email == request.Email, wallet => wallet, cancellationToken);
        if (wallet == null || wallet.Id == ObjectId.Empty)
            throw new AccountNotFoundException($"P2P wallet with email {request.Email} is not found");
        var scryptService = new KeyStoreScryptService();
        var loadedAccount = _accountManager.LoadAccountFromKeyStore(scryptService.SerializeKeyStoreToJson(wallet.KeyStore), wallet.Hash);
        var balanceInEther = await _accountManager.GetAccountBalanceAsync(loadedAccount);
        return new(wallet.Id.ToString(), loadedAccount.Address, balanceInEther);
    }
}