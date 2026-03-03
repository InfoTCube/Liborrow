using API.DTOs.Books;
using API.Entities;
using API.Helpers;
using API.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace API.Data;

public class BookRepository : IBookRepository
{
    private readonly DataContext _context;

    public BookRepository(DataContext context)
    {
        _context = context;
    }

    public async Task<Book?> GetBookByIsbnAsync(string isbn, CancellationToken ct)
    {
        return await _context.Books.FindAsync(isbn, ct);
    }

    public async Task<UserBook?> GetUserBookByIdAndUserIdAsync(string isbn, Guid userId, CancellationToken ct)
    {
        return await _context.UserBooks
            .FirstOrDefaultAsync(ub => ub.ISBN == isbn && ub.UserId == userId, ct);
    }

    public async Task AddBookAsync(Book book, CancellationToken ct)
    {
        await _context.Books.AddAsync(book, ct);
    }

    public async Task<UserBook> AddUserBookAsync(Guid userId, string isbn, string? notes, CancellationToken ct)
    {
        var userBook = new UserBook
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            ISBN = isbn,
            AddedAt = DateTime.UtcNow,
            IsAvailable = true,
            Notes = notes
        };
        
        await _context.UserBooks.AddAsync(userBook, ct);
        return userBook;
    }

    public async Task<bool> UserOwnsBookAsync(Guid userId, string isbn, CancellationToken ct)
    {
        return await _context.UserBooks
            .AnyAsync(ub => ub.UserId == userId && ub.ISBN == isbn, ct);
    }

    public async Task<PagedList<UserBook>> GetUserBooksAsync(Guid userId, ElementParams elementParams, CancellationToken ct)
    {
        var books = _context.UserBooks
            .Where(ub => ub.UserId == userId)
            .Include(ub => ub.Book);
            
        return await PagedList<UserBook>.CreateAsync(books, elementParams.PageNumber, elementParams.PageSize, ct);
    }

    public async Task<PagedList<UserBook>> SearchFriendsBooksAsync(Guid userId, string query, ElementParams elementParams, 
        CancellationToken ct)
    {
        var friendIds = await _context.Friendships
            .Where(f =>
                f.RequesterId == userId || f.ReceiverId == userId)
            .Select(f => f.RequesterId == userId ? f.ReceiverId : f.RequesterId)
            .ToListAsync(ct);

        var books = _context.UserBooks
            .Where(ub => friendIds.Contains(ub.UserId) && ub.Book.Title.ToLower().Contains(query.ToLower()))
            .Include(ub => ub.Book);
            
        return await PagedList<UserBook>.CreateAsync(books, elementParams.PageNumber, elementParams.PageSize, ct);
    }
}