namespace API.DTOs.Loans;

public record ResponseLoanDto
{
    public bool Accept { get; set; }
    public string? Notes { get; set; }
    public DateTime DueDate { get; set; }
}