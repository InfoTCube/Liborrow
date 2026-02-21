using API.Data;
using API.DTOs.Books;
using API.Entities;
using API.Enums;
using API.Extensions;
using API.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[Authorize]
public class BooksController : BaseApiController
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IBibliotekaNarodowaBooksService _bibliotekaNarodowaBooksService;

    public BooksController(IUnitOfWork unitOfWork, IBibliotekaNarodowaBooksService bibliotekaNarodowaBooksService)
    {
        _unitOfWork = unitOfWork;
        _bibliotekaNarodowaBooksService = bibliotekaNarodowaBooksService;
    }

    [HttpPost]
    public async Task<ActionResult<BookWithOwnerDto>> AddBookToCollection(AddBookDto addBookDto)
    {
        var userId = User.GetUserId();
        var existingBook = await _unitOfWork.Books.GetBookByIsbnAsync(addBookDto.ISBN);

        if (existingBook is not null) 
        {
            if (await _unitOfWork.Books.UserOwnsBookAsync(userId, addBookDto.ISBN))
                return BadRequest("You already own this book.");

            var userBook = await _unitOfWork.Books.AddUserBookAsync(userId, addBookDto.ISBN, addBookDto.Notes);
            
            if (await _unitOfWork.CompleteAsync())
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
            var bookFromApi = await _bibliotekaNarodowaBooksService.GetBookByIsbnAsync(addBookDto.ISBN);

            if (bookFromApi is null)
                return NotFound("Book not found in external API, please provide the book details manually.");

            var newBook = new Book
            {
                ISBN = bookFromApi.ISBN,
                Title = bookFromApi.Title,
                Author = bookFromApi.Author,
                CoverImageUrl = bookFromApi.CoverImageUrl,
                Description = bookFromApi.Description,
                PublishedYear = bookFromApi.PublishedYear,
                PageCount = bookFromApi.PageCount,
                Source = BookSource.BibliotekaNarodowa
            };

            await _unitOfWork.Books.AddBookAsync(newBook);

            var userBook = await _unitOfWork.Books.AddUserBookAsync(userId, addBookDto.ISBN, addBookDto.Notes);

            if (await _unitOfWork.CompleteAsync())
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
            return NotFound($"Error fetching book details, provide the book manually: {ex.Message}");
        }

        return BadRequest("Failed to add book to collection.");
    }

    [HttpPost("manual")]
    public async Task<ActionResult<BookWithOwnerDto>> AddBookManually(AddBookManualDto addBookManualDto)
    {
        var userId = User.GetUserId();

        if (await _unitOfWork.Books.UserOwnsBookAsync(userId, addBookManualDto.ISBN))
            return BadRequest("You already own this book.");

        var existingBook = await _unitOfWork.Books.GetBookByIsbnAsync(addBookManualDto.ISBN);

        if (existingBook is null)
        {
            var newBook = new Book
            {
                ISBN = addBookManualDto.ISBN,
                Title = addBookManualDto.Title,
                Author = addBookManualDto.Author,
                CoverImageUrl = addBookManualDto.CoverImageUrl,
                Description = addBookManualDto.Description,
                PublishedYear = addBookManualDto.PublishedYear,
                PageCount = addBookManualDto.PageCount,
                Source = BookSource.Users
            };

            await _unitOfWork.Books.AddBookAsync(newBook);
        }

        var userBook = await _unitOfWork.Books.AddUserBookAsync(userId, addBookManualDto.ISBN, addBookManualDto.Notes);

        if (await _unitOfWork.CompleteAsync())
        {
            var result = new BookWithOwnerDto
            {
                ISBN = addBookManualDto.ISBN,
                Title = addBookManualDto.Title,
                Author = addBookManualDto.Author,
                CoverImageUrl = addBookManualDto.CoverImageUrl,
                Description = addBookManualDto.Description,
                PublishedYear = addBookManualDto.PublishedYear,
                PageCount = addBookManualDto.PageCount,
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
    public async Task<ActionResult<IEnumerable<BookDto>>> GetMyBooks()
    {
        var userId = User.GetUserId();
        
        var userBooks = await _unitOfWork.Books.GetUserBooksAsync(userId);
        
        return Ok(userBooks);
    }

    [HttpGet("friends/{friendId}")]
    public async Task<ActionResult<IEnumerable<BookDto>>> GetFriendBooks(Guid friendId)
    {
        var userId = User.GetUserId();
        
        var isFriend = await _unitOfWork.Friendships.AreFriendsAsync(userId, friendId);
        
        if (!isFriend)
            return BadRequest("You can only view books of your friends.");
        
        var friendBooks = await _unitOfWork.Books.GetUserBooksAsync(friendId);
        
        return Ok(friendBooks); 
    }
}