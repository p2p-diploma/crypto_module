﻿using Crypto.Application.Handlers.Base;
using Crypto.Application.Queries.ERC20;
using Crypto.Application.Responses.ERC20;
using Crypto.Application.Utils;
using Crypto.Domain.Configuration;
using Crypto.Domain.Interfaces;
using Crypto.Domain.Models;
using MongoDB.Bson;
using Nethereum.KeyStore;
using Nethereum.Util;
using Nethereum.Web3;

namespace Crypto.Application.Handlers.Wallets.ERC20;

public class GetERC20P2PWalletByEmailHandler : WalletHandlerBase<GetErc20P2PWalletByEmailQuery, Erc20P2PWalletWithIdResponse, EthereumP2PWallet<ObjectId>>
{
    private readonly EthereumAccountManager _accountManager;
    private readonly string _tokenAddress;
    public GetERC20P2PWalletByEmailHandler(IWalletsRepository<EthereumP2PWallet<ObjectId>, ObjectId> repository, 
        EthereumAccountManager accountManager, SmartContractSettings settings) : base(repository)
    {
        _accountManager = accountManager;
        _tokenAddress = settings.StandardERC20Address;
    }
    public override async Task<Erc20P2PWalletWithIdResponse> Handle(GetErc20P2PWalletByEmailQuery request, CancellationToken cancellationToken)
    {
        var wallet = await _repository.FindOneAsync(w => w.Email == request.Email, cancellationToken);
        if (wallet == null || wallet.Id == ObjectId.Empty)
            throw new ArgumentException($"Token wallet with email {request.Email} does not exist");
        var scryptService = new KeyStoreScryptService();
        var loadedAccount = _accountManager.LoadAccountFromKeyStore(scryptService.SerializeKeyStoreToJson(wallet.KeyStore), wallet.Hash);
        var web3 = new Web3(loadedAccount, _accountManager.BlockchainUrl);
        var tokensAmount = await web3.Eth.ERC20.GetContractService(_tokenAddress).BalanceOfQueryAsync(loadedAccount.Address);
        return new(wallet.Id.ToString(), loadedAccount.Address, Web3.Convert.FromWei(tokensAmount, UnitConversion.EthUnit.Gwei));
    }

    
}