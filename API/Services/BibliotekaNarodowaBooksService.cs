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
            ProcessField(field, book);

        book.CoverImageUrl = null;

        if(string.IsNullOrEmpty(book.Author))
            book.Author = "Unknown Author";

        book.Author = book.Author.TrimEnd(',', ' ');

        return book;
    }

    private void ProcessField(JsonElement field, BookDto book)
    {
        if (TryProcessTitleField(field, out var title)) 
        {
            book.Title = title;
        }
        else if (TryProcessAuthorField(field, out var author)) 
        {
            AppendAuthor(book, author);
        }
        else if (TryProcessPublishField(field, out var year)) 
        {
            book.PublishedYear = year;
        }
        else if (TryProcessPagesField(field, out var pageCount)) 
        {
            book.PageCount = pageCount;
        }
        else if (TryProcessDescriptionField(field, out var description) 
            && string.IsNullOrEmpty(book.Description)) 
        {
            book.Description = description;
        }
    }

    private bool TryProcessTitleField(JsonElement field, out string title)
    {
        title = null;
        if (!field.TryGetProperty("245", out var titleField))
            return false;

        var mainTitle = ExtractSubfieldValue(titleField, "a");
        var subtitle = ExtractSubfieldValue(titleField, "b");
        
        title = string.IsNullOrEmpty(subtitle) 
            ? mainTitle 
            : $"{mainTitle} - {subtitle}";
        
        return true;
    }

    private bool TryProcessAuthorField(JsonElement field, out string author)
    {
        author = null;
        
        if (field.TryGetProperty("100", out var authorField))
        {
            author = ExtractSubfieldValue(authorField, "a");
            return !string.IsNullOrEmpty(author);
        }
        
        if (field.TryGetProperty("700", out var additionalAuthorField) 
            && IsAuthor(additionalAuthorField))
        {
            author = ExtractSubfieldValue(additionalAuthorField, "a");
            return !string.IsNullOrEmpty(author);
        }
        
        return false;
    }

    private bool TryProcessPublishField(JsonElement field, out string year)
    {
        year = null;
        if (!field.TryGetProperty("260", out var publishField))
            return false;

        year = ExtractSubfieldValue(publishField, "c")?.TrimEnd('.', ' ');
        return year != null;
    }

    private bool TryProcessPagesField(JsonElement field, out int pageCount)
    {
        pageCount = 0;
        if (!field.TryGetProperty("300", out var pagesField))
            return false;

        var pagesInfo = ExtractSubfieldValue(pagesField, "a");
        if (string.IsNullOrEmpty(pagesInfo))
            return false;

        var match = System.Text.RegularExpressions.Regex.Match(pagesInfo, @"(\d+)");
        if (match.Success && int.TryParse(match.Groups[1].Value, out pageCount))
            return true;

        return false;
    }

    private bool TryProcessDescriptionField(JsonElement field, out string description)
    {
        description = null;
        if (!field.TryGetProperty("650", out var subjectField))
            return false;

        description = ExtractSubfieldValue(subjectField, "a");
        return !string.IsNullOrEmpty(description);
    }

    private void AppendAuthor(BookDto book, string author)
    {
        var convertedAuthor = ConvertAuthor(author);
        
        if (string.IsNullOrEmpty(book.Author))
            book.Author = convertedAuthor;
        else
            book.Author += $", {convertedAuthor}";
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
            if (TryGetAuthorRole(subfield, out var role) && IsAuthorRole(role))
            {
                return true;
            }
        }
        
        return false;
    }

    private bool TryGetAuthorRole(JsonElement subfield, out string role)
    {
        role = null;
        if (subfield.TryGetProperty("e", out var roleElement))
        {
            role = roleElement.GetString()?.ToLowerInvariant();
            return !string.IsNullOrEmpty(role);
        }
        return false;
    }

    private bool IsAuthorRole(string role)
    {
        var authorIndicators = new[] { "autor", "author" };
        return authorIndicators.Any(indicator => role.Contains(indicator));
    }
}