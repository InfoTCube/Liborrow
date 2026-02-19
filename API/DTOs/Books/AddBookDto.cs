using System.ComponentModel.DataAnnotations;

namespace API.DTOs.Books;

public record AddBookDto
{
    [Required]
    public string ISBN { get; set; } = string.Empty;
    public string? Notes { get; set; }
}