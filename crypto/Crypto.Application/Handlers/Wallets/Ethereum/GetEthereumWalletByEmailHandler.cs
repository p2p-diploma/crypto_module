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

public class GetEthereumWalletByEmailHandler : EthereumWalletBaseHandler<GetEthereumWalletByEmailQuery, EthereumWalletWithIdResponse>
{
    private readonly EthereumAccountManager _accountManager;
    public GetEthereumWalletByEmailHandler(IEthereumWalletsRepository<ObjectId> repository, EthereumAccountManager accountManager)
        : base(repository)
    {
        _accountManager = accountManager;
    }
    public override async Task<EthereumWalletWithIdResponse> Handle(GetEthereumWalletByEmailQuery request, CancellationToken cancellationToken)
    {
        if (request.IncludeBalance)
        {
            var wallet = await _repository.FindOneAndProjectAsync(w => w.Email == request.Email, wallet => wallet,
                cancellationToken);
            if (wallet == null || wallet.Id == ObjectId.Empty)
                throw new AccountNotFoundException($"Wallet with email {request.Email} is not found");
            var scryptService = new KeyStoreScryptService();
            var loadedAccount = _accountManager.LoadAccountFromKeyStore(scryptService.SerializeKeyStoreToJson(wallet.KeyStore), wallet.Hash);
            var balanceInEther = await _accountManager.GetAccountBalanceAsync(loadedAccount);
            return new(wallet.Id.ToString(), balanceInEther, loadedAccount.Address);
        }
        var walletWithNoBalance = await _repository
            .FindOneAndProjectAsync(w => w.Email == request.Email,
                res => new { res.Id, res.KeyStore.Address }, cancellationToken);
        if (walletWithNoBalance == null || walletWithNoBalance.Id == ObjectId.Empty)
            throw new AccountNotFoundException($"Wallet with email {request.Email} is not found");
        return new(walletWithNoBalance.Id.ToString(), decimal.Zero, walletWithNoBalance.Address);
    }

    
}