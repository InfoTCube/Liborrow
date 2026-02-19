namespace API.Interfaces;

public interface IUnitOfWork
{
    IBookRepository Books { get; }
    Task<bool> CompleteAsync();
}