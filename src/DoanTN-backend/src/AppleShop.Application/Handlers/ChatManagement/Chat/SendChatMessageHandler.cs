using AppleShop.Application.Requests.ChatManagement.Chat;
using AppleShop.Application.Requests.DTOs.ChatManagement.Chat;
using AppleShop.Application.Services;
using AppleShop.Domain.Abstractions.IRepositories.ChatManagement;
using AppleShop.Domain.Abstractions.IRepositories.UserManagement;
using AppleShop.Share.Abstractions;
using AppleShop.Share.Shared;
using GenerativeAI;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Threading.RateLimiting;

namespace AppleShop.Application.Handlers.ChatManagement.Chat
{
    public class SendChatMessageHandler : IRequestHandler<SendChatMessageRequest, Result<ChatMessageDTO>>
    {
        private readonly IConversationRepository conversationRepository;
        private readonly IChatMessageRepository chatMessageRepository;
        private readonly IUserRepository userRepository;
        private readonly ICacheService cacheService;
        private readonly IConfiguration configuration;
        private readonly IChatService chatService;
        private readonly ProductService productService;
        private readonly INotificationService notificationService;

        private static readonly RateLimiter rateLimiter = new TokenBucketRateLimiter(new TokenBucketRateLimiterOptions
        {
            TokenLimit = 10,
            QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
            QueueLimit = 2,
            ReplenishmentPeriod = TimeSpan.FromMinutes(1),
            TokensPerPeriod = 10,
            AutoReplenishment = true
        });
        private static readonly string[] RemovePhrases = new[]
        {
            "tôi muốn mua", "tôi muốn hỏi", "tôi muốn đặt", "tôi muốn xem", "bạn có bán", "có bán", "giá", "cho hỏi", "mua", "bán", "sản phẩm", "có", "không", "bao nhiêu", "là gì", "ở đâu", "shop", "apple", "shop apple", "user"
        };

        public SendChatMessageHandler(IConversationRepository conversationRepository,
                                    IChatMessageRepository chatMessageRepository,
                                    IUserRepository userRepository,
                                    ICacheService cacheService,
                                    IConfiguration configuration,
                                    IChatService chatService,
                                    ProductService productService,
                                    INotificationService notificationService)
        {
            this.conversationRepository = conversationRepository;
            this.chatMessageRepository = chatMessageRepository;
            this.userRepository = userRepository;
            this.cacheService = cacheService;
            this.configuration = configuration;
            this.chatService = chatService;
            this.productService = productService;
            this.notificationService = notificationService;
        }

