using AppleShop.Application.Requests.DTOs.UserManagement.Admin;
using AppleShop.Application.Requests.UserManagement.Admin;
using AppleShop.Domain.Abstractions.IRepositories.OrderManagement;
using AppleShop.Domain.Abstractions.IRepositories.UserManagement;
using AppleShop.Share.Abstractions;
using AppleShop.Share.Enumerations;
using AppleShop.Share.Shared;
using MediatR;

namespace AppleShop.Application.Handlers.UserManagement.Admin
{
    public class SearchUserHandler : IRequestHandler<SearchUserRequest, Result<List<ManageUserDTO>>>
    {
        private readonly IUserRepository userRepository;
        private readonly IFileService fileService;
        private readonly IOrderRepository orderRepository;

        public SearchUserHandler(IUserRepository userRepository, IFileService fileService, IOrderRepository orderRepository)
        {
            this.userRepository = userRepository;
            this.fileService = fileService;
            this.orderRepository = orderRepository;
        }

        public async Task<Result<List<ManageUserDTO>>> Handle(SearchUserRequest request, CancellationToken cancellationToken)
        {
            var users = userRepository.FindAll(x => x.Email.ToLower().Contains(request.Email.ToLower())).ToList();
            var orders = orderRepository.FindAll().ToLookup(o => o.UserId);
            var userDtos = users.Select(u => new ManageUserDTO
            {
                Id = u.Id,
                Avatar = !string.IsNullOrEmpty(u.ImageUrl) && u.ImageUrl.StartsWith("https")
                        ? u.ImageUrl
                        : fileService.GetFullPathFileServer(u.ImageUrl),
                UserName = u.Username,
                Email = u.Email,
                Role = ((Role)u.Role).ToString(),
                IsActived = u.IsActived,
                TotalOrders = orders.Contains(u.Id) ? orders[u.Id].Count() : 0,
                CreatedAt = u.CreatedAt
            }).ToList();

            return Result<List<ManageUserDTO>>.Ok(userDtos);
        }
    }
}