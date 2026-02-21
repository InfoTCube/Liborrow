using API.DTOs.Friendships;
using API.Entities;

namespace API.Interfaces;

public interface IFriendshipRepository
{
    Task<bool> SendFriendRequestAsync(Guid userId, Guid friendId);
    Task<Friendship> GetFriendshipByIdAsync(Guid friendshipId);
    Task<IEnumerable<FriendDto>> GetUserFriendsAsync(Guid userId);
    Task<IEnumerable<FriendRequestDto>> GetPendingRequestsAsync(Guid userId);
    Task<bool> RemoveFriendAsync(Guid userId, Guid friendId);
    Task<bool> CancelFriendRequestAsync(Guid userId, Guid friendshipId);
    Task<bool> AreFriendsAsync(Guid userId, Guid friendId);
}