using API.DTOs.Users;
using API.Entities;

namespace API.Interfaces;

public interface IUserRepository
{
    Task<AppUser?> GetUserByIdAsync(Guid id);
    Task<AppUser?> GetUserByEmailAsync(string email);
    Task<AppUser?> GetUserByUsernameAsync(string username);
    
    Task<IEnumerable<UserSearchDto>> SearchUsersAsync(Guid currentUserId, string query);
    
    //Task<UserProfileDto?> GetUserProfileAsync(Guid userId, Guid currentUserId);
    
    //Task<UserStatsDto> GetUserStatsAsync(Guid userId);
    
    //Task<int> GetUserBooksCountAsync(Guid userId);
    //Task<int> GetUserFriendsCountAsync(Guid userId);
    
    //void UpdateUser(AppUser user);
}