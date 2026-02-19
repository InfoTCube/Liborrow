using API.Enums;

namespace API.DTOs.Loans;

public record LoanDto
{
    public Guid Id { get; set; }
    public string ISBN { get; set; } = string.Empty;
    public string BookTitle { get; set; } = string.Empty;
    public string BookAuthor { get; set; } = string.Empty;
    public string BookCoverUrl { get; set; } = string.Empty;
    public Guid OwnerId { get; set; }
    public string OwnerName { get; set; } = string.Empty;
    public Guid BorrowerId { get; set; }
    public string BorrowerName { get; set; } = string.Empty;
    public DateTime RequestedAt { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public DateTime? LoanDate { get; set; }
    public DateTime? DueDate { get; set; }
    public DateTime? ReturnedAt { get; set; }
    public LoanStatus Status { get; set; }
    public string? RequestMessage { get; set; }
}