using Crypto.Application.Commands.ERC20;
using Crypto.Application.Handlers.Base;
using Crypto.Application.Utils;
using Crypto.Domain.Configuration;
using Crypto.Domain.Interfaces;
using Crypto.Domain.Models;
using MongoDB.Bson;
using Nethereum.KeyStore;
using Nethereum.RPC.Accounts;
using Nethereum.Web3;
using Nethereum.Web3.Accounts;

namespace Crypto.Application.Handlers.ERC20;

public class TransferERC20ToP2PWalletHandler : ERC20TransferHandlerBase<TransferERC20ToP2PWalletCommand, bool>
{
    private readonly IWalletsRepository<EthereumP2PWallet<ObjectId>, ObjectId> _p2pWalletsRepository;
    private readonly IWalletsRepository<EthereumWallet<ObjectId>, ObjectId> _platformWalletsRepository;
    public TransferERC20ToP2PWalletHandler(EthereumAccountManager accountManager, SmartContractSettings settings, 
        IWalletsRepository<EthereumWallet<ObjectId>, ObjectId> platformWalletsRepository, 
        IWalletsRepository<EthereumP2PWallet<ObjectId>, ObjectId> p2PWalletsRepository) 
        : base(accountManager, settings)
    {
        _platformWalletsRepository = platformWalletsRepository;
        _p2pWalletsRepository = p2PWalletsRepository;
    }
    
    public override async Task<bool> Handle(TransferERC20ToP2PWalletCommand request, CancellationToken token)
    {
        var userId = ObjectId.Parse(request.WalletId);
        var userWallet = await _platformWalletsRepository.FindOneAsync(w => w.Id == userId, token);
        if (userWallet == null || userWallet.Id == ObjectId.Empty)
            throw new ArgumentException($"Wallet with id {request.WalletId} does not exist");
        var scryptService = new KeyStoreScryptService();
        var p2pWallet = await _p2pWalletsRepository.FindOneAsync(w => w.UserId == userId, token);
        var account = _accountManager.LoadAccountFromKeyStore(scryptService.SerializeKeyStoreToJson(userWallet.KeyStore), userWallet.Hash);
        return await Transfer(p2pWallet.KeyStore.Address, request.Amount, account, token);
    }
}