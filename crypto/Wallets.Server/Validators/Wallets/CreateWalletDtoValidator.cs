using System.ComponentModel.DataAnnotations;
using Crypto.Domain.Dtos.Wallets;
using FluentValidation;

namespace Wallets.Server.Validators.Wallets;

public class CreateWalletDtoValidator : AbstractValidator<CreateWalletDto>
{
    private readonly EmailAddressAttribute _emailValidator = new();
    public CreateWalletDtoValidator()
    {
        RuleFor(r => r.Email).NotEmpty().Must(e => _emailValidator.IsValid(e)).WithMessage("User email is invalid");
        RuleFor(r => r.Password).NotEmpty().WithMessage("Password is invalid");
    }
}