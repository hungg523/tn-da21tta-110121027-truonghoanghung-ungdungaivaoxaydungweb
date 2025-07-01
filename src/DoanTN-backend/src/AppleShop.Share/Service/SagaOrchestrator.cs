using AppleShop.Share.Abstractions;
using AppleShop.Share.Shared;

namespace AppleShop.Share.Service
{
    public class SagaOrchestrator
    {
        private readonly List<ISagaStep> steps = new();

        public void AddStep(ISagaStep step)
        {
            steps.Add(step);
        }

        public async Task<Result<object>> ExecuteAsync(CancellationToken cancellationToken)
        {
            var executedSteps = new List<ISagaStep>();

            try
            {
                foreach (var step in steps)
                {
                    await step.ExecuteAsync(cancellationToken);
                    executedSteps.Add(step);
                }

                return Result<object>.Ok();
            }
            catch (Exception)
            {
                foreach (var step in Enumerable.Reverse(executedSteps))
                {
                    try { await step.RollbackAsync(cancellationToken); }
                    catch { /* Ghi log rollback fail nếu cần */ }
                }

                throw;
            }
        }
    }
}