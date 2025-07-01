namespace AppleShop.Share.Abstractions
{
    public interface IShareEventDispatcher
    {
        Task PublishAsync<T>(T domainEvent, CancellationToken? cancellationToken = null) where T : IDomainEvent;
    }
}