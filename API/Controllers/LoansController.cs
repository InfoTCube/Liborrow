using API.DTOs.Loans;
using API.Entities;
using API.Enums;
using API.Extensions;
using API.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[Authorize]
public class LoansController : BaseApiController
{
    private readonly IUnitOfWork _unitOfWork;

    public LoansController(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    [HttpPost]
    public async Task<IActionResult> RequestLoan(BorrowRequestDto borrowequestDto)
    {
        var userBook = await _unitOfWork.Books.GetUserBookByIdAndUserIdAsync(borrowequestDto.ISBN, borrowequestDto.OwnerId);

        if(await _unitOfWork.Loans.IsBookAvailableForLoanAsync(borrowequestDto.OwnerId, borrowequestDto.ISBN) is false)
            return BadRequest("This book is currently not available for loan.");

        if(await _unitOfWork.Loans.HasPendingLoanAsync(borrowequestDto.OwnerId, borrowequestDto.ISBN, User.GetUserId()))
            return BadRequest("You already have a pending loan request for this book.");

        var newLoan = new Loan
        {
            ISBN = borrowequestDto.ISBN,
            OwnerId = borrowequestDto.OwnerId,
            BorrowerId = User.GetUserId(),
            RequestMessage = borrowequestDto.Message,
            RequestedAt = DateTime.UtcNow,
            Status = LoanStatus.Pending,
            UserBookId = userBook.Id
        };

        await _unitOfWork.Loans.AddLoanAsync(newLoan);

        if(await _unitOfWork.CompleteAsync()) return Ok();

        return BadRequest("Failed to request loan");
    }

    [HttpPost("respond/{loanId}")]
    public async Task<ActionResult> RespondToLoan(Guid loanId, ResponseLoanDto respondToLoanDto)
    {
        var loan = await _unitOfWork.Loans.GetLoanByIdAsync(loanId);

        if (loan == null) return NotFound("Loan request not found");
        if (loan.OwnerId != User.GetUserId()) return Unauthorized("You are not authorized to respond to this loan request");

        loan.Status = respondToLoanDto.Accept ? LoanStatus.Approved : LoanStatus.Declined;

        if(respondToLoanDto.Accept)
        {
            loan.ApprovedAt = DateTime.UtcNow;
            loan.DueDate = respondToLoanDto.DueDate;
            loan.Notes = respondToLoanDto.Notes;
        }

        if(await _unitOfWork.CompleteAsync()) return Ok();

        return BadRequest("Failed to respond to loan request");
    }

    [HttpPost("loan/{loanId}")]
    public async Task<ActionResult> LoanBook(Guid loanId)
    {
        var loan = await _unitOfWork.Loans.GetLoanByIdAsync(loanId);

        if (loan == null) return NotFound("Loan request not found");
        if (loan.OwnerId != User.GetUserId() && loan.BorrowerId != User.GetUserId()) return Unauthorized("You are not authorized to respond to this loan request");

        loan.Status = LoanStatus.Active;
        loan.LoanDate = DateTime.UtcNow;

        var userBook = await _unitOfWork.Books.GetUserBookByIdAndUserIdAsync(loan.ISBN, loan.OwnerId);
        if (userBook != null) userBook.IsAvailable = false;

        if(await _unitOfWork.CompleteAsync()) return Ok();

        return BadRequest("Failed to loan book");
    }

    [HttpPost("return/{loanId}")]
    public async Task<ActionResult> ReturnLoan(Guid loanId)
    {
        var loan = await _unitOfWork.Loans.GetLoanByIdAsync(loanId);

        if (loan == null) return NotFound("Loan not found");
        if (loan.BorrowerId != User.GetUserId()) return Unauthorized("You are not authorized to return this loan");

        if(loan.Status != LoanStatus.Active && loan.Status != LoanStatus.Overdue)
            return BadRequest("Only active or overdue loans can be returned");

        loan.Status = LoanStatus.Returned;
        loan.ReturnedAt = DateTime.UtcNow;

        if(await _unitOfWork.CompleteAsync()) return Ok();

        return BadRequest("Failed to return loan");
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<LoanDto>>> GetLoans()
    {
        var loans = await _unitOfWork.Loans.GetActiveLoansAsync(User.GetUserId());
        return Ok(loans);
    }

    [HttpGet("borrowed")]
    public async Task<ActionResult<IEnumerable<LoanDto>>> GetBorrowedLoans()
    {
        var borrowedLoans = await _unitOfWork.Loans.GetLoansForBorrowerAsync(User.GetUserId());
        return Ok(borrowedLoans);
    }

    [HttpGet("lent")]
    public async Task<ActionResult<IEnumerable<LoanDto>>> GetLentLoans()
    {
        var lentLoans = await _unitOfWork.Loans.GetLoansForOwnerAsync(User.GetUserId());
        return Ok(lentLoans);
    }

    [HttpGet("history")]
    public async Task<ActionResult<IEnumerable<LoanDto>>> GetLoanHistory()
    {
        var loanHistory = await _unitOfWork.Loans.GetLoanHistoryAsync(User.GetUserId());
        return Ok(loanHistory);
    }

    [HttpGet("pending-requests")]
    public async Task<ActionResult<IEnumerable<LoanDto>>> GetPendingRequests()
    {
        var pendingRequests = await _unitOfWork.Loans.GetPendingRequestsForOwnerAsync(User.GetUserId());
        return Ok(pendingRequests);
    }

    [HttpGet("pending-requests/borrower")]
    public async Task<ActionResult<IEnumerable<LoanDto>>> GetPendingRequestsFromBorrower()
    {
        var pendingRequests = await _unitOfWork.Loans.GetPendingRequestsFromBorrowerAsync(User.GetUserId());
        return Ok(pendingRequests);
    }
}