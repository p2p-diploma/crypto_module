using Crypto.Application.Handlers.Base;
using Crypto.Application.Queries.Ethereum;
using Crypto.Application.Responses.Ethereum;
using Crypto.Domain.Accounts;
using Crypto.Domain.Exceptions;
using Crypto.Domain.Interfaces;
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
        var wallet = await _repository.FindOneAsync(w => w.Email == request.Email, wallet => wallet, cancellationToken);
        if (wallet == null || wallet.Id == ObjectId.Empty)
            throw new NotFoundException($"P2P wallet with email {request.Email} is not found");
        if (wallet.UnlockDate == DateTime.Now)
        {
            await _repository.Unlock(wallet.Id);
        }
        var scryptService = new KeyStoreScryptService();
        var loadedAccount = _accountManager.LoadAccountFromKeyStore(scryptService.SerializeKeyStoreToJson(wallet.KeyStore), wallet.Hash);
        var balanceInEther = await _accountManager.GetAccountBalanceAsync(loadedAccount);
        return new(wallet.Id.ToString(), loadedAccount.Address, balanceInEther, wallet.IsLocked);
    }
}