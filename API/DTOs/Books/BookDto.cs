namespace API.DTOs.Books;

public record BookDto
{
    public string ISBN { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Author { get; set; }
    public string? CoverImageUrl { get; set; }
    public string? Description { get; set; }
    public string? PublishedYear { get; set; }
    public int? PageCount { get; set; }
    public bool IsAvailable { get; set; }
    public DateTime AddedAt { get; set; }
    public string? Notes { get; set; }
}