        public async Task<Result<ChatMessageDTO>> Handle(SendChatMessageRequest request, CancellationToken cancellationToken)
        {
            try
            {
                using var lease = await rateLimiter.AcquireAsync(1, cancellationToken);
                if (!lease.IsAcquired) return new Result<ChatMessageDTO> { Message = "Bạn đã gửi quá nhiều tin nhắn. Vui lòng đợi một lát." };

                var user = await userRepository.FindByIdAsync(request.UserId, false);
                bool isGuest = user is null;
                Domain.Entities.ChatManagement.Conversations conversation = null;
                if (isGuest)
                {
                    conversation = await conversationRepository.FindSingleAsync(x => x.Status == request.Status, true);
                }
                else
                {
                    conversation = await conversationRepository.FindSingleAsync(x => x.UserId == user.Id, true);
                }

                var userMessage = new Domain.Entities.ChatManagement.ChatMessages
                {
                    ConversationId = conversation.Id,
                    SenderType = isGuest ? "guest" : "user",
                    SenderId = isGuest ? null : request.UserId,
                    Message = request.Message,
                    TimeStamp = DateTime.Now,
                    IsFromBot = false
                };
                chatMessageRepository.Create(userMessage);
                await chatMessageRepository.SaveChangesAsync(cancellationToken);

                await chatService.SendMessage($"Conversation-{conversation.Id}", request.Message, "user", isGuest ? null : request.UserId);

                if (isGuest)
                {
                    var recentMessagesGuest = await chatMessageRepository.FindAll(x => x.ConversationId == conversation.Id)
                        .OrderByDescending(x => x.TimeStamp)
                        .Take(5)
                        .OrderBy(x => x.TimeStamp)
                        .ToListAsync(cancellationToken);
                    var contextPromptGuest = string.Join("\n", recentMessagesGuest.Select(m => $"{m.SenderType}: {m.Message}"));
                    var botResponseGuest = await GenerateBotResponse(contextPromptGuest, cancellationToken);
                    var botMessageGuest = new Domain.Entities.ChatManagement.ChatMessages
                    {
                        ConversationId = conversation.Id,
                        SenderType = "bot",
                        Message = botResponseGuest,
                        TimeStamp = DateTime.Now,
                        IsFromBot = true
                    };
                    chatMessageRepository.Create(botMessageGuest);
                    await chatMessageRepository.SaveChangesAsync(cancellationToken);
                    await chatService.SendMessage($"Conversation-{conversation.Id}", botResponseGuest, "bot", (int?)null);
                    conversation.IsBotHandled = true;
                    conversationRepository.Update(conversation);
                    await conversationRepository.SaveChangesAsync(cancellationToken);
                    await cacheService.RemoveAsync($"conversation_{conversation.Id}");
                    return new Result<ChatMessageDTO>
                    {
                        IsSuccess = true,
                        Data = new ChatMessageDTO
                        {
                            ChatId = userMessage.Id,
                            ConversationId = userMessage.ConversationId,
                            SenderType = userMessage.SenderType,
                            SenderId = userMessage.SenderId,
                            Message = userMessage.Message,
                            TimeStamp = userMessage.TimeStamp,
                            IsFromBot = userMessage.IsFromBot
                        }
                    };
                }

                if (!conversation.IsBotHandled.Value)
                {
                    await cacheService.RemoveAsync($"conversation_{conversation.Id}");
                    await cacheService.RemoveAsync($"pending_conversations");
                    return new Result<ChatMessageDTO>
                    {
                        IsSuccess = true,
                        Data = new ChatMessageDTO
                        {
                            ChatId = userMessage.Id,
                            ConversationId = userMessage.ConversationId,
                            SenderType = userMessage.SenderType,
                            SenderId = userMessage.SenderId,
                            Message = userMessage.Message,
                            TimeStamp = userMessage.TimeStamp,
                            IsFromBot = userMessage.IsFromBot
                        }
                    };
                }

                var shouldTransferToAdmin = await ShouldTransferToAdmin(request.Message);
                if (shouldTransferToAdmin)
                {
                    conversation.Status = "pending";
                    conversation.IsBotHandled = false;
                    conversationRepository.Update(conversation);
                    await conversationRepository.SaveChangesAsync(cancellationToken);

                    var adminTransferMessage = await GetAdminTransferMessage();
                    var adminBotMessage = new Domain.Entities.ChatManagement.ChatMessages
                    {
                        ConversationId = conversation.Id,
                        SenderType = "bot",
                        Message = adminTransferMessage,
                        TimeStamp = DateTime.Now,
                        IsFromBot = true
                    };
                    chatMessageRepository.Create(adminBotMessage);
                    await chatMessageRepository.SaveChangesAsync(cancellationToken);

                    await chatService.SendMessage($"Conversation-{conversation.Id}", adminTransferMessage, "bot", (int?)null);
                    await chatService.UpdateConversationStatus($"Conversation-{conversation.Id}", "pending");

                    await cacheService.RemoveAsync($"conversation_{conversation.Id}");
                    await cacheService.RemoveAsync("pending_conversations");

                    await notificationService.NotifyNewPendingMessage(conversation.Id.Value, request.Message);
                }

                var recentMessages = await chatMessageRepository.FindAll(x => x.ConversationId == conversation.Id)
                    .OrderByDescending(x => x.TimeStamp)
                    .Take(5)
                    .OrderBy(x => x.TimeStamp)
                    .ToListAsync(cancellationToken);

                var contextPrompt = string.Join("\n", recentMessages.Select(m => $"{m.SenderType}: {m.Message}"));

                var botResponse = await GenerateBotResponse(contextPrompt, cancellationToken);
                var botMessage = new Domain.Entities.ChatManagement.ChatMessages
                {
                    ConversationId = conversation.Id,
                    SenderType = "bot",
                    Message = botResponse,
                    TimeStamp = DateTime.Now,
                    IsFromBot = true
                };
                chatMessageRepository.Create(botMessage);
                await chatMessageRepository.SaveChangesAsync(cancellationToken);

                await chatService.SendMessage($"Conversation-{conversation.Id}", botResponse, "bot", (int?)null);

                conversation.IsBotHandled = true;
                conversationRepository.Update(conversation);
                await conversationRepository.SaveChangesAsync(cancellationToken);

                await cacheService.RemoveAsync($"conversation_{conversation.Id}");

                return new Result<ChatMessageDTO>
                {
                    IsSuccess = true,
                    Data = new ChatMessageDTO
                    {
                        ChatId = userMessage.Id,
                        ConversationId = userMessage.ConversationId,
                        SenderType = userMessage.SenderType,
                        SenderId = userMessage.SenderId,
                        Message = userMessage.Message,
                        TimeStamp = userMessage.TimeStamp,
                        IsFromBot = userMessage.IsFromBot
                    }
                };
            }
            catch (Exception)
            {
                throw;
            }
        }

