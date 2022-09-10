using Crypto.Application.Handlers.Base;
using Crypto.Application.Queries.ERC20;
using Crypto.Application.Responses.ERC20;
using Crypto.Domain.Accounts;
using Crypto.Domain.Interfaces;
using MongoDB.Bson;
using Nethereum.KeyStore;

namespace Crypto.Application.Handlers.Wallets.ERC20;

public class GetERC20P2PWalletByIdHandler : EthereumP2PWalletBaseHandler<GetErc20P2PWalletByIdQuery, Erc20P2PWalletResponse>
{
    private readonly Erc20AccountManager _accountManager;
    public GetERC20P2PWalletByIdHandler(IEthereumP2PWalletsRepository<ObjectId> repository, 
        Erc20AccountManager accountManager) : base(repository)
    {
        _accountManager = accountManager;
    }
    public override async Task<Erc20P2PWalletResponse> Handle(GetErc20P2PWalletByIdQuery request, CancellationToken token)
    {
        var parsedId = ObjectId.Parse(request.WalletId);
        var wallet = await _repository.FindOneAsync(w => w.Id == parsedId,wallet => wallet, token);
        if (wallet == null || wallet.Id == ObjectId.Empty)
            throw new ArgumentException($"Token wallet with id {request.WalletId} does not exist");
        if (wallet.UnlockDate == DateTime.Now)
            await _repository.Unlock(wallet.Id);
        var scryptService = new KeyStoreScryptService();
        var loadedAccount = _accountManager.LoadAccountFromKeyStore(scryptService.SerializeKeyStoreToJson(wallet.KeyStore), wallet.Hash);
        var tokensAmount = await _accountManager.GetAccountBalanceAsync(loadedAccount);
        return new(loadedAccount.Address, tokensAmount, wallet.IsLocked);
    }
}