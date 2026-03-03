using API.Interfaces;

namespace API.Data;

public class UnitOfWork : IUnitOfWork
{
    private readonly DataContext _context;

    public UnitOfWork(DataContext context)
    {
        _context = context;
    }

    public IBookRepository Books => new BookRepository(_context);
    public IUserRepository Users => new UserRepository(_context);
    public IFriendshipRepository Friendships => new FriendshipRepository(_context);
    public ILoanRepository Loans => new LoanRepository(_context);

    public async Task<bool> CompleteAsync(CancellationToken ct) => await _context.SaveChangesAsync(ct) > 0;
}