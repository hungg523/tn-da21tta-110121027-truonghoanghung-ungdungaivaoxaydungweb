"use client"

import { useEffect, useState, useRef } from "react"
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card"
import { chatService, Conversation, ChatMessage } from "@/services/chat.service"
import { format } from "date-fns"
import { vi } from "date-fns/locale"
import { Avatar, AvatarFallback, AvatarImage } from "@/components/ui/avatar"
import { Button } from "@/components/ui/button"
import { Input } from "@/components/ui/input"
import { ScrollArea } from "@/components/ui/scroll-area"
import { Badge } from "@/components/ui/badge"
import { Send, Bot } from "lucide-react"
import { Switch } from "@/components/ui/switch"
import { toast } from "sonner"

// Đặt avatar mặc định cho guest
const GUEST_AVATAR = 'https://ui-avatars.com/api/?name=Guest&background=cccccc&color=222222';

function formatMessage(message: string | undefined | null): string {
  if (!message) return "";

  const lines = message.split('\n');
  let count = 1;
  const formatted = lines.map(line => {
    if (/^\s*[\*-]\s?/.test(line)) {
      const content = line.replace(/^\s*[\*-]\s?/, '');
      return `${count++}. ${content}`;
    }
    return line;
  }).join('\n');

  let html = formatted
    .replace(/\n/g, '<br>')
    .replace(/\*\*(.*?)\*\*/g, '<strong>$1</strong>')
    .replace(/\*(.*?)\*/g, '<em>$1</em>');

  html = html.replace(
    /(https?:\/\/[^\s<]+)/g,
    (url) => `<a href="${url}" target="_blank" rel="noopener noreferrer" style="color:#2563eb;text-decoration:underline;">${shortenLink(url)}</a>`
  );

  return html;
}

function shortenLink(url: string): string {
  if (url.includes('/product-variant/get-detail/')) return 'Xem chi tiết sản phẩm';
  try {
    const u = new URL(url);
    return u.hostname;
  } catch {
    return url;
  }
}

