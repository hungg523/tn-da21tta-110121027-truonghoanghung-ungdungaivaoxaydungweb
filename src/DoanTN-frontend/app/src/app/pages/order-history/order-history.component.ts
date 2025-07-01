import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Router } from '@angular/router';
import { CheckoutService, OrderHistory } from '../../services/checkout.service';
import { NotificationService } from '../../services/notification.service';
import { ReturnService, ReturnHistory } from '../../services/return.service';
import { MatDialog } from '@angular/material/dialog';
import { RefundModalComponent } from '../../components/refund-modal/refund-modal.component';
import { ReviewModalComponent } from '../../components/review-modal/review-modal.component';
import Swal from 'sweetalert2';
import { PageBannerComponent } from '../../components/page-banner/page-banner.component';
import { ReturnDetailModalComponent } from '../../components/return-detail-modal/return-detail-modal.component';
import { OrderDetailModalComponent } from '../../components/order-detail-modal/order-detail-modal.component';

export interface OrderTab {
  id: number;
  name: string;
  status?: number;
  count?: number;
}

@Component({
  selector: 'app-order-history',
  standalone: true,
  imports: [CommonModule, RouterModule, PageBannerComponent],
  templateUrl: './order-history.component.html',
  styleUrls: ['./order-history.component.css']
})
export class OrderHistoryComponent implements OnInit {
  orders: OrderHistory[] = [];
  loading: boolean = false;
  error: string | null = null;
  activeTab: number = 0;
  returnOrders: ReturnHistory[] = [];
  
  // Phân trang
  pageSize: number = 20;
  skipOrders: number = 0;
  skipReturns: number = 0;
  totalOrders: number = 0;
  totalReturns: number = 0;
  hasMoreOrders: boolean = true;
  hasMoreReturns: boolean = true;

  tabs: OrderTab[] = [
    { id: 0, name: 'Tất cả'},
    { id: 1, name: 'Chờ xử lý', status: 0 },
    { id: 2, name: 'Chờ lấy hàng', status: 1 },
    { id: 3, name: 'Chờ giao hàng', status: 2 },
    { id: 4, name: 'Đã nhận hàng', status: 3 },
    { id: 5, name: 'Đã hủy hàng', status: 4 },
    { id: 6, name: 'Trả hàng', status: 5 }
  ];

  constructor(
    private checkoutService: CheckoutService,
    private returnService: ReturnService,
    private notificationService: NotificationService,
    private dialog: MatDialog,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.loadOrderHistory();
  }

  loadOrderHistory(status: number | null = null, append: boolean = false): void {
    this.loading = true;
    this.error = null;
    const options: any = { skip: this.skipOrders, take: this.pageSize };
    if (status !== null && status !== undefined) {
      options.status = status;
    }
    this.checkoutService.getOrderHistory(options).subscribe({
      next: (response) => {
        if (response.isSuccess) {
          if (append) {
            this.orders = [...this.orders, ...response.data.items];
          } else {
            this.orders = response.data.items;
          }
          this.totalOrders = response.data.totalItems;
          this.hasMoreOrders = response.data.items.length === this.pageSize;
          this.updateTabCounts();
        } else {
          this.error = 'Không thể tải lịch sử đơn hàng';
        }
        this.loading = false;
      },
      error: (error) => {
        if (error.statusCode === 404 || (error.error && error.error.statusCode === 404)) {
          this.orders = [];
          this.error = null;
        } else if (error.message && error.message.includes('404')) {
          this.orders = [];
          this.error = null;
        } else {
          this.error = error.message;
        }
        this.loading = false;
      }
    });
  }

  updateTabCounts(): void {
    // Reset all counts
    this.tabs.forEach(tab => tab.count = 0);
    
    // Count orders for each status
    this.orders.forEach(order => {
      // Chuyển status từ string sang number
      const statusNumber = Number(order.status);
      if (!isNaN(statusNumber)) {
        const tab = this.tabs.find(t => t.status === statusNumber);
        if (tab) {
          tab.count = (tab.count || 0) + 1;
        }
      }
    });

    console.log('Updated tab counts:', this.tabs); // Để debug
  }

  shouldShowBadge(tab: OrderTab): boolean {
    // Chỉ hiển thị badge cho 3 tab: Chờ xử lý (0), Chờ lấy hàng (1), Chờ giao hàng (2)
    return tab.status === 0 || tab.status === 1 || tab.status === 2;
  }

