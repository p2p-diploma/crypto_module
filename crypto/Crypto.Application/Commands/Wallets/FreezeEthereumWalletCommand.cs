using MediatR;

namespace Crypto.Application.Commands.Wallets;

public record FreezeEthereumWalletCommand(string WalletId) : IRequest<bool>;