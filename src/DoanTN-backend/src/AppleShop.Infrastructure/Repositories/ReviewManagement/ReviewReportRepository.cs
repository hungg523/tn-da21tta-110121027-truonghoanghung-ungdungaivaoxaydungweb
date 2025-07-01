using AppleShop.Domain.Abstractions.IRepositories.ReviewManagement;
using AppleShop.Domain.Entities.ReviewManagement;
using AppleShop.Infrastructure.Repositories.Base;

namespace AppleShop.Infrastructure.Repositories.ReviewManagement
{
    public class ReviewReportRepository(ApplicationDbContext context) : GenericRepository<ReviewReport>(context), IReviewReportRepository
    {
    }
}