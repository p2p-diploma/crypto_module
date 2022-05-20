using Crypto.Application.Commands.Wallets.Buy;
using Crypto.Application.Commands.Wallets.Sell;
using Crypto.Domain.Configuration;
using FluentValidation;

namespace Wallets.Server.Validators.Sell;

public class ReduceAmountToSellValidator : AbstractValidator<ReduceAmountToSellCommand>
{
    public ReduceAmountToSellValidator()
    {
        RuleFor(c => c.WalletId).NotEmpty().Must(IsParsable).WithMessage("Wallet id is invalid");
        RuleFor(c => c.Amount).GreaterThan(0).WithMessage("Amount to sell must be greater than 0");
    }
}