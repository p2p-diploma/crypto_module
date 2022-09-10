using Crypto.Application.Handlers.Base;
using Crypto.Application.Queries.Ethereum;
using Crypto.Application.Responses.Ethereum;
using Crypto.Domain.Accounts;
using Crypto.Domain.Exceptions;
using Crypto.Domain.Interfaces;
using MongoDB.Bson;
using Nethereum.KeyStore;

namespace Crypto.Application.Handlers.Wallets.Ethereum;

public class GetEthereumP2PWalletByIdHandler : EthereumP2PWalletBaseHandler<GetEthereumP2PWalletByIdQuery, EthereumP2PWalletResponse>
{
    private readonly EthereumAccountManager _accountManager;
    public GetEthereumP2PWalletByIdHandler(IEthereumP2PWalletsRepository<ObjectId> repository, 
        EthereumAccountManager accountManager) : base(repository)
    {
        _accountManager = accountManager;
    }

    public override async Task<EthereumP2PWalletResponse> Handle(GetEthereumP2PWalletByIdQuery request, CancellationToken cancellationToken)
    {
        var parsedId = ObjectId.Parse(request.WalletId);
        var wallet = await _repository.FindOneAsync(w => w.Id == parsedId, wallet => wallet, cancellationToken);
        if (wallet == null || wallet.Id == ObjectId.Empty)
            throw new NotFoundException($"P2P wallet with id {request.WalletId} is not found");
        if (wallet.UnlockDate == DateTime.Now)
        {
            await _repository.Unlock(wallet.Id);
        }
        var scryptService = new KeyStoreScryptService();
        var loadedAccount = _accountManager.LoadAccountFromKeyStore(scryptService.SerializeKeyStoreToJson(wallet.KeyStore), wallet.Hash);
        var balanceInEther = await _accountManager.GetAccountBalanceAsync(loadedAccount);
        return new(loadedAccount.Address, balanceInEther, wallet.IsLocked);
    }

    
}