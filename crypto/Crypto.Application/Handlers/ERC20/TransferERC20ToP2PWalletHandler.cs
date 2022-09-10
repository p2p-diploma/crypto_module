using Crypto.Application.Commands.ERC20;
using Crypto.Application.Handlers.Base;
using Crypto.Domain.Accounts;
using Crypto.Domain.Exceptions;
using Crypto.Domain.Interfaces;
using Crypto.Domain.Models;
using MongoDB.Bson;
using Nethereum.KeyStore;

namespace Crypto.Application.Handlers.ERC20;

public class TransferERC20ToP2PWalletHandler : EthereumWalletBaseHandler<TransferERC20ToP2PWalletCommand, TransactionDetails>
{
    private readonly Erc20AccountManager _accountManager;
    private readonly IEthereumP2PWalletsRepository<ObjectId> _p2pWalletsRepository;
    public TransferERC20ToP2PWalletHandler(Erc20AccountManager accountManager, 
        IEthereumWalletsRepository<ObjectId> repository, 
        IEthereumP2PWalletsRepository<ObjectId> p2PWalletsRepository) : base(repository)
    {
        _accountManager = accountManager;
        _p2pWalletsRepository = p2PWalletsRepository;
    }
    
    public override async Task<TransactionDetails> Handle(TransferERC20ToP2PWalletCommand request, CancellationToken token)
    {
        var userId = ObjectId.Parse(request.WalletId);
        var userWallet = await _repository.FindOneAsync(w => w.Id == userId, wallet => wallet, token);
        if (userWallet == null) throw new NotFoundException($"Wallet with id {userId} is not found");
        if (userWallet.IsLocked) throw new AccountLockedException(userWallet.UnlockDate!.Value);
        if (userWallet == null || userWallet.Id == ObjectId.Empty)
            throw new ArgumentException($"Wallet with id {request.WalletId} does not exist");
        var scryptService = new KeyStoreScryptService();
        var p2pWallet = await _p2pWalletsRepository.FindOneAsync(w => w.Id == userId, wallet => wallet, token);
        if (p2pWallet == null) throw new NotFoundException($"P2P wallet with id {userId} is not found");
        if (p2pWallet.IsLocked) throw new AccountLockedException(p2pWallet.UnlockDate!.Value);
        var account = _accountManager.LoadAccountFromKeyStore(scryptService.SerializeKeyStoreToJson(userWallet.KeyStore), userWallet.Hash);
        return await _accountManager.TransferAsync(p2pWallet.KeyStore.Address, request.Amount, account, token);
    }
}