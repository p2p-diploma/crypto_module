using Crypto.Application.Responses.ERC20;
using MediatR;

namespace Crypto.Application.Queries.ERC20;

public record GetErc20WalletByEmailQuery(string Email) : IRequest<Erc20WalletWithIdResponse>;