import { Component, OnInit, ElementRef, ViewChild, AfterViewInit } from '@angular/core';
import { MinigameService, Coupon } from '../../services/minigame.service';
import { CouponService } from '../../services/coupon.service';
import { AuthService } from '../../services/auth.service';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-lucky-wheel',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './lucky-wheel.component.html',
  styleUrls: ['./lucky-wheel.component.css']
})
export class LuckyWheelComponent implements OnInit, AfterViewInit {
  @ViewChild('wheel') wheel!: ElementRef;
  @ViewChild('spinButton') spinButton!: ElementRef;

  coupons: Coupon[] = [];
  canSpin = false;
  isSpinning = false;
  selectedCoupon: Coupon | null = null;
  showResult = false;
  isLoggedIn = false;
  showWheel = false;
  wheelGradient = '';
  wheelSize = 300;
  spinCount = 0;
  segmentColors = [
    '#2196f3', // xanh dương
    '#ff9800', // cam
    '#ffd600', // vàng
    '#43a047', // xanh lá
    '#e53935', // đỏ
    '#8e24aa', // tím
    '#00bcd4', // xanh ngọc
    '#ec407a', // hồng
  ];
  dots: number[] = [];
  wheelRotation = 0;

  constructor(
    private minigameService: MinigameService,
    private couponService: CouponService,
    private authService: AuthService
  ) { }

  ngOnInit() {
    this.isLoggedIn = this.authService.isLoggedIn();
    if (this.isLoggedIn) {
      this.loadCoupons();
      this.checkSpinToday();
    }
    // Tạo 24 chấm tròn quanh vòng quay
    this.dots = Array.from({ length: 24 }, (_, i) => i);
  }

  ngAfterViewInit() {
    this.initializeWheel();
  }

  loadCoupons() {
    this.minigameService.getRandomCoupons().subscribe({
      next: (response) => {
        if (response.isSuccess) {
          this.coupons = response.data;
          this.updateWheelGradient();
        }
      },
      error: (error) => {
        console.error('Error loading coupons:', error);
      }
    });
  }

  checkSpinToday() {
    this.minigameService.checkSpinToday().subscribe({
      next: (response) => {
        if (response.isSuccess) {
          this.canSpin = response.data.canSpin;
          this.spinCount = response.data.spinCount ?? (this.canSpin ? 1 : 0);
        }
      },
      error: (error) => {
        console.error('Error checking spin:', error);
      }
    });
  }

  initializeWheel() {
    if (!this.wheel || !this.coupons.length) return;

    const wheel = this.wheel.nativeElement;
    wheel.innerHTML = '';
    const segments = this.coupons.length;
    const segmentAngle = 360 / segments;

    for (let i = 0; i < segments; i++) {
      const coupon = this.coupons[i];
      const segment = document.createElement('div');
      segment.className = 'segment';
      // Tạo hình quạt cho mỗi segment
      segment.style.transform = `rotate(${i * segmentAngle}deg)`;
      segment.style.clipPath = `polygon(50% 50%, 0% 0%, 100% 0%, 50% 50%, ${100 * Math.cos(((i + 1) * segmentAngle - 90) * Math.PI / 180) + 50}% ${100 * Math.sin(((i + 1) * segmentAngle - 90) * Math.PI / 180) + 50}%)`;
      segment.style.width = '100%';
      segment.style.height = '100%';
      segment.style.position = 'absolute';
      segment.style.left = '0';
      segment.style.top = '0';

      // Hiển thị tên mã coupon ở giữa mỗi segment
      const text = document.createElement('div');
      text.className = 'segment-text';
      text.textContent = coupon.code;
      text.style.position = 'absolute';
      text.style.left = '50%';
      text.style.top = '15%';
      text.style.transform = `rotate(${segmentAngle / 2}deg) translate(-50%, 0)`;
      text.style.width = '80px';
      text.style.textAlign = 'center';
      text.style.fontSize = '14px';
      text.style.color = '#fff';
      segment.appendChild(text);
      wheel.appendChild(segment);
    }
  }

  getSegmentPath(index: number): string {
    const n = this.coupons.length;
    const angle = 2 * Math.PI / n;
    const r = this.wheelSize / 2;
    const start = index * angle - Math.PI / 2;
    const end = start + angle;
    const x1 = r + r * Math.cos(start);
    const y1 = r + r * Math.sin(start);
    const x2 = r + r * Math.cos(end);
    const y2 = r + r * Math.sin(end);
    return `M${r},${r} L${x1},${y1} A${r},${r} 0 0,1 ${x2},${y2} Z`;
  }

  getSegmentColor(index: number): string {
    return this.segmentColors[index % this.segmentColors.length];
  }

  getLabelTransform(index: number): string {
    const n = this.coupons.length;
    const angle = 360 / n;
    const rotate = index * angle + angle / 2;
    const r = this.wheelSize / 2;
    return `translate(${r},${r}) rotate(${rotate}) translate(0,-${r - 55}) rotate(${-rotate})`;
  }

