using Crypto.Application.Commands.Wallets;
using Crypto.Application.Queries;
using Crypto.Domain.Configuration;
using FluentValidation;

namespace Wallets.Server.Validators.Buy;

public class GetAmountToBuyQueryValidator : AbstractValidator<GetAmountToBuyQuery>
{
    public GetAmountToBuyQueryValidator()
    {
        RuleFor(c => c.WalletId).NotEmpty().Must(IsParsable).WithMessage("Wallet id is invalid");
    }
}