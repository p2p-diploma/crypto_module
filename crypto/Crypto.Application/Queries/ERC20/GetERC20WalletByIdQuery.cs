using Crypto.Application.Responses.ERC20;
using MediatR;

namespace Crypto.Application.Queries.ERC20;

public record GetERC20WalletByIdQuery(string Id) : IRequest<ERC20WalletResponse>;