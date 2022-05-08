using System.ComponentModel.DataAnnotations;
using Crypto.Domain.Dtos.Wallets;
using FluentValidation;

namespace Wallets.Server.Validators.Wallets;

public class LoadWalletDtoValidator : AbstractValidator<LoadWalletDto>
{
    private readonly EmailAddressAttribute _emailValidator = new();
    public LoadWalletDtoValidator()
    {
        RuleFor(r => r.Password).NotEmpty().WithMessage("Password is invalid");
        RuleFor(r => r.PrivateKey).NotEmpty().WithMessage("Private key is empty");
        RuleFor(r => r.Email).NotEmpty().Must(e => _emailValidator.IsValid(e)).WithMessage("User email is invalid");
    }
}