import { Component, Inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { ReviewService } from '../../services/review.service';

export interface ReviewModalData {
  variantId: number;
  oiId: number;
  productName: string;
}

@Component({
  selector: 'app-review-modal',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <div class="review-modal">
      <div class="modal-header">
        <h2>Đánh giá sản phẩm</h2>
        <button type="button" class="close-btn" (click)="cancel()">
          <i class="fas fa-times"></i>
        </button>
      </div>

      <div class="modal-body">
        <div class="product-info">
          <h3>{{ data.productName }}</h3>
        </div>

        <div class="rating-section">
          <span class="rating-label">Đánh giá của bạn</span>
          <div class="stars">
            <i *ngFor="let star of [1,2,3,4,5]" 
               class="fas" 
               [class.fa-star]="star <= rating"
               [class.fa-star-o]="star > rating"
               (click)="setRating(star)"
               [class.active]="star <= rating">
            </i>
          </div>
          <span class="rating-text" *ngIf="rating > 0">
            {{ getRatingText(rating) }}
          </span>
        </div>

        <div class="comment-section">
          <label>Nhận xét của bạn</label>
          <textarea 
            [(ngModel)]="comment" 
            placeholder="Chia sẻ những điều bạn thích về sản phẩm (tối đa 500 ký tự)"
            maxlength="500"
            rows="4">
          </textarea>
          <span class="char-count" [class.limit]="comment.length >= 500">
            {{comment.length}}/500
          </span>
        </div>

        <div class="media-section">
          <label>
            Thêm hình ảnh/video
            <span class="sub-label">(tối đa 5 file)</span>
          </label>
          
          <input 
            type="file" 
            #fileInput
            (change)="onFileSelected($event)"
            accept=".png,.jpg,.jpeg,.pdf,.webp,.mp4,.avi,.mov,.mkv,.flv"
            multiple
            style="display: none">
          
          <div class="upload-zone" (click)="fileInput.click()" *ngIf="selectedFiles.length < 5">
            <i class="fas fa-cloud-upload-alt"></i>
            <span>Kéo thả file hoặc click để tải lên</span>
            <small>Hỗ trợ: JPG, PNG, MP4 (Ảnh tối đa 2MB, Video tối đa 5MB)</small>
          </div>
          
          <div class="preview-list" *ngIf="selectedFiles.length > 0">
            <div *ngFor="let file of selectedFiles; let i = index" class="preview-item">
              <i class="fas" [class.fa-image]="isImage(file)" [class.fa-video]="isVideo(file)"></i>
              <span class="file-name">{{file.name}}</span>
              <button type="button" class="remove-btn" (click)="removeFile(i)">
                <i class="fas fa-times"></i>
              </button>
            </div>
          </div>
        </div>

        <div class="error-message" *ngIf="error">
          <i class="fas fa-exclamation-circle"></i>
          {{error}}
        </div>
      </div>

      <div class="modal-footer">
        <button type="button" class="cancel-btn" (click)="cancel()">Hủy</button>
        <button type="button" 
                class="submit-btn" 
                [disabled]="!isValid() || loading" 
                (click)="submit()">
          <i class="fas fa-spinner fa-spin" *ngIf="loading"></i>
          <span>Gửi đánh giá</span>
        </button>
      </div>
    </div>
  `,
  styles: [`
    .review-modal {
      background: white;
      border-radius: 8px;
      overflow: hidden;
      width: 100%;
      max-width: 600px;
    }

    .modal-header {
      padding: 20px;
      border-bottom: 1px solid #e9ecef;
      display: flex;
      justify-content: space-between;
      align-items: center;
    }

    .modal-header h2 {
      margin: 0;
      color: #1a1a1a;
      font-size: 1.25rem;
    }

    .close-btn {
      background: none;
      border: none;
      color: #6c757d;
      cursor: pointer;
      font-size: 1.25rem;
      padding: 0;
    }

    .modal-body {
      padding: 20px;
    }

    .product-info h3 {
      margin: 0 0 20px;
      color: #1a1a1a;
      font-size: 1rem;
    }

    .rating-section {
      margin-bottom: 24px;
      text-align: center;
    }

    .rating-label {
      display: block;
      margin-bottom: 10px;
      color: #1a1a1a;
      font-weight: 500;
    }

    .stars {
      display: flex;
      gap: 8px;
      justify-content: center;
      margin-bottom: 10px;
    }

    .stars i {
      color: #ddd;
      cursor: pointer;
      font-size: 24px;
      transition: all 0.2s ease;
    }

    .stars i.active {
      color: #ffc107;
    }

    .rating-text {
      display: block;
      color: #2ecc71;
      font-size: 0.9rem;
    }

    .comment-section {
      margin-bottom: 24px;
    }

    .comment-section label {
      display: block;
      margin-bottom: 8px;
      color: #1a1a1a;
      font-weight: 500;
    }

    textarea {
      width: 100%;
      padding: 12px;
      border: 1px solid #ddd;
      border-radius: 8px;
      resize: vertical;
      font-size: 0.9rem;
      transition: border-color 0.2s ease;
    }

    textarea:focus {
      outline: none;
      border-color: #2ecc71;
    }

    .char-count {
      display: block;
      text-align: right;
      color: #6c757d;
      font-size: 0.8rem;
      margin-top: 4px;
    }

    .char-count.limit {
      color: #e74c3c;
    }

    .media-section {
      margin-bottom: 24px;
    }

    .media-section label {
      display: block;
      margin-bottom: 8px;
      color: #1a1a1a;
      font-weight: 500;
    }

    .sub-label {
      color: #6c757d;
      font-size: 0.8rem;
      font-weight: normal;
    }

    .upload-zone {
      border: 2px dashed #ddd;
      border-radius: 8px;
      padding: 24px;
      text-align: center;
      cursor: pointer;
      transition: all 0.2s ease;
    }

    .upload-zone:hover {
      border-color: #2ecc71;
      background: #f8f9fa;
    }

    .upload-zone i {
      font-size: 32px;
      color: #2ecc71;
      margin-bottom: 8px;
    }

    .upload-zone span {
      display: block;
      color: #1a1a1a;
      margin-bottom: 4px;
    }

    .upload-zone small {
      color: #6c757d;
    }

    .preview-list {
      margin-top: 16px;
    }

    .preview-item {
      display: flex;
      align-items: center;
      gap: 12px;
      padding: 8px 12px;
      background: #f8f9fa;
      border-radius: 4px;
      margin-bottom: 8px;
    }

    .preview-item i {
      color: #2ecc71;
    }

    .file-name {
      flex: 1;
      font-size: 0.9rem;
      color: #1a1a1a;
      white-space: nowrap;
      overflow: hidden;
      text-overflow: ellipsis;
    }

    .remove-btn {
      background: none;
      border: none;
      color: #e74c3c;
      cursor: pointer;
      padding: 4px;
    }

    .error-message {
      display: flex;
      align-items: center;
      gap: 8px;
      padding: 12px;
      background: #fff3f3;
      color: #e74c3c;
      border-radius: 4px;
      margin-bottom: 16px;
    }

    .modal-footer {
      padding: 20px;
      border-top: 1px solid #e9ecef;
      display: flex;
      justify-content: flex-end;
      gap: 12px;
    }

    .cancel-btn {
      padding: 8px 16px;
      background: #f8f9fa;
      border: 1px solid #ddd;
      border-radius: 4px;
      color: #6c757d;
      cursor: pointer;
      font-size: 0.9rem;
      transition: all 0.2s ease;
    }

    .cancel-btn:hover {
      background: #e9ecef;
    }

    .submit-btn {
      padding: 8px 24px;
      background: #2ecc71;
      border: none;
      border-radius: 4px;
      color: white;
      cursor: pointer;
      font-size: 0.9rem;
      display: flex;
      align-items: center;
      gap: 8px;
      transition: all 0.2s ease;
    }

    .submit-btn:hover:not(:disabled) {
      background: #27ae60;
    }

    .submit-btn:disabled {
      opacity: 0.7;
      cursor: not-allowed;
    }
  `]
})
export class ReviewModalComponent {
  rating: number = 0;
  comment: string = '';
  selectedFiles: File[] = [];
  error: string = '';
  loading: boolean = false;

  constructor(
    private dialogRef: MatDialogRef<ReviewModalComponent>,
    private reviewService: ReviewService,
    @Inject(MAT_DIALOG_DATA) public data: ReviewModalData
  ) {}

  getRatingText(rating: number): string {
    switch(rating) {
      case 1: return 'Rất không hài lòng';
      case 2: return 'Không hài lòng';
      case 3: return 'Bình thường';
      case 4: return 'Hài lòng';
      case 5: return 'Rất hài lòng';
      default: return '';
    }
  }

  isImage(file: File): boolean {
    return ['.jpg', '.jpeg', '.png', '.webp'].some(ext => 
      file.name.toLowerCase().endsWith(ext)
    );
  }

  isVideo(file: File): boolean {
    return ['.mp4', '.avi', '.mov', '.mkv', '.flv'].some(ext => 
      file.name.toLowerCase().endsWith(ext)
    );
  }

  setRating(value: number): void {
    this.rating = value;
  }

  async onFileSelected(event: any): Promise<void> {
    const files: FileList = event.target.files;
    
    if (this.selectedFiles.length + files.length > 5) {
      this.error = 'Chỉ được chọn tối đa 5 file';
      return;
    }

    for (let i = 0; i < files.length; i++) {
      const file = files[i];
      const validation = this.reviewService.validateFile(file);
      
      if (!validation.isValid) {
        this.error = validation.error || 'Lỗi không xác định';
        return;
      }

      this.selectedFiles.push(file);
    }

    this.error = '';
  }

  removeFile(index: number): void {
    this.selectedFiles.splice(index, 1);
  }

  isValid(): boolean {
    return this.rating > 0 && this.comment.trim().length > 0;
  }

  async submit(): Promise<void> {
    if (!this.isValid()) return;

    this.loading = true;
    this.error = '';

    try {
      const imageDatas: string[] = [];
      for (const file of this.selectedFiles) {
        const base64Data = await this.reviewService.convertFileToBase64(file);
        imageDatas.push(base64Data);
      }

      const request = {
        variantId: this.data.variantId,
        oiId: this.data.oiId,
        rating: this.rating,
        comment: this.comment.trim(),
        imageDatas
      };

      await this.reviewService.createReview(request).toPromise();
      this.dialogRef.close(true);
    } catch (error: any) {
      this.error = error.message;
    } finally {
      this.loading = false;
    }
  }

  cancel(): void {
    this.dialogRef.close();
  }
} 