  onTabChange(tabId: number): void {
    this.activeTab = tabId;
    this.loading = true;
    this.error = null;
    this.skipOrders = 0;
    this.skipReturns = 0;
    this.orders = [];
    this.returnOrders = [];
    this.hasMoreOrders = true;
    this.hasMoreReturns = true;
    if (tabId === 6) { // Tab trả hàng
      this.loadReturnHistory();
    } else {
      const selectedTab = this.tabs.find(tab => tab.id === tabId);
      if (tabId === 0) { // Tab tất cả
        this.loadOrderHistory(null);
      } else {
        this.loadOrderHistory(selectedTab?.status);
      }
    }
  }

  getStatusClass(status: string): string {
    switch (status.toLowerCase()) {
      case 'pending':
        return 'status-pending';
      case 'packed':
        return 'status-packed';
      case 'shipping':
        return 'status-shipping';
      case 'delivered':
        return 'status-delivered';
      case 'cancelled':
        return 'status-cancelled';
      case 'pendingreturn':
        return 'status-pending-return';
      case 'approvedreturn':
        return 'status-approved-return';
      case 'rejectedreturn':
        return 'status-rejected-return';
      default:
        return '';
    }
  }

  getStatusText(status: string): string {
    switch (status.toLowerCase()) {
      case 'pending':
        return 'Chờ xử lý';
      case 'packed':
        return 'Đang đóng gói';
      case 'shipping':
        return 'Đang giao hàng';
      case 'delivered':
        return 'Đã giao hàng';
      case 'cancelled':
        return 'Đã hủy';
      case 'pendingreturn':
        return 'Đang chờ xử lý trả hàng';
      case 'approvedreturn':
        return 'Đã chấp nhận trả hàng';
      case 'rejectedreturn':
        return 'Đã từ chối trả hàng';
      default:
        return status;
    }
  }

  openPaymentUrl(paymentUrl: string): void {
    if (paymentUrl && paymentUrl !== 'Payment has expired.') {
      window.location.href = paymentUrl;
    }
  }

  formatPrice(price: number): string {
    return new Intl.NumberFormat('vi-VN', {
      style: 'currency',
      currency: 'VND'
    }).format(price);
  }

  canRequestCancel(status: string): boolean {
    return ['pending', 'packed'].includes(status.toLowerCase());
  }

  requestCancel(oiId: number): void {
    Swal.fire({
      title: 'Xác nhận hủy đơn',
      text: 'Bạn có chắc chắn muốn hủy đơn hàng này không?',
      icon: 'warning',
      showCancelButton: true,
      confirmButtonColor: '#dc3545',
      cancelButtonColor: '#6c757d',
      confirmButtonText: 'Có, hủy đơn',
      cancelButtonText: 'Không'
    }).then((result) => {
      if (result.isConfirmed) {
        this.loading = true;
        this.checkoutService.requestCancelOrder(oiId).subscribe({
          next: (response) => {
            if (response.isSuccess) {
              this.notificationService.show('Yêu cầu hủy đơn hàng thành công');
              // Chuyển đến tab Đã hủy hàng (id = 5) và load dữ liệu
              this.activeTab = 5;
              this.loadOrderHistory(4);
            } else {
              this.notificationService.show('Không thể hủy đơn hàng', 'error');
            }
            this.loading = false;
          },
          error: (error) => {
            this.notificationService.show(error.message, 'error');
            this.loading = false;
          }
        });
      }
    });
  }

  confirmReceived(oiId: number): void {
    Swal.fire({
      title: 'Xác nhận đã nhận hàng',
      text: 'Bạn xác nhận đã nhận được hàng?',
      icon: 'question',
      showCancelButton: true,
      confirmButtonColor: '#28a745',
      cancelButtonColor: '#6c757d',
      confirmButtonText: 'Đã nhận hàng',
      cancelButtonText: 'Chưa nhận'
    }).then((result) => {
      if (result.isConfirmed) {
        this.loading = true;
        const request = {
          itemStatus: 3 // Delivered status
        };
        this.checkoutService.requestSuccessOrder(oiId).subscribe({
          next: (response) => {
            if (response.isSuccess) {
              this.notificationService.show('Xác nhận đã nhận hàng thành công');
              // Chuyển đến tab Đã nhận hàng (id = 4) và load dữ liệu
              this.activeTab = 4;
              this.loadOrderHistory(3);
            } else {
              this.notificationService.show('Không thể cập nhật trạng thái đơn hàng', 'error');
            }
            this.loading = false;
          },
          error: (error) => {
            this.notificationService.show(error.message, 'error');
            this.loading = false;
          }
        });
      }
    });
  }

