using AppleShop.Share.Abstractions;
using MassTransit;

namespace AppleShop.Share.Service
{
    public class MasstransitEventDispatcher : IShareEventDispatcher
    {
        private readonly IPublishEndpoint publishEndpoint;

        public MasstransitEventDispatcher(IPublishEndpoint publishEndpoint)
        {
            this.publishEndpoint = publishEndpoint;
        }

        public async Task PublishAsync<T>(T DomainEvent, CancellationToken? cancellationToken = null) where T : IDomainEvent
        {
            if (cancellationToken is null)
            {
                var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(1));
                cancellationToken = cancellationTokenSource.Token;
            }

            try
            {
                await publishEndpoint.Publish(DomainEvent, cancellationToken.Value);
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}