using API.DTOs.Books;
using FluentValidation;

namespace API.Validators;

public class AddBookValidator : AbstractValidator<AddBookDto>
{
    public AddBookValidator()
    {
        RuleFor(x => x.ISBN)
            .NotEmpty().WithMessage("ISBN is required.")
            .Matches(@"^(?:97[89]-?)?\d{1,5}-?\d{1,7}-?\d{1,6}-?[\dX]$").WithMessage("Invalid ISBN format.");
    }
}

public class AddBookManualValidator : AbstractValidator<AddBookManualDto>
{
    public AddBookManualValidator()
    {
        RuleFor(x => x.ISBN)
            .NotEmpty().WithMessage("ISBN is required.")
            .Matches(@"^(?:97[89]-?)?\d{1,5}-?\d{1,7}-?\d{1,6}-?[\dX]$").WithMessage("Invalid ISBN format.");
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required.");
        RuleFor(x => x.Author)
            .MaximumLength(255).When(x => !string.IsNullOrEmpty(x.Author));
        RuleFor(x => x.CoverImageUrl)
            .Must(uri => Uri.IsWellFormedUriString(uri, UriKind.Absolute))
            .When(x => !string.IsNullOrEmpty(x.CoverImageUrl))
            .WithMessage("CoverImageUrl must be a valid URL.");
        RuleFor(x => x.PageCount)
            .GreaterThan(0).When(x => x.PageCount.HasValue)
            .WithMessage("PageCount must be greater than 0.");
    }
}