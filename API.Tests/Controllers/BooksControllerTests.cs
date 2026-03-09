using API.Controllers;
using API.DTOs.Books;
using API.Entities;
using API.Helpers;
using API.Interfaces;
using API.Tests.TestHelpers;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;

namespace API.Tests.Controllers;

public class BooksControllerTests : TestBase
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IBibliotekaNarodowaBooksService> _bibliotekaNarodowaBooksServiceMock;
    private readonly Mock<ILogger<BooksController>> _loggerMock;
    private readonly BooksController _controller;

    public BooksControllerTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _bibliotekaNarodowaBooksServiceMock = new Mock<IBibliotekaNarodowaBooksService>();
        _loggerMock = new Mock<ILogger<BooksController>>();
        _controller = new BooksController(_unitOfWorkMock.Object, _bibliotekaNarodowaBooksServiceMock.Object, _loggerMock.Object);
        SetupFakeUser(_controller);
    }

    #region AddBookToCollection Tests
    [Fact]
    public async Task AddBookToCollection_BookFromExternalApi_ReturnsOkResult()
    {
        // Arrange
        var addBookDto = new AddBookDto { ISBN = "1234567890", Notes = "Great book!" };
        var bookFromExternalApi = new BookDto { ISBN = "1234567890", Title = "Test Book", Author = "Test Author" };

        _unitOfWorkMock.Setup(u => u.Books.GetBookByIsbnAsync(addBookDto.ISBN, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Book?)null);

        _bibliotekaNarodowaBooksServiceMock.Setup(s => s.GetBookByIsbnAsync(addBookDto.ISBN, It.IsAny<CancellationToken>()))
            .ReturnsAsync(bookFromExternalApi);

        _unitOfWorkMock.Setup(u => u.Books.AddUserBookAsync(It.IsAny<Guid>(), addBookDto.ISBN, addBookDto.Notes, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new UserBook { UserId = TestUserId, ISBN = addBookDto.ISBN, Notes = addBookDto.Notes, AddedAt = DateTime.UtcNow });

        _unitOfWorkMock.Setup(u => u.CompleteAsync(It.IsAny<CancellationToken>())).ReturnsAsync(true);

        // Act
        var result = await _controller.AddBookToCollection(addBookDto, CancellationToken.None);

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();
        var okResult = result.Result as OkObjectResult;
        var returnedBook = okResult.Value as BookWithOwnerDto;
        returnedBook.Should().NotBeNull();
        returnedBook.ISBN.Should().Be(bookFromExternalApi.ISBN);
        returnedBook.Title.Should().Be(bookFromExternalApi.Title);
        returnedBook.Author.Should().Be(bookFromExternalApi.Author);
    }

    [Fact]
    public async Task AddBookToCollection_BookExistsInDatabase_ReturnsOkResult()
    {
        // Arrange
        var addBookDto = new AddBookDto { ISBN = "1234567890", Notes = "Great book!" };
        var existingBook = new Book { ISBN = "1234567890", Title = "Existing Book", Author = "Existing Author" };

        _unitOfWorkMock.Setup(u => u.Books.GetBookByIsbnAsync(addBookDto.ISBN, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingBook);

        _unitOfWorkMock.Setup(u => u.Books.UserOwnsBookAsync(TestUserId, addBookDto.ISBN, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _unitOfWorkMock.Setup(u => u.Books.AddUserBookAsync(TestUserId, addBookDto.ISBN, addBookDto.Notes, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new UserBook { UserId = TestUserId, ISBN = addBookDto.ISBN, Notes = addBookDto.Notes, AddedAt = DateTime.UtcNow });

        _unitOfWorkMock.Setup(u => u.CompleteAsync(It.IsAny<CancellationToken>())).ReturnsAsync(true);

        // Act
        var result = await _controller.AddBookToCollection(addBookDto, CancellationToken.None);

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();
        var okResult = result.Result as OkObjectResult;
        var returnedBook = okResult.Value as BookWithOwnerDto;
        returnedBook.Should().NotBeNull();
        returnedBook.ISBN.Should().Be(existingBook.ISBN);
        returnedBook.Title.Should().Be(existingBook.Title);
        returnedBook.Author.Should().Be(existingBook.Author);
    }

    [Fact]
    public async Task AddBookToCollection_UserAlreadyOwnsBook_ReturnsBadRequest()
    {
        // Arrange
        var addBookDto = new AddBookDto { ISBN = "1234567890", Notes = "Great book!" };
        var existingBook = new Book { ISBN = "1234567890", Title = "Existing Book", Author = "Existing Author" };

        _unitOfWorkMock.Setup(u => u.Books.GetBookByIsbnAsync(addBookDto.ISBN, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingBook);

        _unitOfWorkMock.Setup(u => u.Books.UserOwnsBookAsync(TestUserId, addBookDto.ISBN, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.AddBookToCollection(addBookDto, CancellationToken.None);

        // Assert
        result.Result.Should().BeOfType<BadRequestObjectResult>();
        var badRequestResult = result.Result as BadRequestObjectResult;
        badRequestResult.Value.Should().Be("You already own this book.");
    }

    [Fact]
    public async Task AddBookToCollection_BookNotFoundInExternalApi_ReturnsNotFound()
    {
        // Arrange
        var addBookDto = new AddBookDto { ISBN = "1234567890", Notes = "Great book!" };

        _unitOfWorkMock.Setup(u => u.Books.GetBookByIsbnAsync(addBookDto.ISBN, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Book?)null);

        _bibliotekaNarodowaBooksServiceMock.Setup(s => s.GetBookByIsbnAsync(addBookDto.ISBN, It.IsAny<CancellationToken>()))
            .ReturnsAsync((BookDto?)null);

        // Act
        var result = await _controller.AddBookToCollection(addBookDto, CancellationToken.None);

        // Assert
        result.Result.Should().BeOfType<NotFoundObjectResult>();
        var notFoundResult = result.Result as NotFoundObjectResult;
        notFoundResult.Value.Should().Be("Book not found in external API, please provide the book details manually.");
    }

    [Fact]
    public async Task AddBookToCollection_ExternalApiThrowsException_ReturnsInternalServerError()
    {
        // Arrange
        var addBookDto = new AddBookDto { ISBN = "1234567890", Notes = "Great book!" };

        _unitOfWorkMock.Setup(u => u.Books.GetBookByIsbnAsync(addBookDto.ISBN, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Book?)null);

        _bibliotekaNarodowaBooksServiceMock.Setup(s => s.GetBookByIsbnAsync(addBookDto.ISBN, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("External API error"));

        // Act
        var result = await _controller.AddBookToCollection(addBookDto, CancellationToken.None);

        // Assert
        result.Result.Should().BeOfType<NotFoundObjectResult>();
        var notFoundResult = result.Result as NotFoundObjectResult;
        notFoundResult.Value.Should().Be("Error fetching book details, provide the book manually!");
    }

    [Fact]
    public async Task AddBookToCollection_FailedToAddBook_ReturnsBadRequest()
    {
        // Arrange
        var addBookDto = new AddBookDto { ISBN = "1234567890", Notes = "Great book!" };
        var bookFromExternalApi = new BookDto { ISBN = "1234567890", Title = "Test Book", Author = "Test Author" };

        _unitOfWorkMock.Setup(u => u.Books.GetBookByIsbnAsync(addBookDto.ISBN, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Book?)null);

        _bibliotekaNarodowaBooksServiceMock.Setup(s => s.GetBookByIsbnAsync(addBookDto.ISBN, It.IsAny<CancellationToken>()))
            .ReturnsAsync(bookFromExternalApi);

        _unitOfWorkMock.Setup(u => u.Books.AddUserBookAsync(It.IsAny<Guid>(), addBookDto.ISBN, addBookDto.Notes, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new UserBook { UserId = TestUserId, ISBN = addBookDto.ISBN, Notes = addBookDto.Notes, AddedAt = DateTime.UtcNow });

        _unitOfWorkMock.Setup(u => u.CompleteAsync(It.IsAny<CancellationToken>())).ReturnsAsync(false);

        // Act
        var result = await _controller.AddBookToCollection(addBookDto, CancellationToken.None);

        // Assert
        result.Result.Should().BeOfType<BadRequestObjectResult>();
        var badRequestResult = result.Result as BadRequestObjectResult;
        badRequestResult.Value.Should().Be("Failed to add book to collection.");
    }
    #endregion

    #region AddBookManually Tests
    [Fact]
    public async Task AddBookManually_BookNotInDatabase_AddsBookAndReturnsOk()
    {
        // Arrange
        var addBookManualDto = new AddBookManualDto { ISBN = "1234567890", Title = "Manual Book", Author = "Manual Author", Notes = "Great book!" };

        _unitOfWorkMock.Setup(u => u.Books.GetBookByIsbnAsync(addBookManualDto.ISBN, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Book?)null);

        _unitOfWorkMock.Setup(u => u.Books.AddUserBookAsync(TestUserId, addBookManualDto.ISBN, addBookManualDto.Notes, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new UserBook { UserId = TestUserId, ISBN = addBookManualDto.ISBN, Notes = addBookManualDto.Notes, AddedAt = DateTime.UtcNow });

        _unitOfWorkMock.Setup(u => u.CompleteAsync(It.IsAny<CancellationToken>())).ReturnsAsync(true);

        // Act
        var result = await _controller.AddBookManually(addBookManualDto, CancellationToken.None);

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();
        var okResult = result.Result as OkObjectResult;
        var returnedBook = okResult.Value as BookWithOwnerDto;
        returnedBook.Should().NotBeNull();
        returnedBook.ISBN.Should().Be(addBookManualDto.ISBN);
        returnedBook.Title.Should().Be(addBookManualDto.Title);
        returnedBook.Author.Should().Be(addBookManualDto.Author);
    }

    [Fact]
    public async Task AddBookManually_BookExistsInDatabase_AddsUserBookAndReturnsOk()
    {
        // Arrange
        var addBookManualDto = new AddBookManualDto { ISBN = "1234567890", Title = "Manual Book", Author = "Manual Author", Notes = "Great book!" };
        var existingBook = new Book { ISBN = "1234567890", Title = "Existing Book", Author = "Existing Author" };

        _unitOfWorkMock.Setup(u => u.Books.GetBookByIsbnAsync(addBookManualDto.ISBN, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingBook);

        _unitOfWorkMock.Setup(u => u.Books.AddUserBookAsync(TestUserId, addBookManualDto.ISBN, addBookManualDto.Notes, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new UserBook { UserId = TestUserId, ISBN = addBookManualDto.ISBN, Notes = addBookManualDto.Notes, AddedAt = DateTime.UtcNow });

        _unitOfWorkMock.Setup(u => u.CompleteAsync(It.IsAny<CancellationToken>())).ReturnsAsync(true);

        // Act
        var result = await _controller.AddBookManually(addBookManualDto, CancellationToken.None);

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();
        var okResult = result.Result as OkObjectResult;
        var returnedBook = okResult.Value as BookWithOwnerDto;
        returnedBook.Should().NotBeNull();
        returnedBook.ISBN.Should().Be(existingBook.ISBN);
        returnedBook.Title.Should().Be(existingBook.Title);
        returnedBook.Author.Should().Be(existingBook.Author);
    }

    [Fact]
    public async Task AddBookManually_UserAlreadyOwnsBook_ReturnsBadRequest()
    {
        // Arrange
        var addBookManualDto = new AddBookManualDto { ISBN = "1234567890", Title = "Manual Book", Author = "Manual Author", Notes = "Great book!" };

        _unitOfWorkMock.Setup(u => u.Books.UserOwnsBookAsync(TestUserId, addBookManualDto.ISBN, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.AddBookManually(addBookManualDto, CancellationToken.None);

        // Assert
        result.Result.Should().BeOfType<BadRequestObjectResult>();
        var badRequestResult = result.Result as BadRequestObjectResult;
        badRequestResult.Value.Should().Be("You already own this book.");
    }

    [Fact]
    public async Task AddBookManually_FailedToAddBook_ReturnsBadRequest()
    {
        // Arrange
        var addBookManualDto = new AddBookManualDto { ISBN = "1234567890", Title = "Manual Book", Author = "Manual Author", Notes = "Great book!" };

        _unitOfWorkMock.Setup(u => u.Books.GetBookByIsbnAsync(addBookManualDto.ISBN, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Book?)null);

        _unitOfWorkMock.Setup(u => u.Books.AddUserBookAsync(TestUserId, addBookManualDto.ISBN, addBookManualDto.Notes, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new UserBook { UserId = TestUserId, ISBN = addBookManualDto.ISBN, Notes = addBookManualDto.Notes, AddedAt = DateTime.UtcNow });

        _unitOfWorkMock.Setup(u => u.CompleteAsync(It.IsAny<CancellationToken>())).ReturnsAsync(false);

        // Act
        var result = await _controller.AddBookManually(addBookManualDto, CancellationToken.None);

        // Assert
        result.Result.Should().BeOfType<BadRequestObjectResult>();
        var badRequestResult = result.Result as BadRequestObjectResult;
        badRequestResult.Value.Should().Be("Failed to add book to collection.");
    }
    #endregion

    #region GetMyBooks Tests
    [Fact]
    public async Task GetMyBooks_UserHasSomeBooks_ReturnsOkResultWithBooks()
    {
        // Arrange
        var elementParams = new ElementParams { PageNumber = 1, PageSize = 10 };
        var userBooks = new List<UserBook>
        {
            new UserBook { UserId = TestUserId, ISBN = "1234567890", Notes = "Great book!", AddedAt = DateTime.UtcNow, Book = new Book { ISBN = "1234567890", Title = "Test Book 1", Author = "Test Author 1" } },
            new UserBook { UserId = TestUserId, ISBN = "0987654321", Notes = "Another great book!", AddedAt = DateTime.UtcNow, Book = new Book { ISBN = "0987654321", Title = "Test Book 2", Author = "Test Author 2" } }
        };

        var returnedBooks = new List<BookDto>
        {
            new BookDto { ISBN = "1234567890", Title = "Test Book 1", Author = "Test Author 1", CoverImageUrl="", Description = "", PublishedYear = "0000", PageCount=0, Notes = "Great book!", AddedAt = userBooks[0].AddedAt },
            new BookDto { ISBN = "0987654321", Title = "Test Book 2", Author = "Test Author 2", CoverImageUrl="", Description = "", PublishedYear = "0000", PageCount=0, Notes = "Another great book!", AddedAt = userBooks[1].AddedAt }
        };

        _unitOfWorkMock.Setup(u => u.Books.GetUserBooksAsync(TestUserId, elementParams, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PagedList<UserBook>(userBooks, userBooks.Count, elementParams.PageNumber, elementParams.PageSize));

        // Act
        var result = await _controller.GetMyBooks(elementParams, CancellationToken.None);

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();
        var okResult = result.Result as OkObjectResult;
        okResult.Value.Should().BeEquivalentTo(returnedBooks);
    }

    [Fact]
    public async Task GetMyBooks_UserHasNoBooks_ReturnsOkResultWithEmptyList()
    {
        // Arrange
        var elementParams = new ElementParams { PageNumber = 1, PageSize = 10 };
        var userBooks = new List<UserBook>();

        _unitOfWorkMock.Setup(u => u.Books.GetUserBooksAsync(TestUserId, elementParams, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PagedList<UserBook>(userBooks, userBooks.Count, elementParams.PageNumber, elementParams.PageSize));

        // Act
        var result = await _controller.GetMyBooks(elementParams, CancellationToken.None);

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();
        var okResult = result.Result as OkObjectResult;
        okResult.Value.Should().BeEquivalentTo(new List<BookDto>());
    }
    #endregion

    #region GetFriendBooks Tests
    [Fact]
    public async Task GetFriendBooks_FriendHasBooks_ReturnsOkResultWithBooks()
    {
        // Arrange
        var friendId = Guid.NewGuid();
        var elementParams = new ElementParams { PageNumber = 1, PageSize = 10 };
        var friendBooks = new List<UserBook>
        {
            new UserBook { UserId = friendId, ISBN = "1234567890", Notes = "Great book!", AddedAt = DateTime.UtcNow, Book = new Book { ISBN = "1234567890", Title = "Test Book 1", Author = "Test Author 1" } },
            new UserBook { UserId = friendId, ISBN = "0987654321", Notes = "Another great book!", AddedAt = DateTime.UtcNow, Book = new Book { ISBN = "0987654321", Title = "Test Book 2", Author = "Test Author 2" } }
        };

        var returnedBooks = new List<BookDto>
        {
            new BookDto { ISBN = "1234567890", Title = "Test Book 1", Author = "Test Author 1", CoverImageUrl="", Description = "", PublishedYear = "0000", PageCount=0, Notes = "Great book!", AddedAt = friendBooks[0].AddedAt },
            new BookDto { ISBN = "0987654321", Title = "Test Book 2", Author = "Test Author 2", CoverImageUrl="", Description = "", PublishedYear = "0000", PageCount=0, Notes = "Another great book!", AddedAt = friendBooks[1].AddedAt }
        };

        _unitOfWorkMock.Setup(u => u.Friendships.AreFriendsAsync(TestUserId, friendId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        _unitOfWorkMock.Setup(u => u.Books.GetUserBooksAsync(friendId, elementParams, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PagedList<UserBook>(friendBooks, friendBooks.Count, elementParams.PageNumber, elementParams.PageSize));

        // Act
        var result = await _controller.GetFriendBooks(friendId, elementParams, CancellationToken.None);

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();
        var okResult = result.Result as OkObjectResult;
        okResult.Value.Should().BeEquivalentTo(returnedBooks);
    }

    [Fact]
    public async Task GetFriendBooks_NotFriends_ReturnsBadRequest()
    {
        // Arrange
        var friendId = Guid.NewGuid();
        var elementParams = new ElementParams { PageNumber = 1, PageSize = 10 };

        _unitOfWorkMock.Setup(u => u.Friendships.AreFriendsAsync(TestUserId, friendId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await _controller.GetFriendBooks(friendId, elementParams, CancellationToken.None);

        // Assert
        result.Result.Should().BeOfType<BadRequestObjectResult>();
        var badRequestResult = result.Result as BadRequestObjectResult;
        badRequestResult.Value.Should().Be("You can only view books of your friends.");
    }
    #endregion

    #region SearchBooks Tests
    [Fact]
    public async Task SearchBooks_FriendsHaveMatchingBooks_ReturnsOkResultWithBooks()
    {
        // Arrange
        var query = "test";
        var elementParams = new ElementParams { PageNumber = 1, PageSize = 10 };
        var friendId = Guid.NewGuid();

        var matchingBooks = new List<UserBook>
        {
            new UserBook { UserId = friendId, ISBN = "1234567890", Notes = "Great book!", AddedAt = DateTime.UtcNow, Book = new Book { ISBN = "1234567890", Title = "Test Book 1", Author = "Test Author 1" } },
            new UserBook { UserId = friendId, ISBN = "0987654321", Notes = "Another great book!", AddedAt = DateTime.UtcNow, Book = new Book { ISBN = "0987654321", Title = "Test Book 2", Author = "Test Author 2" } }
        };

        var returnedBooks = new List<BookDto>
        {
            new BookDto { ISBN = "1234567890", Title = "Test Book 1", Author = "Test Author 1", CoverImageUrl="", Description = "", PublishedYear = "0000", PageCount=0, Notes = "Great book!", AddedAt = matchingBooks[0].AddedAt },
            new BookDto { ISBN = "0987654321", Title = "Test Book 2", Author = "Test Author 2", CoverImageUrl="", Description = "", PublishedYear = "0000", PageCount=0, Notes = "Another great book!", AddedAt = matchingBooks[1].AddedAt }
        };

        _unitOfWorkMock.Setup(u => u.Books.SearchFriendsBooksAsync(TestUserId, query, elementParams, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PagedList<UserBook>(matchingBooks, matchingBooks.Count, elementParams.PageNumber, elementParams.PageSize));

        // Act
        var result = await _controller.SearchBooks(query, elementParams, CancellationToken.None);

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();
        var okResult = result.Result as OkObjectResult;
        okResult.Value.Should().BeEquivalentTo(returnedBooks);
    }

    [Fact]
    public async Task SearchBooks_NoMatchingBooks_ReturnsOkResultWithEmptyList()
    {
        // Arrange
        var query = "nonexistent";
        var elementParams = new ElementParams { PageNumber = 1, PageSize = 10 };
        var matchingBooks = new List<UserBook>();

        _unitOfWorkMock.Setup(u => u.Books.SearchFriendsBooksAsync(TestUserId, query, elementParams, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PagedList<UserBook>(matchingBooks, matchingBooks.Count, elementParams.PageNumber, elementParams.PageSize));

        // Act
        var result = await _controller.SearchBooks(query, elementParams, CancellationToken.None);

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();
        var okResult = result.Result as OkObjectResult;
        okResult.Value.Should().BeEquivalentTo(new List<BookDto>());
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task SearchBooks_NullOrWhitespaceQuery_ReturnsOkResultWithEmptyList(string? query)
    {
        // Arrange
        var elementParams = new ElementParams { PageNumber = 1, PageSize = 10 };
        var matchingBooks = new List<UserBook>();

        _unitOfWorkMock.Setup(u => u.Books.SearchFriendsBooksAsync(TestUserId, query!, elementParams, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PagedList<UserBook>(matchingBooks, matchingBooks.Count, elementParams.PageNumber, elementParams.PageSize));

        // Act
        var result = await _controller.SearchBooks(query, elementParams, CancellationToken.None);

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();
        var okResult = result.Result as OkObjectResult;
        okResult.Value.Should().BeEquivalentTo(new List<BookDto>());
    }
    #endregion
}