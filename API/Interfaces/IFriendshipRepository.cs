using API.DTOs.Friendships;
using API.Entities;
using API.Helpers;

namespace API.Interfaces;

public interface IFriendshipRepository
{
    Task<bool> SendFriendRequestAsync(Guid userId, Guid friendId);
    Task<Friendship> GetFriendshipByIdAsync(Guid friendshipId);
    Task<PagedList<Friendship>> GetUserFriendsAsync(Guid userId, ElementParams elementParams);
    Task<PagedList<Friendship>> GetPendingRequestsAsync(Guid userId, ElementParams elementParams);
    Task<bool> RemoveFriendAsync(Guid userId, Guid friendId);
    Task<bool> CancelFriendRequestAsync(Guid userId, Guid friendshipId);
    Task<bool> AreFriendsAsync(Guid userId, Guid friendId);
}