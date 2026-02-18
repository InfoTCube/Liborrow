using System.ComponentModel.DataAnnotations;

namespace API.Entities;

public class Book
{
    [Key]
    public string? ISBN { get; set; }

    public string? Title { get; set; }
    public string? Author { get; set; }
    public string? CoverImageUrl { get; set; }
    public string? Description { get; set; }
    public DateOnly? PublishedDate { get; set; }
    public int? PageCount { get; set; }

    public ICollection<UserBook> OwnedBy { get; set; } = new List<UserBook>();
    public ICollection<Loan> Loans { get; set; } = new List<Loan>();
}