        private async Task<bool> ShouldTransferToAdmin(string message)
        {
            var keywords = new[]
            {
                "khiếu nại", "phàn nàn", "không hài lòng", "thất vọng",
                "lỗi", "hỏng", "vấn đề", "sự cố",
                "hoàn tiền", "đổi trả", "bồi thường",
                "quá tệ", "tồi tệ", "không chấp nhận được",
                "giám đốc", "quản lý", "người phụ trách", "admin", "chủ", "chủ shop",
                "tôi muốn nói chuyện với người thật",
                "tôi cần hỗ trợ trực tiếp",
                "tôi không hài lòng với câu trả lời"
            };

            return keywords.Any(k => message.ToLower().Contains(k.ToLower()));
        }

        private async Task<string> GetAdminTransferMessage()
        {
            var messages = new[]
            {
                "Tôi hiểu rằng vấn đề này cần được xử lý bởi đội ngũ hỗ trợ của chúng tôi. Tôi sẽ chuyển tiếp yêu cầu của bạn ngay lập tức.",
                "Để đảm bảo bạn nhận được sự hỗ trợ tốt nhất, tôi sẽ kết nối bạn với đội ngũ chuyên môn của chúng tôi.",
                "Tôi đánh giá cao phản hồi của bạn. Để giải quyết vấn đề này một cách triệt để, tôi sẽ chuyển tiếp cho đội ngũ hỗ trợ của chúng tôi.",
                "Cảm ơn bạn đã chia sẻ. Để đảm bảo bạn nhận được sự hỗ trợ phù hợp nhất, tôi sẽ kết nối bạn với đội ngũ chuyên môn của AppleShop."
            };

            return messages[new Random().Next(messages.Length)];
        }

