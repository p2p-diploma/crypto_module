using Crypto.Application.Handlers.Base;
using Crypto.Application.Queries.ERC20;
using Crypto.Application.Responses.ERC20;
using Crypto.Domain.Accounts;
using Crypto.Domain.Interfaces;
using MongoDB.Bson;
using Nethereum.KeyStore;

namespace Crypto.Application.Handlers.Wallets.ERC20;

public class GetERC20P2PWalletByEmailHandler : EthereumP2PWalletBaseHandler<GetErc20P2PWalletByEmailQuery, Erc20P2PWalletWithIdResponse>
{
    private readonly Erc20AccountManager _accountManager;
    public GetERC20P2PWalletByEmailHandler(IEthereumP2PWalletsRepository<ObjectId> repository, Erc20AccountManager accountManager) 
        : base(repository)
    {
        _accountManager = accountManager;
    }
    public override async Task<Erc20P2PWalletWithIdResponse> Handle(GetErc20P2PWalletByEmailQuery request, CancellationToken cancellationToken)
    {
        var wallet = await _repository.FindOneAsync(w => w.Email == request.Email, wallet => wallet, cancellationToken);
        if (wallet == null || wallet.Id == ObjectId.Empty)
            throw new ArgumentException($"Token wallet with email {request.Email} does not exist");
        if (wallet.UnlockDate == DateTime.Now)
            await _repository.Unlock(wallet.Id);
        var scryptService = new KeyStoreScryptService();
        var loadedAccount = _accountManager.LoadAccountFromKeyStore(scryptService.SerializeKeyStoreToJson(wallet.KeyStore), wallet.Hash);
        var tokensAmount = await _accountManager.GetAccountBalanceAsync(loadedAccount);
        return new(wallet.Id.ToString(), loadedAccount.Address, tokensAmount, wallet.IsLocked);
    }

    
}