export default function ChatPage() {
  const [conversations, setConversations] = useState<Conversation[]>([])
  const [selectedConversationId, setSelectedConversationId] = useState<string | number | null>(null)
  const [newMessage, setNewMessage] = useState("")
  const [isLoading, setIsLoading] = useState(true)
  const messagesEndRef = useRef<HTMLDivElement>(null)

  // Load danh sách cuộc hội thoại
  const loadConversations = async () => {
    try {
      setIsLoading(true)
      const data = await chatService.getPendingConversations()
      setConversations(data)
      if (data.length > 0 && !selectedConversationId) {
        setSelectedConversationId(data[0].conversationId)
        chatService.startOrJoinConnection(Number(data[0].conversationId))
      }
    } catch (error) {
      console.error("Error loading conversations:", error)
    } finally {
      setIsLoading(false)
    }
  }

  // Xử lý tin nhắn mới từ SignalR
  const handleNewMessage = (message: ChatMessage) => {
    let msgConvId = String(message.conversationId);
    if (msgConvId.startsWith('Conversation-')) {
      msgConvId = msgConvId.replace('Conversation-', '');
    }
    setConversations(prevConversations => {
      return prevConversations.map(conv => {
        if (String(conv.conversationId) === msgConvId) {
          return {
            ...conv,
            messages: [...conv.messages, message]
          };
        }
        return conv;
      });
    });
    // Tự động scroll xuống cuối khi nhận tin nhắn mới
    setTimeout(() => {
      messagesEndRef.current?.scrollIntoView({ behavior: "smooth" });
    }, 100);
  }

  // Xử lý cuộc hội thoại mới từ SignalR
  const handleNewConversation = (conversation: Conversation) => {
    setConversations(prev => [conversation, ...prev])
  }

  // Gửi tin nhắn
  const handleSendMessage = async () => {
    if (!newMessage.trim() || !selectedConversationId) return;

    try {
      await chatService.replyToConversation(Number(selectedConversationId), newMessage);
      setNewMessage("");
    } catch (error) {
      console.error("Error sending message:", error);
    }
  }

  // Tự động cuộn xuống tin nhắn mới nhất
  const scrollToBottom = () => {
    messagesEndRef.current?.scrollIntoView({ behavior: "smooth" })
  }

  // Xử lý bật/tắt bot
  const handleToggleBot = async (isEnabled: boolean) => {
    if (!selectedConversationId) return;

    try {
      await chatService.toggleBotChat(Number(selectedConversationId), isEnabled);
      
      // Cập nhật trạng thái bot trong conversation
      setConversations(prev => {
        return prev.map(conv => {
          if (String(conv.conversationId) === String(selectedConversationId)) {
            return {
              ...conv,
              isBotHandled: isEnabled
            };
          }
          return conv;
        });
      });

      toast.success(isEnabled ? "Đã bật bot chat" : "Đã tắt bot chat");
    } catch (error) {
      console.error("Error toggling bot:", error);
      toast.error("Không thể bật/tắt bot chat");
    }
  };

  useEffect(() => {
    loadConversations()

    // Đăng ký lắng nghe SignalR
    chatService.onNewMessage(handleNewMessage)
    chatService.onNewConversation(handleNewConversation)

    // Cleanup khi component unmount
    return () => {
      chatService.offNewMessage(handleNewMessage)
      chatService.offNewConversation(handleNewConversation)
    }
  }, [])

  useEffect(() => {
    scrollToBottom()
  }, [selectedConversationId])

  const currentConv = conversations.find(conv => String(conv.conversationId) === String(selectedConversationId));

  return (
    <div className="h-[calc(100vh-4rem)] p-4">
      <div className="grid grid-cols-12 gap-4 h-full">
        {/* Danh sách cuộc hội thoại */}
        <Card className="col-span-3">
          <CardHeader>
            <CardTitle>Cuộc hội thoại</CardTitle>
          </CardHeader>
          <CardContent className="p-0">
            <ScrollArea className="h-[calc(100vh-12rem)]">
              {conversations.map((conv) => (
                <div
                  key={conv.conversationId}
                  className={`p-4 border-b cursor-pointer hover:bg-muted ${
                    selectedConversationId === conv.conversationId
                      ? "bg-muted"
                      : ""
                  }`}
                  onClick={() => {
                    setSelectedConversationId(conv.conversationId);
                    chatService.startOrJoinConnection(Number(conv.conversationId));
                  }}
                >
                  <div className="flex items-center gap-3">
                    <Avatar>
                      <AvatarImage src={String(conv.status)?.startsWith('guest') ? GUEST_AVATAR : conv.avartar} alt={String(conv.status)?.startsWith('guest') ? `guest_${conv.conversationId}` : conv.userName} />
                      <AvatarFallback>
                        {String(conv.status)?.startsWith('guest') ? 'G' : (conv.userName ? conv.userName.charAt(0) : null)}
                      </AvatarFallback>
                    </Avatar>
                    <div className="flex-1 min-w-0">
                      <div className="flex items-center justify-between">
                        <p className="font-medium truncate">
                          {String(conv.status)?.startsWith('guest') ? `guest_${conv.conversationId}` : conv.userName}
                        </p>
                        <span className="text-xs text-muted-foreground">
                          {format(new Date(conv.createdAt), "HH:mm")}
                        </span>
                      </div>
                      <p className="text-sm text-muted-foreground truncate">
                        {conv.messages[conv.messages.length - 1]?.message}
                      </p>
                    </div>
                    {conv.status === "pending" && (
                      <Badge variant="destructive">Mới</Badge>
                    )}
                  </div>
                </div>
              ))}
            </ScrollArea>
          </CardContent>
        </Card>

        {/* Khung chat */}
        <Card className="col-span-9">
          <CardHeader>
            <CardTitle>
              {selectedConversationId ? (
                <div className="flex items-center justify-between">
                  <div className="flex items-center gap-3">
                    <Avatar>
                      <AvatarImage
                        src={String(currentConv?.status)?.startsWith('guest') ? GUEST_AVATAR : currentConv?.avartar}
                        alt={String(currentConv?.status)?.startsWith('guest') ? `guest_${currentConv?.conversationId}` : currentConv?.userName}
                      />
                      <AvatarFallback>
                        {String(currentConv?.status)?.startsWith('guest')
                          ? 'G'
                          : currentConv?.userName
                          ? currentConv.userName.charAt(0)
                          : null}
                      </AvatarFallback>
                    </Avatar>
                    <div>
                      <p className="font-bold text-2xl">
                        {String(currentConv?.status)?.startsWith('guest')
                          ? `guest_${currentConv?.conversationId}`
                          : currentConv?.userName}
                      </p>
                      <span className="text-sm text-muted-foreground">
                        {currentConv?.createdAt && !isNaN(new Date(currentConv.createdAt).getTime()) &&
                          format(new Date(currentConv.createdAt), "HH:mm dd/MM/yyyy", { locale: vi })}
                      </span>
                    </div>
                  </div>
                  <div className="flex items-center gap-2">
                    <Bot className="h-5 w-5 text-muted-foreground" />
                    <Switch
                      checked={currentConv?.isBotHandled}
                      onCheckedChange={handleToggleBot}
                    />
                  </div>
                </div>
              ) : (
                "Chọn cuộc hội thoại"
              )}
            </CardTitle>
          </CardHeader>
          <CardContent className="p-0">
            <div className="flex flex-col h-[calc(100vh-12rem)]">
              {/* Khu vực tin nhắn */}
              <ScrollArea className="flex-1 p-4">
                {currentConv?.messages.map((msg, idx) => (
                  <div
                    key={msg.chatId && msg.chatId !== 0 ? msg.chatId : `${msg.senderId}-${msg.timeStamp}-${idx}`}
                    className={`flex mb-4 ${
                      msg.senderType === "admin" ? "justify-end" : "justify-start"
                    }`}
                  >
                    <div className={`flex gap-2 items-center ${msg.senderType === "admin" ? "flex-row-reverse" : ""}`}>
                      <Avatar className={`h-8 w-8 ${msg.senderType === "admin" ? "bg-primary" : msg.senderType === "bot" ? "bg-primary/10" : ""}`}>
                        {msg.senderType === "user" && (
                          <>
                            <AvatarImage src={currentConv.avartar} alt={currentConv.userName} />
                            <AvatarFallback>{currentConv.userName.charAt(0)}</AvatarFallback>
                          </>
                        )}
                        {msg.senderType === "admin" && (
                          <AvatarFallback>
                            <svg
                              xmlns="http://www.w3.org/2000/svg"
                              viewBox="0 0 24 24"
                              fill="none"
                              stroke="currentColor"
                              strokeWidth="2"
                              strokeLinecap="round"
                              strokeLinejoin="round"
                              className="h-5 w-5 text-primary-foreground"
                            >
                              <path d="M19 21v-2a4 4 0 0 0-4-4H9a4 4 0 0 0-4 4v2" />
                              <circle cx="12" cy="7" r="4" />
                            </svg>
                          </AvatarFallback>
                        )}
                        {msg.senderType === "bot" && (
                          <AvatarFallback>
                            <svg
                              xmlns="http://www.w3.org/2000/svg"
                              viewBox="0 0 24 24"
                              fill="none"
                              stroke="currentColor"
                              strokeWidth="2"
                              strokeLinecap="round"
                              strokeLinejoin="round"
                              className="h-5 w-5 text-primary"
                            >
                              <path d="M12 8V4H8" />
                              <rect width="16" height="12" x="4" y="8" rx="2" />
                              <path d="M2 14h2" />
                              <path d="M20 14h2" />
                              <path d="M15 13v2" />
                              <path d="M9 13v2" />
                            </svg>
                          </AvatarFallback>
                        )}
                      </Avatar>
                      <div
                        className={`max-w-[70%] rounded-lg p-3 ${
                          msg.senderType === "admin"
                            ? "bg-primary text-primary-foreground"
                            : msg.senderType === "bot"
                            ? "bg-muted"
                            : "bg-secondary"
                        }`}
                      >
                        <p className="whitespace-pre-wrap" dangerouslySetInnerHTML={{__html: formatMessage(msg.message)}} />
                        <span className="text-xs opacity-70">
                          {msg.timeStamp && !isNaN(new Date(msg.timeStamp).getTime())
                            ? format(new Date(msg.timeStamp), "HH:mm")
                            : ""}
                        </span>
                      </div>
                    </div>
                  </div>
                ))}
                <div ref={messagesEndRef} />
              </ScrollArea>

              {/* Khu vực nhập tin nhắn */}
              <div className="border-t p-4">
                <div className="flex gap-2">
                  <Input
                    placeholder="Nhập tin nhắn..."
                    value={newMessage}
                    onChange={(e) => setNewMessage(e.target.value)}
                    onKeyDown={(e) => {
                      if (e.key === "Enter" && !e.shiftKey) {
                        e.preventDefault()
                        handleSendMessage()
                      }
                    }}
                  />
                  <Button
                    onClick={handleSendMessage}
                    disabled={!newMessage.trim() || !selectedConversationId}
                  >
                    <Send className="h-4 w-4" />
                  </Button>
                </div>
              </div>
            </div>
          </CardContent>
        </Card>
      </div>
    </div>
  )
}
