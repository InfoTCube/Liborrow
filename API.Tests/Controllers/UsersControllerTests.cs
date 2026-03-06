using System.Security.Claims;
using API.Controllers;
using API.DTOs.Users;
using API.Helpers;
using API.Interfaces;
using API.Tests.TestHelpers;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace API.Tests.Controllers;

public class UsersControllerTests : TestBase
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly UsersController _controller;

    public UsersControllerTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _controller = new UsersController(_unitOfWorkMock.Object);
        SetupFakeUser(_controller);
    }

    #region SearchUsers Tests

    [Fact]
    public async Task SearchUsers_WithValidQuery_ReturnsOkResult()
    {
        // Arrange
        var query = "john";
        var elementParams = new ElementParams { PageNumber = 1, PageSize = 10 };

        var users = new List<UserSearchDto>
        {
            new UserSearchDto { UserName = "john_doe", BooksCount = 5, CreatedAt = DateTime.UtcNow.AddDays(-10), Email = "john.doe@example.com", friendshipStatus = Enums.FriendshipStatus.Declined, UserId = Guid.NewGuid() },
            new UserSearchDto { UserName = "johnny_smith", BooksCount = 3, CreatedAt = DateTime.UtcNow.AddDays(-20), Email = "johnny.smith@example.com", friendshipStatus = Enums.FriendshipStatus.Accepted, UserId = Guid.NewGuid() }
        };

        _unitOfWorkMock.Setup(u => u.Users.SearchUsersAsync(TestUserId, query, elementParams, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PagedList<UserSearchDto>(users, users.Count, elementParams.PageNumber, elementParams.PageSize));

        // Act
        var result = await _controller.SearchUsers(query, elementParams, CancellationToken.None);

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();
        var okResult = result.Result as OkObjectResult;
        okResult.Value.Should().BeEquivalentTo(users);
    }

    [Fact]
    public async Task SearchUsers_WithEmptyQuery_ReturnsOkResultWithEmptyList()
    {
        // Arrange
        var query = "";
        var elementParams = new ElementParams { PageNumber = 1, PageSize = 10 };
        var users = new List<UserSearchDto>();

        _unitOfWorkMock.Setup(u => u.Users.SearchUsersAsync(TestUserId, query, elementParams, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PagedList<UserSearchDto>(users, users.Count, elementParams.PageNumber, elementParams.PageSize));

        // Act
        var result = await _controller.SearchUsers(query, elementParams, CancellationToken.None);

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();
        var okResult = result.Result as OkObjectResult;
        okResult.Value.Should().BeEquivalentTo(users);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task SearchUsers_WithNullOrWhitespaceQuery_HandlesGracefully(string? query)
    {
        // Arrange
        var elementParams = new ElementParams { PageNumber = 1, PageSize = 10 };
        var users = new List<UserSearchDto>();

        _unitOfWorkMock.Setup(u => u.Users.SearchUsersAsync(TestUserId, query!, elementParams, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PagedList<UserSearchDto>(users, users.Count, elementParams.PageNumber, elementParams.PageSize));

        // Act
        var result = await _controller.SearchUsers(query!, elementParams, CancellationToken.None);

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();
    }

    #endregion
}