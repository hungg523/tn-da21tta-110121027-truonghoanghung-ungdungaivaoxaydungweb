import { environment } from '../environments/environment';
import { authInterceptor } from '@/interceptors/auth.interceptor';
import * as signalR from "@microsoft/signalr";

// Interface cho tin nhắn
export interface ChatMessage {
  chatId: number;
  conversationId: number;
  senderType: 'user' | 'bot' | 'admin';
  senderId: number | null;
  message: string;
  timeStamp: string;
  isFromBot: boolean;
}

// Interface cho cuộc hội thoại
export interface Conversation {
  conversationId: number;
  userId: number;
  userName: string;
  avartar: string;
  status: 'pending' | 'handled';
  isBotHandled: boolean;
  createdAt: string;
  messages: ChatMessage[];
}

// Interface cho response API
interface ApiResponse<T> {
  isSuccess: boolean;
  statusCode: number;
  data: T;
}

class ChatService {
    private apiUrlV1 = `${environment.apiUrlV1}/chat`;
  private hubConnection: signalR.HubConnection | null = null;
  private messageCallbacks: ((message: ChatMessage) => void)[] = [];
  private conversationCallbacks: ((conversation: Conversation) => void)[] = [];
  private statusCallbacks: ((data: { conversationId: string, status: string, timeStamp: string }) => void)[] = [];
  private currentConversationId: string | number | null = null;

  constructor() {}

  private async initializeSignalR() {
    console.log('Initializing SignalR connection...');
    if (this.hubConnection) {
      console.log('Stopping existing connection...');
      await this.hubConnection.stop();
    }

    this.hubConnection = new signalR.HubConnectionBuilder()
      .withUrl(`https://localhost:7001/chathub`)
      .withAutomaticReconnect()
      .build();

    // Đăng ký callback khi kết nối lại thành công
    this.hubConnection.onreconnected(() => {
      console.log('SignalR reconnected, re-registering callbacks');
      this.registerAllCallbacks();
      if (this.currentConversationId) {
        this.hubConnection?.invoke('JoinConversation', `Conversation-${this.currentConversationId}`);
      }
    });

    this.registerAllCallbacks();

    try {
      await this.hubConnection.start();
      console.log('SignalR connection started successfully');
    } catch (err) {
      console.error('Error starting SignalR connection:', err);
    }
  }

  // Hàm đăng ký lại toàn bộ callback
  private registerAllCallbacks() {
    // Xóa các callback cũ để tránh lặp
    this.hubConnection?.off('ReceiveMessage');
    this.hubConnection?.off('NewConversation');
    this.hubConnection?.off('ConversationStatusChanged');

    // Đăng ký duy nhất 1 lần cho mỗi sự kiện
    this.hubConnection?.on('ReceiveMessage', (data: any) => {
        console.log('Received message:', data);
        const message: ChatMessage = {
          chatId: data.messageId,
          conversationId: data.conversationId,
          senderType: data.senderType,
          senderId: data.senderId,
          message: data.content || data.message || "",
          timeStamp: data.timeStamp,
          isFromBot: !data.isFromAdmin
        };
        this.messageCallbacks.forEach(cb => cb(message));
    });
    this.hubConnection?.on('NewConversation', (data: any) => {
        console.log('New conversation:', data);
        this.conversationCallbacks.forEach(cb => cb(data));
    });
    this.hubConnection?.on('ConversationStatusChanged', (data: any) => {
        console.log('Conversation status changed:', data);
        this.statusCallbacks.forEach(cb => cb(data));
    });
  }

  private async ensureConnection() {
    if (!this.hubConnection || this.hubConnection.state === signalR.HubConnectionState.Disconnected) {
      console.log('No active or disconnected connection, initializing new connection...');
      await this.initializeSignalR();
    }
  }

  // New method to start/ensure connection and join a conversation
  async startOrJoinConnection(conversationId: string | number) {
    this.currentConversationId = conversationId;
    console.log('Ensuring connection and joining conversation:', conversationId);
    await this.ensureConnection();
    if (this.hubConnection && this.hubConnection.state === signalR.HubConnectionState.Connected) {
      try {
        await this.hubConnection.invoke("JoinConversation", `Conversation-${conversationId}`);
        console.log('Successfully joined conversation:', conversationId);
      } catch (err) {
        console.error('Error joining conversation:', err);
      }
    }
  }

  // Tham gia vào một cuộc hội thoại
  async joinConversation(conversationId: string | number) {
    console.log('Joining conversation:', conversationId);
    await this.ensureConnection();
    if (this.hubConnection && this.hubConnection.state === signalR.HubConnectionState.Connected) {
      try {
        await this.hubConnection.invoke("JoinConversation", `Conversation-${conversationId}`);
        console.log('Successfully joined conversation:', conversationId);
      } catch (err) {
        console.error('Error joining conversation:', err);
      }
    }
  }

  // Rời khỏi một cuộc hội thoại
  async leaveConversation(conversationId: number) {
    console.log('Leaving conversation:', conversationId);
    if (this.hubConnection) {
      try {
        await this.hubConnection.invoke("LeaveConversation", conversationId.toString());
        await this.hubConnection.stop();
        this.hubConnection = null;
        console.log('Successfully left conversation:', conversationId);
      } catch (err) {
        console.error('Error leaving conversation:', err);
      }
    }
  }

