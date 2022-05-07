namespace Crypto.Domain.Dtos.Wallets;

public record LoadWalletDto(string Email, string Password, string PrivateKey);