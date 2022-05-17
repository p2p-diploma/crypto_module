using Crypto.Application.Queries.ERC20;
using Crypto.Application.Responses.ERC20;
using Crypto.Application.Utils;
using Crypto.Domain.Configuration;
using Crypto.Domain.Interfaces;
using Crypto.Domain.Models;
using MediatR;
using MongoDB.Bson;
using Nethereum.KeyStore;
using Nethereum.Util;
using Nethereum.Web3;

namespace Crypto.Application.Handlers.Wallets.ERC20;

public class GetERC20WalletByEmailHandler : IRequestHandler<GetERC20WalletByEmailQuery, ERC20WalletResponse>
{
    private readonly IWalletsRepository<EthereumWallet<ObjectId>, ObjectId> _repository;
    private readonly EthereumAccountManager _accountManager;
    private readonly string _tokenAddress;
    public GetERC20WalletByEmailHandler(IWalletsRepository<EthereumWallet<ObjectId>, ObjectId> repository, 
        EthereumAccountManager accountManager, SmartContractSettings settings)
    {
        _repository = repository;
        _accountManager = accountManager;
        _tokenAddress = settings.StandardERC20Address;
    }
    public async Task<ERC20WalletResponse> Handle(GetERC20WalletByEmailQuery request, CancellationToken cancellationToken)
    {
        var wallet = await _repository.FindOneAsync(w => w.Email == request.Email, cancellationToken);
        if (wallet.Email == string.Empty)
            throw new ArgumentException($"Token wallet with email {request.Email} does not exist");
        var scryptService = new KeyStoreScryptService();
        var loadedAccount = _accountManager.LoadAccountFromKeyStore(scryptService.SerializeKeyStoreToJson(wallet.KeyStore), wallet.Hash);
        var web3 = new Web3(loadedAccount, _accountManager.BlockchainUrl);
        var tokensAmount = await web3.Eth.ERC20.GetContractService(_tokenAddress).BalanceOfQueryAsync(loadedAccount.Address);
        return new(loadedAccount.Address, Web3.Convert.FromWei(tokensAmount, UnitConversion.EthUnit.Gwei));
    }
}