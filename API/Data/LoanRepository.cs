using API.DTOs.Loans;
using API.Entities;
using API.Enums;
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

    public async Task AddLoanAsync(Loan loan)
    {
        await _context.Loans.AddAsync(loan);
    }

    public async Task<IEnumerable<LoanDto>> GetActiveLoansAsync(Guid userId)
    {
        var loans = await _context.Loans
            .Where(l => l.BorrowerId == userId && (l.Status == LoanStatus.Active || l.Status == LoanStatus.Overdue))
            .Include(l => l.Book)
            .Include(l => l.Owner)
            .Include(l => l.Borrower)
            .Select(l => new LoanDto
            {
                Id = l.Id,
                ISBN = l.Book.ISBN,
                BookTitle = l.Book.Title,
                BookAuthor = l.Book.Author,
                BookCoverUrl = l.Book.CoverImageUrl,
                OwnerId = l.OwnerId,
                BorrowerId = l.BorrowerId,
                BorrowerName = l.Borrower.UserName,
                OwnerName = l.Owner.UserName,
                RequestedAt = l.RequestedAt,
                ApprovedAt = l.ApprovedAt,
                LoanDate = l.LoanDate,
                DueDate = l.DueDate,
                ReturnedAt = l.ReturnedAt,
                RequestMessage = l.RequestMessage,
                Notes = l.Notes,
                Status = l.Status
            })
            .ToListAsync();

        return loans;
    }

    public async Task<Loan?> GetLoanByIdAsync(Guid id)
    {
        return await _context.Loans.FindAsync(id);
    }

    public async Task<IEnumerable<LoanDto>> GetLoanHistoryAsync(Guid userId)
    {
        return await _context.Loans
            .Where(l => (l.BorrowerId == userId || l.OwnerId == userId) && 
                l.Status != LoanStatus.Active && l.Status != LoanStatus.Pending && l.Status != LoanStatus.Overdue)
            .Include(l => l.Book)
            .Include(l => l.Owner)
            .Include(l => l.Borrower)
            .Select(l => new LoanDto
            {
                Id = l.Id,
                ISBN = l.Book.ISBN,
                BookTitle = l.Book.Title,
                BookAuthor = l.Book.Author,
                BookCoverUrl = l.Book.CoverImageUrl,
                OwnerId = l.OwnerId,
                BorrowerId = l.BorrowerId,
                BorrowerName = l.Borrower.UserName,
                OwnerName = l.Owner.UserName,
                RequestedAt = l.RequestedAt,
                ApprovedAt = l.ApprovedAt,
                LoanDate = l.LoanDate,
                DueDate = l.DueDate,
                ReturnedAt = l.ReturnedAt,
                RequestMessage = l.RequestMessage,
                Notes = l.Notes,
                Status = l.Status
            })
            .ToListAsync();
    }

    public async Task<IEnumerable<LoanDto>> GetLoansForBorrowerAsync(Guid borrowerId, LoanStatus? status = null)
    {
        var query = _context.Loans
            .Where(l => l.BorrowerId == borrowerId);

        if (status is not null)
            query = query.Where(l => l.Status == status.Value);

        return await query
            .Include(l => l.Book)
            .Include(l => l.Owner)
            .Include(l => l.Borrower)
            .Select(l => new LoanDto
            {
                Id = l.Id,
                ISBN = l.Book.ISBN,
                BookTitle = l.Book.Title,
                BookAuthor = l.Book.Author,
                BookCoverUrl = l.Book.CoverImageUrl,
                OwnerId = l.OwnerId,
                BorrowerId = l.BorrowerId,
                BorrowerName = l.Borrower.UserName,
                OwnerName = l.Owner.UserName,
                RequestedAt = l.RequestedAt,
                ApprovedAt = l.ApprovedAt,
                LoanDate = l.LoanDate,
                DueDate = l.DueDate,
                ReturnedAt = l.ReturnedAt,
                RequestMessage = l.RequestMessage,
                Notes = l.Notes,
                Status = l.Status
            })
            .ToListAsync();
    }

    public async Task<IEnumerable<LoanDto>> GetLoansForOwnerAsync(Guid ownerId, LoanStatus? status = null)
    {
        var query = _context.Loans
            .Where(l => l.OwnerId == ownerId);

        if (status is not null)
            query = query.Where(l => l.Status == status.Value);

        return await query
            .Include(l => l.Book)
            .Include(l => l.Owner)
            .Include(l => l.Borrower)
            .Select(l => new LoanDto
            {
                Id = l.Id,
                ISBN = l.Book.ISBN,
                BookTitle = l.Book.Title,
                BookAuthor = l.Book.Author,
                BookCoverUrl = l.Book.CoverImageUrl,
                OwnerId = l.OwnerId,
                BorrowerId = l.BorrowerId,
                BorrowerName = l.Borrower.UserName,
                OwnerName = l.Owner.UserName,
                RequestedAt = l.RequestedAt,
                ApprovedAt = l.ApprovedAt,
                LoanDate = l.LoanDate,
                DueDate = l.DueDate,
                ReturnedAt = l.ReturnedAt,
                RequestMessage = l.RequestMessage,
                Notes = l.Notes,
                Status = l.Status
            })
            .ToListAsync();
    }

    public async Task<IEnumerable<LoanDto>> GetPendingRequestsForOwnerAsync(Guid ownerId)
    {
        return await _context.Loans
            .Where(l => l.OwnerId == ownerId && l.Status == LoanStatus.Pending)
            .Include(l => l.Book)
            .Include(l => l.Owner)
            .Include(l => l.Borrower)
            .Select(l => new LoanDto
            {
                Id = l.Id,
                ISBN = l.Book.ISBN,
                BookTitle = l.Book.Title,
                BookAuthor = l.Book.Author,
                BookCoverUrl = l.Book.CoverImageUrl,
                OwnerId = l.OwnerId,
                BorrowerId = l.BorrowerId,
                BorrowerName = l.Borrower.UserName,
                OwnerName = l.Owner.UserName,
                RequestedAt = l.RequestedAt,
                ApprovedAt = l.ApprovedAt,
                LoanDate = l.LoanDate,
                DueDate = l.DueDate,
                ReturnedAt = l.ReturnedAt,
                RequestMessage = l.RequestMessage,
                Notes = l.Notes,
                Status = l.Status
            })
            .ToListAsync();
    }

    public async Task<IEnumerable<LoanDto>> GetPendingRequestsFromBorrowerAsync(Guid borrowerId)
    {
        return await _context.Loans
            .Where(l => l.BorrowerId == borrowerId && l.Status == LoanStatus.Pending)
            .Include(l => l.Book)
            .Include(l => l.Owner)
            .Include(l => l.Borrower)
            .Select(l => new LoanDto
            {
                Id = l.Id,
                ISBN = l.Book.ISBN,
                BookTitle = l.Book.Title,
                BookAuthor = l.Book.Author,
                BookCoverUrl = l.Book.CoverImageUrl,
                OwnerId = l.OwnerId,
                BorrowerId = l.BorrowerId,
                BorrowerName = l.Borrower.UserName,
                OwnerName = l.Owner.UserName,
                RequestedAt = l.RequestedAt,
                ApprovedAt = l.ApprovedAt,
                LoanDate = l.LoanDate,
                DueDate = l.DueDate,
                ReturnedAt = l.ReturnedAt,
                RequestMessage = l.RequestMessage,
                Notes = l.Notes,
                Status = l.Status
            })
            .ToListAsync();
    }

    public async Task<bool> HasPendingLoanAsync(Guid ownerId, string isbn, Guid borrowerId)
    {
        return await _context.UserBooks
            .Where(ub => ub.UserId == ownerId && ub.Book.ISBN == isbn)
            .SelectMany(ub => ub.Loans)
            .AnyAsync(l => l.BorrowerId == borrowerId && l.Status == LoanStatus.Pending);
    }

    public async Task<bool> IsBookAvailableForLoanAsync(Guid ownerId, string isbn)
    {
        return await _context.UserBooks
            .Where(ub => ub.UserId == ownerId && ub.Book.ISBN == isbn)
            .AnyAsync(b => b.IsAvailable);
    }
}