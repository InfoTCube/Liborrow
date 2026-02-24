using API.DTOs.Loans;
using API.Entities;
using API.Enums;
using API.Helpers;

namespace API.Interfaces;

public interface ILoanRepository
{
    Task<Loan?> GetLoanByIdAsync(Guid id);
    Task AddLoanAsync(Loan loan);

    Task<PagedList<LoanDto>> GetLoansForOwnerAsync(Guid ownerId, ElementParams elementParams, LoanStatus? status = null);
    Task<PagedList<LoanDto>> GetPendingRequestsForOwnerAsync(Guid ownerId, ElementParams elementParams);

    Task<PagedList<LoanDto>> GetLoansForBorrowerAsync(Guid borrowerId, ElementParams elementParams, LoanStatus? status = null);
    Task<PagedList<LoanDto>> GetPendingRequestsFromBorrowerAsync(Guid borrowerId, ElementParams elementParams);

    Task<PagedList<LoanDto>> GetActiveLoansAsync(Guid userId, ElementParams elementParams);

    Task<PagedList<LoanDto>> GetLoanHistoryAsync(Guid userId, ElementParams elementParams);

    Task<bool> IsBookAvailableForLoanAsync(Guid ownerId, string isbn);
    Task<bool> HasPendingLoanAsync(Guid ownerId, string isbn, Guid borrowerId);
}