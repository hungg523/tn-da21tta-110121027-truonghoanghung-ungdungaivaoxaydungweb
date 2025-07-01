import { Component, OnInit, OnDestroy, ViewChild, ElementRef, AfterViewInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ChatService, ChatMessage, Conversation } from '../../services/chat.service';
import { Subscription } from 'rxjs';
import { trigger, transition, style, animate } from '@angular/animations';
import { v4 as uuidv4 } from 'uuid';

const GUEST_STATUS_KEY = 'chat_guest_status';
const GUEST_STATUS_EXPIRE_KEY = 'chat_guest_status_expire';

@Component({
  selector: 'app-chat-box',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <!-- Nút pop-up -->
    <button *ngIf="!isExpanded" class="chat-popup-btn" (click)="toggleChat()">
      <i class="fas fa-comments"></i>
    </button>

    <!-- Khung chat -->
    <div class="chat-box" [class.expanded]="isExpanded" *ngIf="isExpanded">
      <div class="chat-header" (click)="toggleChat()">
        <div class="chat-title">
          <i class="fas fa-comments"></i>
          <span>Chat với chúng tôi</span>
        </div>
        <div class="chat-controls">
          <i class="fas fa-times"></i>
        </div>
      </div>
      
      <div class="chat-body" #chatBody>
        <div class="welcome-message" *ngIf="!conversation">
          <p>HưngApple xin chào!</p>
          <p>Chúng tôi có thể giúp gì cho bạn!</p>
        </div>
        
        <div class="messages" *ngIf="conversation">
          <div *ngFor="let message of conversation.messages; let i = index" 
               class="message animate-message" 
               [class.user-message]="message.senderType === 'user'"
               [class.bot-message]="message.senderType === 'bot' && message.isFromBot"
               [class.admin-message]="message.senderType === 'admin'"
               [@messageAnimation]>
            <div class="message-avatar">
              <ng-container *ngIf="message.senderType === 'user'">
                <img [src]="getUserAvatar()" [alt]="getUserName()" />
              </ng-container>
              <ng-container *ngIf="message.senderType === 'bot'">
                <img [src]="botAvatar" alt="Bot" />
              </ng-container>
              <ng-container *ngIf="message.senderType === 'admin'">
                <img [src]="adminAvatar" alt="Admin" />
              </ng-container>
            </div>
            <div class="message-content">
              <div class="message-username">
                <ng-container *ngIf="message.senderType === 'user'">{{getUserName()}}</ng-container>
                <ng-container *ngIf="message.senderType === 'bot' && message.isFromBot">Bot</ng-container>
                <ng-container *ngIf="message.senderType === 'admin'">Quản trị viên</ng-container>
              </div>
              <div class="message-text" [innerHTML]="formatMessage(message.message)"></div>
              <div class="message-time">{{formatTime(message.timeStamp)}}</div>
            </div>
          </div>
          <!-- Hiển thị trạng thái đang soạn tin -->
          <div class="typing-indicator" *ngIf="isTyping">
            <div class="dot"></div><div class="dot"></div><div class="dot"></div>
            <span>Đang soạn tin...</span>
          </div>
        </div>
      </div>

      <div class="chat-footer" *ngIf="isExpanded">
        <form (ngSubmit)="sendMessage()" #messageForm="ngForm">
          <div class="input-group">
            <input type="text" 
                   [(ngModel)]="newMessage" 
                   name="message" 
                   placeholder="Nhập tin nhắn của bạn..."
                   [disabled]="isSending || !isSignalRConnected">
            <button type="submit" [disabled]="!newMessage.trim() || isSending || !isSignalRConnected">
              <i class="fas fa-paper-plane" *ngIf="!isSending"></i>
              <span class="loader" *ngIf="isSending"></span>
            </button>
          </div>
        </form>
      </div>
    </div>
  `,
  animations: [
    trigger('messageAnimation', [
      transition(':enter', [
        style({ opacity: 0, transform: 'translateY(20px)' }),
        animate('0.3s cubic-bezier(.25,.8,.25,1)', style({ opacity: 1, transform: 'none' }))
      ])
    ])
  ],
  styles: [`
    .chat-popup-btn {
      position: fixed;
      bottom: 100px;
      right: 30px;
      width: 60px;
      height: 60px;
      border-radius: 50%;
      background: linear-gradient(135deg, #007AFF 60%, #00c6ff 100%);
      color: white;
      border: none;
      box-shadow: 0 4px 24px rgba(0,0,0,0.18);
      font-size: 2rem;
      z-index: 1100;
      display: flex;
      align-items: center;
      justify-content: center;
      cursor: pointer;
      transition: background 0.3s, box-shadow 0.3s;
    }
    .chat-popup-btn:hover {
      background: linear-gradient(135deg, #0056b3 60%, #00aaff 100%);
      box-shadow: 0 8px 32px rgba(0,0,0,0.22);
    }
    .chat-box {
      position: fixed;
      bottom: 90px;
      right: 20px;
      width: 370px;
      background: #fff;
      border-radius: 20px;
      box-shadow: 0 8px 32px rgba(0,0,0,0.18);
      z-index: 1200;
      transition: all 0.3s cubic-bezier(.25,.8,.25,1);
      display: flex;
      flex-direction: column;
      max-height: 540px;
      overflow: hidden;
    }
    .chat-header {
      padding: 18px 20px;
      background: linear-gradient(135deg, #007AFF 60%, #00c6ff 100%);
      color: white;
      border-radius: 20px 20px 0 0;
      cursor: pointer;
      display: flex;
      justify-content: space-between;
      align-items: center;
      font-size: 1.1rem;
      font-weight: 600;
      box-shadow: 0 2px 8px rgba(0,0,0,0.04);
    }
    .chat-title {
      display: flex;
      align-items: center;
      gap: 10px;
    }
    .chat-body {
      flex: 1;
      overflow-y: auto;
      padding: 18px 16px 10px 16px;
      display: flex;
      flex-direction: column;
      gap: 12px;
      background: #f7fafd;
    }
    .welcome-message {
      text-align: center;
      color: #666;
      padding: 20px;
    }
    .messages {
      display: flex;
      flex-direction: column;
      gap: 14px;
    }
    .message {
      max-width: 85%;
      padding: 13px 18px 13px 14px;
      border-radius: 18px;
      position: relative;
      display: flex;
      gap: 10px;
      align-items: flex-end;
      background: #fff;
      box-shadow: 0 2px 8px rgba(0,0,0,0.04);
      opacity: 1;
      animation: fadeInUp 0.3s;
      min-height: 48px;
    }
    .user-message {
      align-self: flex-end;
      background: linear-gradient(135deg, #007AFF 60%, #00c6ff 100%);
      color: white;
      flex-direction: row-reverse;
      box-shadow: 0 2px 8px rgba(0,122,255,0.08);
    }
    .bot-message {
      align-self: flex-start;
      background: #f0f4ff;
      color: #333;
    }
    .admin-message {
      align-self: flex-start;
      background: #ffe0b2;
      color: #333;
    }
    .message-avatar {
      width: 36px;
      height: 36px;
      border-radius: 50%;
      overflow: hidden;
      flex-shrink: 0;
      display: flex;
      align-items: center;
      justify-content: center;
      background: #eee;
      box-shadow: 0 1px 4px rgba(0,0,0,0.06);
      margin-bottom: 2px;
      border: 2px solid #fff;
    }
    .message-avatar img {
      width: 100%;
      height: 100%;
      object-fit: cover;
      border-radius: 50%;
    }
    .message-content {
      display: flex;
      flex-direction: column;
      gap: 2px;
      min-width: 0;
    }
    .message-username {
      font-size: 0.93em;
      font-weight: 600;
      margin-bottom: 2px;
      color: #007AFF;
      text-transform: capitalize;
    }
    .user-message .message-username {
      color: #fff;
    }
    .bot-message .message-username {
      color: #007AFF;
    }
    .admin-message .message-username {
      color: #e65100;
    }
    .message-text {
      word-break: break-word;
      font-size: 1.07em;
      line-height: 1.5;
      margin-bottom: 2px;
    }
    .message-time {
      font-size: 0.78em;
      opacity: 0.6;
      margin-top: 2px;
      text-align: right;
      font-weight: 400;
    }
    .chat-footer {
      padding: 16px 18px;
      border-top: 1px solid #eee;
      background: #f7fafd;
    }
    .input-group {
      display: flex;
      gap: 10px;
    }
    input {
      flex: 1;
      padding: 12px 14px;
      border: 1px solid #ddd;
      border-radius: 8px;
      outline: none;
      font-size: 1em;
      background: #fff;
      transition: border 0.2s;
    }
    input:focus {
      border: 1.5px solid #007AFF;
    }
    button[type="submit"] {
      padding: 0 18px;
      background: linear-gradient(135deg, #007AFF 60%, #00c6ff 100%);
      color: white;
      border: none;
      border-radius: 8px;
      cursor: pointer;
      transition: background 0.3s;
      font-size: 1.3em;
      display: flex;
      align-items: center;
      justify-content: center;
      min-width: 48px;
      min-height: 48px;
      position: relative;
    }
    button[type="submit"]:hover {
      background: linear-gradient(135deg, #0056b3 60%, #00aaff 100%);
    }
    button[type="submit"]:disabled {
      background: #ccc;
      cursor: not-allowed;
    }
    .loader {
      border: 3px solid #f3f3f3;
      border-top: 3px solid #007AFF;
      border-radius: 50%;
      width: 18px;
      height: 18px;
      animation: spin 1s linear infinite;
      display: inline-block;
    }
    .chat-body::-webkit-scrollbar {
      width: 6px;
    }
    .chat-body::-webkit-scrollbar-track {
      background: #f1f1f1;
    }
    .chat-body::-webkit-scrollbar-thumb {
      background: #b3d1ff;
      border-radius: 3px;
    }
    .chat-body::-webkit-scrollbar-thumb:hover {
      background: #007AFF;
    }
    .typing-indicator {
      display: flex;
      align-items: center;
      gap: 6px;
      margin-left: 8px;
      margin-bottom: 6px;
      color: #888;
      font-size: 0.98em;
      font-style: italic;
      min-height: 24px;
    }
    .typing-indicator .dot {
      width: 7px;
      height: 7px;
      background: #b3d1ff;
      border-radius: 50%;
      display: inline-block;
      margin-right: 2px;
      animation: blink 1.4s infinite both;
    }
    .typing-indicator .dot:nth-child(1) { animation-delay: 0s; }
    .typing-indicator .dot:nth-child(2) { animation-delay: 0.2s; }
    .typing-indicator .dot:nth-child(3) { animation-delay: 0.4s; }
    @keyframes fadeInUp {
      from { opacity: 0; transform: translateY(20px); }
      to { opacity: 1; transform: none; }
    }
    @keyframes spin {
      0% { transform: rotate(0deg); }
      100% { transform: rotate(360deg); }
    }
    @keyframes blink {
      0%, 80%, 100% { opacity: 0.2; }
      40% { opacity: 1; }
    }
    .product-link {
      display: inline-block;
      background: #e3f0ff;
      color: #007aff !important;
      font-weight: 600;
      padding: 6px 16px;
      border-radius: 18px;
      text-decoration: none !important;
      margin: 4px 0;
      transition: background 0.2s, color 0.2s, box-shadow 0.2s;
      box-shadow: 0 2px 8px rgba(0,122,255,0.07);
      border: 1px solid #b3d1ff;
      font-size: 1em;
    }
    .product-link:hover {
      background: #007aff;
      color: #fff !important;
      box-shadow: 0 4px 16px rgba(0,122,255,0.13);
      text-decoration: none;
    }
  `]
})
export class ChatBoxComponent implements OnInit, OnDestroy, AfterViewInit {
  @ViewChild('chatBody') chatBody!: ElementRef;
  
  isExpanded = false;
  conversation: Conversation | null = null;
  newMessage = '';
  isSending = false;
  isTyping = false;
  isSignalRConnected = false;
  private messageSubscription: Subscription | null = null;
  private typingTimeout: any;
  private guestStatus: string | null = null;

  // Avatar mẫu
  defaultUserAvatar = 'https://ui-avatars.com/api/?name=User&background=007AFF&color=fff';
  botAvatar = 'https://cdn-icons-png.flaticon.com/512/4712/4712035.png';
  adminAvatar = 'https://cdn-icons-png.flaticon.com/512/3135/3135715.png';
  GUEST_AVATAR = 'https://ui-avatars.com/api/?name=Guest&background=cccccc&color=222222';

  constructor(private chatService: ChatService) {}

  ngOnInit() {
    this.messageSubscription = this.chatService.messageReceived.subscribe(message => {
      if (this.conversation) {
        // Kiểm tra xem tin nhắn nhận được có trùng với tin nhắn pending không
        const existingIndex = this.conversation.messages.findIndex(
          (msg) => msg.pending && msg.message === message.message && msg.senderId === message.senderId
        );

        if (existingIndex !== -1) {
          // Nếu trùng, thay thế tin nhắn pending bằng tin nhắn nhận được từ server
          this.conversation.messages[existingIndex] = message;
        } else {
          // Nếu không trùng (tin nhắn từ người khác), thêm vào cuối
          this.conversation.messages.push(message);
        }
        this.scrollToBottom();
      }
    });
    this.chatService.signalRConnected.subscribe(() => {
      this.isSignalRConnected = true;
    });
  }

  ngAfterViewInit() {
    this.scrollToBottom();
  }

  ngOnDestroy() {
    if (this.messageSubscription) {
      this.messageSubscription.unsubscribe();
    }
    this.chatService.disconnect();
    if (this.typingTimeout) clearTimeout(this.typingTimeout);
  }

  toggleChat() {
    this.isExpanded = !this.isExpanded;
    if (this.isExpanded) {
      this.loadConversationAndConnect();
      setTimeout(() => this.scrollToBottom(), 100);
    } else {
      this.chatService.disconnect();
    }
  }

  private getGuestStatus(): string {
    const now = Date.now();
    const expire = localStorage.getItem(GUEST_STATUS_EXPIRE_KEY);
    const status = localStorage.getItem(GUEST_STATUS_KEY);
    if (status && expire && now < +expire) {
      return status;
    }
    // Tạo mới
    const guid = uuidv4();
    // Lấy substring(8), chỉ giữ lại chữ và số
    let sub = guid.substring(8).toLowerCase().replace(/[^a-z0-9]/g, '');
    const guestStatus = `guest_${sub}`;
    localStorage.setItem(GUEST_STATUS_KEY, guestStatus);
    localStorage.setItem(GUEST_STATUS_EXPIRE_KEY, (now + 24*60*60*1000).toString());
    return guestStatus;
  }

  private isGuest(): boolean {
    // Nếu không có conversation hoặc userId = 0 thì là guest
    return !this.conversation || !this.conversation.userId;
  }

  loadConversationAndConnect() {
    let status: string | undefined = undefined;
    if (!this.conversation || !this.conversation.userId) {
      status = this.getGuestStatus();
      this.guestStatus = status;
    }
    this.chatService.getConversation(status).subscribe({
      next: (response) => {
        if (response.isSuccess && response.data) {
          this.conversation = response.data;
          this.chatService.startConnection(this.conversation.conversationId);
          setTimeout(() => this.scrollToBottom(), 100);
        }
      },
      error: (error) => {
        console.error('Error loading conversation:', error);
      }
    });
  }

  loadConversation() {
    this.chatService.getConversation().subscribe({
      next: (response) => {
        if (response.isSuccess && response.data) {
          this.conversation = response.data;
          setTimeout(() => this.scrollToBottom(), 100);
        }
      },
      error: (error) => {
        console.error('Error loading conversation:', error);
      }
    });
  }

  sendMessage() {
    if (!this.newMessage.trim() || this.isSending || !this.isSignalRConnected) return;

    const tempMessage: ChatMessage = {
      chatId: 0,
      conversationId: this.conversation ? this.conversation.conversationId : 0,
      senderType: 'user' as 'user',
      senderId: this.conversation ? this.conversation.userId : 0,
      message: this.newMessage,
      timeStamp: new Date().toISOString(),
      isFromBot: false,
      pending: true // Đánh dấu là tin nhắn pending
    };

    // Hiển thị ngay trên UI
    if (this.conversation) {
      this.conversation.messages.push(tempMessage);
      this.scrollToBottom();
    }

    const sendingText = this.newMessage;
    this.newMessage = '';
    this.isSending = true;

    let status: string | undefined = undefined;
    if (this.isGuest()) {
      status = this.guestStatus || this.getGuestStatus();
    }

    this.chatService.sendMessage(sendingText, status).subscribe({
      next: (response) => {
        this.isSending = false;
        // Tin nhắn đã được xử lý hiển thị trong messageReceived subscribe
      },
      error: (error) => {
        this.isSending = false;
        console.error('Error sending message:', error);
        // (Tùy chọn) Cập nhật trạng thái gửi thất bại cho tin nhắn pending tương ứng
      }
    });
  }

  showTyping() {
    this.isTyping = true;
    if (this.typingTimeout) clearTimeout(this.typingTimeout);
    this.typingTimeout = setTimeout(() => {
      this.isTyping = false;
    }, 2000);
  }

  private scrollToBottom() {
    if (this.chatBody) {
      const element = this.chatBody.nativeElement;
      setTimeout(() => {
        element.scrollTop = element.scrollHeight;
      }, 50);
    }
  }

  formatTime(timestamp: string): string {
    const date = new Date(timestamp);
    return date.toLocaleTimeString('vi-VN', {
      hour: '2-digit',
      minute: '2-digit'
    });
  }

  formatMessage(message: string): string {
    let formatted = message
      .replace(/\n/g, '<br>')
      .replace(/\*\*(.*?)\*\*/g, '<strong>$1</strong>')
      .replace(/\*(.*?)\*/g, '<em>$1</em>');

    formatted = formatted.replace(/https?:\/\/[^\s]+\/api\/v1\/product-variant\/get-detail\/(\d+)/g, (match, id) => {
      return `<a href="/product-detail/${id}" class="product-link">🔗 Xem chi tiết sản phẩm</a>`;
    });

    formatted = formatted.replace(/(https?:\/\/[^\s<]+)/g, (url) => {
      if (url.includes('/api/v1/product-variant/get-detail/')) return url;
      return `<a href="${url}" target="_blank" rel="noopener" style="color:#007aff;text-decoration:underline;">${url}</a>`;
    });

    return formatted;
  }

  getUserAvatar(): string {
    if (!this.conversation) return this.defaultUserAvatar;
    if (!this.conversation.userId || this.conversation.status?.toLowerCase().startsWith('guest')) {
      return this.GUEST_AVATAR;
    }
    return this.conversation.avartar || this.defaultUserAvatar;
  }

  getUserName(): string {
    if (!this.conversation) return 'Bạn';
    if (!this.conversation.userId || this.conversation.status?.toLowerCase().startsWith('guest')) {
      return this.conversation.userName || 'Guest';
    }
    return this.conversation.userName || 'Bạn';
  }
}