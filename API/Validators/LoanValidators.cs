using API.DTOs.Loans;
using FluentValidation;

namespace API.Validators;

public class BorrowRequestValidator : AbstractValidator<BorrowRequestDto>
{
    public BorrowRequestValidator()
    {
        RuleFor(x => x.ISBN)
            .NotEmpty().WithMessage("ISBN is required.")
            .Matches(@"^(?:97[89]-?)?\d{1,5}-?\d{1,7}-?\d{1,6}-?[\dX]$").WithMessage("Invalid ISBN format.");
        
        RuleFor(x => x.OwnerId)
            .NotEmpty().WithMessage("OwnerId is required.");
    }
}