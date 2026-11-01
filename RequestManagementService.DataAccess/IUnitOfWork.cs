namespace RequestManagementService.DataAccess;

public interface IUnitOfWork
{
    Task CommitAsync(CancellationToken cancellationToken);
}
