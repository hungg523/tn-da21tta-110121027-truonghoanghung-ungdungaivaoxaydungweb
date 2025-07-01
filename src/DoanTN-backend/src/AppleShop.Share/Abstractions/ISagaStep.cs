namespace AppleShop.Share.Abstractions
{
    public interface ISagaStep
    {
        Task ExecuteAsync(CancellationToken cancellationToken);
        Task RollbackAsync(CancellationToken cancellationToken);
    }
}