using AppleShop.Application.Requests.DTOs.UserManagement.Admin;
using AppleShop.Application.Requests.UserManagement.Admin;
using AppleShop.Domain.Abstractions.IRepositories.OrderManagement;
using AppleShop.Domain.Abstractions.IRepositories.UserManagement;
using AppleShop.Share.Abstractions;
using AppleShop.Share.Enumerations;
using AppleShop.Share.Shared;
using MediatR;
using System.Collections.Generic;

namespace AppleShop.Application.Handlers.UserManagement.Admin
{
    public class GetAllUserHandler : IRequestHandler<GetAllUserRequest, Result<ManageUserListResponseDTO>>
    {
        private readonly IUserRepository userRepository;
        private readonly IFileService fileService;
        private readonly IOrderRepository orderRepository;

        public GetAllUserHandler(IUserRepository userRepository, IFileService fileService, IOrderRepository orderRepository)
        {
            this.userRepository = userRepository;
            this.fileService = fileService;
            this.orderRepository = orderRepository;
        }

        public async Task<Result<ManageUserListResponseDTO>> Handle(GetAllUserRequest request, CancellationToken cancellationToken)
        {
            var query = userRepository.FindAll();
            if (request.Role is not null) query = query.Where(x => x.Role == request.Role);
            if (request.IsActived is not null) query = query.Where(x => x.IsActived == request.IsActived);

            var users = query.ToList();
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

            var totalItems = userDtos.Count();
            var pagedDtos = userDtos.Skip(request.Skip ?? 0).Take(request.Take ?? totalItems).ToList();
            return Result<ManageUserListResponseDTO>.Ok(new ManageUserListResponseDTO { TotalItems = totalItems, Items = pagedDtos });
        }
    }
}