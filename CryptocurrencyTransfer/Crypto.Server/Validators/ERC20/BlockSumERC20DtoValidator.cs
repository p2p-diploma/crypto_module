using Crypto.Domain.Dtos.ERC20;
using FluentValidation;
using Nethereum.Web3;

namespace Crypto.Server.Validators.ERC20;

public class BlockSumERC20DtoValidator : AbstractValidator<BlockSumERC20Dto>
{
    public BlockSumERC20DtoValidator()
    {
        RuleFor(r => r.Recipient).NotEmpty().Must(Web3.IsChecksumAddress).WithMessage("Recipient's address is invalid");
        RuleFor(r => r.Sender).NotEmpty().Must(Web3.IsChecksumAddress).WithMessage("Recipient's address is invalid");
        RuleFor(r => r.TokensAmount).Must(t => t > 0).WithMessage("Tokens amounts is greater than 0");
    }
}