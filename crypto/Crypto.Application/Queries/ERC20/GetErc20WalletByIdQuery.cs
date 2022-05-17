using Crypto.Application.Responses.ERC20;
using MediatR;

namespace Crypto.Application.Queries.ERC20;

public record GetErc20WalletByIdQuery(string Id) : IRequest<Erc20WalletResponse>;