        private async Task<List<string>> ExtractProductKeywords(string query)
        {
            try
            {
                var apiKey = configuration["Gemini:ApiKey"];
                var client = new GenerativeModel(model: "gemini-2.0-flash", apiKey: apiKey);
                var prompt = $"Hãy trích xuất tất cả tên sản phẩm Apple mà người dùng muốn tìm trong câu sau: '{query}'. Trả về mỗi tên sản phẩm trên một dòng, không giải thích gì thêm.";
                var response = await client.GenerateContentAsync(prompt);
                var productNames = response.Text?.Split('\n').Select(x => x.Trim()).Where(x => !string.IsNullOrEmpty(x)).Distinct().ToList();
                if (productNames != null && productNames.Count > 0)
                {
                    return productNames;
                }
            }
            catch(Exception)
            {
                throw;
            }

            var keyword = query.ToLower();
            var extraRemove = new[] {
                "guest", "bot", "xin lỗi", "vui lòng", "phù hợp", "yêu cầu", "bạn", "tôi", "shop", "apple", "nhé", "cụ thể", "hơn", "với", "những", "nào", "tìm thấy", "không", "sản phẩm", "admin", "user", "được", "của", "cho", "cần", "giúp", "muốn", "mua", "bán", "giá", "liên hệ", "vui lòng", "vấn đề", "thông tin", "hỏi", "đặt", "xem", "có", "là", "ở đâu", "bao nhiêu", "là gì", "được không", "còn không", "còn hàng", "đánh giá", "chi tiết", "xem chi tiết"
            };
            foreach (var phrase in RemovePhrases.Concat(extraRemove).Distinct()) keyword = keyword.Replace(phrase, "");
            keyword = keyword.Replace(" và ", ",").Replace(" hoặc ", ",").Replace("/", ",").Replace("|", ",");
            keyword = System.Text.RegularExpressions.Regex.Replace(keyword, @"[^\p{L}\p{N}, ]", " ");
            keyword = System.Text.RegularExpressions.Regex.Replace(keyword, @"\s+", " ").Trim();
            var rawKeywords = keyword.Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(k => k.Trim())
                .Where(k => k.Length > 1)
                .ToList();
            var keywords = new List<string>();
            foreach (var k in rawKeywords)
            {
                var cleaned = k;
                cleaned = System.Text.RegularExpressions.Regex.Replace(cleaned, @"^\d+\s*", "");
                cleaned = cleaned.Trim();
                if (!string.IsNullOrEmpty(cleaned) && !keywords.Contains(cleaned))
                    keywords.Add(cleaned);
            }
            if (keywords.Count == 1)
            {
                var single = keywords[0];
                var parts = single.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                var grouped = string.Join(" ", parts.Distinct());
                keywords[0] = grouped;
            }
            return keywords;
        }

