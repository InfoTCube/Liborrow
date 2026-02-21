using API.DTOs.Friendships;
using API.Entities;
using API.Enums;
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

    public async Task<bool> AreFriendsAsync(Guid userId, Guid friendId)
    {
        return await _context.Friendships.AnyAsync(f =>
            ((f.RequesterId == userId && f.ReceiverId == friendId) ||
             (f.RequesterId == friendId && f.ReceiverId == userId)) &&
             f.Status == FriendshipStatus.Accepted);
    }

    public async Task<bool> CancelFriendRequestAsync(Guid userId, Guid friendshipId)
    {
        var friendship = await _context.Friendships
            .FirstOrDefaultAsync(f => 
                f.Id == friendshipId && 
                f.RequesterId == userId && 
                f.Status == FriendshipStatus.Pending);

        if (friendship == null)
            return false;

        _context.Friendships.Remove(friendship);
        return true;
    }

    public async Task<Friendship> GetFriendshipByIdAsync(Guid friendshipId)
    {
        return await _context.Friendships.FindAsync(friendshipId);
    }

    public async Task<IEnumerable<FriendRequestDto>> GetPendingRequestsAsync(Guid userId)
    {
        return await _context.Friendships
            .Include(fr => fr.Requester)
            .Where(fr => fr.ReceiverId == userId && fr.Status == FriendshipStatus.Pending)
            .Select(fr => new FriendRequestDto
            {
                FriendshipId = fr.Id,
                RequesterId = fr.RequesterId,
                RequesterName = fr.Requester.UserName,
                RequestedAt = fr.CreatedAt,
                Status = fr.Status
            })
            .ToListAsync();
    }

    public async Task<IEnumerable<FriendDto>> GetUserFriendsAsync(Guid userId)
    {
        return await _context.Friendships
            .Include(f => f.Requester)
            .Include(f => f.Receiver)
            .Where(f => (f.RequesterId == userId || f.ReceiverId == userId) && f.Status == FriendshipStatus.Accepted)
            .Select(f => new FriendDto
            {
                UserId = f.RequesterId == userId ? f.ReceiverId : f.RequesterId,
                UserName = f.RequesterId == userId ? f.Receiver.UserName : f.Requester.UserName,
                FriendshipId = f.Id
            })
            .ToListAsync();
    }

    public async Task<bool> RemoveFriendAsync(Guid userId, Guid friendId)
    {
        var friendship = await _context.Friendships
            .FirstOrDefaultAsync(f => 
                ((f.RequesterId == userId && f.ReceiverId == friendId) ||
                (f.RequesterId == friendId && f.ReceiverId == userId)) &&
                f.Status == FriendshipStatus.Accepted);

        if (friendship == null)
            return false;

        _context.Friendships.Remove(friendship);
        return true;
    }

    public async Task<bool> SendFriendRequestAsync(Guid userId, Guid friendId)
    {
        if (await _context.Friendships.AnyAsync(f =>
                (f.RequesterId == userId && f.ReceiverId == friendId) ||
                (f.RequesterId == friendId && f.ReceiverId == userId)))
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