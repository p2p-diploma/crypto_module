using MediatR;

namespace Crypto.Application.Commands.Wallets;

public record LockEthereumWalletCommand(string Email) : IRequest<bool>;