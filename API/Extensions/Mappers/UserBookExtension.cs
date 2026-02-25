using API.DTOs.Books;
using API.Entities;

namespace API.Extensions.Mappers;

public static class UserBookExtension
{
    public static BookDto ToBookDto(this UserBook userBook)
    {
        return new BookDto
        {
            ISBN = userBook.Book?.ISBN ?? string.Empty,
            Title = userBook.Book?.Title ?? string.Empty,
            Author = userBook.Book?.Author ?? string.Empty,
            CoverImageUrl = userBook.Book?.CoverImageUrl ?? string.Empty,
            Description = userBook.Book?.Description ?? string.Empty,
            PublishedYear = userBook.Book?.PublishedYear ?? "0000",
            PageCount = userBook.Book?.PageCount ?? 0,
            IsAvailable = userBook.IsAvailable,
            Notes = userBook.Notes,
            AddedAt = userBook.AddedAt
        };
    }

    public static IEnumerable<BookDto> ToBookDto(this IEnumerable<UserBook> userBooks)
    {
        if (userBooks == null || !userBooks.Any()) return new List<BookDto>();
        
        return userBooks.Select(ub => ub.ToBookDto());
    }

}