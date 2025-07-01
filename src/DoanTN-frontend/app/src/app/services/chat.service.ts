import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, Subject } from 'rxjs';
import { environment } from '../../environments/environment';
import * as signalR from '@microsoft/signalr';

export interface ChatMessage {
  chatId: number;
  conversationId: number;
  senderType: 'user' | 'bot' | 'admin';
  senderId: number | null;
  message: string;
  timeStamp: string;
  isFromBot: boolean;
  pending?: boolean;
}

export interface Conversation {
  conversationId: number;
  userId: number;
  userName: string;
  avartar: string;
  status: string;
  isBotHandled: boolean;
  createdAt: string;
  messages: ChatMessage[];
}

export interface ChatResponse {
  isSuccess: boolean;
  statusCode: number;
  data: Conversation;
}

export interface MessageResponse {
  isSuccess: boolean;
  statusCode: number;
  data: ChatMessage;
}

@Injectable({
  providedIn: 'root'
})
export class ChatService {
  private apiUrl = `${environment.apiUrlV1}/chat`;
  private hubConnection: signalR.HubConnection;
  public messageReceived = new Subject<ChatMessage>();
  public signalRConnected = new Subject<void>();
  private currentConversationId: string | number | null = null;

  constructor(private http: HttpClient) {
    this.hubConnection = new signalR.HubConnectionBuilder()
      .withUrl(`https://localhost:7001/chathub`)
      .withAutomaticReconnect()
      .build();

    this.hubConnection.on('ReceiveMessage', (message: ChatMessage) => {
      this.messageReceived.next(message);
    });

    this.hubConnection.onreconnected(() => {
      if (this.currentConversationId) {
        this.hubConnection.invoke('JoinConversation', `Conversation-${this.currentConversationId}`);
      }
    });
  }

  startConnection(conversationId: string | number) {
    this.currentConversationId = conversationId;
    this.hubConnection
      .start()
      .then(() => {
        console.log('SignalR Connected');
        this.hubConnection.invoke('JoinConversation', `Conversation-${conversationId}`)
          .then(() => {
            this.signalRConnected.next();
          });
      })
      .catch(err => console.error('SignalR Connection Error:', err));
  }

  getConversation(status?: string): Observable<ChatResponse> {
    if (status) {
      return this.http.get<ChatResponse>(`${this.apiUrl}/conversation`, { params: { status } });
    }
    return this.http.get<ChatResponse>(`${this.apiUrl}/conversation`);
  }

  sendMessage(message: string, status?: string): Observable<MessageResponse> {
    const body: any = { message };
    if (status) body.status = status;
    return this.http.post<MessageResponse>(`${this.apiUrl}/message`, body);
  }

  disconnect() {
    if (this.hubConnection) {
      this.hubConnection.stop();
    }
  }
}