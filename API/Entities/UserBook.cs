using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace API.Entities;

public class UserBook
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }
    public AppUser? User { get; set; }

    public string? ISBN { get; set; }
    public Book? Book { get; set; }

    public DateTime AddedAt { get; set; }
    public bool IsAvailable { get; set; }
    public string? Notes { get; set; }

    public ICollection<Loan> Loans { get; set; } = new List<Loan>();
}