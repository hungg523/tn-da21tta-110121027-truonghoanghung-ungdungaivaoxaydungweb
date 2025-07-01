using AppleShop.Share.Abstractions;

namespace AppleShop.Application.Requests.UserManagement.UserAddress
{
    public class DeleteUserAddressRequest : ICommand
    {
        public int? Id { get; set; }
    }
}