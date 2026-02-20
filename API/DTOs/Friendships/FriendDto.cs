namespace API.DTOs.Friendships;

public record FriendDto
{
    public Guid UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public Guid FriendshipId { get; set; }
}