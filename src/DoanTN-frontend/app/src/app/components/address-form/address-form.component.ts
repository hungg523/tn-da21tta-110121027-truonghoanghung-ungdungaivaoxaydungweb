import { Component, OnInit, Output, EventEmitter, Inject, PLATFORM_ID, Input } from '@angular/core';
import { CommonModule, isPlatformBrowser } from '@angular/common';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { FormGroup, FormBuilder, Validators } from '@angular/forms';
import { LocationService } from '../../services/location.service';
import { UserAddressService, CreateAddressRequest } from '../../services/address.service';
import { NotificationService } from '../../services/notification.service';

@Component({
  selector: 'app-address-form',
  standalone: true,
  imports: [CommonModule, FormsModule, ReactiveFormsModule],
  template: `
    <div class="address-form-overlay" (click)="onOverlayClick($event)">
      <div class="address-form-container">
        <div class="address-form-header">
          <h3>{{editMode ? 'Chỉnh sửa địa chỉ' : 'Thêm địa chỉ mới'}}</h3>
          <button class="close-button" (click)="closeForm()">
            <i class="fa fa-times"></i>
          </button>
        </div>

        <form [formGroup]="addressForm" (ngSubmit)="onSubmit()" class="address-form">
          <div class="form-row">
            <div class="form-group">
              <label for="firstName">Họ</label>
              <input type="text" id="firstName" formControlName="firstName" class="form-input">
              <div *ngIf="addressForm.get('firstName')?.invalid && addressForm.get('firstName')?.touched" class="error-message">
                Vui lòng nhập họ
              </div>
            </div>

            <div class="form-group">
              <label for="lastName">Tên</label>
              <input type="text" id="lastName" formControlName="lastName" class="form-input">
              <div *ngIf="addressForm.get('lastName')?.invalid && addressForm.get('lastName')?.touched" class="error-message">
                Vui lòng nhập tên
              </div>
            </div>
          </div>

          <div class="form-group">
            <label for="phoneNumber">Số điện thoại</label>
            <input type="tel" id="phoneNumber" formControlName="phoneNumber" class="form-input">
            <div *ngIf="addressForm.get('phoneNumber')?.invalid && addressForm.get('phoneNumber')?.touched" class="error-message">
              Vui lòng nhập số điện thoại hợp lệ
            </div>
          </div>

          <div class="form-group">
            <label for="addressLine">Địa chỉ cụ thể</label>
            <input type="text" id="addressLine" formControlName="addressLine" class="form-input">
            <div *ngIf="addressForm.get('addressLine')?.invalid && addressForm.get('addressLine')?.touched" class="error-message">
              Vui lòng nhập địa chỉ cụ thể
            </div>
          </div>

          <div class="form-group">
            <label for="province">Tỉnh/Thành phố</label>
            <select id="province" formControlName="province" class="form-input" (change)="onProvinceChange()">
              <option value="">Chọn Tỉnh/Thành phố</option>
              <option *ngFor="let province of provinces" [value]="province.code">
                {{province.name}}
              </option>
            </select>
            <div *ngIf="isLoadingProvinces" class="loading-text">
              <i class="fa fa-spinner fa-spin"></i> Đang tải...
            </div>
          </div>

          <div class="form-group">
            <label for="district">Quận/Huyện</label>
            <select id="district" formControlName="district" class="form-input" (change)="onDistrictChange()" [disabled]="!addressForm.get('province')?.value">
              <option value="">Chọn Quận/Huyện</option>
              <option *ngFor="let district of districts" [value]="district.code">
                {{district.name}}
              </option>
            </select>
            <div *ngIf="isLoadingDistricts" class="loading-text">
              <i class="fa fa-spinner fa-spin"></i> Đang tải...
            </div>
          </div>

          <div class="form-group">
            <label for="ward">Phường/Xã</label>
            <select id="ward" formControlName="ward" class="form-input" [disabled]="!addressForm.get('district')?.value">
              <option value="">Chọn Phường/Xã</option>
              <option *ngFor="let ward of wards" [value]="ward.code">
                {{ward.name}}
              </option>
            </select>
            <div *ngIf="isLoadingWards" class="loading-text">
              <i class="fa fa-spinner fa-spin"></i> Đang tải...
            </div>
          </div>

          <div class="form-actions">
            <button type="button" class="btn-cancel" (click)="closeForm()">
              <i class="fa fa-times"></i>
              Hủy
            </button>
            <button type="submit" class="btn-submit" [disabled]="!addressForm.valid || isSubmitting">
              <i class="fa fa-check"></i>
              <span *ngIf="!isSubmitting">{{editMode ? 'Cập nhật' : 'Thêm địa chỉ'}}</span>
              <span *ngIf="isSubmitting">
                <i class="fa fa-spinner fa-spin"></i>
                Đang xử lý...
              </span>
            </button>
          </div>
        </form>
      </div>
    </div>
  `,
  styles: [`
    .address-form-overlay {
      position: fixed;
      top: 0;
      left: 0;
      right: 0;
      bottom: 0;
      background-color: rgba(0, 0, 0, 0.5);
      display: flex;
      align-items: center;
      justify-content: center;
      z-index: 1000;
    }

    .address-form-container {
      background: white;
      border-radius: 12px;
      padding: 24px;
      width: 100%;
      max-width: 500px;
      max-height: 90vh;
      overflow-y: auto;
      box-shadow: 0 4px 20px rgba(0, 0, 0, 0.15);
    }

    .address-form-header {
      display: flex;
      justify-content: space-between;
      align-items: center;
      margin-bottom: 24px;
      padding-bottom: 16px;
      border-bottom: 1px solid #eee;
    }

    .address-form-header h3 {
      margin: 0;
      font-size: 20px;
      color: #2c3e50;
      font-weight: 600;
    }

    .close-button {
      background: none;
      border: none;
      font-size: 20px;
      color: #666;
      cursor: pointer;
      padding: 8px;
      border-radius: 50%;
      transition: all 0.3s ease;
    }

    .close-button:hover {
      background-color: #f8f9fa;
      color: #333;
    }

    .address-form {
      display: flex;
      flex-direction: column;
      gap: 12px;
    }

    .form-row {
      display: grid;
      grid-template-columns: 1fr 1fr;
      gap: 12px;
    }

    .form-group {
      display: flex;
      flex-direction: column;
      gap: 4px;
    }

    .form-group label {
      font-size: 14px;
      color: #4a4a4a;
      font-weight: 500;
      margin-bottom: 2px;
    }

    .form-input {
      padding: 8px 12px;
      border: 1px solid #dce0e4;
      border-radius: 8px;
      font-size: 14px;
      transition: all 0.3s ease;
      background-color: #fff;
    }

    .form-input:focus {
      outline: none;
      border-color: #3498db;
      box-shadow: 0 0 0 3px rgba(52, 152, 219, 0.1);
    }

    .form-input:disabled {
      background-color: #f8f9fa;
      cursor: not-allowed;
    }

    .error-message {
      color: #e74c3c;
      font-size: 12px;
      margin-top: 4px;
    }

    .loading-text {
      color: #666;
      font-size: 13px;
      margin-top: 4px;
    }

    .form-actions {
      display: flex;
      justify-content: flex-end;
      gap: 12px;
      margin-top: 24px;
      padding-top: 20px;
      border-top: 1px solid #eee;
    }

    .btn-cancel, .btn-submit {
      padding: 12px 24px;
      border-radius: 8px;
      font-size: 14px;
      font-weight: 500;
      display: flex;
      align-items: center;
      gap: 8px;
      transition: all 0.3s ease;
      cursor: pointer;
    }

    .btn-cancel {
      background-color: #fff;
      border: 1px solid #dce0e4;
      color: #666;
    }

    .btn-cancel:hover {
      background-color: #f8f9fa;
      border-color: #cbd5e1;
      color: #333;
    }

    .btn-submit {
      background: linear-gradient(135deg, #3498db, #2980b9);
      border: none;
      color: white;
    }

    .btn-submit:hover:not(:disabled) {
      background: linear-gradient(135deg, #2980b9, #2472a4);
      transform: translateY(-1px);
      box-shadow: 0 4px 12px rgba(52, 152, 219, 0.2);
    }

    .btn-submit:disabled {
      opacity: 0.7;
      cursor: not-allowed;
    }

    @media (max-width: 576px) {
      .form-row {
        grid-template-columns: 1fr;
      }

      .address-form-container {
        margin: 15px;
        padding: 16px;
      }

      .btn-cancel, .btn-submit {
        padding: 10px 20px;
      }
    }
  `]
})
export class AddressFormComponent implements OnInit {
  @Input() addressId?: string;
  @Input() editData?: any;
  @Output() close = new EventEmitter<void>();
  @Output() addressAdded = new EventEmitter<void>();
  @Output() addressUpdated = new EventEmitter<void>();

