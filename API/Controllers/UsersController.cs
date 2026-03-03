using API.DTOs.Users;
using API.Extensions;
using API.Helpers;
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
    public async Task<ActionResult<IEnumerable<UserSearchDto>>> SearchUsers([FromQuery] string query, [FromQuery] ElementParams elementParams, 
        CancellationToken ct)
    {
        var currentUserId = User.GetUserId();
        
        var users = await _unitOfWork.Users.SearchUsersAsync(currentUserId, query, elementParams, ct);

        Response.AddPaginationHeader(users.CurrentPage, users.PageSize, 
            users.TotalCount, users.TotalPages);
        
        return Ok(users);
    }
}