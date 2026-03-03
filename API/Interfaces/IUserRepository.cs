using API.DTOs.Users;
using API.Entities;
using API.Helpers;

namespace API.Interfaces;

public interface IUserRepository
{
    Task<AppUser?> GetUserByIdAsync(Guid id, CancellationToken ct);
    Task<AppUser?> GetUserByEmailAsync(string email, CancellationToken ct);
    Task<AppUser?> GetUserByUsernameAsync(string username, CancellationToken ct);
    
    Task<PagedList<UserSearchDto>> SearchUsersAsync(Guid currentUserId, string query, ElementParams elementParams, CancellationToken ct);
    
    //Task<UserProfileDto?> GetUserProfileAsync(Guid userId, Guid currentUserId);
    
    //Task<UserStatsDto> GetUserStatsAsync(Guid userId);
    
    //Task<int> GetUserBooksCountAsync(Guid userId);
    //Task<int> GetUserFriendsCountAsync(Guid userId);
    
    //void UpdateUser(AppUser user);
}