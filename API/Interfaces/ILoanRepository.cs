using API.DTOs.Loans;
using API.Entities;
using API.Enums;
using API.Helpers;

namespace API.Interfaces;

public interface ILoanRepository
{
    Task<Loan?> GetLoanByIdAsync(Guid id, CancellationToken ct);
    Task AddLoanAsync(Loan loan, CancellationToken ct);

    Task<PagedList<Loan>> GetLoansForOwnerAsync(Guid ownerId, ElementParams elementParams, CancellationToken ct, LoanStatus? status = null);
    Task<PagedList<Loan>> GetPendingRequestsForOwnerAsync(Guid ownerId, ElementParams elementParams, CancellationToken ct);

    Task<PagedList<Loan>> GetLoansForBorrowerAsync(Guid borrowerId, ElementParams elementParams, CancellationToken ct, LoanStatus? status = null);
    Task<PagedList<Loan>> GetPendingRequestsFromBorrowerAsync(Guid borrowerId, ElementParams elementParams, CancellationToken ct);

    Task<PagedList<Loan>> GetActiveLoansAsync(Guid userId, ElementParams elementParams, CancellationToken ct);

    Task<PagedList<Loan>> GetLoanHistoryAsync(Guid userId, ElementParams elementParams, CancellationToken ct);

    Task<bool> IsBookAvailableForLoanAsync(Guid ownerId, string isbn, CancellationToken ct);
    Task<bool> HasPendingLoanAsync(Guid ownerId, string isbn, Guid borrowerId, CancellationToken ct);
}