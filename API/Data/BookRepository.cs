using API.DTOs.Books;
using API.Entities;
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

    public async Task<Book?> GetBookByIsbnAsync(string isbn)
    {
        return await _context.Books.FindAsync(isbn);
    }

    public async Task<UserBook?> GetUserBookByIdAndUserIdAsync(string isbn, Guid userId)
    {
        return await _context.UserBooks
            .FirstOrDefaultAsync(ub => ub.ISBN == isbn && ub.UserId == userId);
    }

    public async Task AddBookAsync(Book book)
    {
        await _context.Books.AddAsync(book);
    }

    public async Task<UserBook> AddUserBookAsync(Guid userId, string isbn, string? notes)
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
        
        await _context.UserBooks.AddAsync(userBook);
        return userBook;
    }

    public async Task<bool> UserOwnsBookAsync(Guid userId, string isbn)
    {
        return await _context.UserBooks
            .AnyAsync(ub => ub.UserId == userId && ub.ISBN == isbn);
    }

    public async Task<IEnumerable<BookDto>> GetUserBooksAsync(Guid userId)
    {
        return await _context.UserBooks
            .Where(ub => ub.UserId == userId)
            .Join(_context.Books, ub => ub.ISBN, b => b.ISBN, (ub, b) => new BookDto
            {
                ISBN = b.ISBN,
                Title = b.Title,
                Author = b.Author,
                CoverImageUrl = b.CoverImageUrl,
                Description = b.Description,
                PublishedYear = b.PublishedYear,
                PageCount = b.PageCount,
                IsAvailable = ub.IsAvailable,
                Notes = ub.Notes,
                AddedAt = ub.AddedAt
            })
            .ToListAsync();
    }

    public async Task<IEnumerable<BookDto>> SearchFriendsBooksAsync(Guid userId, string query)
    {
        var friendIds = await _context.Friendships
            .Where(f =>
                f.RequesterId == userId || f.ReceiverId == userId)
            .Select(f => f.RequesterId == userId ? f.ReceiverId : f.RequesterId)
            .ToListAsync();

        return await _context.UserBooks
            .Where(ub => friendIds.Contains(ub.UserId) && ub.Book.Title.ToLower().Contains(query.ToLower()))
            .Join(_context.Books, ub => ub.ISBN, b => b.ISBN, (ub, b) => new BookDto
            {
                ISBN = b.ISBN,
                Title = b.Title,
                Author = b.Author,
                CoverImageUrl = b.CoverImageUrl,
                Description = b.Description,
                PublishedYear = b.PublishedYear,
                PageCount = b.PageCount,
                IsAvailable = ub.IsAvailable,
                Notes = ub.Notes,
                AddedAt = ub.AddedAt
            })
            .ToListAsync();
    }
}