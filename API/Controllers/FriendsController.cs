using API.DTOs.Friendships;
using API.Enums;
using API.Extensions;
using API.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[Authorize]
public class FriendsController : BaseApiController
{
    private readonly IUnitOfWork _unitOfWork;

    public FriendsController(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    [HttpPost("request/{friendId}")]
    public async Task<ActionResult> SendFriendRequest(Guid friendId)
    {
        var userId = User.GetUserId();
        
        if (userId == friendId)
            return BadRequest("Cannot send friend request to yourself");
        
        var result = await _unitOfWork.Friendships.SendFriendRequestAsync(userId, friendId);
        
        if (!result)
            return BadRequest("Friend request already exists or user is already a friend");
        
        await _unitOfWork.CompleteAsync();
        
        return Ok();
    }
    
    [HttpDelete("request/{friendshipId}/cancel")]
    public async Task<ActionResult> CancelFriendRequest(Guid friendshipId)
    {
        var userId = User.GetUserId();
        
        var success = await _unitOfWork.Friendships.CancelFriendRequestAsync(userId, friendshipId);
        
        if (!success)
            return NotFound("Friend request not found or you don't have permission to cancel it");
        
        if (await _unitOfWork.CompleteAsync())
            return NoContent();
        
        return BadRequest("Failed to cancel friend request");
    }

    [HttpPut("accept/{friendshipId}")]
    public async Task<ActionResult> AcceptFriendRequest(Guid friendshipId)
    {
        var userId = User.GetUserId();
        
        var friendship = await _unitOfWork.Friendships.GetFriendshipByIdAsync(friendshipId);
        
        if (friendship == null || friendship.ReceiverId != userId)
            return NotFound();
        
        friendship.Status = FriendshipStatus.Accepted;
        await _unitOfWork.CompleteAsync();
        
        return Ok();
    }

    [HttpPut("decline/{friendshipId}")]
    public async Task<ActionResult> DeclineFriendRequest(Guid friendshipId)
    {
        var userId = User.GetUserId();
        
        var friendship = await _unitOfWork.Friendships.GetFriendshipByIdAsync(friendshipId);
        
        if (friendship == null || friendship.ReceiverId != userId)
            return NotFound();
        
        friendship.Status = FriendshipStatus.Declined;
        await _unitOfWork.CompleteAsync();
        
        return Ok();
    }

    [HttpDelete("{friendId}")]
    public async Task<ActionResult> RemoveFriend(Guid friendId)
    {
        var userId = User.GetUserId();
        
        var success = await _unitOfWork.Friendships.RemoveFriendAsync(userId, friendId);
        
        if (!success)
            return NotFound("Friendship not found");
        
        if (await _unitOfWork.CompleteAsync())
            return NoContent();
        
        return BadRequest("Failed to remove friend");
    }
    
    [HttpGet]
    public async Task<ActionResult<IEnumerable<FriendDto>>> GetFriends()
    {
        var userId = User.GetUserId();
        
        var friends = await _unitOfWork.Friendships.GetUserFriendsAsync(userId);
        
        return Ok(friends);
    }
    
    [HttpGet("requests")]
    public async Task<ActionResult<IEnumerable<FriendRequestDto>>> GetPendingRequests()
    {
        var userId = User.GetUserId();
        
        var requests = await _unitOfWork.Friendships.GetPendingRequestsAsync(userId);
        
        return Ok(requests);
    }
}