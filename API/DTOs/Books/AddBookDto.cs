using System.ComponentModel.DataAnnotations;

namespace API.DTOs.Books;

public record AddBookDto
{
    [Required]
    [RegularExpression(@"^(?:97[89]-?)?\d{1,5}-?\d{1,7}-?\d{1,6}-?[\dX]$", ErrorMessage = "Invalid ISBN format.")]
    public string ISBN { get; set; } = string.Empty;
    public string? Notes { get; set; }
}