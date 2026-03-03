using System.Text.Json;
using API.DTOs.Books;
using API.Interfaces;

namespace API.Services;

public class BibliotekaNarodowaBooksService : IBibliotekaNarodowaBooksService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<BibliotekaNarodowaBooksService> _logger;

    public BibliotekaNarodowaBooksService(HttpClient httpClient, ILogger<BibliotekaNarodowaBooksService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
        _httpClient.BaseAddress = new Uri("https://data.bn.org.pl/api/institutions/");
    }

    public async Task<BookDto?> GetBookByIsbnAsync(string isbn, CancellationToken ct)
    {
        try
        {
            var response = await _httpClient.GetAsync($"bibs.json?isbnIssn={isbn}", ct);
            
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Biblioteka Narodowa API returned {StatusCode} for ISBN: {Isbn}", 
                    response.StatusCode, isbn);
                return null;
            }

            var json = await response.Content.ReadAsStringAsync(ct);
            using var document = JsonDocument.Parse(json);
            var root = document.RootElement;

            if (root.TryGetProperty("bibs", out var bibs) && bibs.GetArrayLength() > 0)
            {
                var firstBib = bibs[0];
                
                var book = ExtractBookFromMarc(firstBib, isbn);
                return book;
            }

            _logger.LogInformation("No book found in Biblioteka Narodowa for ISBN: {Isbn}", isbn);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching book from Biblioteka Narodowa API for ISBN: {Isbn}", isbn);
            return null;
        }
    }

    private BookDto ExtractBookFromMarc(JsonElement bibElement, string isbn)
    {
        var book = new BookDto
        {
            ISBN = isbn,
            Title = "Unknown Title",
            Author = ""
        };

        if (!bibElement.TryGetProperty("marc", out var marc))
            return book;

        if (!marc.TryGetProperty("fields", out var fields))
            return book;

        foreach (var field in fields.EnumerateArray())
        {
            if (field.TryGetProperty("245", out var titleField))
            {
                var title = ExtractSubfieldValue(titleField, "a");
                var subtitle = ExtractSubfieldValue(titleField, "b");
                
                book.Title = title;
                if (!string.IsNullOrEmpty(subtitle))
                {
                    book.Title += " - " + subtitle;
                }
            }
            else if (field.TryGetProperty("100", out var authorField))
            {
                var author = ExtractSubfieldValue(authorField, "a");
                if (!string.IsNullOrEmpty(author))
                {
                    book.Author = ConvertAuthor(author);
                }
            }
            else if (field.TryGetProperty("700", out var additionalAuthorField))
            {
                var additionalAuthor = ExtractSubfieldValue(additionalAuthorField, "a");
                if (!string.IsNullOrEmpty(additionalAuthor))
                {
                    if (!IsAuthor(additionalAuthorField))
                        continue;

                    book.Author +=  ConvertAuthor(additionalAuthor) + ", ";
                }
            }
            else if (field.TryGetProperty("260", out var publishField))
            {
                book.PublishedYear = ExtractSubfieldValue(publishField, "c");
                book.PublishedYear = book.PublishedYear?.TrimEnd('.', ' ');
            }
            else if (field.TryGetProperty("300", out var pagesField))
            {
                var pagesInfo = ExtractSubfieldValue(pagesField, "a");
                if (!string.IsNullOrEmpty(pagesInfo))
                {
                    var match = System.Text.RegularExpressions.Regex.Match(pagesInfo, @"(\d+)");
                    if (match.Success && int.TryParse(match.Groups[1].Value, out int pageCount))
                    {
                        book.PageCount = pageCount;
                    }
                }
            }
            else if (field.TryGetProperty("650", out var subjectField) && string.IsNullOrEmpty(book.Description))
            {
                var subject = ExtractSubfieldValue(subjectField, "a");
                if (!string.IsNullOrEmpty(subject))
                {
                    book.Description = subject;
                }
            }
        }

        book.CoverImageUrl = null;

        if(string.IsNullOrEmpty(book.Author))
        {
            book.Author = "Unknown Author";
        }
        book.Author = book.Author.TrimEnd(',', ' ');

        return book;
    }

    private string? ExtractSubfieldValue(JsonElement field, string subfieldCode)
    {
        if (!field.TryGetProperty("subfields", out var subfields))
            return null;

        foreach (var subfield in subfields.EnumerateArray())
        {
            if (subfield.TryGetProperty(subfieldCode, out var value))
            {
                return value.GetString()?.Trim(' ', ':', '/', ',');
            }
        }

        return null;
    }

    private string? ConvertAuthor(string author)
    {
        if (string.IsNullOrWhiteSpace(author))
            return author;

        var parts = author.Split(',', StringSplitOptions.TrimEntries);
        
        if (parts.Length >= 2)
        {
            var lastName = parts[0];
            var firstNames = string.Join(" ", parts.Skip(1));
            
            return $"{firstNames} {lastName}";
        }
        
        return author;
    }

    private bool IsAuthor(JsonElement field)
    {
        if (!field.TryGetProperty("subfields", out var subfields))
            return false;

        foreach (var subfield in subfields.EnumerateArray())
        {
            if (subfield.TryGetProperty("e", out var role))
            {
                var roleValue = role.GetString()?.ToLowerInvariant();
                
                if (!string.IsNullOrEmpty(roleValue))
                {
                    if (roleValue.Contains("autor") || 
                        roleValue.Contains("author"))
                    {
                        return true;
                    }
                }
            }
        }
        
        return false;
    }
}