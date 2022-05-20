using Crypto.Application.Handlers.Base;
using Crypto.Application.Queries.Ethereum;
using Crypto.Application.Responses.Ethereum;
using Crypto.Application.Utils;
using Crypto.Domain.Exceptions;
using Crypto.Domain.Interfaces;
using Crypto.Domain.Models;
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
        var wallet = await _repository.FindOneAndProjectAsync(w => w.Id == parsedId, wallet => wallet, token);
        if (wallet == null || wallet.Id == ObjectId.Empty)
            throw new AccountNotFoundException($"Wallet with id {request.Id} is not found");
        var scryptService = new KeyStoreScryptService();
        var loadedAccount = _accountManager.LoadAccountFromKeyStore(scryptService.SerializeKeyStoreToJson(wallet.KeyStore), wallet.Hash);
        var balanceInEther = await _accountManager.GetAccountBalanceInEtherAsync(loadedAccount);
        return new EthereumWalletResponse(balanceInEther, loadedAccount.Address, loadedAccount.PrivateKey);
    }

    
}