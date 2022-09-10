using Crypto.Application.Handlers.Base;
using Crypto.Application.Queries.Ethereum;
using Crypto.Application.Responses.Ethereum;
using Crypto.Domain.Accounts;
using Crypto.Domain.Exceptions;
using Crypto.Domain.Interfaces;
using MongoDB.Bson;
using Nethereum.KeyStore;

namespace Crypto.Application.Handlers.Wallets.Ethereum;

public class GetEthereumWalletByIdHandler : EthereumWalletBaseHandler<GetEthereumWalletByIdQuery, EthereumWalletResponse>
{
    private readonly EthereumAccountManager _accountManager;
    public GetEthereumWalletByIdHandler(IEthereumWalletsRepository<ObjectId> repository, EthereumAccountManager accountManager)
        : base(repository)
    {
        _accountManager = accountManager;
    }
    public override async Task<EthereumWalletResponse> Handle(GetEthereumWalletByIdQuery request, CancellationToken token)
    {
        var parsedId = ObjectId.Parse(request.Id);
        var wallet = await _repository.FindOneAsync(w => w.Id == parsedId, wallet => wallet, token);
        if (wallet == null || wallet.Id == ObjectId.Empty)
            throw new NotFoundException($"Wallet with id {request.Id} is not found");
        if (wallet.UnlockDate == DateTime.Now)
            await _repository.Unlock(wallet.Id);
        var scryptService = new KeyStoreScryptService();
        var loadedAccount = _accountManager.LoadAccountFromKeyStore(scryptService.SerializeKeyStoreToJson(wallet.KeyStore), wallet.Hash);
        var balanceInEther = await _accountManager.GetAccountBalanceAsync(loadedAccount);
        return new EthereumWalletResponse(balanceInEther, loadedAccount.Address, wallet.IsLocked);
    }

    
}