  openRefundModal(order: OrderHistory): void {
    const dialogRef = this.dialog.open(RefundModalComponent, {
      width: '500px',
      data: {
        orderItemId: order.oiId,
        quantity: order.quantity
      }
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result) {
        this.loading = true;
        this.returnService.requestRefund(result).subscribe({
          next: (response) => {
            if (response.isSuccess) {
              Swal.fire({
                title: 'Thành công!',
                text: 'Yêu cầu hoàn tiền đã được gửi thành công',
                icon: 'success',
                confirmButtonText: 'Đóng'
              });
              this.loadOrderHistory();
            } else {
              Swal.fire({
                title: 'Lỗi!',
                text: 'Không thể gửi yêu cầu hoàn tiền',
                icon: 'error',
                confirmButtonText: 'Đóng'
              });
            }
            this.loading = false;
          },
          error: (error) => {
            Swal.fire({
              title: 'Lỗi!',
              text: error.message,
              icon: 'error',
              confirmButtonText: 'Đóng'
            });
            this.loading = false;
          }
        });
      }
    });
  }

  rebuyItem(order: OrderHistory): void {
    // Chuyển hướng đến trang chi tiết sản phẩm với variantId
    this.router.navigate(['/product-detail', order.variantId], {
      queryParams: {
        quantity: order.quantity
      }
    });
  }

  openReviewModal(order: OrderHistory): void {
    const dialogRef = this.dialog.open(ReviewModalComponent, {
      width: '500px',
      data: {
        variantId: order.variantId,
        oiId: order.oiId,
        productName: order.name
      }
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result === true) {
        this.notificationService.show('Đánh giá của bạn đã được gửi thành công');
      }
    });
  }

  loadReturnHistory(append: boolean = false) {
    this.loading = true;
    this.error = null;
    this.returnService.getReturnHistory({ skip: this.skipReturns, take: this.pageSize }).subscribe({
      next: (response) => {
        if (response.isSuccess) {
          if (append) {
            this.returnOrders = [...this.returnOrders, ...response.data.items];
          } else {
            this.returnOrders = response.data.items;
          }
          this.totalReturns = response.data.totalItems;
          this.hasMoreReturns = response.data.items.length === this.pageSize;
        } else {
          this.error = 'Có lỗi xảy ra khi tải dữ liệu';
        }
        this.loading = false;
      },
      error: (error) => {
        this.error = 'Không thể tải dữ liệu lịch sử trả hàng';
        this.loading = false;
      }
    });
  }

  getReturnStatusText(status: string): string {
    switch (status.toLowerCase()) {
      case 'pendingreturn':
        return 'Đang chờ xử lý trả hàng';
      case 'approvedreturn':
        return 'Đã chấp nhận trả hàng';
      case 'rejectedreturn':
        return 'Đã từ chối trả hàng';
      default:
        return status;
    }
  }

  getReturnStatusClass(status: string): string {
    switch (status.toLowerCase()) {
      case 'pendingreturn':
        return 'status-pending';
      case 'approvedreturn':
        return 'status-delivered';
      case 'rejectedreturn':
        return 'status-cancelled';
      default:
        return '';
    }
  }

  openReturnDetail(returnId: number): void {
    this.loading = true;
    this.returnService.getReturnDetail(returnId).subscribe({
      next: (response) => {
        if (response.isSuccess) {
          this.dialog.open(ReturnDetailModalComponent, {
            width: '600px',
            data: response.data
          });
        } else {
          this.notificationService.show('Không thể tải thông tin chi tiết đơn trả hàng', 'error');
        }
        this.loading = false;
      },
      error: (error) => {
        this.notificationService.show(error.message, 'error');
        this.loading = false;
      }
    });
  }

  openOrderDetail(oiId: number): void {
    this.loading = true;
    this.checkoutService.getOrderDetail(oiId).subscribe({
      next: (response) => {
        if (response.isSuccess) {
          this.dialog.open(OrderDetailModalComponent, {
            width: '600px',
            data: response.data
          });
        } else {
          this.notificationService.show('Không thể tải thông tin chi tiết đơn hàng', 'error');
        }
        this.loading = false;
      },
      error: (error) => {
        this.notificationService.show(error.message, 'error');
        this.loading = false;
      }
    });
  }

  loadMoreOrders() {
    if (!this.loading && this.hasMoreOrders) {
      this.skipOrders += this.pageSize;
      const selectedTab = this.tabs.find(tab => tab.id === this.activeTab);
      if (this.activeTab === 0) {
        this.loadOrderHistory(null, true);
      } else {
        this.loadOrderHistory(selectedTab?.status, true);
      }
    }
  }

  loadMoreReturns() {
    if (!this.loading && this.hasMoreReturns) {
      this.skipReturns += this.pageSize;
      this.loadReturnHistory(true);
    }
  }

  getRemainingOrdersCount(): number {
    return Math.max(0, this.totalOrders - this.orders.length);
  }
  getRemainingReturnsCount(): number {
    return Math.max(0, this.totalReturns - this.returnOrders.length);
  }
} 