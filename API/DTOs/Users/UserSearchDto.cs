using API.Enums;

namespace API.DTOs.Users;

public record UserSearchDto
{
    public Guid UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public int BooksCount { get; set; }
    public FriendshipStatus friendshipStatus { get; set; }
    public DateTime CreatedAt { get; set; }
}