        private async Task<string> GetProductInformation(string query)
        {
            var normalizedQuery = System.Text.RegularExpressions.Regex.Replace(query.ToLower().Trim(), @"\s+", " ");
            var cacheKey = $"product_search_{normalizedQuery}";
            var cachedResult = await cacheService.GetAsync<string>(cacheKey);
            if (!string.IsNullOrEmpty(cachedResult)) return cachedResult;

            decimal? minPrice = null, maxPrice = null;
            var matchBelow = System.Text.RegularExpressions.Regex.Match(query, @"dưới\s*(\d+[.,]?\d*)\s*(tr|triệu|vnd|vnđ|k|nghìn|ngàn)?", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            if (matchBelow.Success)
            {
                var val = decimal.Parse(matchBelow.Groups[1].Value.Replace(",", "."));
                if (matchBelow.Groups[2].Value.StartsWith("tr")) val *= 1_000_000;
                if (matchBelow.Groups[2].Value.StartsWith("k") || matchBelow.Groups[2].Value.StartsWith("ngh")) val *= 1_000;
                maxPrice = val;
            }

            var matchAbove = System.Text.RegularExpressions.Regex.Match(query, @"(trên|từ)\s*(\d+[.,]?\d*)\s*(tr|triệu|vnd|vnđ|k|nghìn|ngàn)?", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            if (matchAbove.Success)
            {
                var val = decimal.Parse(matchAbove.Groups[2].Value.Replace(",", "."));
                if (matchAbove.Groups[3].Value.StartsWith("tr")) val *= 1_000_000;
                if (matchAbove.Groups[3].Value.StartsWith("k") || matchAbove.Groups[3].Value.StartsWith("ngh")) val *= 1_000;
                minPrice = val;
            }

            var matchRange = System.Text.RegularExpressions.Regex.Match(query, @"(từ|khoảng)\s*(\d+[.,]?\d*)\s*(tr|triệu|vnd|vnđ|k|nghìn|ngàn)?\s*(đến|-)\s*(\d+[.,]?\d*)\s*(tr|triệu|vnd|vnđ|k|nghìn|ngàn)?", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            if (matchRange.Success)
            {
                var val1 = decimal.Parse(matchRange.Groups[2].Value.Replace(",", "."));
                var val2 = decimal.Parse(matchRange.Groups[5].Value.Replace(",", "."));
                if (matchRange.Groups[3].Value.StartsWith("tr")) val1 *= 1_000_000;
                if (matchRange.Groups[3].Value.StartsWith("k") || matchRange.Groups[3].Value.StartsWith("ngh")) val1 *= 1_000;
                if (matchRange.Groups[6].Value.StartsWith("tr")) val2 *= 1_000_000;
                if (matchRange.Groups[6].Value.StartsWith("k") || matchRange.Groups[6].Value.StartsWith("ngh")) val2 *= 1_000;
                minPrice = val1;
                maxPrice = val2;
            }

            var keywords = await ExtractProductKeywords(query);
            if (keywords.Count == 0 && (minPrice.HasValue || maxPrice.HasValue))
            {
                keywords = new List<string>();
            }
            else if (keywords.Count == 0)
            {
                keywords.Add(query.ToLower());
            }

            var allProducts = await productService.GetAllProducts();
            var allFlashSaleProducts = await productService.GetFlashSaleProducts();

            var matchedProducts = allProducts.Data
                .Where(x =>
                    (keywords.Count == 0 || keywords.Any(kw => x.Name.ToLower().Contains(kw) || x.Description.ToLower().Contains(kw))) &&
                    (!minPrice.HasValue || (x.DiscountPrice ?? x.Price) >= minPrice) &&
                    (!maxPrice.HasValue || (x.DiscountPrice ?? x.Price) <= maxPrice)
                )
                .Distinct()
                .Take(5)
                .ToList();

            if (!matchedProducts.Any())
            {
                var apiKey = configuration["Gemini:ApiKey"];
                var client = new GenerativeModel(model: "gemini-2.0-flash", apiKey: apiKey);
                var prompt = $"Người dùng muốn tìm sản phẩm với nhu cầu: '{query}'. Hãy liệt kê tối đa 5 tên sản phẩm Apple phổ biến, mỗi tên trên 1 dòng, không giải thích gì thêm.";
                var geminiResponse = await client.GenerateContentAsync(prompt);
                var productNames = geminiResponse.Text?.Split('\n').Select(x => x.Trim()).Where(x => !string.IsNullOrEmpty(x)).ToList() ?? new List<string>();
                var geminiProducts = await productService.GetAllProducts();
                var geminiFlashSaleProducts = await productService.GetFlashSaleProducts();
                var smartMatched = geminiProducts.Data.Where(x => productNames.Any(name => x.Name.ToLower().Contains(name.ToLower()))).Take(5).ToList();
                if (!smartMatched.Any())
                {
                    return "Xin lỗi, tôi không tìm thấy sản phẩm phù hợp với yêu cầu của bạn. Bạn vui lòng nhập tên sản phẩm cụ thể hơn nhé!";
                }
                var smartResponse = "Tôi gợi ý một số sản phẩm phổ biến phù hợp với nhu cầu của bạn:\n\n";
                foreach (var product in smartMatched)
                {
                    var isFlashSale = geminiFlashSaleProducts.Data.Any(x => x.VariantId == product.VariantId);
                    var discount = isFlashSale ? Math.Round((1 - product.DiscountPrice.Value / product.Price.Value) * 100) : 0;
                    smartResponse += $"- {product.Name}\n";
                    smartResponse += $"  💰 Giá: {product.Price:N0} VNĐ\n";
                    if (isFlashSale) smartResponse += $"  🔥 Đang giảm giá {discount}% (còn {product.DiscountPrice:N0} VNĐ)\n";
                    smartResponse += $"  📦 Còn hàng: {product.ActualStock} sản phẩm\n";
                    if (product.AverageRating > 0) smartResponse += $"  ⭐ Đánh giá: {product.AverageRating}/5 ({product.TotalReviews} đánh giá)\n";
                    smartResponse += $"  🔗 Xem chi tiết: {configuration["ApiSettings:BaseUrl"]}/api/v1/product-variant/get-detail/{product.VariantId}\n\n";
                }
                await cacheService.SetAsync(cacheKey, smartResponse, TimeSpan.FromMinutes(5));
                return smartResponse;
            }

            var response = "Tôi tìm thấy một số sản phẩm phù hợp:\n\n";
            foreach (var product in matchedProducts)
            {
                var isFlashSale = allFlashSaleProducts.Data.Any(x => x.VariantId == product.VariantId);
                var discount = isFlashSale ? Math.Round((1 - product.DiscountPrice.Value / product.Price.Value) * 100) : 0;

                response += $"- {product.Name}\n";
                response += $"  💰 Giá: {product.Price:N0} VNĐ\n";
                if (isFlashSale) response += $"  🔥 Đang giảm giá {discount}% (còn {product.DiscountPrice:N0} VNĐ)\n";
                response += $"  📦 Còn hàng: {product.ActualStock} sản phẩm\n";
                if (product.AverageRating > 0) response += $"  ⭐ Đánh giá: {product.AverageRating}/5 ({product.TotalReviews} đánh giá)\n";
                response += $"  🔗 Xem chi tiết: {configuration["ApiSettings:BaseUrl"]}/api/v1/product-variant/get-detail/{product.VariantId}\n\n";
            }

            await cacheService.SetAsync(cacheKey, response, TimeSpan.FromMinutes(5));
            return response;
        }

        private async Task<string> GetFlashSaleInformation()
        {
            try
            {
                var allFlashSaleProducts = await productService.GetFlashSaleProducts();
                if (allFlashSaleProducts.Data is null || !allFlashSaleProducts.Data.Any()) return "Hiện tại không có sản phẩm nào trong chương trình Flash Sale.";

                var response = "🔥 FLASH SALE - ƯU ĐÃI SỐC 🔥\n\n";
                var feUrl = configuration["ApiSettings:FeUrl"] ?? "https://localhost:4200";
                foreach (var product in allFlashSaleProducts.Data.Take(5))
                {
                    var discount = Math.Round((1 - product.DiscountPrice.Value / product.Price.Value) * 100);
                    response += $"- {product.Name}\n";
                    response += $"  💰 Giá gốc: {product.Price:N0} VNĐ\n";
                    response += $"  🔥 Giảm {discount}% (còn {product.DiscountPrice:N0} VNĐ)\n";
                    response += $"  📦 Còn hàng: {product.ActualStock} sản phẩm\n";
                    response += $"  🔗 Xem chi tiết: {feUrl}/api/v1/product-variant/get-detail/{product.VariantId}\n\n";
                }

                return response;
            }
            catch
            {
                return "Xin lỗi, tôi không thể truy xuất thông tin Flash Sale lúc này.";
            }
        }

        private async Task<string> GetOrderProcessInfo()
        {
            return @"🎯 QUY TRÌNH ĐẶT HÀNG NHANH CHÓNG:

            1️. Lựa chọn sản phẩm yêu thích và thêm vào giỏ hàng
            2️. Xác nhận địa chỉ giao hàng
            3️. Chọn phương thức thanh toán (COD hoặc online)
            4️. Áp dụng mã giảm giá (nếu có)
            5️. Hoàn tất đơn hàng

            🚚 MIỄN PHÍ VẬN CHUYỂN cho đơn hàng từ 2.000.000 VNĐ

            Bạn cần hỗ trợ thêm về bước nào không?";
        }

        private async Task<string> GetReturnPolicyInfo()
        {
            return @"📦 CHÍNH SÁCH ĐỔI TRẢ:

            ⏰ Thời gian: 07 ngày kể từ khi nhận hàng
            ✅ Điều kiện: Sản phẩm còn nguyên vẹn, đầy đủ phụ kiện

            Các trường hợp được hỗ trợ:
            - Sản phẩm bị lỗi kỹ thuật
            - Sản phẩm bị hư hỏng khi vận chuyển
            - Giao sai sản phẩm/màu sắc
            - Thiếu phụ kiện

            Nếu bạn cần đổi trả/hoàn tiền, vui lòng cung cấp:
            1. Hình ảnh sản phẩm
            2. Video mở hộp (nếu có)
            3. Mô tả chi tiết vấn đề

            Bạn có cần hỗ trợ thêm về chính sách đổi trả không?";
        }

        public async Task<string> GenerateBotResponse(string contextPrompt, CancellationToken cancellationToken = default)
        {
            try
            {
                if (contextPrompt.ToLower().Contains("flash sale") ||
                    contextPrompt.ToLower().Contains("khuyến mãi") ||
                    contextPrompt.ToLower().Contains("giảm giá"))
                {
                    return await GetFlashSaleInformation();
                }

                if (contextPrompt.ToLower().Contains("sản phẩm") ||
                    contextPrompt.ToLower().Contains("giá") ||
                    contextPrompt.ToLower().Contains("mua") ||
                    contextPrompt.ToLower().Contains("có bán"))
                {
                    var lastUserMessage = contextPrompt.Split('\n').Reverse().FirstOrDefault(m => m.Trim().StartsWith("user:", StringComparison.OrdinalIgnoreCase) || m.Trim().StartsWith("guest:", StringComparison.OrdinalIgnoreCase));
                    if (!string.IsNullOrEmpty(lastUserMessage))
                    {
                        var msg = lastUserMessage.Contains(":") ? lastUserMessage.Substring(lastUserMessage.IndexOf(":") + 1).Trim() : lastUserMessage.Trim();
                        return await GetProductInformation(msg);
                    }

                    var lastMsg = contextPrompt.Split('\n').LastOrDefault();
                    if (!string.IsNullOrEmpty(lastMsg))
                    {
                        var msg = lastMsg.Contains(":") ? lastMsg.Substring(lastMsg.IndexOf(":") + 1).Trim() : lastMsg.Trim();
                        return await GetProductInformation(msg);
                    }

                    return await GetProductInformation("");
                }

                if (contextPrompt.ToLower().Contains("đặt hàng") ||
                    contextPrompt.ToLower().Contains("mua hàng") ||
                    contextPrompt.ToLower().Contains("thanh toán") ||
                    contextPrompt.ToLower().Contains("vận chuyển"))
                {
                    return await GetOrderProcessInfo();
                }

                if (contextPrompt.ToLower().Contains("đổi trả") ||
                    contextPrompt.ToLower().Contains("hoàn tiền") ||
                    contextPrompt.ToLower().Contains("bảo hành") ||
                    contextPrompt.ToLower().Contains("khiếu nại"))
                {
                    return await GetReturnPolicyInfo();
                }

                var apiKey = configuration["Gemini:ApiKey"];
                var client = new GenerativeModel(model: "gemini-2.0-flash", apiKey: apiKey);

                var prompt = $@"Bạn là trợ lý ảo của Hưng Apple - cửa hàng bán lẻ các sản phẩm Apple chính hãng tại Việt Nam.

                    Lịch sử hội thoại:
                    {contextPrompt}

                    Hướng dẫn trả lời:
                    1. Trả lời ngắn gọn, rõ ràng và thân thiện
                    2. Tập trung vào thông tin về sản phẩm, giá cả, chính sách bảo hành
                    3. Nếu không chắc chắn, đề nghị chuyển tiếp cho nhân viên hỗ trợ
                    4. Không trả lời các câu hỏi về chính trị, tôn giáo hoặc nội dung không phù hợp
                    5. Luôn giữ thái độ chuyên nghiệp và lịch sự
                    6. Nếu khách hàng hỏi về sản phẩm, hãy gợi ý các sản phẩm phù hợp
                    7. Nếu có chương trình khuyến mãi, hãy thông báo cho khách hàng
                    8. Nếu khách hàng hỏi về quy trình đặt hàng, hãy giải thích các bước đơn giản
                    9. Nếu khách hàng hỏi về chính sách đổi trả, hãy giải thích rõ ràng và hướng dẫn họ liên hệ admin nếu cần

                    Hãy trả lời câu hỏi của khách hàng một cách hữu ích nhất.";

                var response = await client.GenerateContentAsync(prompt, cancellationToken);

                return response.Text ?? "Xin lỗi, tôi không có câu trả lời phù hợp lúc này.";
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}