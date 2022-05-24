using Crypto.Application.Responses;
using Crypto.Domain.Models;
using MediatR;

namespace Crypto.Application.Commands.ERC20;

public record TransferERC20ToP2PWalletCommand(string WalletId, decimal Amount) : IRequest<TransactionResponse>;