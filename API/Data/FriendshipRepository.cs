using API.DTOs.Friendships;
using API.Entities;
using API.Enums;
using API.Helpers;
using API.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace API.Data;

public class FriendshipRepository : IFriendshipRepository
{
    private readonly DataContext _context;

    public FriendshipRepository(DataContext context)
    {
        _context = context;
    }

    public async Task<bool> AreFriendsAsync(Guid userId, Guid friendId, CancellationToken ct)
    {
        return await _context.Friendships.AnyAsync(f =>
            ((f.RequesterId == userId && f.ReceiverId == friendId) ||
             (f.RequesterId == friendId && f.ReceiverId == userId)) &&
             f.Status == FriendshipStatus.Accepted, ct);
    }

    public async Task<bool> CancelFriendRequestAsync(Guid userId, Guid friendshipId, CancellationToken ct)
    {
        var friendship = await _context.Friendships
            .FirstOrDefaultAsync(f => 
                f.Id == friendshipId && 
                f.RequesterId == userId && 
                f.Status == FriendshipStatus.Pending, ct);

        if (friendship == null)
            return false;

        _context.Friendships.Remove(friendship);
        return true;
    }

    public async Task<Friendship> GetFriendshipByIdAsync(Guid friendshipId, CancellationToken ct)
    {
        return await _context.Friendships.FindAsync(friendshipId, ct);
    }

    public async Task<PagedList<Friendship>> GetPendingRequestsAsync(Guid userId, ElementParams elementParams, CancellationToken ct)
    {
        var friendRequests = _context.Friendships
            .Include(fr => fr.Requester)
            .Where(fr => fr.ReceiverId == userId && fr.Status == FriendshipStatus.Pending);

        return await PagedList<Friendship>.CreateAsync(friendRequests, elementParams.PageNumber, elementParams.PageSize, ct);
    }

    public async Task<PagedList<Friendship>> GetUserFriendsAsync(Guid userId, ElementParams elementParams, CancellationToken ct)
    {
        var friends = _context.Friendships
            .Include(f => f.Requester)
            .Include(f => f.Receiver)
            .Where(f => (f.RequesterId == userId || f.ReceiverId == userId) && f.Status == FriendshipStatus.Accepted);
            
        return await PagedList<Friendship>.CreateAsync(friends, elementParams.PageNumber, elementParams.PageSize, ct);
    }

    public async Task<bool> RemoveFriendAsync(Guid userId, Guid friendId, CancellationToken ct)
    {
        var friendship = await _context.Friendships
            .FirstOrDefaultAsync(f => 
                ((f.RequesterId == userId && f.ReceiverId == friendId) ||
                (f.RequesterId == friendId && f.ReceiverId == userId)) &&
                f.Status == FriendshipStatus.Accepted, ct);

        if (friendship == null)
            return false;

        _context.Friendships.Remove(friendship);
        return true;
    }

    public async Task<bool> SendFriendRequestAsync(Guid userId, Guid friendId, CancellationToken ct)
    {
        if (await _context.Friendships.AnyAsync(f =>
                (f.RequesterId == userId && f.ReceiverId == friendId) ||
                (f.RequesterId == friendId && f.ReceiverId == userId), ct))
        {
            return false; // Already friends or request exists
        }

        var friendship = new Friendship
        {
            RequesterId = userId,
            ReceiverId = friendId,
            Status = FriendshipStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };

        _context.Friendships.Add(friendship);
        return true;
    }
}