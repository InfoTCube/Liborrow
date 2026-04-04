using API.DTOs.Books;
using API.Enums;
using API.Extensions;
using API.Extensions.Mappers;
using API.Helpers;
using API.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[Authorize]
public class BooksController : BaseApiController
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IBibliotekaNarodowaBooksService _bibliotekaNarodowaBooksService;
    private readonly ILogger<BooksController> _logger;

    public BooksController(IUnitOfWork unitOfWork, IBibliotekaNarodowaBooksService bibliotekaNarodowaBooksService, ILogger<BooksController> logger)
    {
        _unitOfWork = unitOfWork;
        _bibliotekaNarodowaBooksService = bibliotekaNarodowaBooksService;
        _logger = logger;
    }

    [HttpPost]
    public async Task<ActionResult<BookWithOwnerDto>> AddBookToCollection(AddBookDto addBookDto, CancellationToken ct)
    {
        var userId = User.GetUserId();
        var existingBook = await _unitOfWork.Books.GetBookByIsbnAsync(addBookDto.ISBN, ct);

        if (existingBook is not null) 
        {
            if (await _unitOfWork.Books.UserOwnsBookAsync(userId, addBookDto.ISBN, ct))
                return BadRequest("You already own this book.");

            var userBook = await _unitOfWork.Books.AddUserBookAsync(userId, addBookDto.ISBN, addBookDto.Notes, ct);
            
            if (await _unitOfWork.CompleteAsync(ct))
            {
                var result = new BookWithOwnerDto
                {
                    ISBN = existingBook.ISBN ?? string.Empty,
                    Title = existingBook.Title ?? "Unknown Title",
                    Author = existingBook.Author,
                    CoverImageUrl = existingBook.CoverImageUrl,
                    Description = existingBook.Description,
                    PublishedYear = existingBook.PublishedYear,
                    PageCount = existingBook.PageCount,
                    IsAvailable = userBook.IsAvailable,
                    Notes = userBook.Notes,
                    AddedAt = userBook.AddedAt,
                    OwnerId = userBook.UserId,
                    OwnerName = User.GetUserName()
                };
                return Ok(result);
            }
        }

        // If book doesn't exist in db, try to fetch from Biblioteka Narodowa API
        try
        {
            var bookFromApi = await _bibliotekaNarodowaBooksService.GetBookByIsbnAsync(addBookDto.ISBN, ct);

            if (bookFromApi is null)
                return NotFound("Book not found in external API, please provide the book details manually.");

            var newBook = bookFromApi.ToBook(BookSource.BibliotekaNarodowa);

            await _unitOfWork.Books.AddBookAsync(newBook, ct);

            var userBook = await _unitOfWork.Books.AddUserBookAsync(userId, addBookDto.ISBN, addBookDto.Notes, ct);

            if (await _unitOfWork.CompleteAsync(ct))
            {
                var result = new BookWithOwnerDto
                {
                    ISBN = newBook.ISBN,
                    Title = newBook.Title,
                    Author = newBook.Author,
                    CoverImageUrl = newBook.CoverImageUrl,
                    Description = newBook.Description,
                    Notes = userBook.Notes,
                    AddedAt = userBook.AddedAt,
                    IsAvailable = userBook.IsAvailable,
                    OwnerId = userId,
                    OwnerName = User.Identity?.Name ?? "Unknown",
                };
                return Ok(result);
            }
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error fetching book details from external API");
            return NotFound($"Error fetching book details, provide the book manually!");
        }

        return BadRequest("Failed to add book to collection.");
    }

    [HttpPost("manual")]
    public async Task<ActionResult<BookWithOwnerDto>> AddBookManually(AddBookManualDto addBookManualDto, CancellationToken ct)
    {
        var userId = User.GetUserId();

        if (await _unitOfWork.Books.UserOwnsBookAsync(userId, addBookManualDto.ISBN, ct))
            return BadRequest("You already own this book.");

        var existingBook = await _unitOfWork.Books.GetBookByIsbnAsync(addBookManualDto.ISBN, ct);

        if (existingBook is null)
        {
            var newBook = addBookManualDto.ToBook(BookSource.Users);

            await _unitOfWork.Books.AddBookAsync(newBook, ct);
        }

        var userBook = await _unitOfWork.Books.AddUserBookAsync(userId, addBookManualDto.ISBN, addBookManualDto.Notes, ct);

        if (await _unitOfWork.CompleteAsync(ct))
        {
            var result = new BookWithOwnerDto
            {
                ISBN = existingBook is null ? addBookManualDto.ISBN : existingBook.ISBN,
                Title = existingBook is null ? addBookManualDto.Title : existingBook.Title,
                Author = existingBook is null ? addBookManualDto.Author : existingBook.Author,
                CoverImageUrl = existingBook is null ? addBookManualDto.CoverImageUrl : existingBook.CoverImageUrl,
                Description = existingBook is null ? addBookManualDto.Description : existingBook.Description,
                PublishedYear = existingBook is null ? addBookManualDto.PublishedYear : existingBook.PublishedYear,
                PageCount = existingBook is null ? addBookManualDto.PageCount : existingBook.PageCount,
                IsAvailable = userBook.IsAvailable,
                Notes = userBook.Notes,
                AddedAt = userBook.AddedAt,
                OwnerId = userId,
                OwnerName = User.Identity?.Name ?? "Unknown",
            };

            return Ok(result);
        }

        return BadRequest("Failed to add book to collection.");
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<BookDto>>> GetMyBooks([FromQuery] ElementParams elementParams, CancellationToken ct)
    {
        var userId = User.GetUserId();
        
        var userBooks = await _unitOfWork.Books.GetUserBooksAsync(userId, elementParams, ct);

        Response.AddPaginationHeader(userBooks.CurrentPage, userBooks.PageSize, 
            userBooks.TotalCount, userBooks.TotalPages);
        
        return Ok(userBooks.ToBookDto());
    }

    [HttpGet("friends/{friendId}")]
    public async Task<ActionResult<IEnumerable<BookDto>>> GetFriendBooks(Guid friendId, [FromQuery] ElementParams elementParams, 
        CancellationToken ct)
    {
        var userId = User.GetUserId();
        
        var isFriend = await _unitOfWork.Friendships.AreFriendsAsync(userId, friendId, ct);
        
        if (!isFriend)
            return BadRequest("You can only view books of your friends.");
        
        var friendBooks = await _unitOfWork.Books.GetUserBooksAsync(friendId, elementParams, ct);

        Response.AddPaginationHeader(friendBooks.CurrentPage, friendBooks.PageSize, 
            friendBooks.TotalCount, friendBooks.TotalPages);
        
        return Ok(friendBooks.ToBookDto()); 
    }

    [HttpGet("search-friends")]
    public async Task<ActionResult<IEnumerable<BookDto>>> SearchBooks([FromQuery] string query, [FromQuery] ElementParams elementParams,
         CancellationToken ct)
    {
        var userId = User.GetUserId();
        
        var books = await _unitOfWork.Books.SearchFriendsBooksAsync(userId, query, elementParams, ct);
        
        Response.AddPaginationHeader(books.CurrentPage, books.PageSize, 
            books.TotalCount, books.TotalPages);
        
        return Ok(books.ToBookDto());
    }
}