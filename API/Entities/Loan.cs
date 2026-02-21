using API.Enums;

namespace API.Entities;

public class Loan
{
    public Guid Id { get; set; }

    public string? ISBN { get; set; }
    public Book? Book { get; set; }

    public Guid OwnerId { get; set; }
    public AppUser? Owner { get; set; }

    public Guid BorrowerId { get; set; }
    public AppUser? Borrower { get; set; }

    public DateTime RequestedAt { get; set; } = DateTime.UtcNow;
    public DateTime ApprovedAt { get; set; }
    public DateTime? LoanDate { get; set; }
    public DateTime? DueDate { get; set; }
    public DateTime? ReturnedAt { get; set; }

    public LoanStatus Status { get; set; }

    public string? RequestMessage { get; set; }
    public string? Notes { get; set; }

    public Guid UserBookId { get; set; }
    public UserBook? UserBook { get; set; }
}