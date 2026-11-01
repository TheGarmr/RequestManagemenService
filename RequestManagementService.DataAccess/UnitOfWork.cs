namespace RequestManagementService.DataAccess;

internal sealed class UnitOfWork(RequestManagementDbContext dbContext) : IUnitOfWork
{
    public Task CommitAsync(CancellationToken cancellationToken)
    {
        return dbContext.SaveChangesAsync(cancellationToken);
    }
}
