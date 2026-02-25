using API.DTOs.Loans;
using API.Entities;
using API.Enums;
using API.Helpers;

namespace API.Interfaces;

public interface ILoanRepository
{
    Task<Loan?> GetLoanByIdAsync(Guid id);
    Task AddLoanAsync(Loan loan);

    Task<PagedList<Loan>> GetLoansForOwnerAsync(Guid ownerId, ElementParams elementParams, LoanStatus? status = null);
    Task<PagedList<Loan>> GetPendingRequestsForOwnerAsync(Guid ownerId, ElementParams elementParams);

    Task<PagedList<Loan>> GetLoansForBorrowerAsync(Guid borrowerId, ElementParams elementParams, LoanStatus? status = null);
    Task<PagedList<Loan>> GetPendingRequestsFromBorrowerAsync(Guid borrowerId, ElementParams elementParams);

    Task<PagedList<Loan>> GetActiveLoansAsync(Guid userId, ElementParams elementParams);

    Task<PagedList<Loan>> GetLoanHistoryAsync(Guid userId, ElementParams elementParams);

    Task<bool> IsBookAvailableForLoanAsync(Guid ownerId, string isbn);
    Task<bool> HasPendingLoanAsync(Guid ownerId, string isbn, Guid borrowerId);
}