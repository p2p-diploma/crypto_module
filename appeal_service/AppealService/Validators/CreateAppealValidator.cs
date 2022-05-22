using AppealService.Dtos;
using FluentValidation;

namespace AppealService.Validators;

public class CreateAppealValidator : AbstractValidator<CreateAppealDto>
{
    public CreateAppealValidator()
    {
        RuleFor(c => c.BuyerEmail).EmailAddress().WithMessage("Invalid buyer email");
        RuleFor(c => c.SellerEmail).EmailAddress().WithMessage("Invalid seller email");
        RuleFor(c => c.LotId).NotEmpty().WithMessage("Lot id is null or empty");
    }
}