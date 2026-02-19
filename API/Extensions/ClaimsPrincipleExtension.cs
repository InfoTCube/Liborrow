using System.Security.Claims;

namespace API.Extensions;

public static class ClaimsPrincipleExtension
{
    public static Guid GetUserId(this ClaimsPrincipal user)
    {
        var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        
        if (string.IsNullOrEmpty(userIdClaim))
            throw new UnauthorizedAccessException("User ID not found in token");
            
        return Guid.Parse(userIdClaim);
    }

    public static string GetUserName(this ClaimsPrincipal user)
    {
        return user.FindFirst(ClaimTypes.Name)?.Value ?? string.Empty;
    }
    
    public static string GetUserEmail(this ClaimsPrincipal user)
    {
        return user.FindFirst(ClaimTypes.Email)?.Value ?? string.Empty;
    }
}