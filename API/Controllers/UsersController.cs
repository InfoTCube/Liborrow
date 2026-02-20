using API.DTOs.Users;
using API.Extensions;
using API.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

public class UsersController : BaseApiController
{
    private readonly IUnitOfWork _unitOfWork;

    public UsersController(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    [Authorize]
    [HttpGet("search")]
    public async Task<ActionResult<IEnumerable<UserSearchDto>>> SearchUsers([FromQuery] string query)
    {
        var currentUserId = User.GetUserId();
        
        var users = await _unitOfWork.Users.SearchUsersAsync(currentUserId, query);
        
        return Ok(users);
    }
}