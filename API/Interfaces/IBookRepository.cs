using API.DTOs.Books;
using API.Entities;
using API.Helpers;

namespace API.Interfaces;

public interface IBookRepository
{
    Task<Book?> GetBookByIsbnAsync(string isbn);
    Task<UserBook?> GetUserBookByIdAndUserIdAsync(string isbn, Guid userId);
    Task<bool> UserOwnsBookAsync(Guid userId, string isbn);
    Task AddBookAsync(Book book);
    Task<UserBook> AddUserBookAsync(Guid userId, string isbn, string? notes);
    Task<PagedList<UserBook>> GetUserBooksAsync(Guid userId, ElementParams elementParams);
    Task<PagedList<UserBook>> SearchFriendsBooksAsync(Guid userId, string query, ElementParams elementParams);
}