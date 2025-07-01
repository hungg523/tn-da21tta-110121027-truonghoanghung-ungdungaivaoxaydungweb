using AppleShop.Application.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace AppleShop.Application.Services
{
    public interface INotificationService
    {
        Task NotifyNewPendingMessage(int conversationId, string message);
        Task NotifyNewOrder(int orderId, string orderNumber);
        Task NotifyNewReturnRequest(int returnId, string orderNumber);
        Task NotifyNewReportedComment(int commentId, string productName);
    }

    public class NotificationService : INotificationService
    {
        private readonly IHubContext<NotificationHub> hubContext;

        public NotificationService(IHubContext<NotificationHub> hubContext)
        {
            this.hubContext = hubContext;
        }

        public async Task NotifyNewPendingMessage(int conversationId, string message)
        {
            await hubContext.Clients.Group("Admins")
                .SendAsync("NewPendingMessage", new
                {
                    ConversationId = conversationId,
                    Message = message,
                    Timestamp = DateTime.Now
                });
        }

        public async Task NotifyNewOrder(int orderId, string orderNumber)
        {
            await hubContext.Clients.Group("Admins")
                .SendAsync("NewOrder", new
                {
                    OrderId = orderId,
                    OrderNumber = orderNumber,
                    Timestamp = DateTime.Now
                });
        }

        public async Task NotifyNewReturnRequest(int returnId, string orderNumber)
        {
            await hubContext.Clients.Group("Admins")
                .SendAsync("NewReturnRequest", new
                {
                    ReturnId = returnId,
                    OrderNumber = orderNumber,
                    Timestamp = DateTime.Now
                });
        }

        public async Task NotifyNewReportedComment(int commentId, string productName)
        {
            await hubContext.Clients.Group("Admins")
                .SendAsync("NewReportedComment", new
                {
                    CommentId = commentId,
                    ProductName = productName,
                    Timestamp = DateTime.Now
                });
        }
    }
} 