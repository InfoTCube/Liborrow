using API.DTOs.Loans;
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

    public async Task AddLoanAsync(Loan loan)
    {
        await _context.Loans.AddAsync(loan);
    }

    public async Task<PagedList<LoanDto>> GetActiveLoansAsync(Guid userId, ElementParams elementParams)
    {
        var loans = _context.Loans
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
            });

        return await PagedList<LoanDto>.CreateAsync(loans, elementParams.PageNumber, elementParams.PageSize);
    }

    public async Task<Loan?> GetLoanByIdAsync(Guid id)
    {
        return await _context.Loans.FindAsync(id);
    }

    public async Task<PagedList<LoanDto>> GetLoanHistoryAsync(Guid userId, ElementParams elementParams)
    {
        var loans = _context.Loans
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
            });

        return await PagedList<LoanDto>.CreateAsync(loans, elementParams.PageNumber, elementParams.PageSize);
    }

    public async Task<PagedList<LoanDto>> GetLoansForBorrowerAsync(Guid borrowerId, ElementParams elementParams, LoanStatus? status = null)
    {
        var query = _context.Loans
            .Where(l => l.BorrowerId == borrowerId);

        if (status is not null)
            query = query.Where(l => l.Status == status.Value);

        var loans = query
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
            });

        return await PagedList<LoanDto>.CreateAsync(loans, elementParams.PageNumber, elementParams.PageSize);
    }

    public async Task<PagedList<LoanDto>> GetLoansForOwnerAsync(Guid ownerId, ElementParams elementParams, LoanStatus? status = null)
    {
        var query = _context.Loans
            .Where(l => l.OwnerId == ownerId);

        if (status is not null)
            query = query.Where(l => l.Status == status.Value);

        var loans = query
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
            });

        return await PagedList<LoanDto>.CreateAsync(loans, elementParams.PageNumber, elementParams.PageSize);
    }

    public async Task<PagedList<LoanDto>> GetPendingRequestsForOwnerAsync(Guid ownerId, ElementParams elementParams)
    {
        var loans = _context.Loans
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
            });

        return await PagedList<LoanDto>.CreateAsync(loans, elementParams.PageNumber, elementParams.PageSize);
    }

    public async Task<PagedList<LoanDto>> GetPendingRequestsFromBorrowerAsync(Guid borrowerId, ElementParams elementParams)
    {
        var loans = _context.Loans
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
            });

        return await PagedList<LoanDto>.CreateAsync(loans, elementParams.PageNumber, elementParams.PageSize);
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