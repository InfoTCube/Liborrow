using API.DTOs.Books;

namespace API.Interfaces;

public interface IBibliotekaNarodowaBooksService
{
    Task<BookDto?> GetBookByIsbnAsync(string isbn, CancellationToken ct);
}