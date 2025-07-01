using AppleShop.Application.Requests.ChatManagement.Chat;
using AppleShop.Application.Requests.DTOs.ChatManagement.Chat;
using AppleShop.Domain.Abstractions.IRepositories.ChatManagement;
using AppleShop.Domain.Abstractions.IRepositories.UserManagement;
using AppleShop.Domain.Entities.ChatManagement;
using AppleShop.Share.Abstractions;
using AppleShop.Share.Shared;
using MediatR;

namespace AppleShop.Application.Handlers.ChatManagement.Chat
{
    public class GetConversationHandler : IRequestHandler<GetConversationRequest, Result<ConversationDTO>>
    {
        private readonly IConversationRepository conversationRepository;
        private readonly IChatMessageRepository chatMessageRepository;
        private readonly ICacheService cacheService;
        private readonly IUserRepository userRepository;
        private readonly IFileService fileService;

        public GetConversationHandler(IConversationRepository conversationRepository,
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

        public async Task<Result<ConversationDTO>> Handle(GetConversationRequest request, CancellationToken cancellationToken)
        {
            Conversations? conversation = new();
            var message = "Chào bạn, tôi là trợ lý ảo của Hưng Apple. Bạn cần hỗ trợ gì?";
            if (request.UserId is not null)
            {
                conversation = await conversationRepository.FindSingleAsync(x => x.UserId == request.UserId, false);
                if (conversation is null)
                {
                    conversation = new Conversations
                    {
                        UserId = request.UserId,
                        IsBotHandled = true,
                        CreatedAt = DateTime.Now
                    };
                    conversationRepository.Create(conversation);
                    await conversationRepository.SaveChangesAsync(cancellationToken);

                    var chatMessage = new ChatMessages
                    {
                        ConversationId = conversation.Id,
                        SenderType = "bot",
                        Message = message,
                        TimeStamp = DateTime.Now,
                        IsFromBot = true,
                    };
                    chatMessageRepository.Create(chatMessage);
                    await chatMessageRepository.SaveChangesAsync(cancellationToken);
                }
            }
            else
            {
                var now = DateTime.Now;
                var guestConversations = conversationRepository.FindAll(x => x.Status != null && x.Status.StartsWith("guest") && x.CreatedAt.HasValue && (now - x.CreatedAt.Value).TotalDays >= 1, true).OrderByDescending(x => x.CreatedAt).ToList();

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

                conversation = await conversationRepository.FindSingleAsync(x => x.Status == request.Status, true);
                if (conversation is null)
                {
                    conversation = new Conversations
                    {
                        Status = request.Status,
                        IsBotHandled = true,
                        CreatedAt = DateTime.Now
                    };
                    conversationRepository.Create(conversation);
                    await conversationRepository.SaveChangesAsync(cancellationToken);

                    var chatMessage = new ChatMessages
                    {
                        ConversationId = conversation.Id,
                        SenderType = "bot",
                        Message = message,
                        TimeStamp = DateTime.Now,
                        IsFromBot = true,
                    };
                    chatMessageRepository.Create(chatMessage);
                    await chatMessageRepository.SaveChangesAsync(cancellationToken);
                }
            }
            var cacheKey = $"conversation_{conversation.Id}";
            var cachedResult = await cacheService.GetAsync<Result<ConversationDTO>>(cacheKey);
            if (cachedResult is not null) return cachedResult;

            var user = await userRepository.FindByIdAsync(request.UserId, false);

            var messages = chatMessageRepository.FindAll(x => x.ConversationId == conversation.Id).OrderBy(x => x.TimeStamp).ToList();

            var result = new ConversationDTO
            {
                ConversationId = conversation.Id,
                UserId = conversation.UserId ?? null,
                UserName = user is null ? "Khách" : user.Username,
                Avartar = user is null ? "" : (user.ImageUrl.StartsWith("https") ? user.ImageUrl : fileService.GetFullPathFileServer(user.ImageUrl)),
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
            };

            var finalResult = Result<ConversationDTO>.Ok(result);
            await cacheService.SetAsync(cacheKey, finalResult, TimeSpan.FromHours(6));
            return finalResult;
        }
    }
}