using Crypto.Application.Responses.Ethereum;
using MediatR;

namespace Crypto.Application.Commands.Wallets;

public record LoadEthereumWalletCommand(string Email, string Password, string PrivateKey) : IRequest<CreatedEthereumWalletResponse>;