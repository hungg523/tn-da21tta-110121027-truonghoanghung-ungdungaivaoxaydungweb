using AppleShop.Application.Requests.EventManagement.Spin;
using AppleShop.Domain.Abstractions.IRepositories.EventManagement;
using AppleShop.Domain.Entities.EventManagement;
using AppleShop.Share.Shared;
using MediatR;

namespace AppleShop.Application.Handlers.EventManagement.Spin
{
    public class SaveSpinHistoryHandler : IRequestHandler<SaveSpinHistoryRequest, Result<object>>
    {
        private readonly ISpinHistoryRepository spinHistoryRepository;

        public SaveSpinHistoryHandler(ISpinHistoryRepository spinHistoryRepository)
        {
            this.spinHistoryRepository = spinHistoryRepository;
        }

        public async Task<Result<object>> Handle(SaveSpinHistoryRequest request, CancellationToken cancellationToken)
        {
            using var transaction = await spinHistoryRepository.BeginTransactionAsync(cancellationToken);
            try
            {
                var spin = new SpinHistory
                {
                    UserId = request.UserId,
                    CouponId = request.CouponId,
                    SpinDate = DateOnly.FromDateTime(DateTime.Today),
                    CreatedAt = DateTime.Now
                };
                spinHistoryRepository.Create(spin);
                await spinHistoryRepository.SaveChangesAsync(cancellationToken);
                transaction.Commit();
                return Result<object>.Ok();
            }
            catch (Exception)
            {
                transaction.Rollback();
                throw;
            }
        }
    }
}