using API.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace API.Data;

public class DataContext : IdentityDbContext<AppUser, IdentityRole<Guid>, Guid>
{
    public DataContext(DbContextOptions options) : base(options)
    {
    }

    public DbSet<Book> Books { get; set; }
    public DbSet<UserBook> UserBooks { get; set; }
    public DbSet<Loan> Loans { get; set; }
    public DbSet<Friendship> Friendships { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<AppUser>(entity =>
        {
            entity.HasMany(u => u.Books)
                .WithOne(ub => ub.User)
                .HasForeignKey(ub => ub.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(u => u.LentBooks)
                .WithOne(l => l.Owner)
                .HasForeignKey(l => l.OwnerId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasMany(u => u.BorrowedBooks)
                .WithOne(l => l.Borrower)
                .HasForeignKey(l => l.BorrowerId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasMany(u => u.ReceivedFriendRequests)
                .WithOne(f => f.Receiver)
                .HasForeignKey(f => f.ReceiverId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(u => u.SentFriendRequests)
                .WithOne(f => f.Requester)
                .HasForeignKey(f => f.RequesterId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<Book>(entity =>
        {
            entity.HasKey(b => b.ISBN);

            entity.HasMany(b => b.OwnedBy)
                .WithOne(ub => ub.Book)
                .HasForeignKey(ub => ub.ISBN)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasMany(b => b.Loans)
                .WithOne(l => l.Book)
                .HasForeignKey(l => l.ISBN)
                .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<UserBook>(entity =>
        {
            entity.HasOne(ub => ub.User)
                .WithMany(u => u.Books)
                .HasForeignKey(ub => ub.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(ub => ub.Book)
                .WithMany(b => b.OwnedBy)
                .HasForeignKey(ub => ub.ISBN)
                .OnDelete(DeleteBehavior.Restrict);

            // Ensure a user can't have the same book twice
            entity.HasIndex(ub => new { ub.UserId, ub.ISBN }).IsUnique();
        });

        builder.Entity<Friendship>(entity =>
        {
            // Ensure unique friendships (can't have duplicate friend requests)
            entity.HasIndex(f => new { f.RequesterId, f.ReceiverId }).IsUnique();

            entity.HasOne(f => f.Requester)
                .WithMany(u => u.SentFriendRequests)
                .HasForeignKey(f => f.RequesterId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(f => f.Receiver)
                .WithMany(u => u.ReceivedFriendRequests)
                .HasForeignKey(f => f.ReceiverId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<Loan>(entity =>
        {
            entity.HasOne(l => l.Book)
                .WithMany(b => b.Loans)
                .HasForeignKey(l => l.ISBN)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(l => l.Owner)
                .WithMany(u => u.LentBooks)
                .HasForeignKey(l => l.OwnerId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(l => l.Borrower)
                .WithMany(u => u.BorrowedBooks)
                .HasForeignKey(l => l.BorrowerId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}