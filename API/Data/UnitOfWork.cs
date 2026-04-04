using API.Interfaces;

namespace API.Data;

public class UnitOfWork : IUnitOfWork
{
    private readonly DataContext _context;

    private IBookRepository _books;
    private IUserRepository _users;
    private IFriendshipRepository _friendships;
    private ILoanRepository _loans;

    public UnitOfWork(DataContext context)
    {
        _context = context;
    }

    public IBookRepository Books => _books ??= new BookRepository(_context);
    public IUserRepository Users => _users ??= new UserRepository(_context);
    public IFriendshipRepository Friendships => _friendships ??= new FriendshipRepository(_context);
    public ILoanRepository Loans => _loans ??= new LoanRepository(_context);

    public async Task<bool> CompleteAsync(CancellationToken ct) => await _context.SaveChangesAsync(ct) > 0;
}