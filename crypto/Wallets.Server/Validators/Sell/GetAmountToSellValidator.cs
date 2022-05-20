using Crypto.Application.Queries;
using Crypto.Domain.Configuration;
using FluentValidation;

namespace Wallets.Server.Validators.Sell;

public class GetAmountToSellValidator : AbstractValidator<GetAmountToSellQuery>
{
    public GetAmountToSellValidator()
    {
        RuleFor(c => c.WalletId).NotEmpty().Must(IsParsable).WithMessage("Wallet id is invalid");
    }
}