namespace API.Interfaces;

public interface IUnitOfWork
{
    IBookRepository Books { get; }
    IUserRepository Users { get; }
    IFriendshipRepository Friendships { get; }
    
    Task<bool> CompleteAsync();
}