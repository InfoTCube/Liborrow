using API.Enums;

namespace API.DTOs.Friendships;

public record FriendRequestDto
{
    public Guid FriendshipId { get; set; }
    public Guid RequesterId { get; set; }
    public string RequesterName { get; set; } = string.Empty;
    public DateTime RequestedAt { get; set; }
    public FriendshipStatus Status { get; set; }
}