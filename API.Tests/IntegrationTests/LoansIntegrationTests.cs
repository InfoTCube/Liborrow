using System.Net;
using System.Net.Http.Json;
using API.DTOs.Books;
using API.DTOs.Loans;
using API.Enums;
using API.Tests.IntegrationTests.Fixtures;
using API.Tests.IntegrationTests.Helpers;
using FluentAssertions;

namespace API.Tests.IntegrationTests;

[Collection("Integration")]
public class LoansIntegrationTests : IClassFixture<IntegrationTestFixture>
{
    private readonly IntegrationTestFixture _factory;

    public LoansIntegrationTests(IntegrationTestFixture factory) => _factory = factory;

    [Fact]
    public async Task FullLoanLifecycle_Request_Approve_Activate_Return()
    {
        // Arrange: Two users - owner has a book
        var clientA = _factory.Factory.CreateClient();
        var owner = await IntegrationTestHelpers.RegisterUserAsync(
            clientA, "owner", "owner@example.com", "Password123!"
        );
        var ownerClient = _factory.Factory.CreateAuthenticatedClient(owner);

        var clientB = _factory.Factory.CreateClient();
        var borrower = await IntegrationTestHelpers.RegisterUserAsync(
            clientB, "borrower", "borrower@example.com", "Password123!"
        );
        var borrowerClient = _factory.Factory.CreateAuthenticatedClient(borrower);

        await ownerClient.PostAsJsonAsync("/api/books/manual", new AddBookManualDto
        {
            ISBN = "9780123456789",
            Title = "The Great Gatsby",
            Author = "F. Scott Fitzgerald",
            PublishedYear = "2024",
            PageCount = 180,
        });

        // Act: Borrower requests the loan
        var requestResponse = await borrowerClient.PostAsJsonAsync("/api/loans", new
        {
            ISBN = "9780123456789",
            OwnerId = owner.Id,
            Message = "Can I borrow this book?"
        });
        requestResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // Act: Owner sees the loan request and approves it
        var pendingResponse = await ownerClient.GetAsync("/api/loans/pending-requests");
        pendingResponse.EnsureSuccessStatusCode();
        var pendingLoans = await pendingResponse.Content.ReadFromJsonAsync<List<LoanDto>>();
        pendingLoans.Should().ContainSingle(l => l.ISBN == "9780123456789");

        var loanId = pendingLoans.First().Id;

        var approveResponse = await ownerClient.PostAsJsonAsync($"/api/loans/respond/{loanId}", new ResponseLoanDto
        {
            Accept = true,
            DueDate = DateTime.UtcNow.AddDays(14),
            Notes = "Enjoy the book!"
        });
        approveResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // Act: Borrower activates the loan (book picked up)
        var activateResponse = await borrowerClient.PostAsync($"/api/loans/loan/{loanId}", null);
        activateResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // Act: Borrower returns the book
        var returnResponse = await borrowerClient.PostAsync($"/api/loans/return/{loanId}", null);
        returnResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // Owner's loan history shows returned book
        var historyResponse = await ownerClient.GetAsync("/api/loans/history");
        historyResponse.EnsureSuccessStatusCode();
        var history = await historyResponse.Content.ReadFromJsonAsync<List<LoanDto>>();
        history.Should().ContainSingle(l => l.Id == loanId && l.Status == LoanStatus.Returned);

        // Assert: Book is available again
        var booksResponse = await ownerClient.GetAsync("/api/books");

        booksResponse.EnsureSuccessStatusCode();
        var books = await booksResponse.Content.ReadFromJsonAsync<List<BookWithOwnerDto>>();
        books.Should().Contain(b => b.ISBN == "9780123456789" && b.IsAvailable);
    }

    [Fact]
    public async Task RequestLoan_BookNotAvailable_ReturnsBadRequest()
    {
        // Arrange: Owner has a book that is already loaned out
        var clientA = _factory.Factory.CreateClient();
        var owner = await IntegrationTestHelpers.RegisterUserAsync(
            clientA, "owner2", "owner2@example.com", "Password123!"
        );
        var ownerClient = _factory.Factory.CreateAuthenticatedClient(owner);

        await ownerClient.PostAsJsonAsync("/api/books/manual", new AddBookManualDto
        {
            ISBN = "9780123456789",
            Title = "The Great Gatsby",
            Author = "F. Scott Fitzgerald",
            PublishedYear = "2024",
            PageCount = 180,
        });

        var clientB = _factory.Factory.CreateClient();
        var borrower1 = await IntegrationTestHelpers.RegisterUserAsync(
            clientB, "borrower1", "borrower1@example.com", "Password123!"
        );
        var borrower1Client = _factory.Factory.CreateAuthenticatedClient(borrower1);

        var clientC = _factory.Factory.CreateClient();
        var borrower2 = await IntegrationTestHelpers.RegisterUserAsync(
            clientC, "borrower2", "borrower2@example.com", "Password123!"
        );
        var borrower2Client = _factory.Factory.CreateAuthenticatedClient(borrower2);

        await borrower1Client.PostAsJsonAsync("/api/loans", new
        {
            ISBN = "9780123456789",
            OwnerId = owner.Id,
            Message = "I want it!"
        });

        var pendingResponse = await ownerClient.GetAsync("/api/loans/pending-requests");
        var pendingLoans = await pendingResponse.Content.ReadFromJsonAsync<List<LoanDto>>();
        var loanId = pendingLoans.First().Id;

        await ownerClient.PostAsJsonAsync($"/api/loans/respond/{loanId}", new ResponseLoanDto
        {
            Accept = true,
            DueDate = DateTime.UtcNow.AddDays(14),
            Notes = "Enjoy!"
        });

        await ownerClient.PostAsync($"/api/loans/loan/{loanId}", null);

        // Act: Another borrower tries to request the same book
        var secondRequest = await borrower2Client.PostAsJsonAsync("/api/loans", new
        {
            ISBN = "9780123456789",
            OwnerId = owner.Id,
            Message = "Can I borrow this too?"
        });

        // Assert: Second request is rejected because the book is not available
        secondRequest.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var errorContent = await secondRequest.Content.ReadAsStringAsync();
        errorContent.Should().Contain("not available for loan");
    }
}