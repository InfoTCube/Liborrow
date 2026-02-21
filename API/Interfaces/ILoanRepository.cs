using API.DTOs.Loans;
using API.Entities;
using API.Enums;

namespace API.Interfaces;

public interface ILoanRepository
{
    Task<Loan?> GetLoanByIdAsync(Guid id);
    Task AddLoanAsync(Loan loan);

    Task<IEnumerable<LoanDto>> GetLoansForOwnerAsync(Guid ownerId, LoanStatus? status = null);
    Task<IEnumerable<LoanDto>> GetPendingRequestsForOwnerAsync(Guid ownerId);

    Task<IEnumerable<LoanDto>> GetLoansForBorrowerAsync(Guid borrowerId, LoanStatus? status = null);
    Task<IEnumerable<LoanDto>> GetPendingRequestsFromBorrowerAsync(Guid borrowerId);

    Task<IEnumerable<LoanDto>> GetActiveLoansAsync(Guid userId);

    Task<IEnumerable<LoanDto>> GetLoanHistoryAsync(Guid userId);

    Task<bool> IsBookAvailableForLoanAsync(Guid ownerId, string isbn);
    Task<bool> HasPendingLoanAsync(Guid ownerId, string isbn, Guid borrowerId);
}