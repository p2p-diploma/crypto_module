using Crypto.Application.Handlers.Base;
using Crypto.Application.Queries.Ethereum;
using Crypto.Application.Responses.Ethereum;
using Crypto.Domain.Accounts;
using Crypto.Domain.Exceptions;
using Crypto.Domain.Interfaces;
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
            var wallet = await _repository.FindOneAsync(w => w.Email == request.Email, wallet => wallet,
                cancellationToken);
            if (wallet == null || wallet.Id == ObjectId.Empty)
                throw new NotFoundException($"Wallet with email {request.Email} is not found");
            if (wallet.UnlockDate == DateTime.Now)
            {
                await _repository.Unlock(wallet.Id);
            }
            var scryptService = new KeyStoreScryptService();
            var loadedAccount = _accountManager.LoadAccountFromKeyStore(scryptService.SerializeKeyStoreToJson(wallet.KeyStore), wallet.Hash);
            var balanceInEther = await _accountManager.GetAccountBalanceAsync(loadedAccount);
            return new(wallet.Id.ToString(), balanceInEther, loadedAccount.Address, wallet.IsLocked);
        }
        var walletWithNoBalance = await _repository
            .FindOneAsync(w => w.Email == request.Email,
                res => new { res.Id, res.KeyStore.Address, IsLocked = res.IsLocked, UnlockDate = res.UnlockDate }, cancellationToken);
        if (walletWithNoBalance == null || walletWithNoBalance.Id == ObjectId.Empty)
            throw new NotFoundException($"Wallet with email {request.Email} is not found");
        if (walletWithNoBalance.UnlockDate == DateTime.Now)
        {
            await _repository.Unlock(walletWithNoBalance.Id);
        }
        return new(walletWithNoBalance.Id.ToString(), decimal.Zero, walletWithNoBalance.Address, walletWithNoBalance.IsLocked);
    }
}