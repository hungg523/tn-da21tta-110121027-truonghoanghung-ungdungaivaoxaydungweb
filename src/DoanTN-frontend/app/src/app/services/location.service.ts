import { Injectable, Inject, PLATFORM_ID } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, catchError, map, of } from 'rxjs';
import { environment } from '../../environments/environment';

export interface Province {
  code: string;
  name: string;
  codename: string;
  division_type: string;
  phone_code: number;
}

export interface District {
  code: string;
  name: string;
  codename: string;
  division_type: string;
  province_code: string;
}

export interface Ward {
  code: string;
  name: string;
  codename: string;
  division_type: string;
  district_code: string;
}

@Injectable({
  providedIn: 'root'
})
export class LocationService {
  private apiUrl = `${environment.apiUrlV1}/user-address`;

  constructor(
    private http: HttpClient,
    @Inject(PLATFORM_ID) private platformId: Object
  ) {}

  getProvinces(): Observable<any[]> {
    return this.http.get<any[]>(`${this.apiUrl}/provinces`);
  }

  getDistricts(provinceCode: string): Observable<any> {
    return this.http.get<any>(`${this.apiUrl}/districts/${provinceCode}`);
  }

  getWards(districtCode: string): Observable<any> {
    return this.http.get<any>(`${this.apiUrl}/wards/${districtCode}`);
  }

  getLocationName(locations: any[], code: string): string {
    const location = locations.find(l => String(l.code) === String(code));
    return location ? location.name : '';
  }
} 