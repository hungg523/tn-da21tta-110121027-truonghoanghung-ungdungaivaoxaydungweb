import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';

export interface UserAddress {
  addressId: number;
  fullName: string;
  phoneNumber: string;
  address: string;
}

export interface UserAddressResponse {
  isSuccess: boolean;
  statusCode: number;
  data: UserAddress[];
}

export interface CreateAddressRequest {
  firstName: string;
  lastName: string;
  addressLine: string;
  phoneNumber: string;
  province: string;
  district: string;
  ward: string;
}

export interface AddressResponse {
  isSuccess: boolean;
  statusCode: number;
}

export interface UpdateAddressRequest {
  firstName: string;
  lastName: string;
  addressLine: string;
  phoneNumber: string;
  province: string;
  district: string;
  ward: string;
}

@Injectable({
  providedIn: 'root'
})
export class UserAddressService {
  private apiUrl = `${environment.apiUrlV1}/user-address`;

  constructor(private http: HttpClient) {}

  getAllAddresses(): Observable<UserAddressResponse> {
    return this.http.get<UserAddressResponse>(`${this.apiUrl}/get-all`);
  }

  createAddress(request: CreateAddressRequest): Observable<AddressResponse> {
    return this.http.post<AddressResponse>(`${this.apiUrl}/create`, request);
  }

  updateAddress(id: string, request: UpdateAddressRequest): Observable<AddressResponse> {
    return this.http.put<AddressResponse>(`${this.apiUrl}/update/${id}`, request);
  }

  deleteAddress(id: number): Observable<AddressResponse> {
    return this.http.delete<AddressResponse>(`${this.apiUrl}/delete/${id}`);
  }
} 