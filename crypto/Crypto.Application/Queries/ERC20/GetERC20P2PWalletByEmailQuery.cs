using Crypto.Application.Responses.ERC20;
using MediatR;

namespace Crypto.Application.Queries.ERC20;

public record GetERC20P2PWalletByEmailQuery(string Email) : IRequest<ERC20P2PWalletResponse>;