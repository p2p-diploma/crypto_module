using Crypto.Application.Commands.Wallets.Buy;
using Crypto.Domain.Configuration;
using FluentValidation;

namespace Wallets.Server.Validators.Buy;

public class ReduceAmountToBuyValidator : AbstractValidator<ReduceAmountToBuyCommand>
{
    public ReduceAmountToBuyValidator()
    {
        RuleFor(c => c.WalletId).NotEmpty().Must(IsParsable).WithMessage("Wallet id is invalid");
        RuleFor(c => c.Amount).GreaterThan(0).WithMessage("Amount to buy must be greater than 0");
    }
}