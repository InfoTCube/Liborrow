using API.DTOs.Books;
using API.Entities;
using API.Enums;

namespace API.Extensions.Mappers;

public static class BooksExtensions
{
    public static Book ToBook(this BookDto bookDto, BookSource source)
    {
        return new Book
        {
            ISBN = bookDto.ISBN,
            Title = bookDto.Title,
            Author = bookDto.Author,
            CoverImageUrl = bookDto.CoverImageUrl,
            Description = bookDto.Description,
            PublishedYear = bookDto.PublishedYear,
            PageCount = bookDto.PageCount,
            Source = source
        };
    }

    public static Book ToBook(this AddBookManualDto bookDto, BookSource source)
    {
        return new Book
        {
            ISBN = bookDto.ISBN,
            Title = bookDto.Title,
            Author = bookDto.Author,
            CoverImageUrl = bookDto.CoverImageUrl,
            Description = bookDto.Description,
            PublishedYear = bookDto.PublishedYear,
            PageCount = bookDto.PageCount,
            Source = source
        };
    }
}