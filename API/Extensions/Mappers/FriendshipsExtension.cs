using API.DTOs.Friendships;
using API.Entities;

namespace API.Extensions.Mappers;

public static class FriendshipsExtension
{
    public static FriendRequestDto ToFriendRequestDto(this Friendship friendship)
    {
        return new FriendRequestDto
        {
            FriendshipId = friendship.Id,
            RequesterId = friendship.RequesterId,
            RequesterName = friendship.Requester?.UserName ?? "Unknown",
            RequestedAt = friendship.CreatedAt,
            Status = friendship.Status
        };
    }

    public static IEnumerable<FriendRequestDto> ToFriendRequestDto(this IEnumerable<Friendship> friendships)
    {
        if(friendships == null || !friendships.Any()) return new List<FriendRequestDto>();

        return friendships.Select(f => f.ToFriendRequestDto());
    }

    public static FriendDto ToFriendDto(this Friendship friendship, Guid currentUserId)
    {
         var friend = friendship.RequesterId == currentUserId
            ? friendship.Receiver
            : friendship.Requester;
     
        return new FriendDto
        {
            UserId = friend!.Id,
            UserName = friend.UserName ?? "Unknown",
            FriendshipId = friendship.Id
        };
    }

    public static IEnumerable<FriendDto> ToFriendDto(this IEnumerable<Friendship> friendships, Guid currentUserId)
    {
        if(friendships == null || !friendships.Any()) return new List<FriendDto>();

        return friendships.Select(f => f.ToFriendDto(currentUserId));
    }
}