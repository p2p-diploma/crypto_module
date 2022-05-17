using Crypto.Application.Responses.ERC20;
using MediatR;

namespace Crypto.Application.Queries.ERC20;

public record GetErc20P2PWalletByIdQuery(string WalletId) : IRequest<Erc20P2PWalletResponse>;