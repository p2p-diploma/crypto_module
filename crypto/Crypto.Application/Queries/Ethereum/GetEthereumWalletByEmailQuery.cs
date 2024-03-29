﻿using Crypto.Application.Responses.Ethereum;
using MediatR;

namespace Crypto.Application.Queries.Ethereum;

public record GetEthereumWalletByEmailQuery(string Email, bool IncludeBalance = true) : IRequest<EthereumWalletWithIdResponse>;