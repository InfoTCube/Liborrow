using API.DTOs.Friendships;
using API.Entities;
using API.Helpers;

namespace API.Interfaces;

public interface IFriendshipRepository
{
    Task<bool> SendFriendRequestAsync(Guid userId, Guid friendId, CancellationToken ct);
    Task<Friendship> GetFriendshipByIdAsync(Guid friendshipId, CancellationToken ct);
    Task<PagedList<Friendship>> GetUserFriendsAsync(Guid userId, ElementParams elementParams, CancellationToken ct);
    Task<PagedList<Friendship>> GetPendingRequestsAsync(Guid userId, ElementParams elementParams, CancellationToken ct);
    Task<bool> RemoveFriendAsync(Guid userId, Guid friendId, CancellationToken ct);
    Task<bool> CancelFriendRequestAsync(Guid userId, Guid friendshipId, CancellationToken ct);
    Task<bool> AreFriendsAsync(Guid userId, Guid friendId, CancellationToken ct);
}