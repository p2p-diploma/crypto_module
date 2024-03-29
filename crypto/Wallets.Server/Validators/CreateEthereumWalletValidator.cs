﻿using Crypto.Application.Commands.Ethereum;
using Crypto.Application.Commands.Wallets;
using FluentValidation;

namespace Wallets.Server.Validators;

public class CreateEthereumWalletValidator : AbstractValidator<CreateEthereumWalletCommand>
{
    public CreateEthereumWalletValidator()
    {
        RuleFor(c => c.Password).NotEmpty().WithMessage("Password is empty").MinimumLength(8).WithMessage("Password's length must be greater than 8 characters");
        RuleFor(c => c.Email).NotEmpty().WithMessage("Email is empty").EmailAddress().WithMessage("Email is invalid");
    }
}