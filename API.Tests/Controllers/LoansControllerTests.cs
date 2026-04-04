using API.Controllers;
using API.DTOs.Loans;
using API.Entities;
using API.Enums;
using API.Helpers;
using API.Interfaces;
using API.Tests.TestHelpers;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace API.Tests.Controllers;

public class LoansControllerTests : TestBase
{
    private readonly LoansController _controller;
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;

    public LoansControllerTests()
    {
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _controller = new LoansController(_mockUnitOfWork.Object);
        SetupFakeUser(_controller);
    }

    #region RequestLoan Tests
    [Fact]
    public async Task RequestLoan_BookExistsAndAvailable_ReturnsOk()
    {
        // Arrange
        var request = new BorrowRequestDto
        {
            ISBN = "1234567890",
            OwnerId = Guid.NewGuid(),
            Message = "I'd like to borrow this book."
        };

        var newLoan = new Loan
        {
            ISBN = request.ISBN,
            OwnerId = request.OwnerId,
            BorrowerId = TestUserId,
            RequestMessage = request.Message,
            RequestedAt = DateTime.UtcNow,
            Status = LoanStatus.Pending,
            UserBookId = Guid.NewGuid()
        };

        _mockUnitOfWork.Setup(u => u.Books.GetUserBookByIdAndUserIdAsync(request.ISBN, request.OwnerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new UserBook { Id = newLoan.UserBookId });

        _mockUnitOfWork.Setup(l => l.Loans.IsBookAvailableForLoanAsync(request.OwnerId, request.ISBN, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        _mockUnitOfWork.Setup(l => l.Loans.HasPendingLoanAsync(request.OwnerId, request.ISBN, It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _mockUnitOfWork.Setup(u => u.Loans.AddLoanAsync(newLoan, It.IsAny<CancellationToken>()));

        _mockUnitOfWork.Setup(u => u.CompleteAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.RequestLoan(request, It.IsAny<CancellationToken>());

        // Assert
        result.Should().BeOfType<OkResult>();
    }

    [Fact]
    public async Task RequestLoan_BookNotAvailable_ReturnsBadRequest()
    {
        // Arrange
        var request = new BorrowRequestDto
        {
            ISBN = "1234567890",
            OwnerId = Guid.NewGuid(),
            Message = "I'd like to borrow this book."
        };

        _mockUnitOfWork.Setup(u => u.Books.GetUserBookByIdAndUserIdAsync(request.ISBN, request.OwnerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new UserBook { Id = Guid.NewGuid() });

        _mockUnitOfWork.Setup(l => l.Loans.IsBookAvailableForLoanAsync(request.OwnerId, request.ISBN, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await _controller.RequestLoan(request, It.IsAny<CancellationToken>());

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>()
            .Which.Value.Should().Be("This book is currently not available for loan.");
    }

    [Fact]
    public async Task RequestLoan_AlreadyHasPendingLoan_ReturnsBadRequest()
    {
        // Arrange
        var request = new BorrowRequestDto
        {
            ISBN = "1234567890",
            OwnerId = Guid.NewGuid(),
            Message = "I'd like to borrow this book."
        };

        _mockUnitOfWork.Setup(u => u.Books.GetUserBookByIdAndUserIdAsync(request.ISBN, request.OwnerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new UserBook { Id = Guid.NewGuid() });

        _mockUnitOfWork.Setup(l => l.Loans.IsBookAvailableForLoanAsync(request.OwnerId, request.ISBN, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        _mockUnitOfWork.Setup(l => l.Loans.HasPendingLoanAsync(request.OwnerId, request.ISBN, It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.RequestLoan(request, It.IsAny<CancellationToken>());

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>()
            .Which.Value.Should().Be("You already have a pending loan request for this book.");
    }

    [Fact]
    public async Task RequestLoan_FailedToRequest_ReturnsBadRequest()
    {
        // Arrange
        var request = new BorrowRequestDto
        {
            ISBN = "1234567890",
            OwnerId = Guid.NewGuid(),
            Message = "I'd like to borrow this book."
        };

        var newLoan = new Loan
        {
            ISBN = request.ISBN,
            OwnerId = request.OwnerId,
            BorrowerId = TestUserId,
            RequestMessage = request.Message,
            RequestedAt = DateTime.UtcNow,
            Status = LoanStatus.Pending,
            UserBookId = Guid.NewGuid()
        };

        _mockUnitOfWork.Setup(u => u.Books.GetUserBookByIdAndUserIdAsync(request.ISBN, request.OwnerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new UserBook { Id = newLoan.UserBookId });

        _mockUnitOfWork.Setup(l => l.Loans.IsBookAvailableForLoanAsync(request.OwnerId, request.ISBN, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        _mockUnitOfWork.Setup(l => l.Loans.HasPendingLoanAsync(request.OwnerId, request.ISBN, It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _mockUnitOfWork.Setup(u => u.Loans.AddLoanAsync(newLoan, It.IsAny<CancellationToken>()));

        _mockUnitOfWork.Setup(u => u.CompleteAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await _controller.RequestLoan(request, It.IsAny<CancellationToken>());

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>()
            .Which.Value.Should().Be("Failed to request loan");
    }
    #endregion

    #region RespondToLoan Tests
    [Fact]
    public async Task RespondToLoan_AcceptLoanWithValidData_ReturnsOk()
    {
        // Arrange
        var loanId = Guid.NewGuid();
        var respondToLoanDto = new ResponseLoanDto
        {
            Accept = true,
            DueDate = DateTime.UtcNow.AddDays(14),
            Notes = "Enjoy the book!"
        };

        var existingLoan = new Loan
        {
            Id = loanId,
            OwnerId = TestUserId,
            ISBN = "1234567890",
            BorrowerId = Guid.NewGuid(),
            Status = LoanStatus.Pending
        };

        _mockUnitOfWork.Setup(l => l.Loans.GetLoanByIdAsync(loanId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingLoan);

        _mockUnitOfWork.Setup(u => u.CompleteAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.RespondToLoan(loanId, respondToLoanDto, It.IsAny<CancellationToken>());

        // Assert
        result.Should().BeOfType<OkResult>();
        existingLoan.Status.Should().Be(LoanStatus.Approved);
        existingLoan.ApprovedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(10));
        existingLoan.DueDate.Should().Be(respondToLoanDto.DueDate);
        existingLoan.Notes.Should().Be(respondToLoanDto.Notes);
    }

    [Fact]
    public async Task RespondToLoan_DeclineLoan_ReturnsOk()
    {
        // Arrange
        var loanId = Guid.NewGuid();
        var respondToLoanDto = new ResponseLoanDto
        {
            Accept = false
        };

        var existingLoan = new Loan
        {
            Id = loanId,
            OwnerId = TestUserId,
            ISBN = "1234567890",
            BorrowerId = Guid.NewGuid(),
            Status = LoanStatus.Pending
        };

        _mockUnitOfWork.Setup(l => l.Loans.GetLoanByIdAsync(loanId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingLoan);

        _mockUnitOfWork.Setup(u => u.CompleteAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.RespondToLoan(loanId, respondToLoanDto, It.IsAny<CancellationToken>());

        // Assert
        result.Should().BeOfType<OkResult>();
        existingLoan.Status.Should().Be(LoanStatus.Declined);
    }

    [Fact]
    public async Task RespondToLoan_LoanNotFound_ReturnsNotFound()
    {
        // Arrange
        var loanId = Guid.NewGuid();
        var respondToLoanDto = new ResponseLoanDto
        {
            Accept = true,
            DueDate = DateTime.UtcNow.AddDays(14),
            Notes = "Enjoy the book!"
        };

        _mockUnitOfWork.Setup(l => l.Loans.GetLoanByIdAsync(loanId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Loan?)null);

        // Act
        var result = await _controller.RespondToLoan(loanId, respondToLoanDto, It.IsAny<CancellationToken>());

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>()
            .Which.Value.Should().Be("Loan request not found");
    }

    [Fact]
    public async Task RespondToLoan_UserIsNotTheOwner_ReturnsUnauthorized()
    {
        // Arrange
        var loanId = Guid.NewGuid();
        var respondToLoanDto = new ResponseLoanDto
        {
            Accept = true,
            DueDate = DateTime.UtcNow.AddDays(14),
            Notes = "Enjoy the book!"
        };

        var existingLoan = new Loan
        {
            Id = loanId,
            OwnerId = Guid.NewGuid(), // Different owner
            ISBN = "1234567890",
            BorrowerId = Guid.NewGuid(),
            Status = LoanStatus.Pending
        };

        _mockUnitOfWork.Setup(l => l.Loans.GetLoanByIdAsync(loanId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingLoan);

        // Act
        var result = await _controller.RespondToLoan(loanId, respondToLoanDto, It.IsAny<CancellationToken>());

        // Assert
        result.Should().BeOfType<UnauthorizedObjectResult>()
            .Which.Value.Should().Be("You are not authorized to respond to this loan request");
    }

    [Fact]
    public async Task RespondToLoan_FailedToRespond_ReturnsBadRequest()
    {
        // Arrange
        var loanId = Guid.NewGuid();
        var respondToLoanDto = new ResponseLoanDto
        {
            Accept = true,
            DueDate = DateTime.UtcNow.AddDays(14),
            Notes = "Enjoy the book!"
        };

        var existingLoan = new Loan
        {
            Id = loanId,
            OwnerId = TestUserId,
            ISBN = "1234567890",
            BorrowerId = Guid.NewGuid(),
            Status = LoanStatus.Pending
        };

        _mockUnitOfWork.Setup(l => l.Loans.GetLoanByIdAsync(loanId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingLoan);

        _mockUnitOfWork.Setup(u => u.CompleteAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await _controller.RespondToLoan(loanId, respondToLoanDto, It.IsAny<CancellationToken>());

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>()
            .Which.Value.Should().Be("Failed to respond to loan request");
    }
    #endregion

    #region LoanBook Tests
    [Fact]
    public async Task LoanBook_ValidRequest_ReturnsOk()
    {
        // Arrange
        var loanId = Guid.NewGuid();
        var existingLoan = new Loan
        {
            Id = loanId,
            OwnerId = TestUserId,
            ISBN = "1234567890",
            BorrowerId = Guid.NewGuid(),
            Status = LoanStatus.Approved,
            DueDate = DateTime.UtcNow.AddDays(14)
        };

        _mockUnitOfWork.Setup(l => l.Loans.GetLoanByIdAsync(loanId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingLoan);

        _mockUnitOfWork.Setup(u => u.CompleteAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        _mockUnitOfWork.Setup(u => u.Books.GetUserBookByIdAndUserIdAsync(existingLoan.ISBN, existingLoan.OwnerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new UserBook { Id = Guid.NewGuid(), IsAvailable = true });

        // Act
        var result = await _controller.LoanBook(loanId, It.IsAny<CancellationToken>());

        // Assert
        result.Should().BeOfType<OkResult>();
        existingLoan.Status.Should().Be(LoanStatus.Active);
        existingLoan.LoanDate.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(10));
    }

    [Fact]
    public async Task LoanBook_LoanNotFound_ReturnsNotFound()
    {
        // Arrange
        var loanId = Guid.NewGuid();

        _mockUnitOfWork.Setup(l => l.Loans.GetLoanByIdAsync(loanId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Loan?)null);

        // Act
        var result = await _controller.LoanBook(loanId, It.IsAny<CancellationToken>());

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>()
            .Which.Value.Should().Be("Loan request not found");
    }

    [Fact]
    public async Task LoanBook_UserIsNotTheOwner_ReturnsUnauthorized()
    {
        // Arrange
        var loanId = Guid.NewGuid();
        var existingLoan = new Loan
        {
            Id = loanId,
            OwnerId = Guid.NewGuid(), // Different owner
            ISBN = "1234567890",
            BorrowerId = Guid.NewGuid(),
            Status = LoanStatus.Approved,
            DueDate = DateTime.UtcNow.AddDays(14)
        };

        _mockUnitOfWork.Setup(l => l.Loans.GetLoanByIdAsync(loanId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingLoan);

        // Act
        var result = await _controller.LoanBook(loanId, It.IsAny<CancellationToken>());

        // Assert
        result.Should().BeOfType<UnauthorizedObjectResult>()
            .Which.Value.Should().Be("You are not authorized to respond to this loan request");
    }
    #endregion

    #region ReturnLoan Tests
    [Fact]
    public async Task ReturnLoan_ValidRequest_ReturnsOk()
    {
        // Arrange
        var loanId = Guid.NewGuid();
        var existingLoan = new Loan
        {
            Id = loanId,
            OwnerId = Guid.NewGuid(),
            ISBN = "1234567890",
            BorrowerId = TestUserId,
            Status = LoanStatus.Active,
            DueDate = DateTime.UtcNow.AddDays(14)
        };

        _mockUnitOfWork.Setup(l => l.Loans.GetLoanByIdAsync(loanId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingLoan);

        _mockUnitOfWork.Setup(u => u.CompleteAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        _mockUnitOfWork.Setup(u => u.Books.GetUserBookByIdAndUserIdAsync(existingLoan.ISBN, existingLoan.OwnerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new UserBook { Id = Guid.NewGuid(), IsAvailable = false });

        // Act
        var result = await _controller.ReturnLoan(loanId, It.IsAny<CancellationToken>());

        // Assert
        result.Should().BeOfType<OkResult>();
        existingLoan.Status.Should().Be(LoanStatus.Returned);
    }

    [Fact]
    public async Task ReturnLoan_LoanNotFound_ReturnsNotFound()
    {
        // Arrange
        var loanId = Guid.NewGuid();

        _mockUnitOfWork.Setup(l => l.Loans.GetLoanByIdAsync(loanId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Loan?)null);

        // Act
        var result = await _controller.ReturnLoan(loanId, It.IsAny<CancellationToken>());

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>()
            .Which.Value.Should().Be("Loan not found");
    }

    [Fact]
    public async Task ReturnLoan_UserIsNotTheBorrower_ReturnsUnauthorized()
    {
        // Arrange
        var loanId = Guid.NewGuid();
        var existingLoan = new Loan
        {
            Id = loanId,
            OwnerId = Guid.NewGuid(),
            ISBN = "1234567890",
            BorrowerId = Guid.NewGuid(), // Different borrower
            Status = LoanStatus.Active,
            DueDate = DateTime.UtcNow.AddDays(14)
        };

        _mockUnitOfWork.Setup(l => l.Loans.GetLoanByIdAsync(loanId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingLoan);

        // Act
        var result = await _controller.ReturnLoan(loanId, It.IsAny<CancellationToken>());

        // Assert
        result.Should().BeOfType<UnauthorizedObjectResult>()
            .Which.Value.Should().Be("You are not authorized to return this loan");
    }
    #endregion

    #region GetLoans Tests
    [Fact]
    public async Task GetLoans_ReturnsOkWithLoans()
    {
        // Arrange
        var elementParams = new ElementParams { PageNumber = 1, PageSize = 10 };
        var userId = Guid.NewGuid();
        var loans = new List<Loan>
        {
            new Loan { Id = Guid.NewGuid(), ISBN = "1234567890", OwnerId = userId, BorrowerId = Guid.NewGuid(), Status = LoanStatus.Active },
            new Loan { Id = Guid.NewGuid(), ISBN = "0987654321", OwnerId = userId, BorrowerId = Guid.NewGuid(), Status = LoanStatus.Approved }
        };

        SetupFakeUser(_controller, userId);
        _mockUnitOfWork.Setup(l => l.Loans.GetActiveLoansAsync(userId, elementParams, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PagedList<Loan>(loans, 2, elementParams.PageNumber, elementParams.PageSize));

        // Act
        var result = await _controller.GetLoans(elementParams, It.IsAny<CancellationToken>());

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetLoans_ReturnsOkWithEmptyList()
    {
        // Arrange
        var elementParams = new ElementParams { PageNumber = 1, PageSize = 10 };
        var userId = Guid.NewGuid();

        SetupFakeUser(_controller, userId);
        _mockUnitOfWork.Setup(l => l.Loans.GetActiveLoansAsync(userId, elementParams, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PagedList<Loan>(new List<Loan>(), 0, elementParams.PageNumber, elementParams.PageSize));

        // Act
        var result = await _controller.GetLoans(elementParams, It.IsAny<CancellationToken>());

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();
    }
    #endregion

    #region GetBorrowedLoans Tests
    [Fact]
    public async Task GetBorrowedLoans_ReturnsOkWithLoans()
    {
        // Arrange
        var elementParams = new ElementParams { PageNumber = 1, PageSize = 10 };
        var userId = Guid.NewGuid();
        var loans = new List<Loan>
        {
            new Loan { Id = Guid.NewGuid(), ISBN = "1234567890", OwnerId = Guid.NewGuid(), BorrowerId = userId, Status = LoanStatus.Active },
            new Loan { Id = Guid.NewGuid(), ISBN = "0987654321", OwnerId = Guid.NewGuid(), BorrowerId = userId, Status = LoanStatus.Approved }
        };

        SetupFakeUser(_controller, userId);
        _mockUnitOfWork.Setup(l => l.Loans.GetLoansForBorrowerAsync(userId, elementParams, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PagedList<Loan>(loans, 2, elementParams.PageNumber, elementParams.PageSize));

        // Act
        var result = await _controller.GetBorrowedLoans(elementParams, It.IsAny<CancellationToken>());

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();
    }
    #endregion

    #region GetLentLoans Tests
    [Fact]
    public async Task GetLentLoans_ReturnsOkWithLoans()
    {
        // Arrange
        var elementParams = new ElementParams { PageNumber = 1, PageSize = 10 };
        var userId = Guid.NewGuid();
        var loans = new List<Loan>
        {
            new Loan { Id = Guid.NewGuid(), ISBN = "1234567890", OwnerId = userId, BorrowerId = Guid.NewGuid(), Status = LoanStatus.Active },
            new Loan { Id = Guid.NewGuid(), ISBN = "0987654321", OwnerId = userId, BorrowerId = Guid.NewGuid(), Status = LoanStatus.Approved }
        };

        SetupFakeUser(_controller, userId);
        _mockUnitOfWork.Setup(l => l.Loans.GetLoansForOwnerAsync(userId, elementParams, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PagedList<Loan>(loans, 2, elementParams.PageNumber, elementParams.PageSize));

        // Act
        var result = await _controller.GetLentLoans(elementParams, It.IsAny<CancellationToken>());

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();
    }
    #endregion

    #region GetLoanHistory Tests
    [Fact]
    public async Task GetLoanHistory_ReturnsOkWithLoans()
    {
        // Arrange
        var elementParams = new ElementParams { PageNumber = 1, PageSize = 10 };
        var userId = Guid.NewGuid();
        var loans = new List<Loan>
        {
            new Loan { Id = Guid.NewGuid(), ISBN = "1234567890", OwnerId = userId, BorrowerId = Guid.NewGuid(), Status = LoanStatus.Active },
            new Loan { Id = Guid.NewGuid(), ISBN = "0987654321", OwnerId = userId, BorrowerId = Guid.NewGuid(), Status = LoanStatus.Approved }
        };

        SetupFakeUser(_controller, userId);
        _mockUnitOfWork.Setup(l => l.Loans.GetLoanHistoryAsync(userId, elementParams, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PagedList<Loan>(loans, 2, elementParams.PageNumber, elementParams.PageSize));

        // Act
        var result = await _controller.GetLoanHistory(elementParams, It.IsAny<CancellationToken>());

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetLoanHistory_ReturnsOkWithEmptyList()
    {
        // Arrange
        var elementParams = new ElementParams { PageNumber = 1, PageSize = 10 };
        var userId = Guid.NewGuid();

        SetupFakeUser(_controller, userId);
        _mockUnitOfWork.Setup(l => l.Loans.GetLoanHistoryAsync(userId, elementParams, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PagedList<Loan>(new List<Loan>(), 0, elementParams.PageNumber, elementParams.PageSize));

        // Act
        var result = await _controller.GetLoanHistory(elementParams, It.IsAny<CancellationToken>());

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();
    }
    #endregion

    #region GetPendingRequests Tests
    [Fact]
    public async Task GetPendingRequests_ReturnsOkWithLoans()
    {
        // Arrange
        var elementParams = new ElementParams { PageNumber = 1, PageSize = 10 };
        var userId = Guid.NewGuid();
        var loans = new List<Loan>
        {
            new Loan { Id = Guid.NewGuid(), ISBN = "1234567890", OwnerId = userId, BorrowerId = Guid.NewGuid(), Status = LoanStatus.Pending }
        };

        SetupFakeUser(_controller, userId);
        _mockUnitOfWork.Setup(l => l.Loans.GetPendingRequestsForOwnerAsync(userId, elementParams, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PagedList<Loan>(loans, 1, elementParams.PageNumber, elementParams.PageSize));

        // Act
        var result = await _controller.GetPendingRequests(elementParams, It.IsAny<CancellationToken>());

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetPendingRequests_ReturnsOkWithEmptyList()
    {
        // Arrange
        var elementParams = new ElementParams { PageNumber = 1, PageSize = 10 };
        var userId = Guid.NewGuid();

        SetupFakeUser(_controller, userId);
        _mockUnitOfWork.Setup(l => l.Loans.GetPendingRequestsForOwnerAsync(userId, elementParams, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PagedList<Loan>(new List<Loan>(), 0, elementParams.PageNumber, elementParams.PageSize));

        // Act
        var result = await _controller.GetPendingRequests(elementParams, It.IsAny<CancellationToken>());

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();
    }
    #endregion

    #region GetPendingRequestsForBorrower Tests
    [Fact]
    public async Task GetPendingRequestsForBorrower_ReturnsOkWithLoans()
    {
        // Arrange
        var elementParams = new ElementParams { PageNumber = 1, PageSize = 10 };
        var userId = Guid.NewGuid();
        var loans = new List<Loan>
        {
            new Loan { Id = Guid.NewGuid(), ISBN = "1234567890", OwnerId = Guid.NewGuid(), BorrowerId = userId, Status = LoanStatus.Pending }
        };

        SetupFakeUser(_controller, userId);
        _mockUnitOfWork.Setup(l => l.Loans.GetPendingRequestsFromBorrowerAsync(userId, elementParams, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PagedList<Loan>(loans, 1, elementParams.PageNumber, elementParams.PageSize));

        // Act
        var result = await _controller.GetPendingRequestsFromBorrower(elementParams, It.IsAny<CancellationToken>());

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    
    public async Task GetPendingRequestsFromBorrower_ReturnsOkWithEmptyList()
    {
        // Arrange
        var elementParams = new ElementParams { PageNumber = 1, PageSize = 10 };
        var userId = Guid.NewGuid();

        SetupFakeUser(_controller, userId);
        _mockUnitOfWork.Setup(l => l.Loans.GetPendingRequestsFromBorrowerAsync(userId, elementParams, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PagedList<Loan>(new List<Loan>(), 0, elementParams.PageNumber, elementParams.PageSize));

        // Act
        var result = await _controller.GetPendingRequestsFromBorrower(elementParams, It.IsAny<CancellationToken>());

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();
    }
    #endregion
}