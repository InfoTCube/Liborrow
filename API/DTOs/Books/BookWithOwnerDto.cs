namespace API.DTOs.Books;

public record BookWithOwnerDto : BookDto
{
    public Guid OwnerId { get; set; }
    public string OwnerName { get; set; } = string.Empty;
}