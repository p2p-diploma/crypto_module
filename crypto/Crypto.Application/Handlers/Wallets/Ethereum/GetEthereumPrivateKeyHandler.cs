using Crypto.Application.Handlers.Base;
using Crypto.Application.Queries.Ethereum;
using Crypto.Domain.Accounts;
using Crypto.Domain.Exceptions;
using Crypto.Domain.Interfaces;
using MongoDB.Bson;
using Nethereum.KeyStore;

namespace Crypto.Application.Handlers.Wallets.Ethereum;

public class GetEthereumPrivateKeyHandler : EthereumWalletBaseHandler<GetPrivateKeyQuery, string>
{
    private readonly EthereumAccountManager _accountManager;
    public GetEthereumPrivateKeyHandler(IEthereumWalletsRepository<ObjectId> repository, 
        EthereumAccountManager accountManager) : base(repository)
    {
        _accountManager = accountManager;
    }

    public override async Task<string> Handle(GetPrivateKeyQuery request, CancellationToken cancellationToken)
    {
        var id = ObjectId.Parse(request.Id);
        var wallet = await _repository.FindOneAsync(w => w.Email == request.Email, w => w, cancellationToken);
        if (wallet == null || wallet.Id != id) throw new NotFoundException("You are unauthorized");
        if (wallet.UnlockDate == DateTime.Now)
        {
            await _repository.Unlock(wallet.Id);
        }
        else
        {
            if (wallet.IsLocked) throw new AccountLockedException(unlockDate: wallet.UnlockDate!.Value);
        }
        var keyService = new KeyStoreScryptService();
        var account = _accountManager.LoadAccountFromKeyStore(keyService.SerializeKeyStoreToJson(wallet.KeyStore), wallet.Hash);
        return account.PrivateKey;
    }
}