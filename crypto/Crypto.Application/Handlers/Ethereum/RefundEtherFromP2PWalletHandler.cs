using Crypto.Application.Commands.Ethereum;
using Crypto.Application.Handlers.Base;
using Crypto.Application.Responses;
using Crypto.Domain.Accounts;
using Crypto.Domain.Exceptions;
using Crypto.Domain.Interfaces;
using Crypto.Domain.Models;
using MongoDB.Bson;
using Nethereum.KeyStore;

namespace Crypto.Application.Handlers.Ethereum;

public class RefundEtherFromP2PWalletHandler : EthereumP2PWalletBaseHandler<RefundEtherFromP2PWalletCommand, TransactionResponse>
{
    private readonly EthereumAccountManager _accountManager;
    private readonly IEthereumWalletsRepository<ObjectId> _platformWalletsRepository;
    public RefundEtherFromP2PWalletHandler(EthereumAccountManager accountManager, 
        IEthereumP2PWalletsRepository<ObjectId> repository, 
        IEthereumWalletsRepository<ObjectId> platformWalletsRepository) : base(repository)
    {
        _accountManager = accountManager;
        _platformWalletsRepository = platformWalletsRepository;
    }
    
    public override async Task<TransactionResponse> Handle(RefundEtherFromP2PWalletCommand request, CancellationToken cancellationToken)
    {
        var userId = ObjectId.Parse(request.WalletId);
        var p2pWallet = await _repository.FindOneAndProjectAsync(w => w.Id == userId,wallet => wallet, cancellationToken);
        if (p2pWallet == null) throw new AccountNotFoundException($"P2P wallet with id {userId} is not found");
        if (p2pWallet.IsFrozen) throw new AccountFrozenException();
        if (p2pWallet == null || p2pWallet.Id == ObjectId.Empty) 
            throw new AccountNotFoundException($"P2P wallet with id {request.WalletId} is not found");
        
        var userWallet = await _platformWalletsRepository.FindOneAndProjectAsync(w => w.Id == userId,wallet => wallet, cancellationToken);
        if (userWallet == null) throw new AccountNotFoundException($"Wallet with id {userId} is not found");
        if (userWallet.IsFrozen) throw new AccountFrozenException();
        var scryptService = new KeyStoreScryptService();
        var account = _accountManager.LoadAccountFromKeyStore(scryptService.SerializeKeyStoreToJson(p2pWallet.KeyStore), p2pWallet.Hash);
        return await _accountManager.TransferAsync(userWallet.KeyStore.Address, request.Amount, account, cancellationToken);
    }

}