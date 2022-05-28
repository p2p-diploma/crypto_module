using Crypto.Application.Handlers.Base;
using Crypto.Application.Queries.ERC20;
using Crypto.Application.Responses.ERC20;
using Crypto.Domain.Accounts;
using Crypto.Domain.Contracts;
using Crypto.Domain.Interfaces;
using Crypto.Domain.Models;
using MediatR;
using MongoDB.Bson;
using Nethereum.Contracts.Standards.ERC20.ContractDefinition;
using Nethereum.KeyStore;
using Nethereum.Util;
using Nethereum.Web3;

namespace Crypto.Application.Handlers.Wallets.ERC20;

public class GetERC20WalletByEmailHandler : EthereumWalletBaseHandler<GetErc20WalletByEmailQuery, Erc20WalletWithIdResponse>
{
    private readonly Erc20AccountManager _accountManager;
    public GetERC20WalletByEmailHandler(IEthereumWalletsRepository<ObjectId> repository, Erc20AccountManager accountManager) 
        : base(repository)
    {
        _accountManager = accountManager;
    }
    public override async Task<Erc20WalletWithIdResponse> Handle(GetErc20WalletByEmailQuery request, CancellationToken cancellationToken)
    {
        var wallet = await _repository.FindOneAndProjectAsync(w => w.Email == request.Email, wallet => wallet, cancellationToken);
        if (wallet == null || wallet.Id == ObjectId.Empty)
            throw new ArgumentException($"Token wallet with email {request.Email} does not exist");
        if (wallet.DateOfUnfreeze == DateTime.Now)
            await _repository.Unfreeze(wallet.Id);
        var scryptService = new KeyStoreScryptService();
        var loadedAccount = _accountManager.LoadAccountFromKeyStore(scryptService.SerializeKeyStoreToJson(wallet.KeyStore), wallet.Hash);
        var tokensAmount = await _accountManager.GetAccountBalanceAsync(loadedAccount);
        return new(wallet.Id.ToString(), loadedAccount.Address, tokensAmount, wallet.IsFrozen);
    }
}