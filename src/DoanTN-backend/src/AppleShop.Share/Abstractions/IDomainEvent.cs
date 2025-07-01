namespace AppleShop.Share.Abstractions
{
    public interface IDomainEvent
    {
        public DateTime DateOccurred { get; }
    }
}