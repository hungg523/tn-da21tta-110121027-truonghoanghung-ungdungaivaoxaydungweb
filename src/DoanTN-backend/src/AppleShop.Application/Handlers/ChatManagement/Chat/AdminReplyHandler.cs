using AppleShop.Application.Requests.ChatManagement.Chat;
using AppleShop.Application.Requests.DTOs.ChatManagement.Chat;
using AppleShop.Application.Services;
using AppleShop.Domain.Abstractions.IRepositories.ChatManagement;
using AppleShop.Domain.Abstractions.IRepositories.UserManagement;
using AppleShop.Share.Abstractions;
using AppleShop.Share.Exceptions;
using AppleShop.Share.Shared;
using MediatR;

namespace AppleShop.Application.Handlers.ChatManagement.Chat
{
    public class AdminReplyHandler : IRequestHandler<AdminReplyRequest, Result<ChatMessageDTO>>
    {
        private readonly IConversationRepository conversationRepository;
        private readonly IChatMessageRepository chatMessageRepository;
        private readonly ICacheService cacheService;
        private readonly IChatService chatService;

        public AdminReplyHandler(
            IConversationRepository conversationRepository,
            IChatMessageRepository chatMessageRepository,
            ICacheService cacheService,
            IChatService chatService)
        {
            this.conversationRepository = conversationRepository;
            this.chatMessageRepository = chatMessageRepository;
            this.cacheService = cacheService;
            this.chatService = chatService;
        }

        public async Task<Result<ChatMessageDTO>> Handle(AdminReplyRequest request, CancellationToken cancellationToken)
        {
            var conversation = await conversationRepository.FindByIdAsync(request.ConversationId, true);
            if (conversation is null) AppleException.ThrowNotFound(typeof(Domain.Entities.ChatManagement.Conversations));

            var message = new Domain.Entities.ChatManagement.ChatMessages
            {
                ConversationId = request.ConversationId,
                SenderId = request.AdminId,
                Message = request.Message,
                TimeStamp = DateTime.Now,
                IsFromBot = false,
                SenderType = "admin"
            };

            chatMessageRepository.Create(message);
            await chatMessageRepository.SaveChangesAsync(cancellationToken);

            await chatService.SendMessage($"Conversation-{conversation.Id}", message.Message, message.SenderType, message.SenderId);
            await cacheService.RemoveAsync("pending_conversations");
            return Result<ChatMessageDTO>.Ok(new ChatMessageDTO
            {
                ChatId = message.Id,
                ConversationId = message.ConversationId,
                SenderType = message.SenderType,
                SenderId = message.SenderId,
                Message = message.Message,
                TimeStamp = message.TimeStamp.Value,
                IsFromBot = message.IsFromBot
            });
        }
    }
} 