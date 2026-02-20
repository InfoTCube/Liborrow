using API.Enums;

namespace API.Entities;

public class Friendship
{
    public Guid Id { get; set; }
    public Guid RequesterId { get; set; }
    public AppUser? Requester { get; set; }
    public Guid ReceiverId { get; set; }
    public AppUser? Receiver { get; set; }

    public DateTime CreatedAt { get; set; }
    public FriendshipStatus Status { get; set; } = FriendshipStatus.Pending;
}