  // Gửi tin nhắn qua SignalR
  async sendMessage(conversationId: number, message: string) {
    console.log('Sending message to conversation:', conversationId, message);
    await this.ensureConnection();
    if (this.hubConnection) {
      try {
        await this.hubConnection.invoke("SendMessage", 
          conversationId.toString(),
          message,
          "admin",
          null
        );
        console.log('Message sent successfully');
      } catch (err) {
        console.error('Error sending message:', err);
      }
    }
  }

  // Cập nhật trạng thái cuộc hội thoại
  async updateConversationStatus(conversationId: number, status: 'pending' | 'handled') {
    console.log('Updating conversation status:', conversationId, status);
    await this.ensureConnection();
    if (this.hubConnection) {
      try {
        await this.hubConnection.invoke("UpdateConversationStatus", 
          conversationId.toString(),
          status
        );
        console.log('Conversation status updated successfully');
      } catch (err) {
        console.error('Error updating conversation status:', err);
      }
    }
  }

  // Đăng ký lắng nghe tin nhắn mới
  onNewMessage(callback: (message: ChatMessage) => void) {
    console.log('Registering new message callback');
    this.messageCallbacks.push(callback);
    if (this.hubConnection) {
      this.hubConnection.on('ReceiveMessage', (data: any) => {
        console.log('Received message:', data);
        const message: ChatMessage = {
          chatId: data.messageId,
          conversationId: data.conversationId,
          senderType: data.senderType,
          senderId: data.senderId,
          message: data.content || data.message || "",
          timeStamp: data.timestamp,
          isFromBot: !data.isFromAdmin
        };
        callback(message);
      });
    }
  }

  // Đăng ký lắng nghe cuộc hội thoại mới
  onNewConversation(callback: (conversation: Conversation) => void) {
    console.log('Registering new conversation callback');
    this.conversationCallbacks.push(callback);
    if (this.hubConnection) {
      this.hubConnection.on('NewConversation', (data: any) => {
        console.log('New conversation:', data);
        callback(data);
      });
    }
  }

  // Đăng ký lắng nghe thay đổi trạng thái cuộc hội thoại
  onConversationStatusChanged(callback: (data: { conversationId: string, status: string, timeStamp: string }) => void) {
    console.log('Registering conversation status changed callback');
    this.statusCallbacks.push(callback);
    if (this.hubConnection) {
      this.hubConnection.on('ConversationStatusChanged', (data: any) => {
        console.log('Conversation status changed:', data);
        callback(data);
      });
    }
  }

  // Hủy đăng ký lắng nghe
  offNewMessage(callback: (message: ChatMessage) => void) {
    this.messageCallbacks = this.messageCallbacks.filter(cb => cb !== callback);
    if (this.hubConnection) {
      this.hubConnection.off('ReceiveMessage', callback);
    }
  }

  offNewConversation(callback: (conversation: Conversation) => void) {
    this.conversationCallbacks = this.conversationCallbacks.filter(cb => cb !== callback);
    if (this.hubConnection) {
      this.hubConnection.off('NewConversation', callback);
    }
  }

  offConversationStatusChanged(callback: (data: { conversationId: string, status: string, timeStamp: string }) => void) {
    this.statusCallbacks = this.statusCallbacks.filter(cb => cb !== callback);
    if (this.hubConnection) {
      this.hubConnection.off('ConversationStatusChanged', callback);
    }
  }

  // Lấy danh sách các cuộc hội thoại cần xử lý
  async getPendingConversations(): Promise<Conversation[]> {
    const request = new Request(`${this.apiUrlV1}/admin/pending`, {
      method: 'GET',
      headers: {
        'Content-Type': 'application/json',
      },
      credentials: 'include',
    });

    const response = await authInterceptor(request);
    if (!response.ok) {
      throw new Error('Không thể lấy danh sách cuộc hội thoại');
    }

    const data: ApiResponse<Conversation[]> = await response.json();
    return data.data || [];
  }

  // Trả lời tin nhắn
  async replyToConversation(conversationId: number, message: string): Promise<ChatMessage> {
    const request = new Request(`${this.apiUrlV1}/admin/reply`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
      },
      credentials: 'include',
      body: JSON.stringify({
        conversationId,
        message,
      }),
    });

    const response = await authInterceptor(request);
    if (!response.ok) {
      throw new Error('Không thể gửi tin nhắn');
    }

    const data: ApiResponse<ChatMessage> = await response.json();
    return data.data;
  }

  // Bật/tắt bot chat
  async toggleBotChat(conversationId: number, isBotEnabled: boolean): Promise<void> {
    const request = new Request(`${this.apiUrlV1}/bot/toggle`, {
      method: 'PUT',
      headers: {
        'Content-Type': 'application/json',
      },
      credentials: 'include',
      body: JSON.stringify({
        conversationId,
        isBotEnabled,
      }),
    });

    const response = await authInterceptor(request);
    if (!response.ok) {
      throw new Error('Không thể bật/tắt bot chat');
    }
  }
}

export const chatService = new ChatService();