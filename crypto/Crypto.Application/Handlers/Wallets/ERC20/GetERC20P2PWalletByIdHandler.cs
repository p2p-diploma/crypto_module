﻿using Crypto.Application.Handlers.Base;
using Crypto.Application.Queries.ERC20;
using Crypto.Application.Responses.ERC20;
using Crypto.Application.Utils;
using Crypto.Domain.Interfaces;
using Crypto.Domain.Models;
using MongoDB.Bson;
using Nethereum.KeyStore;
using Nethereum.Util;
using Nethereum.Web3;

namespace Crypto.Application.Handlers.Wallets.ERC20;

public class GetERC20P2PWalletByIdHandler : EthereumP2PWalletBaseHandler<GetErc20P2PWalletByIdQuery, Erc20P2PWalletResponse>
{
    private readonly EthereumAccountManager _accountManager;
    public GetERC20P2PWalletByIdHandler(IEthereumP2PWalletsRepository<ObjectId> repository, 
        EthereumAccountManager accountManager) : base(repository)
    {
        _accountManager = accountManager;
    }
    public override async Task<Erc20P2PWalletResponse> Handle(GetErc20P2PWalletByIdQuery request, CancellationToken token)
    {
        var parsedId = ObjectId.Parse(request.WalletId);
        var wallet = await _repository.FindOneAndProjectAsync(w => w.Id == parsedId,wallet => wallet, token);
        if (wallet == null || wallet.Id == ObjectId.Empty)
            throw new ArgumentException($"Token wallet with id {request.WalletId} does not exist");
        var scryptService = new KeyStoreScryptService();
        var loadedAccount = _accountManager.LoadAccountFromKeyStore(scryptService.SerializeKeyStoreToJson(wallet.KeyStore), wallet.Hash);
        var web3 = new Web3(loadedAccount, _accountManager.BlockchainUrl);
        var tokensAmount = await web3.Eth.ERC20.GetContractService(_accountManager.TokenAddress).BalanceOfQueryAsync(loadedAccount.Address);
        return new(loadedAccount.Address, Web3.Convert.FromWei(tokensAmount, UnitConversion.EthUnit.Gwei));
    }
}