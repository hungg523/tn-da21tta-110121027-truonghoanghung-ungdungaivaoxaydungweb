using AppleShop.Application.Requests.ChatManagement.Chat;
using AppleShop.Application.Requests.DTOs.ChatManagement.Chat;
using AppleShop.Domain.Abstractions.IRepositories.ChatManagement;
using AppleShop.Domain.Abstractions.IRepositories.UserManagement;
using AppleShop.Share.Abstractions;
using AppleShop.Share.Shared;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AppleShop.Application.Handlers.ChatManagement.Chat
{
    public class GetPendingConversationsHandler : IRequestHandler<GetPendingConversationsRequest, Result<List<ConversationDTO>>>
    {
        private readonly IConversationRepository conversationRepository;
        private readonly IChatMessageRepository chatMessageRepository;
        private readonly ICacheService cacheService;
        private readonly IUserRepository userRepository;
        private readonly IFileService fileService;

        public GetPendingConversationsHandler(IConversationRepository conversationRepository,
                                            IChatMessageRepository chatMessageRepository,
                                            ICacheService cacheService,
                                            IUserRepository userRepository,
                                            IFileService fileService)
        {
            this.conversationRepository = conversationRepository;
            this.chatMessageRepository = chatMessageRepository;
            this.cacheService = cacheService;
            this.userRepository = userRepository;
            this.fileService = fileService;
        }

        public async Task<Result<List<ConversationDTO>>> Handle(GetPendingConversationsRequest request, CancellationToken cancellationToken)
        {
            var conversations = conversationRepository.FindAll(null, true).OrderByDescending(x => x.CreatedAt).ToList();

            var now = DateTime.Now;
            var guestConversations = conversations.Where(x => x.Status != null && x.Status.StartsWith("guest") && x.CreatedAt.HasValue && (now - x.CreatedAt.Value).TotalDays >= 1).ToList();
            if (guestConversations.Any())
            {
                foreach (var guestConv in guestConversations)
                {
                    var guestMessages = chatMessageRepository.FindAll(m => m.ConversationId == guestConv.Id).ToList();
                    chatMessageRepository.RemoveMultiple(guestMessages);
                    conversationRepository.Delete(guestConv);
                }
                if (guestConversations.Any())
                {
                    await chatMessageRepository.SaveChangesAsync(cancellationToken);
                    await conversationRepository.SaveChangesAsync(cancellationToken);
                }
            }

            var cacheKey = "pending_conversations";
            var cachedResult = await cacheService.GetAsync<Result<List<ConversationDTO>>>(cacheKey);
            if (cachedResult is not null) return cachedResult;

            var users = userRepository.FindAll(x => conversations.Select(x => x.UserId).Contains(x.Id), false).ToDictionary(u => u.Id);
            var result = new List<ConversationDTO>();

            foreach (var conversation in conversations)
            {
                var messages = await chatMessageRepository.FindAll(x => x.ConversationId == conversation.Id).OrderBy(x => x.TimeStamp).ToListAsync(cancellationToken);
                users.TryGetValue(conversation.UserId ?? 0, out var user);
                result.Add(new ConversationDTO
                {
                    ConversationId = conversation.Id,
                    UserId = conversation.UserId,
                    UserName = user is null ? null : user.Username,
                    Avartar = user is null ? null : (user.ImageUrl.StartsWith("https") ? user.ImageUrl : fileService.GetFullPathFileServer(user.ImageUrl)),
                    Status = conversation.Status,
                    IsBotHandled = conversation.IsBotHandled,
                    CreatedAt = conversation.CreatedAt,
                    Messages = messages.Select(m => new ChatMessageDTO
                    {
                        ChatId = m.Id,
                        ConversationId = m.ConversationId,
                        SenderType = m.SenderType,
                        SenderId = m.SenderId,
                        Message = m.Message,
                        TimeStamp = m.TimeStamp,
                        IsFromBot = m.IsFromBot
                    }).ToList()
                });
            }

            var finalResult = Result<List<ConversationDTO>>.Ok(result);
            await cacheService.SetAsync(cacheKey, finalResult, TimeSpan.FromHours(6));
            return finalResult;
        }
    }
}