namespace API.DTOs.Books;

public class AddBookManualDto
{
    public string ISBN { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Author { get; set; }
    public string? CoverImageUrl { get; set; }
    public string? Description { get; set; }
    public string? PublishedYear { get; set; }
    public int? PageCount { get; set; }
    public string? Notes { get; set; }
}