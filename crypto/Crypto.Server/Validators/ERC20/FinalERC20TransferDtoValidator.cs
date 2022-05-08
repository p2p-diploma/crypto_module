using Crypto.Domain.Dtos.ERC20;
using FluentValidation;
using Nethereum.Web3;

namespace Crypto.Server.Validators.ERC20;

public class FinalERC20TransferDtoValidator : AbstractValidator<FinalERC20TransferDto>
{
    public FinalERC20TransferDtoValidator()
    {
        RuleFor(r => r.Recipient).NotEmpty().Must(Web3.IsChecksumAddress).WithMessage("Recipient's address is invalid");
        RuleFor(r => r.Sender).NotEmpty().Must(Web3.IsChecksumAddress).WithMessage("Sender's address is invalid");
    }
}