using AppleShop.Share.Shared;
using MediatR;

namespace AppleShop.Share.Abstractions
{
    public interface IQuery<T> : IRequest<Result<T>> where T : class
    {
    }
}