using Crypto.Application.Commands.ERC20;
using Crypto.Application.Handlers.Base;
using Crypto.Domain.Accounts;
using Crypto.Domain.Exceptions;
using Crypto.Domain.Interfaces;
using Crypto.Domain.Models;
using MongoDB.Bson;
using Nethereum.KeyStore;

namespace Crypto.Application.Handlers.ERC20;

public class RefundERC20FromP2PWalletHandler : EthereumP2PWalletBaseHandler<RefundERC20FromP2PWalletCommand, TransactionDetails>
{
    private readonly Erc20AccountManager _accountManager;
    private readonly IEthereumWalletsRepository<ObjectId> _platformWalletsRepository;
    public RefundERC20FromP2PWalletHandler(Erc20AccountManager accountManager, 
        IEthereumP2PWalletsRepository<ObjectId> repository, 
        IEthereumWalletsRepository<ObjectId> platformWalletsRepository) : base(repository)
    {
        _accountManager = accountManager;
        _platformWalletsRepository = platformWalletsRepository;
    }
    public override async Task<TransactionDetails> Handle(RefundERC20FromP2PWalletCommand request, CancellationToken cancellationToken)
    {
        var userId = ObjectId.Parse(request.WalletId);
        var p2pWallet = await _repository.FindOneAsync(w => w.Id == userId, wallet => wallet, cancellationToken);
        if (p2pWallet == null) throw new NotFoundException($"P2P wallet with id {userId} is not found");
        if (p2pWallet.IsLocked) throw new AccountLockedException(p2pWallet.UnlockDate!.Value);
        if (p2pWallet == null || p2pWallet.Id == ObjectId.Empty) 
            throw new NotFoundException($"P2P wallet with id {request.WalletId} is not found");
        
        var userWallet = await _platformWalletsRepository.FindOneAsync(w => w.Id == userId, wallet => wallet, cancellationToken);
        if (userWallet == null) throw new NotFoundException($"P2P wallet with id {userId} is not found");
        if (userWallet.IsLocked) throw new AccountLockedException(p2pWallet.UnlockDate!.Value);
        var scryptService = new KeyStoreScryptService();
        var account = _accountManager.LoadAccountFromKeyStore(scryptService.SerializeKeyStoreToJson(p2pWallet.KeyStore), p2pWallet.Hash);
        return await _accountManager.TransferAsync(userWallet.KeyStore.Address, request.Amount, account, cancellationToken);
    }

}