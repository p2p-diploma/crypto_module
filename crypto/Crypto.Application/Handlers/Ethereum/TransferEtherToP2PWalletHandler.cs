using Crypto.Application.Commands.Ethereum;
using Crypto.Application.Handlers.Base;
using Crypto.Application.Responses;
using Crypto.Application.Utils;
using Crypto.Domain.Exceptions;
using Crypto.Domain.Interfaces;
using Crypto.Domain.Models;
using MongoDB.Bson;
using Nethereum.KeyStore;

namespace Crypto.Application.Handlers.Ethereum;

public class TransferEtherToP2PWalletHandler : EthereumWalletBaseHandler<TransferEtherToP2PWalletCommand, TransactionResponse>
{
    private readonly EthereumAccountManager _accountManager;
    private readonly IEthereumP2PWalletsRepository<ObjectId> _p2pWalletsRepository;
    public TransferEtherToP2PWalletHandler(EthereumAccountManager accountManager, 
        IEthereumWalletsRepository<ObjectId> repository, 
        IEthereumP2PWalletsRepository<ObjectId> p2pWalletsRepository) : base(repository)
    {
        _accountManager = accountManager;
        _p2pWalletsRepository = p2pWalletsRepository;
    }
    public override async Task<TransactionResponse> Handle(TransferEtherToP2PWalletCommand request, CancellationToken cancellationToken)
    {
        var userId = ObjectId.Parse(request.WalletId);
        var userWallet = await _repository.FindOneAndProjectAsync(w => w.Id == userId, wallet => wallet, cancellationToken);
        if (userWallet == null) throw new AccountNotFoundException($"Wallet with id {userId} is not found");
        if (userWallet.IsFrozen) throw new AccountFrozenException();
        if (userWallet == null || userWallet.Id == ObjectId.Empty)
            throw new ArgumentException($"Wallet with id {request.WalletId} does not exist");
        
        var p2pWallet = await _p2pWalletsRepository.FindOneAndProjectAsync(w => w.Id == userId, wallet => wallet, cancellationToken);
        if (p2pWallet == null) throw new AccountNotFoundException($"P2P wallet with id {userId} is not found");
        var scryptService = new KeyStoreScryptService();
        var account = _accountManager.LoadAccountFromKeyStore(scryptService.SerializeKeyStoreToJson(userWallet.KeyStore), userWallet.Hash);
        return await _accountManager.Transfer(p2pWallet.KeyStore.Address, request.Amount, account, cancellationToken);
    }

}