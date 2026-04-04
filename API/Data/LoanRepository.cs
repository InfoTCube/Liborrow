using API.Entities;
using API.Enums;
using API.Helpers;
using API.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace API.Data;

public class LoanRepository : ILoanRepository
{
    private readonly DataContext _context;

    public LoanRepository(DataContext context)
    {
        _context = context;
    }

    public async Task AddLoanAsync(Loan loan, CancellationToken ct)
    {
        await _context.Loans.AddAsync(loan, ct);
    }

    public async Task<PagedList<Loan>> GetActiveLoansAsync(Guid userId, ElementParams elementParams, CancellationToken ct)
    {
        var loans = _context.Loans
            .Where(l => l.BorrowerId == userId && (l.Status == LoanStatus.Active || l.Status == LoanStatus.Overdue))
            .Include(l => l.Book)
            .Include(l => l.Owner)
            .Include(l => l.Borrower);

        return await PagedList<Loan>.CreateAsync(loans, elementParams.PageNumber, elementParams.PageSize, ct);
    }

    public async Task<Loan?> GetLoanByIdAsync(Guid id, CancellationToken ct)
    {
        return await _context.Loans.FindAsync(id, ct);
    }

    public async Task<PagedList<Loan>> GetLoanHistoryAsync(Guid userId, ElementParams elementParams, CancellationToken ct)
    {
        var loans = _context.Loans
            .Where(l => (l.BorrowerId == userId || l.OwnerId == userId) && 
                l.Status != LoanStatus.Active && l.Status != LoanStatus.Pending && l.Status != LoanStatus.Overdue)
            .Include(l => l.Book)
            .Include(l => l.Owner)
            .Include(l => l.Borrower);

        return await PagedList<Loan>.CreateAsync(loans, elementParams.PageNumber, elementParams.PageSize, ct);
    }

    public async Task<PagedList<Loan>> GetLoansForBorrowerAsync(Guid borrowerId, ElementParams elementParams, 
        CancellationToken ct, LoanStatus? status = null)
    {
        var query = _context.Loans
            .Where(l => l.BorrowerId == borrowerId);

        if (status is not null)
            query = query.Where(l => l.Status == status.Value);

        var loans = query
            .Include(l => l.Book)
            .Include(l => l.Owner)
            .Include(l => l.Borrower);

        return await PagedList<Loan>.CreateAsync(loans, elementParams.PageNumber, elementParams.PageSize, ct);
    }

    public async Task<PagedList<Loan>> GetLoansForOwnerAsync(Guid ownerId, ElementParams elementParams, 
        CancellationToken ct, LoanStatus? status = null)
    {
        var query = _context.Loans
            .Where(l => l.OwnerId == ownerId);

        if (status is not null)
            query = query.Where(l => l.Status == status.Value);

        var loans = query
            .Include(l => l.Book)
            .Include(l => l.Owner)
            .Include(l => l.Borrower);

        return await PagedList<Loan>.CreateAsync(loans, elementParams.PageNumber, elementParams.PageSize, ct);
    }

    public async Task<PagedList<Loan>> GetPendingRequestsForOwnerAsync(Guid ownerId, ElementParams elementParams, 
        CancellationToken ct)
    {
        var loans = _context.Loans
            .Where(l => l.OwnerId == ownerId && l.Status == LoanStatus.Pending)
            .Include(l => l.Book)
            .Include(l => l.Owner)
            .Include(l => l.Borrower);

        return await PagedList<Loan>.CreateAsync(loans, elementParams.PageNumber, elementParams.PageSize, ct);
    }

    public async Task<PagedList<Loan>> GetPendingRequestsFromBorrowerAsync(Guid borrowerId, ElementParams elementParams, 
        CancellationToken ct)
    {
        var loans = _context.Loans
            .Where(l => l.BorrowerId == borrowerId && l.Status == LoanStatus.Pending)
            .Include(l => l.Book)
            .Include(l => l.Owner)
            .Include(l => l.Borrower);

        return await PagedList<Loan>.CreateAsync(loans, elementParams.PageNumber, elementParams.PageSize, ct);
    }

    public async Task<bool> HasPendingLoanAsync(Guid ownerId, string isbn, Guid borrowerId, CancellationToken ct)
    {
        return await _context.UserBooks
            .Where(ub => ub.UserId == ownerId && ub.Book.ISBN == isbn)
            .SelectMany(ub => ub.Loans)
            .AnyAsync(l => l.BorrowerId == borrowerId && l.Status == LoanStatus.Pending, ct);
    }

    public async Task<bool> IsBookAvailableForLoanAsync(Guid ownerId, string isbn, CancellationToken ct)
    {
        return await _context.UserBooks
            .Where(ub => ub.UserId == ownerId && ub.Book.ISBN == isbn)
            .AnyAsync(b => b.IsAvailable, ct);
    }
}