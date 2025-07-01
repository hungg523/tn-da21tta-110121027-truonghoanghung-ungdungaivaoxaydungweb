import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';

export interface UserProfile {
  id: number;
  email: string;
  username: string;
  gender?: string;
  dateOfBirth?: string;
  avatar?: string;
}

export interface UserResponse {
  isSuccess: boolean;
  statusCode: number;
  data: UserProfile;
}

export interface UpdateProfileRequest {
  userName: string;
  gender: string;
  dateOfBirth: string;
  imageData?: string;
}

export interface UpdateProfileResponse {
  isSuccess: boolean;
  statusCode: number;
}

@Injectable({
  providedIn: 'root'
})
export class UserService {
  constructor(private http: HttpClient) {}
  private apiUrl = `${environment.apiUrlV1}/user`;

  getProfile(): Observable<UserResponse> {
    return this.http.get<UserResponse>(`${this.apiUrl}/get-profile`);
  }

  updateProfile(request: UpdateProfileRequest): Observable<UpdateProfileResponse> {
    return this.http.put<UpdateProfileResponse>(`${this.apiUrl}/update-profile`, request);
  }
}