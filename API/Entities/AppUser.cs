using Microsoft.AspNetCore.Identity;

namespace API.Entities;

public class AppUser : IdentityUser<Guid>
{
    public ICollection<UserBook> Books { get; set; } = new List<UserBook>();
    public ICollection<Friendship> SentFriendRequests { get; set; } = new List<Friendship>();
    public ICollection<Friendship> ReceivedFriendRequests { get; set; } = new List<Friendship>();
    public ICollection<Loan> LentBooks { get; set; } = new List<Loan>();
    public ICollection<Loan> BorrowedBooks { get; set; } = new List<Loan>();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}