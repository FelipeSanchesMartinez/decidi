using Decidi.Domain.Interfaces;
using Decidi.Infrastructure.Data;
using Microsoft.EntityFrameworkCore.Storage;

namespace Decidi.Infrastructure.Repositories;

public class UnitOfWork(AppDbContext context) : IUnitOfWork
{
    public async Task<int> SaveChangesAsync() => await context.SaveChangesAsync();

    public async Task<ITransaction> BeginTransactionAsync()
    {
        var tx = await context.Database.BeginTransactionAsync();
        return new EfTransaction(tx);
    }

    public void Dispose() => context.Dispose();

    private sealed class EfTransaction(IDbContextTransaction tx) : ITransaction
    {
        public Task CommitAsync() => tx.CommitAsync();
        public Task RollbackAsync() => tx.RollbackAsync();
        public ValueTask DisposeAsync() => tx.DisposeAsync();
    }
}