  editMode = false;

  addressForm: FormGroup;
  provinces: any[] = [];
  districts: any[] = [];
  wards: any[] = [];
  isSubmitting = false;
  isLoadingProvinces = false;
  isLoadingDistricts = false;
  isLoadingWards = false;

  constructor(
    private fb: FormBuilder,
    private locationService: LocationService,
    private userAddressService: UserAddressService,
    private notificationService: NotificationService,
    @Inject(PLATFORM_ID) private platformId: Object
  ) {
    this.addressForm = this.fb.group({
      firstName: ['', Validators.required],
      lastName: ['', Validators.required],
      phoneNumber: ['', [Validators.required, Validators.pattern('^[0-9]{10}$')]],
      province: ['', Validators.required],
      district: ['', Validators.required],
      ward: ['', Validators.required],
      addressLine: ['', Validators.required]
    });
  }

  ngOnInit(): void {
    if (isPlatformBrowser(this.platformId)) {
      this.fetchProvinces();
      if (this.editData && this.addressId) {
        this.editMode = true;
        this.initEditForm();
      }
    }
  }

  private initEditForm(): void {
    if (this.editData) {
      this.addressForm.patchValue({
        firstName: this.editData.firstName,
        lastName: this.editData.lastName,
        phoneNumber: this.editData.phoneNumber,
        addressLine: this.editData.addressLine
      });

      // Tìm và set giá trị cho province
      this.locationService.getProvinces().subscribe({
        next: (provinces) => {
          this.provinces = provinces;
          const province = this.provinces.find(p => p.name === this.editData.province);
          if (province) {
            this.addressForm.patchValue({ province: province.code });
            this.onProvinceChange();
          }
        }
      });
    }
  }

