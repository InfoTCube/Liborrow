using API.DTOs.Users;
using API.Entities;
using API.Helpers;
using API.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace API.Data;

public class UserRepository : IUserRepository
{
    private readonly DataContext _context;

    public UserRepository(DataContext context)
    {
        _context = context;
    }

    public async Task<AppUser?> GetUserByEmailAsync(string email)
    {
        return await _context.Users
            .FirstOrDefaultAsync(u => u.Email == email.ToLower());
    }

    public async Task<AppUser?> GetUserByIdAsync(Guid id)
    {
        return await _context.Users
            .Include(u => u.Books)
                .ThenInclude(ub => ub.Book)
            .Include(u => u.SentFriendRequests)
            .Include(u => u.ReceivedFriendRequests)
            .FirstOrDefaultAsync(u => u.Id == id);
    }

    public async Task<AppUser?> GetUserByUsernameAsync(string username)
    {
        return await _context.Users
            .FirstOrDefaultAsync(u => u.UserName == username.ToLower());
    }

    public async Task<PagedList<UserSearchDto>> SearchUsersAsync(Guid currentUserId, string query, ElementParams elementParams)
    {
        var users = _context.Users
            .Where(u => u.Id != currentUserId &&
                       (u.UserName.Contains(query) ||
                        u.Email.Contains(query)))
            .Select(u => new UserSearchDto
            {
                UserId = u.Id,
                UserName = u.UserName,
                Email = u.Email,
                BooksCount = u.Books.Count,
                friendshipStatus = _context.Friendships
                    .Where(f => (f.RequesterId == currentUserId && f.ReceiverId == u.Id) ||
                               (f.RequesterId == u.Id && f.ReceiverId == currentUserId))
                    .Select(f => f.Status)
                    .FirstOrDefault(),
                CreatedAt = u.CreatedAt
            });

        return await PagedList<UserSearchDto>.CreateAsync(users, elementParams.PageNumber, elementParams.PageSize);
    }
}