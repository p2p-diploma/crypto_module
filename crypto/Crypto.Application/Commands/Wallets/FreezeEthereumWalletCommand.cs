using MediatR;

namespace Crypto.Application.Commands.Wallets;

public record FreezeEthereumWalletCommand(string Email) : IRequest<bool>;