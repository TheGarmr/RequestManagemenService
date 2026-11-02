namespace RequestManagementService.DataAccess;

public sealed class UnitOfWork : IUnitOfWork
{
    private readonly RequestManagementDbContext _dbContext;

    public UnitOfWork(RequestManagementDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task CommitAsync(CancellationToken cancellationToken)
    {
        return _dbContext.SaveChangesAsync(cancellationToken);
    }
}