  getCouponIcon(type: string | null | undefined): string | null {
    if (!type) return 'assets/icons/sad.png';
    switch (type.toUpperCase()) {
      case 'ORDER': return 'assets/icons/tag.png';
      case 'SHIP': return 'assets/icons/truck.png';
      default: return 'assets/icons/sad.png';
    }
  }

  getCouponLabel(coupon: Coupon): string {
    if ((!coupon.discountPercentage || coupon.discountPercentage === 0) && (!coupon.discountAmount || coupon.discountAmount === 0)) {
      return '';
    }
    if (coupon.discountPercentage && coupon.discountPercentage > 0) {
      return coupon.discountPercentage + '%';
    }
    if (coupon.discountAmount && coupon.discountAmount > 0) {
      return coupon.discountAmount.toLocaleString() + '₫';
    }
    return '';
  }

  spin() {
    if (!this.canSpin || this.isSpinning) return;
    this.isSpinning = true;
    this.canSpin = false;

    // Chọn ngẫu nhiên một coupon
    const n = this.coupons.length;
    const randomIndex = Math.floor(Math.random() * n);
    this.selectedCoupon = this.coupons[randomIndex];

    // Tính góc quay
    const segmentAngle = 360 / n;
    const targetAngle = 360 * 5 + (randomIndex * segmentAngle) + segmentAngle / 2;
    const startAngle = this.wheelRotation % 360;
    const totalAngle = targetAngle + (360 - (startAngle % 360));

    // Animate xoay vòng quay với ease-out
    const duration = 5000;
    const frameRate = 1000 / 60;
    const frames = duration / frameRate;
    const from = this.wheelRotation;
    const to = from + totalAngle;
    let frame = 0;
    const easeOut = (t: number) => 1 - Math.pow(1 - t, 3); // cubic ease-out
    const animate = () => {
      frame++;
      const t = Math.min(frame / frames, 1);
      this.wheelRotation = from + (to - from) * easeOut(t);
      if (frame < frames) {
        requestAnimationFrame(animate);
      } else {
        this.wheelRotation = to;
        this.isSpinning = false;
        this.showResult = true;
        // Lưu lịch sử quay như cũ
        const couponId = this.selectedCoupon?.id ?? null;
        this.minigameService.saveSpinHistory(couponId).subscribe({
          next: (response) => {
            if (response.isSuccess && couponId) {
              this.couponService.saveCoupon(couponId).subscribe();
            }
          }
        });
      }
    };
    animate();
  }

  closeResult() {
    this.showResult = false;
    this.selectedCoupon = null;
    
    // Reset vòng quay
    const wheel = this.wheel.nativeElement;
    wheel.style.transition = 'none';
    wheel.style.transform = 'rotate(0deg)';
    
    // Kích hoạt lại nút quay
    const spinButton = this.spinButton.nativeElement;
    spinButton.disabled = false;
  }

  openWheel() {
    this.showWheel = true;
    // Khởi tạo lại vòng quay khi mở popup
    setTimeout(() => {
      this.initializeWheel();
    }, 100);
  }

  closeWheel() {
    this.showWheel = false;
    this.showResult = false;
    this.selectedCoupon = null;
  }

  updateWheelGradient() {
    if (!this.coupons.length) {
      this.wheelGradient = '#fff';
      return;
    }
    const colors = [
      '#007bff', '#00bcd4', '#ff9800', '#e91e63', '#4caf50', '#9c27b0', '#ffc107', '#f44336', '#3f51b5', '#009688'
    ];
    const n = this.coupons.length;
    const step = 100 / n;
    let gradient = 'conic-gradient(';
    for (let i = 0; i < n; i++) {
      const color = colors[i % colors.length];
      gradient += `${color} ${i * step}%, ${color} ${(i + 1) * step}%`;
      if (i < n - 1) gradient += ', ';
    }
    gradient += ')';
    this.wheelGradient = gradient;
  }

  getLabelStyle(index: number) {
    const n = this.coupons.length;
    const angle = 360 / n;
    return {
      'position': 'absolute',
      'left': '50%',
      'top': '50%',
      'transform': `rotate(${index * angle}deg) translate(0, -135px) rotate(${-index * angle}deg)`,
      'transformOrigin': 'center',
      'width': '60px',
      'textAlign': 'center',
      'fontWeight': 'bold',
      'color': '#fff',
      'fontSize': '12px',
      'pointerEvents': 'none',
      'zIndex': 2,
      'whiteSpace': 'nowrap',
      'overflow': 'hidden',
      'textOverflow': 'ellipsis',
      'lineHeight': '1.2',
    };
  }

  getDotX(i: number): number {
    const r = this.wheelSize / 2;
    const angle = (2 * Math.PI * i) / this.dots.length;
    return r + (r - 18) * Math.cos(angle - Math.PI / 2);
  }
  getDotY(i: number): number {
    const r = this.wheelSize / 2;
    const angle = (2 * Math.PI * i) / this.dots.length;
    return r + (r - 18) * Math.sin(angle - Math.PI / 2);
  }
} 