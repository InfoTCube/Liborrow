using API.DTOs.Books;
using API.Entities;

namespace API.Interfaces;

public interface IBookRepository
{
    Task<Book?> GetBookByIsbnAsync(string isbn);
    Task<UserBook?> GetUserBookByIdAndUserIdAsync(string isbn, Guid userId);
    Task<bool> UserOwnsBookAsync(Guid userId, string isbn);
    Task AddBookAsync(Book book);
    Task<UserBook> AddUserBookAsync(Guid userId, string isbn, string? notes);
    Task<IEnumerable<BookDto>> GetUserBooksAsync(Guid userId);
    Task<IEnumerable<BookDto>> SearchFriendsBooksAsync(Guid userId, string query);
}