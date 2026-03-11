using API.Controllers;
using API.DTOs.Friendships;
using API.Entities;
using API.Enums;
using API.Extensions.Mappers;
using API.Helpers;
using API.Interfaces;
using API.Tests.TestHelpers;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace API.Tests.Controllers;

public class FriendsControllerTests : TestBase
{
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly FriendsController _controller;

    public FriendsControllerTests()
    {
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _controller = new FriendsController(_mockUnitOfWork.Object);
        SetupFakeUser(_controller);
    }

    #region SendFriendRequest Tests
    [Fact]
    public async Task SendFriendRequest_ValidInput_ReturnsOkResult()
    {
        // Arrange
        var friendId = Guid.NewGuid();
        _mockUnitOfWork.Setup(u => u.Friendships.SendFriendRequestAsync(It.IsAny<Guid>(), friendId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        _mockUnitOfWork.Setup(u => u.CompleteAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.SendFriendRequest(friendId, CancellationToken.None);

        // Assert
        result.Should().BeOfType<OkResult>();
    }

    [Fact]
    public async Task SendFriendRequest_FriendRequestAlreadyExists_ReturnsBadRequest()
    {
        // Arrange
        var friendId = Guid.NewGuid();
        _mockUnitOfWork.Setup(u => u.Friendships.SendFriendRequestAsync(It.IsAny<Guid>(), friendId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await _controller.SendFriendRequest(friendId, CancellationToken.None);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>()
            .Which.Value.Should().Be("Friend request already exists or user is already a friend");
    }

    [Fact]
    public async Task SendFriendRequest_SendingRequestToSelf_ReturnsBadRequest()
    {
        // Arrange
        var userId = Guid.NewGuid();
        SetupFakeUser(_controller, userId);

        // Act
        var result = await _controller.SendFriendRequest(userId, CancellationToken.None);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>()
            .Which.Value.Should().Be("Cannot send friend request to yourself");
    }
    #endregion

    #region CancelFriendRequest Tests
    [Fact]
    public async Task CancelFriendRequest_ValidInput_ReturnsNoContent()
    {
        // Arrange
        var friendshipId = Guid.NewGuid();
        _mockUnitOfWork.Setup(u => u.Friendships.CancelFriendRequestAsync(It.IsAny<Guid>(), friendshipId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        _mockUnitOfWork.Setup(u => u.CompleteAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.CancelFriendRequest(friendshipId, CancellationToken.None);

        // Assert
        result.Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public async Task CancelFriendRequest_FriendRequestNotFound_ReturnsNotFound()
    {
        // Arrange
        var friendshipId = Guid.NewGuid();
        _mockUnitOfWork.Setup(u => u.Friendships.CancelFriendRequestAsync(It.IsAny<Guid>(), friendshipId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await _controller.CancelFriendRequest(friendshipId, CancellationToken.None);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>()
            .Which.Value.Should().Be("Friend request not found or you don't have permission to cancel it");
    }

    [Fact]
    public async Task CancelFriendRequest_FailedToCancel_ReturnsBadRequest()
    {
        // Arrange
        var friendshipId = Guid.NewGuid();
        _mockUnitOfWork.Setup(u => u.Friendships.CancelFriendRequestAsync(It.IsAny<Guid>(), friendshipId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        _mockUnitOfWork.Setup(u => u.CompleteAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await _controller.CancelFriendRequest(friendshipId, CancellationToken.None);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>()
            .Which.Value.Should().Be("Failed to cancel friend request");
    }
    #endregion

    #region AcceptFriendRequest Tests
    [Fact]
    public async Task AcceptFriendRequest_ValidInput_ReturnsOkResult()
    {
        // Arrange
        var friendshipId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        SetupFakeUser(_controller, userId);
        _mockUnitOfWork.Setup(u => u.Friendships.GetFriendshipByIdAsync(friendshipId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Friendship { ReceiverId = userId });
        _mockUnitOfWork.Setup(u => u.CompleteAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.AcceptFriendRequest(friendshipId, CancellationToken.None);

        // Assert
        result.Should().BeOfType<OkResult>();
    }

    [Fact]
    public async Task AcceptFriendRequest_FriendshipNotFound_ReturnsNotFound()
    {
        // Arrange
        var friendshipId = Guid.NewGuid();
        _mockUnitOfWork.Setup(u => u.Friendships.GetFriendshipByIdAsync(friendshipId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Friendship)null);

        // Act
        var result = await _controller.AcceptFriendRequest(friendshipId, CancellationToken.None);

        // Assert
        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task AcceptFriendRequest_UserNotReceiver_ReturnsNotFound()
    {
        // Arrange
        var friendshipId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        SetupFakeUser(_controller, userId);
        _mockUnitOfWork.Setup(u => u.Friendships.GetFriendshipByIdAsync(friendshipId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Friendship { ReceiverId = Guid.NewGuid() });

        // Act
        var result = await _controller.AcceptFriendRequest(friendshipId, CancellationToken.None);

        // Assert
        result.Should().BeOfType<NotFoundResult>();
    }
    #endregion

    #region DeclineFriendRequest Tests
    [Fact]
    public async Task DeclineFriendRequest_ValidInput_ReturnsOkResult()
    {
        // Arrange
        var friendshipId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        SetupFakeUser(_controller, userId);
        _mockUnitOfWork.Setup(u => u.Friendships.GetFriendshipByIdAsync(friendshipId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Friendship { ReceiverId = userId });
        _mockUnitOfWork.Setup(u => u.CompleteAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.DeclineFriendRequest(friendshipId, CancellationToken.None);

        // Assert
        result.Should().BeOfType<OkResult>();
    }

    [Fact]
    public async Task DeclineFriendRequest_FriendshipNotFound_ReturnsNotFound()
    {
        // Arrange
        var friendshipId = Guid.NewGuid();
        _mockUnitOfWork.Setup(u => u.Friendships.GetFriendshipByIdAsync(friendshipId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Friendship)null);

        // Act
        var result = await _controller.DeclineFriendRequest(friendshipId, CancellationToken.None);

        // Assert
        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task DeclineFriendRequest_UserNotReceiver_ReturnsNotFound()
    {
        // Arrange
        var friendshipId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        SetupFakeUser(_controller, userId);
        _mockUnitOfWork.Setup(u => u.Friendships.GetFriendshipByIdAsync(friendshipId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Friendship { ReceiverId = Guid.NewGuid() });

        // Act
        var result = await _controller.DeclineFriendRequest(friendshipId, CancellationToken.None);

        // Assert
        result.Should().BeOfType<NotFoundResult>();
    }
    #endregion

    #region RemoveFriend Tests
    [Fact]
    public async Task RemoveFriend_ValidRemove_ReturnsNoContent()
    {
        // Arrange
        var friendId = Guid.NewGuid();
        _mockUnitOfWork.Setup(u => u.Friendships.RemoveFriendAsync(It.IsAny<Guid>(), friendId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        _mockUnitOfWork.Setup(u => u.CompleteAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.RemoveFriend(friendId, CancellationToken.None);

        // Assert
        result.Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public async Task RemoveFriend_FriendshipNotFound_ReturnsNotFound()
    {
        // Arrange
        var friendId = Guid.NewGuid();
        _mockUnitOfWork.Setup(u => u.Friendships.RemoveFriendAsync(It.IsAny<Guid>(), friendId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await _controller.RemoveFriend(friendId, CancellationToken.None);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>()
            .Which.Value.Should().Be("Friendship not found");
    }

    [Fact]
    public async Task RemoveFriend_FailedToRemove_ReturnsBadRequest()
    {
        // Arrange
        var friendId = Guid.NewGuid();
        _mockUnitOfWork.Setup(u => u.Friendships.RemoveFriendAsync(It.IsAny<Guid>(), friendId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        _mockUnitOfWork.Setup(u => u.CompleteAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await _controller.RemoveFriend(friendId, CancellationToken.None);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>()
            .Which.Value.Should().Be("Failed to remove friend");
    }
    #endregion

    #region GetFriends Tests
    [Fact]
    public async Task GetFriends_ValidRequest_ReturnsOk()
    {
        // Arrange
        var elementParams = new ElementParams { PageNumber = 1, PageSize = 10 };
        var userId = Guid.NewGuid();
        var friendships = new List<Friendship>
        {
            new Friendship { Id = Guid.NewGuid(), RequesterId = userId, ReceiverId = Guid.NewGuid(), Status = FriendshipStatus.Accepted },
            new Friendship { Id = Guid.NewGuid(), RequesterId = Guid.NewGuid(), ReceiverId = userId, Status = FriendshipStatus.Accepted }
        };

        SetupFakeUser(_controller, userId);
        _mockUnitOfWork.Setup(u => u.Friendships.GetUserFriendsAsync(userId, elementParams, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PagedList<Friendship>(friendships, 2, elementParams.PageNumber, elementParams.PageSize));

        // Act
        var result = await _controller.GetFriends(elementParams, CancellationToken.None);

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetFriends_NoFriends_ReturnsOkWithEmptyList()
    {
        // Arrange
        var elementParams = new ElementParams { PageNumber = 1, PageSize = 10 };
        var userId = Guid.NewGuid();
        var friendships = new List<Friendship>();

        SetupFakeUser(_controller, userId);
        _mockUnitOfWork.Setup(u => u.Friendships.GetUserFriendsAsync(userId, elementParams, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PagedList<Friendship>(friendships, 0, elementParams.PageNumber, elementParams.PageSize));

        // Act
        var result = await _controller.GetFriends(elementParams, CancellationToken.None);

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();
        var okResult = result.Result as OkObjectResult;
        okResult.Value.Should().BeEquivalentTo(new List<FriendDto>());
    }
    #endregion

    #region GetPendingFriendRequests Tests
    [Fact]
    public async Task GetPendingFriendRequests_ValidRequest_ReturnsOk()
    {
        // Arrange
        var elementParams = new ElementParams { PageNumber = 1, PageSize = 10 };
        var userId = Guid.NewGuid();
        var friendships = new List<Friendship>
        {
            new Friendship { Id = Guid.NewGuid(), RequesterId = Guid.NewGuid(), ReceiverId = userId, Status = FriendshipStatus.Pending },
            new Friendship { Id = Guid.NewGuid(), RequesterId = Guid.NewGuid(), ReceiverId = userId, Status = FriendshipStatus.Pending }
        };
        var friendRequestDtos = friendships.ToFriendRequestDto();

        SetupFakeUser(_controller, userId);
        _mockUnitOfWork.Setup(u => u.Friendships.GetPendingRequestsAsync(userId, elementParams, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PagedList<Friendship>(friendships, 2, elementParams.PageNumber, elementParams.PageSize));

        // Act
        var result = await _controller.GetPendingRequests(elementParams, CancellationToken.None);

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();
        var okResult = result.Result as OkObjectResult;
        okResult.Value.Should().BeEquivalentTo(friendRequestDtos);
    }

    [Fact]
    public async Task GetPendingFriendRequests_NoPendingRequests_ReturnsOkWithEmptyList()
    {
        // Arrange
        var elementParams = new ElementParams { PageNumber = 1, PageSize = 10 };
        var userId = Guid.NewGuid();
        var friendships = new List<Friendship>();

        SetupFakeUser(_controller, userId);
        _mockUnitOfWork.Setup(u => u.Friendships.GetPendingRequestsAsync(userId, elementParams, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PagedList<Friendship>(friendships, 0, elementParams.PageNumber, elementParams.PageSize));

        // Act
        var result = await _controller.GetPendingRequests(elementParams, CancellationToken.None);

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();
        var okResult = result.Result as OkObjectResult;
        okResult.Value.Should().BeEquivalentTo(new List<FriendRequestDto>());
    }
    #endregion
}