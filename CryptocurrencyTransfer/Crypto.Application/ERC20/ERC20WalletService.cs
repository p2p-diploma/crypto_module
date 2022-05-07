using Crypto.Application.Ethereum;
using Crypto.Domain.Configuration;
using Crypto.Domain.Contracts.ERC20;
using Crypto.Domain.Dtos.ERC20;
using Crypto.Domain.Interfaces;
using Crypto.Domain.Models;
using MongoDB.Bson;
using Nethereum.KeyStore;
using Nethereum.Util;
using Nethereum.Web3;

namespace Crypto.Application.ERC20;

public class ERC20WalletService
{
    private StandardERC20Service _token;
    private readonly string _tokenAddress;
    private readonly IWalletsRepository<EthereumWallet<ObjectId>, ObjectId> _repository;
    private readonly EthereumAccountManager _accountManager;

    public ERC20WalletService(SmartContractSettings contractOptions,
        IWalletsRepository<EthereumWallet<ObjectId>, ObjectId> repository, EthereumAccountManager accountManager)
    {
        _repository = repository;
        _tokenAddress = contractOptions.StandardERC20Address;
        _tokenAddress = contractOptions.StandardERC20Address;
        _accountManager = accountManager;
    }

    public async Task<ERC20WalletDto> GetTokenWalletAsync(string id, CancellationToken token = default)
    {
        var parsedId = ObjectId.Parse(id);
        var wallet = await _repository.FindOneAsync(w => w.Id == parsedId, token);
        if (wallet.Email == string.Empty)
            throw new ArgumentException($"Token wallet with id {id} does not exist");
        var scryptService = new KeyStoreScryptService();
        var loadedAccount = _accountManager.LoadAccountFromKeyStore(scryptService.SerializeKeyStoreToJson(wallet.KeyStore), wallet.Hash);
        _token = new(new Web3(loadedAccount, _accountManager.BlockchainUrl), _tokenAddress);
        var tokensAmount = await _token.BalanceOfQueryAsync(loadedAccount.Address);
        return new(loadedAccount.Address, Web3.Convert.FromWei(tokensAmount, UnitConversion.EthUnit.Gwei));
    }
}