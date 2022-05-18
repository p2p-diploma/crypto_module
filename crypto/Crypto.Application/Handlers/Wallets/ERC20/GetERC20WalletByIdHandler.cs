using Crypto.Application.Handlers.Base;
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

public class GetERC20WalletByIdHandler : WalletHandlerBase<GetErc20WalletByIdQuery, Erc20WalletResponse, EthereumWallet<ObjectId>>
{
    private readonly EthereumAccountManager _accountManager;
    private readonly string _tokenAddress;
    public GetERC20WalletByIdHandler(IWalletsRepository<EthereumWallet<ObjectId>, ObjectId> repository, EthereumAccountManager accountManager,
        SmartContractSettings settings) : base(repository)
    {
        _accountManager = accountManager;
        _tokenAddress = settings.StandardERC20Address;
    }
    public override async Task<Erc20WalletResponse> Handle(GetErc20WalletByIdQuery request, CancellationToken token)
    {
        var parsedId = ObjectId.Parse(request.Id);
        var wallet = await _repository.FindOneAsync(w => w.Id == parsedId, token);
        if (wallet == null || wallet.Id == ObjectId.Empty)
            throw new ArgumentException($"Token wallet with id {request.Id} does not exist");
        var scryptService = new KeyStoreScryptService();
        var loadedAccount = _accountManager.LoadAccountFromKeyStore(scryptService.SerializeKeyStoreToJson(wallet.KeyStore), wallet.Hash);
        var web3 = new Web3(loadedAccount, _accountManager.BlockchainUrl);
        var tokensAmount = await web3.Eth.ERC20.GetContractService(_tokenAddress).BalanceOfQueryAsync(loadedAccount.Address);
        return new(loadedAccount.Address, Web3.Convert.FromWei(tokensAmount, UnitConversion.EthUnit.Gwei));
    }
}