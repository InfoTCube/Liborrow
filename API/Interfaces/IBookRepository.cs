using API.DTOs.Books;
using API.Entities;
using API.Helpers;

namespace API.Interfaces;

public interface IBookRepository
{
    Task<Book?> GetBookByIsbnAsync(string isbn, CancellationToken ct);
    Task<UserBook?> GetUserBookByIdAndUserIdAsync(string isbn, Guid userId, CancellationToken ct);
    Task<bool> UserOwnsBookAsync(Guid userId, string isbn, CancellationToken ct);
    Task AddBookAsync(Book book, CancellationToken ct);
    Task<UserBook> AddUserBookAsync(Guid userId, string isbn, string? notes, CancellationToken ct);
    Task<PagedList<UserBook>> GetUserBooksAsync(Guid userId, ElementParams elementParams, CancellationToken ct);
    Task<PagedList<UserBook>> SearchFriendsBooksAsync(Guid userId, string query, ElementParams elementParams, CancellationToken ct);
}