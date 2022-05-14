using Crypto.Application.Responses.Ethereum;
using MediatR;

namespace Crypto.Application.Commands.Wallets;

public record CreateEthereumWalletCommand(string Email, string Password) 
    : IRequest<CreatedEthereumWalletResponse>;