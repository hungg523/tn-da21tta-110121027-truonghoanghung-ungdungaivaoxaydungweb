using AppleShop.Application.Requests.DTOs.EventManagement.Spin;
using AppleShop.Application.Requests.EventManagement.Spin;
using AppleShop.Domain.Abstractions.IRepositories.EventManagement;
using AppleShop.Share.Shared;
using MediatR;

namespace AppleShop.Application.Handlers.EventManagement.Spin
{
    public class CheckSpinTodayQueryHandler : IRequestHandler<CheckSpinTodayRequest, Result<SpinDTO>>
    {
        private readonly ISpinHistoryRepository spinHistoryRepository;

        public CheckSpinTodayQueryHandler(ISpinHistoryRepository spinHistoryRepository)
        {
            this.spinHistoryRepository = spinHistoryRepository;
        }

        public async Task<Result<SpinDTO>> Handle(CheckSpinTodayRequest request, CancellationToken cancellationToken)
        {
            if (request.UserId is null) return Result<SpinDTO>.Ok(new SpinDTO { CanSpin = false });
            var isCanSpin = await spinHistoryRepository.FindSingleAsync(x => x.UserId == request.UserId, false);
            if (isCanSpin is not null && isCanSpin.SpinDate == DateOnly.FromDateTime(DateTime.Today)) return Result<SpinDTO>.Ok(new SpinDTO { CanSpin = false });
            return Result<SpinDTO>.Ok(new SpinDTO { CanSpin = true });
        }
    }
}