  closeForm(): void {
    this.close.emit();
  }

  fetchProvinces(): void {
    this.isLoadingProvinces = true;
    this.provinces = [];
    
    this.locationService.getProvinces().subscribe({
      next: (data) => {
        console.log('Provinces loaded:', data);
        this.provinces = data;
        this.isLoadingProvinces = false;
      },
      error: (error) => {
        console.error('Error loading provinces:', error);
        this.isLoadingProvinces = false;
      }
    });
  }

  onProvinceChange(): void {
    const provinceCode = this.addressForm.get('province')?.value;
    if (provinceCode) {
      this.isLoadingDistricts = true;
      this.districts = [];
      this.wards = [];
      this.addressForm.patchValue({ district: '', ward: '' });

      this.locationService.getDistricts(provinceCode).subscribe({
        next: (data) => {
          console.log('Districts loaded:', data);
          this.districts = data.districts || [];
          this.isLoadingDistricts = false;
        },
        error: (error) => {
          console.error('Error loading districts:', error);
          this.isLoadingDistricts = false;
        }
      });
    } else {
      this.districts = [];
      this.wards = [];
      this.addressForm.patchValue({ district: '', ward: '' });
    }
  }

  onDistrictChange(): void {
    const districtCode = this.addressForm.get('district')?.value;
    if (districtCode) {
      this.isLoadingWards = true;
      this.wards = [];
      this.addressForm.patchValue({ ward: '' });

      this.locationService.getWards(districtCode).subscribe({
        next: (data) => {
          console.log('Wards loaded:', data);
          this.wards = data.wards || [];
          this.isLoadingWards = false;
        },
        error: (error) => {
          console.error('Error loading wards:', error);
          this.isLoadingWards = false;
        }
      });
    } else {
      this.wards = [];
      this.addressForm.patchValue({ ward: '' });
    }
  }

  onSubmit(): void {
    if (this.addressForm.valid) {
      this.isSubmitting = true;
      const formValue = this.addressForm.value;
      
      const province = this.provinces.find(p => String(p.code) === String(formValue.province));
      const district = this.districts.find(d => String(d.code) === String(formValue.district));
      const ward = this.wards.find(w => String(w.code) === String(formValue.ward));

      const request = {
        firstName: formValue.firstName,
        lastName: formValue.lastName,
        phoneNumber: formValue.phoneNumber,
        province: province?.name || '',
        district: district?.name || '',
        ward: ward?.name || '',
        addressLine: formValue.addressLine
      };

      if (this.editMode && this.addressId) {
        this.userAddressService.updateAddress(this.addressId, request).subscribe({
          next: (response) => {
            if (response.isSuccess) {
              this.addressUpdated.emit();
              this.closeForm();
              this.notificationService.show('Cập nhật địa chỉ thành công');
            }
            this.isSubmitting = false;
          },
          error: () => {
            this.isSubmitting = false;
            this.notificationService.show('Có lỗi xảy ra khi cập nhật địa chỉ', 'error');
          }
        });
      } else {
        this.userAddressService.createAddress(request).subscribe({
          next: (response) => {
            if (response.isSuccess) {
              this.addressAdded.emit();
              this.closeForm();
              this.notificationService.show('Thêm địa chỉ thành công');
            }
            this.isSubmitting = false;
          },
          error: () => {
            this.isSubmitting = false;
            this.notificationService.show('Có lỗi xảy ra khi thêm địa chỉ', 'error');
          }
        });
      }
    }
  }

  onOverlayClick(event: MouseEvent): void {
    if (event.target === event.currentTarget) {
      this.closeForm();
    }
  }
} 