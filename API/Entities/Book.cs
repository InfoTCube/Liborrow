using System.ComponentModel.DataAnnotations;
using API.Enums;

namespace API.Entities;

public class Book
{
    [Key]
    public string? ISBN { get; set; }

    public string? Title { get; set; }
    public string? Author { get; set; }
    public string? CoverImageUrl { get; set; }
    public string? Description { get; set; }
    public string? PublishedYear { get; set; }
    public int? PageCount { get; set; }
    public BookSource Source { get; set; }

    public ICollection<UserBook> OwnedBy { get; set; } = new List<UserBook>();
    public ICollection<Loan> Loans { get; set; } = new List<Loan>();
}