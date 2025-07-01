using AppleShop.Share.Shared;
using MediatR;

namespace AppleShop.Share.Abstractions
{
    public interface ICommand : IRequest<Result<object>>
    {
    }

    public interface ICommand<T> : IRequest<Result<T>> where T : class
    {
    }
}