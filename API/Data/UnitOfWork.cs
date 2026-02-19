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

    public async Task<bool> CompleteAsync() => await _context.SaveChangesAsync() > 0;
}