import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { FormGroup, FormBuilder, Validators } from '@angular/forms';
import { UserService, UserProfile, UpdateProfileRequest } from '../../services/user.service';
import { UserAddressService, UserAddress } from '../../services/address.service';
import { PageBannerComponent } from '../../components/page-banner/page-banner.component';
import { AddressFormComponent } from '../../components/address-form/address-form.component';
import { NotificationService } from '../../services/notification.service';
import Swal from 'sweetalert2';

@Component({
  selector: 'app-profile',
  standalone: true,
  imports: [
    CommonModule, 
    FormsModule, 
    ReactiveFormsModule, 
    PageBannerComponent,
    AddressFormComponent
  ],
  templateUrl: './profile.component.html',
  styleUrls: ['./profile.component.css']
})
export class ProfileComponent implements OnInit {
  userProfile: UserProfile | null = null;
  userAddresses: UserAddress[] = [];
  isLoading = true;
  error: string | null = null;
  isEditing = false;
  showAddressForm = false;
  updateForm: FormGroup;
  selectedImage: File | null = null;
  previewImage: string | null = null;
  selectedAddress: any = null;

  constructor(
    private userService: UserService,
    private userAddressService: UserAddressService,
    private fb: FormBuilder,
    private notificationService: NotificationService
  ) {
    this.updateForm = this.fb.group({
      userName: ['', Validators.required],
      gender: [''],
      dateOfBirth: [''],
      imageData: ['']
    });
  }

  ngOnInit(): void {
    this.loadUserProfile();
    this.loadUserAddresses();
  }

  loadUserProfile(): void {
    this.isLoading = true;
    this.error = null;
    
    this.userService.getProfile().subscribe({
      next: (response) => {
        if (response.isSuccess) {
          this.userProfile = response.data;
          this.updateForm.patchValue({
            userName: this.userProfile.username,
            gender: this.userProfile.gender,
            dateOfBirth: this.userProfile.dateOfBirth
          });
        } else {
          this.error = 'Không thể tải thông tin người dùng';
        }
        this.isLoading = false;
      },
      error: (err) => {
        this.error = 'Đã xảy ra lỗi khi tải thông tin người dùng';
        this.isLoading = false;
      }
    });
  }

  loadUserAddresses(): void {
    this.userAddressService.getAllAddresses().subscribe({
      next: (response) => {
        if (response.isSuccess) {
          this.userAddresses = response.data;
        }
      },
      error: (err) => {
        console.error('Lỗi khi tải địa chỉ:', err);
      }
    });
  }

  toggleEdit(): void {
    this.isEditing = !this.isEditing;
    if (!this.isEditing) {
      this.selectedImage = null;
      this.previewImage = null;
    }
  }

  onFileSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    if (input.files && input.files[0]) {
      const file = input.files[0];
      this.selectedImage = file;
      
      // Create preview
      const reader = new FileReader();
      reader.onload = (e) => {
        const result = e.target?.result as string;
        this.previewImage = result;
        // Get base64 string after comma
        const base64String = result.split(',')[1];
        this.updateForm.patchValue({ imageData: base64String });
      };
      reader.readAsDataURL(file);
    }
  }

  onSubmit(): void {
    if (this.updateForm.valid) {
      const formData: UpdateProfileRequest = {
        userName: this.updateForm.get('userName')?.value,
        gender: this.updateForm.get('gender')?.value,
        dateOfBirth: this.updateForm.get('dateOfBirth')?.value,
        imageData: this.updateForm.get('imageData')?.value
      };

      this.userService.updateProfile(formData).subscribe({
        next: (response) => {
          if (response.isSuccess) {
            this.loadUserProfile();
            this.isEditing = false;
            this.error = null;
          } else {
            this.error = 'Không thể cập nhật thông tin';
          }
        },
        error: (err) => {
          this.error = 'Đã xảy ra lỗi khi cập nhật thông tin';
        }
      });
    }
  }

  toggleAddressForm(): void {
    this.showAddressForm = !this.showAddressForm;
    if (!this.showAddressForm) {
      this.selectedAddress = null;
    }
  }

  onAddressAdded(): void {
    this.loadUserAddresses();
  }

  editAddress(address: any): void {
    this.selectedAddress = address;
    this.showAddressForm = true;
  }

  onAddressUpdated(): void {
    this.loadUserAddresses();
    this.selectedAddress = null;
  }

  deleteAddress(addressId: number): void {
    Swal.fire({
      title: 'Bạn có chắc chắn?',
      text: 'Địa chỉ này sẽ bị xóa vĩnh viễn!',
      icon: 'warning',
      showCancelButton: true,
      confirmButtonText: 'Xóa',
      cancelButtonText: 'Hủy',
      confirmButtonColor: '#d33',
      cancelButtonColor: '#3085d6',
      reverseButtons: true
    }).then((result) => {
      if (result.isConfirmed) {
        this.userAddressService.deleteAddress(addressId).subscribe({
          next: (response) => {
            if (response.isSuccess) {
              Swal.fire({
                title: 'Đã xóa!',
                text: 'Địa chỉ đã được xóa thành công.',
                icon: 'success',
                timer: 1500,
                showConfirmButton: false
              });
              this.loadUserAddresses();
            }
          },
          error: (error) => {
            Swal.fire({
              title: 'Lỗi!',
              text: 'Có lỗi xảy ra khi xóa địa chỉ.',
              icon: 'error',
              confirmButtonText: 'Đóng'
            });
          }
        });
      }
    });
  }
} 