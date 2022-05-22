using Crypto.Application.Commands.ERC20;
using Crypto.Application.Handlers.Base;
using Crypto.Application.Responses;
using Crypto.Application.Utils;
using Crypto.Domain.Configuration;
using Crypto.Domain.Exceptions;
using Crypto.Domain.Interfaces;
using Crypto.Domain.Models;
using MongoDB.Bson;
using Nethereum.KeyStore;
using Nethereum.RPC.Accounts;
using Nethereum.Web3;
using Nethereum.Web3.Accounts;

namespace Crypto.Application.Handlers.ERC20;

public class TransferERC20ToP2PWalletHandler : EthereumWalletBaseHandler<TransferERC20ToP2PWalletCommand, TransactionResponse>
{
    private readonly EthereumAccountManager _accountManager;
    private readonly IEthereumP2PWalletsRepository<ObjectId> _p2pWalletsRepository;
    public TransferERC20ToP2PWalletHandler(EthereumAccountManager accountManager, 
        IEthereumWalletsRepository<ObjectId> repository, 
        IEthereumP2PWalletsRepository<ObjectId> p2PWalletsRepository) : base(repository)
    {
        _accountManager = accountManager;
        _p2pWalletsRepository = p2PWalletsRepository;
    }
    
    public override async Task<TransactionResponse> Handle(TransferERC20ToP2PWalletCommand request, CancellationToken token)
    {
        var userId = ObjectId.Parse(request.WalletId);
        var userWallet = await _repository.FindOneAndProjectAsync(w => w.Id == userId, wallet => wallet, token);
        if (userWallet.IsFrozen) throw new AccountFrozenException();
        if (userWallet == null || userWallet.Id == ObjectId.Empty)
            throw new ArgumentException($"Wallet with id {request.WalletId} does not exist");
        var scryptService = new KeyStoreScryptService();
        var p2pWallet = await _p2pWalletsRepository.FindOneAndProjectAsync(w => w.Id == userId, wallet => wallet, token);
        if (p2pWallet.IsFrozen) throw new AccountFrozenException();
        var account = _accountManager.LoadAccountFromKeyStore(scryptService.SerializeKeyStoreToJson(userWallet.KeyStore), userWallet.Hash);
        return await _accountManager.Transfer(p2pWallet.KeyStore.Address, request.Amount, account, token);
    }
}