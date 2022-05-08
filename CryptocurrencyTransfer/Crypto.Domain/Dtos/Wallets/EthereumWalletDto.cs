namespace Crypto.Domain.Dtos.Wallets;

public record EthereumWalletDto(decimal Balance, string Address, string PrivateKey);