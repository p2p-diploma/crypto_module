using Crypto.Application.Commands.Ethereum;
using Crypto.Application.Commands.Wallets;
using FluentValidation;
using Nethereum.Hex.HexConvertors.Extensions;

namespace Wallets.Server.Validators;

public class LoadEthereumWalletValidator : AbstractValidator<LoadEthereumWalletCommand>
{
    public LoadEthereumWalletValidator()
    {
        RuleFor(c => c.Password).NotEmpty().WithMessage("Password is empty")
            .MinimumLength(8).WithMessage("Password's length must be greater than 8 characters");
        RuleFor(c => c.Email).NotEmpty().WithMessage("Email is empty").EmailAddress().WithMessage("Email is invalid");
        RuleFor(c => c.PrivateKey).NotEmpty().WithMessage("Private key is empty")
            .Must(k => k.IsHex()).WithMessage("Private key is invalid");
    }
}