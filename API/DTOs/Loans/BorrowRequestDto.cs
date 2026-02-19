namespace API.DTOs.Loans;

public record BorrowRequestDto
{
    public string ISBN { get; set; } = string.Empty;
    public Guid OwnerId { get; set; }
    public string? Message { get; set; }
}