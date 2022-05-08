using Crypto.Domain.Dtos.Wallets;
using Crypto.Domain.Exceptions;
using Crypto.Domain.Interfaces;
using Crypto.Domain.Models;
using MongoDB.Bson;
using Nethereum.KeyStore;
using Nethereum.Web3.Accounts;
using static BCrypt.Net.BCrypt;
namespace Crypto.Application.Ethereum;

public class EthereumWalletService
{
    private readonly IWalletsRepository<EthereumWallet<ObjectId>, ObjectId> _repository;
    private readonly EthereumAccountManager _accountManager;
    public EthereumWalletService(IWalletsRepository<EthereumWallet<ObjectId>, ObjectId> repository, EthereumAccountManager accountManager)
    {
        _repository = repository;
        _accountManager = accountManager;
    }

    public async Task<CreatedWalletDto?> CreateWalletAsync(CreateWalletDto createWallet, CancellationToken token = default)
    {
        if (await _repository.ExistsAsync(w => w.Email == createWallet.Email, token))
            throw new ArgumentException($"Wallet with email {createWallet.Email} already exists");
        var passwordHash = HashPassword(createWallet.Password);
        var keyStore = EthereumAccountManager.GenerateKeyStore(passwordHash, out string privateKey);
        EthereumWallet<ObjectId> ethereumWallet = new() { Email = createWallet.Email, Hash = passwordHash, KeyStore = keyStore, Id = ObjectId.GenerateNewId(DateTime.Now) };
        try
        {
            await _repository.CreateAsync(ethereumWallet, token).ConfigureAwait(false);
            return new(keyStore.Address, privateKey, ethereumWallet.Id.ToString());
        }
        catch
        {
            return null;
        }
    }
    
    public async Task<CreatedWalletDto?> LoadWalletAsync(LoadWalletDto loadWallet, CancellationToken token = default)
    {
        if (await _repository.ExistsAsync(w => w.Email == loadWallet.Email, token))
            throw new ArgumentException($"Wallet with email {loadWallet.Email} already exists");
        var loadedAccount = new Account(loadWallet.PrivateKey, _accountManager.ChainId);
        if (string.IsNullOrWhiteSpace(loadedAccount.Address))
            throw new AccountNotFoundException("Specified private key is invalid: can't load account");
        var passwordHash = HashPassword(loadWallet.Password);
        var keyStore = EthereumAccountManager.GenerateKeyStore(passwordHash, loadWallet.PrivateKey);
        EthereumWallet<ObjectId> ethereumWallet = new() { Email = loadWallet.Email, Hash = passwordHash, KeyStore = keyStore, Id = ObjectId.GenerateNewId(DateTime.Now) };
        try
        {
            await _repository.CreateAsync(ethereumWallet, token).ConfigureAwait(false);
            return new(keyStore.Address, loadWallet.PrivateKey, ethereumWallet.Id.ToString());
        }
        catch
        {
            return null;
        }
    }
    
    
    public async Task<EthereumWalletDto> GetWalletAsync(string id, CancellationToken token = default)
    {
        var parsedId = ObjectId.Parse(id);
        var wallet = await _repository.FindOneAsync(w => w.Id == parsedId, token);
        if (wallet.Email == string.Empty)
            throw new AccountNotFoundException($"Wallet with id {id} does not exist");
        var scryptService = new KeyStoreScryptService();
        var loadedAccount = _accountManager.LoadAccountFromKeyStore(scryptService.SerializeKeyStoreToJson(wallet.KeyStore), wallet.Hash);
        var balanceInEther = await _accountManager.GetAccountBalanceInEtherAsync(loadedAccount);
        return new EthereumWalletDto(balanceInEther, loadedAccount.Address, loadedAccount.PrivateKey);
    }
}