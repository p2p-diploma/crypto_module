using Crypto.Application.Commands.ERC20;
using Crypto.Application.Handlers.Base;
using Crypto.Application.Utils;
using Crypto.Domain.Configuration;
using Crypto.Domain.Exceptions;
using Crypto.Domain.Interfaces;
using Crypto.Domain.Models;
using MongoDB.Bson;
using Nethereum.KeyStore;

namespace Crypto.Application.Handlers.ERC20;

public class RefundERC20FromP2PWalletHandler : ERC20TransferHandlerBase<RefundERC20FromP2PWalletCommand,bool>
{
    private readonly IWalletsRepository<EthereumP2PWallet<ObjectId>, ObjectId> _p2pWalletsRepository;
    private readonly IWalletsRepository<EthereumWallet<ObjectId>, ObjectId> _platformWalletsRepository;
    
    public RefundERC20FromP2PWalletHandler(EthereumAccountManager accountManager, SmartContractSettings settings, 
        IWalletsRepository<EthereumP2PWallet<ObjectId>, ObjectId> p2pWalletsRepository, 
        IWalletsRepository<EthereumWallet<ObjectId>, ObjectId> platformWalletsRepository) : base(accountManager, settings)
    {
        _p2pWalletsRepository = p2pWalletsRepository;
        _platformWalletsRepository = platformWalletsRepository;
    }

    public override async Task<bool> Handle(RefundERC20FromP2PWalletCommand request, CancellationToken cancellationToken)
    {
        var userId = ObjectId.Parse(request.WalletId);
        var p2pWallet = await _p2pWalletsRepository.FindOneAsync(w => w.UserId == userId, cancellationToken);
        if (p2pWallet == null || p2pWallet.Id == ObjectId.Empty) 
            throw new AccountNotFoundException($"P2P wallet with id {request.WalletId} is not found");
        
        var userWallet = await _platformWalletsRepository.FindOneAsync(w => w.Id == userId, cancellationToken);
        var scryptService = new KeyStoreScryptService();
        var account = _accountManager.LoadAccountFromKeyStore(scryptService.SerializeKeyStoreToJson(p2pWallet.KeyStore), p2pWallet.Hash);
        return await Transfer(userWallet.KeyStore.Address, request.Amount, account, cancellationToken);
    }
}