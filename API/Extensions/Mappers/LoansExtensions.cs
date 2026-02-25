using API.DTOs.Loans;
using API.Entities;

namespace API.Extensions.Mappers;

public static class LoansExtensions
{
    public static LoanDto ToLoanDto(this Loan loan)
    {
        return new LoanDto
        {
            Id = loan.Id,
            ISBN = loan.Book?.ISBN ?? string.Empty,
            BookTitle = loan.Book?.Title ?? "Unknown",
            BookAuthor = loan.Book?.Author ?? "Unknown",
            BookCoverUrl = loan.Book?.CoverImageUrl ?? string.Empty,
            OwnerId = loan.OwnerId,
            BorrowerId = loan.BorrowerId,
            BorrowerName = loan.Borrower?.UserName ?? "Unknown",
            OwnerName = loan.Owner?.UserName ?? "Unknown",
            RequestedAt = loan.RequestedAt,
            ApprovedAt = loan.ApprovedAt,
            LoanDate = loan.LoanDate,
            DueDate = loan.DueDate,
            ReturnedAt = loan.ReturnedAt,
            RequestMessage = loan.RequestMessage,
            Notes = loan.Notes,
            Status = loan.Status
        };
    }

    public static IEnumerable<LoanDto> ToLoanDto(this IEnumerable<Loan> loans)
    {
        if(loans == null || !loans.Any()) return new List<LoanDto>();
        
        return loans.Select(l => l.ToLoanDto());
    }
}