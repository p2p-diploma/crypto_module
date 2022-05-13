﻿using MediatR;

namespace Crypto.Application.Commands.ERC20;
public record TransferERC20FromP2PWalletCommand(string WalletId, string RecipientAddress, decimal Amount) : IRequest<bool>;