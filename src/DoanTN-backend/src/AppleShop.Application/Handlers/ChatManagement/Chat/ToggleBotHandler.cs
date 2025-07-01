using AppleShop.Application.Hubs;
using AppleShop.Application.Requests.ChatManagement.Chat;
using AppleShop.Domain.Abstractions.IRepositories.ChatManagement;
using AppleShop.Share.Abstractions;
using AppleShop.Share.Exceptions;
using AppleShop.Share.Shared;
using MediatR;
using Microsoft.AspNetCore.SignalR;

namespace AppleShop.Application.Handlers.ChatManagement.Chat
{
    public class ToggleBotHandler : IRequestHandler<ToggleBotRequest, Result<object>>
    {
        private readonly IConversationRepository conversationRepository;
        private readonly IHubContext<ChatHub> hubContext;
        private readonly ICacheService cacheService;

        public ToggleBotHandler(IConversationRepository conversationRepository, IHubContext<ChatHub> hubContext, ICacheService cacheService)
        {
            this.conversationRepository = conversationRepository;
            this.hubContext = hubContext;
            this.cacheService = cacheService;
        }

        public async Task<Result<object>> Handle(ToggleBotRequest request, CancellationToken cancellationToken)
        {
            var conversation = await conversationRepository.FindByIdAsync(request.ConversationId, true);
            if (conversation is null) AppleException.ThrowNotFound(typeof(Domain.Entities.ChatManagement.Conversations));

            conversation.IsBotHandled = request.IsBotEnabled;
            conversationRepository.Update(conversation);
            await conversationRepository.SaveChangesAsync(cancellationToken);

            // Thông báo cho client về trạng thái mới của bot
            await hubContext.Clients.Group($"Conversation-{conversation.Id}")
                .SendAsync("BotStatusChanged", new
                {
                    ConversationId = conversation.Id,
                    IsBotEnabled = conversation.IsBotHandled,
                    TimeStamp = DateTime.Now
                }, cancellationToken);

            await cacheService.RemoveAsync("pending_conversations");

            return Result<object>.Ok();
        }
    }
} 