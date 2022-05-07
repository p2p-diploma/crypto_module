namespace Crypto.Domain.Dtos.Wallets;

public record EthereumResponseWalletDto(decimal BalanceInEther, string Address, string PrivateKey);