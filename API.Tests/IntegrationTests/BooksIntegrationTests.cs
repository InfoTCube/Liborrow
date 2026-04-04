using System.Net;
using System.Net.Http.Json;
using API.DTOs.Books;
using API.Tests.IntegrationTests.Fixtures;
using API.Tests.IntegrationTests.Helpers;
using FluentAssertions;

namespace API.Tests.IntegrationTests;

[Collection("Integration")]
public class BooksIntegrationTests : IClassFixture<IntegrationTestFixture>
{
    private readonly IntegrationTestFixture _factory;

    public BooksIntegrationTests(IntegrationTestFixture factory) => _factory = factory;

    [Fact]
    public async Task AddBookManually_ThenGetMyBooks_BookAppearsInList()
    {
        // Arrange: Register user and create authenticated client
        var client = _factory.Factory.CreateClient();
        var user = await IntegrationTestHelpers.RegisterUserAsync(
            client, "bookTester", "bookTester@example.com", "Password123!"
        );
        var authClient = _factory.Factory.CreateAuthenticatedClient(user);

        var addBookDto = new AddBookManualDto
        {
            ISBN = "9780123456789",
            Title = "The Great Gatsby",
            Author = "F. Scott Fitzgerald",
            PublishedYear = "2024",
            PageCount = 180,
        };

        // Act: Add a book manually
        var addResponse = await authClient.PostAsJsonAsync("/api/books/manual", addBookDto);

        // Assert: Book is added successfully
        addResponse.EnsureSuccessStatusCode();
        var addedBook = await addResponse.Content.ReadFromJsonAsync<BookWithOwnerDto>();
        addedBook.Should().NotBeNull();
        addedBook!.ISBN.Should().Be(addBookDto.ISBN);
        addedBook.Title.Should().Be(addBookDto.Title);
        addedBook.Author.Should().Be(addBookDto.Author);
        addedBook.OwnerId.Should().Be(user.Id);

        // Act: Get my books
        var getResponse = await authClient.GetAsync("/api/books");
        getResponse.EnsureSuccessStatusCode();
        var books = await getResponse.Content.ReadFromJsonAsync<List<BookWithOwnerDto>>();

        // Assert: The added book appears in the list
        books.Should().Contain(b => b.ISBN == addBookDto.ISBN && b.Title == addBookDto.Title);
        books[0].IsAvailable.Should().BeTrue();
    }

    [Fact]
    public async Task AddDuplicateBook_ReturnsBadRequest()
    {
        // Arrange: Register user and create authenticated client
        var client = _factory.Factory.CreateClient();
        var user = await IntegrationTestHelpers.RegisterUserAsync(
            client, "duplicateTester", "duplicateTester@example.com", "Password123!"
        );
        var authClient = _factory.Factory.CreateAuthenticatedClient(user);

        var addBookDto = new AddBookManualDto
        {
            ISBN = "9780123456789",
            Title = "The Great Gatsby",
            Author = "F. Scott Fitzgerald",
            PublishedYear = "2024",
            PageCount = 180,
        };

        // Act: Add a book manually
        var addResponse = await authClient.PostAsJsonAsync("/api/books/manual", addBookDto);
        addResponse.EnsureSuccessStatusCode();

        // Act: Try to add the same book again
        var duplicateResponse = await authClient.PostAsJsonAsync("/api/books/manual", addBookDto);

        // Assert: Duplicate book returns BadRequest
        duplicateResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}