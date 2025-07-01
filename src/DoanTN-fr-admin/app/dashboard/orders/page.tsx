"use client"

import { useEffect, useState } from "react"
import { orderService, Order, OrderStatus, OrderDetail, ItemStatus } from "@/services/order.service"
import { Button } from "@/components/ui/button"
import { Card, CardContent } from "@/components/ui/card"
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from "@/components/ui/table"
import { Badge } from "@/components/ui/badge"
import Swal from "sweetalert2"
import Image from "next/image"
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog"
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuTrigger,
} from "@/components/ui/dropdown-menu"
import { ChevronDown } from "lucide-react"

const statusOptions = [
  { value: '', label: 'Tất cả' },
  { value: 0, label: 'Chờ xác nhận' },
  { value: 2, label: 'Đã đóng gói' },
  { value: 3, label: 'Đang giao' },
  { value: 4, label: 'Đã giao' },
  { value: 5, label: 'Đã hủy' },
  { value: 6, label: 'Trạng thái hỗn hợp' },
]

const getStatusBadge = (status: OrderStatus) => {
  switch (status) {
    case OrderStatus.Pending:
      return <Badge variant="secondary" className="bg-yellow-100 text-yellow-800">Chờ xác nhận</Badge>
    case OrderStatus.Confirmed:
      return <Badge variant="secondary" className="bg-blue-100 text-blue-800">Đã xác nhận</Badge>
    case OrderStatus.Packed:
      return <Badge variant="secondary" className="bg-purple-100 text-purple-800">Đã đóng gói</Badge>
    case OrderStatus.Shipping:
      return <Badge variant="secondary" className="bg-orange-100 text-orange-800">Đang giao</Badge>
    case OrderStatus.Successed:
      return <Badge variant="secondary" className="bg-green-100 text-green-800">Đã giao</Badge>
    case OrderStatus.Cancelled:
      return <Badge variant="destructive">Đã hủy</Badge>
    case OrderStatus.Paid:
      return <Badge variant="secondary" className="bg-green-100 text-green-800">Đã thanh toán</Badge>
    case OrderStatus.Failed:
      return <Badge variant="destructive">Thất bại</Badge>
    case OrderStatus.Returned:
      return <Badge variant="secondary" className="bg-gray-100 text-gray-800">Đã trả hàng</Badge>
    case OrderStatus.Mixed:
      return <Badge variant="secondary" className="bg-gray-100 text-gray-800">Hỗn hợp</Badge>
    default:
      return <Badge variant="secondary">Khác</Badge>
  }
}

const getItemStatusBadge = (status: ItemStatus) => {
  switch (status) {
    case ItemStatus.Pending:
      return <Badge variant="secondary" className="bg-yellow-100 text-yellow-800">Chờ xử lý</Badge>
    case ItemStatus.Packed:
      return <Badge variant="secondary" className="bg-purple-100 text-purple-800">Đã đóng gói</Badge>
    case ItemStatus.Shipping:
      return <Badge variant="secondary" className="bg-orange-100 text-orange-800">Đang giao</Badge>
    case ItemStatus.Delivered:
      return <Badge variant="secondary" className="bg-green-100 text-green-800">Đã giao</Badge>
    case ItemStatus.Cancelled:
      return <Badge variant="destructive">Đã hủy</Badge>
    case ItemStatus.PendingReturn:
      return <Badge variant="secondary" className="bg-yellow-100 text-yellow-800">Chờ trả hàng</Badge>
    case ItemStatus.ApprovedReturn:
      return <Badge variant="secondary" className="bg-blue-100 text-blue-800">Đã duyệt trả</Badge>
    case ItemStatus.RejectedReturn:
      return <Badge variant="destructive">Từ chối trả</Badge>
    default:
      return <Badge variant="secondary">Khác</Badge>
  }
}

const formatCurrency = (amount: number) => {
  return new Intl.NumberFormat('vi-VN', {
    style: 'currency',
    currency: 'VND'
  }).format(amount)
}

export default function OrdersPage() {
  const [orders, setOrders] = useState<Order[]>([])
  const [isLoading, setIsLoading] = useState(true)
  const [filterStatus, setFilterStatus] = useState<string>('')
  const [selectedOrder, setSelectedOrder] = useState<OrderDetail | null>(null)
  const [modalOpen, setModalOpen] = useState(false)
  const [skip, setSkip] = useState(0)
  const [take, setTake] = useState(25)
  const [totalItems, setTotalItems] = useState(0)
  const [loadingMore, setLoadingMore] = useState(false)

  const loadData = async (reset = true, customSkip?: number, customTake?: number) => {
    try {
      if (reset) setIsLoading(true)
      const data = await orderService.getAllOrders(filterStatus, customSkip ?? 0, customTake ?? take)
      if (reset) {
        setOrders(data.items)
      } else {
        setOrders(prev => [...prev, ...data.items])
      }
      setTotalItems(data.totalItems)
    } catch (error) {
      Swal.fire({
        icon: 'error',
        title: 'Lỗi',
        text: 'Không thể tải danh sách đơn hàng'
      })
    } finally {
      if (reset) setIsLoading(false)
      setLoadingMore(false)
    }
  }

  useEffect(() => {
    setSkip(0)
    loadData(true, 0, take)
    // eslint-disable-next-line
  }, [filterStatus, take])

  const handleLoadMore = () => {
    setLoadingMore(true)
    const newSkip = skip + take
    setSkip(newSkip)
    loadData(false, newSkip, take)
  }

  const handleOpenDetail = async (orderId: number) => {
    try {
      const detail = await orderService.getOrderDetail(orderId)
      setSelectedOrder(detail)
      setModalOpen(true)
    } catch (error) {
      Swal.fire({
        icon: 'error',
        title: 'Lỗi',
        text: 'Không thể tải chi tiết đơn hàng'
      })
    }
  }

  const handleChangeItemStatus = async (oiId: number, itemStatus: number) => {
    try {
      const result = await Swal.fire({
        title: 'Đổi trạng thái sản phẩm?',
        icon: 'question',
        showCancelButton: true,
        confirmButtonText: 'Xác nhận',
        cancelButtonText: 'Hủy',
      })
      if (result.isConfirmed) {
        await orderService.changeItemStatus(oiId, itemStatus)
        if (selectedOrder) {
          const detail = await orderService.getOrderDetail(selectedOrder.orderId)
          setSelectedOrder(detail)
        }
        Swal.fire({
          icon: 'success',
          title: 'Thành công',
          text: 'Đã cập nhật trạng thái sản phẩm'
        })
      }
    } catch (error) {
      Swal.fire({
        icon: 'error',
        title: 'Lỗi',
        text: 'Không thể cập nhật trạng thái sản phẩm'
      })
    }
  }

  return (
    <div className="space-y-4">
      <div className="flex flex-wrap justify-between items-center gap-2">
        <h1 className="text-3xl font-bold">Quản lý Đơn hàng</h1>
        <div className="flex items-center gap-2">
          <select
            className="border rounded px-3 py-2"
            value={filterStatus}
            onChange={e => setFilterStatus(e.target.value)}
          >
            {statusOptions.map(opt => (
              <option key={opt.value} value={opt.value}>{opt.label}</option>
            ))}
          </select>
          <div className="relative">
            <select
              className="border rounded px-3 py-2 appearance-none pr-8"
              value={take}
              onChange={e => { setTake(Number(e.target.value)); setSkip(0); }}
            >
              {[25, 50, 75, 100].map(num => (
                <option key={num} value={num}>{num}</option>
              ))}
            </select>
            <ChevronDown className="absolute right-2 top-1/2 -translate-y-1/2 w-4 h-4 pointer-events-none text-gray-400" />
          </div>
        </div>
      </div>
      <div className="text-sm text-gray-500">Tổng số đơn: {totalItems}</div>
      <Card>
        <CardContent className="pt-6">
          {isLoading ? (
            <div className="text-center py-4">Đang tải dữ liệu...</div>
          ) : (
            <>
              <Table>
                <TableHeader>
                  <TableRow>
                    <TableHead className="w-[120px]">Mã đơn</TableHead>
                    <TableHead className="w-[200px]">Email</TableHead>
                    <TableHead className="w-[120px]">Trạng thái</TableHead>
                    <TableHead className="w-[100px] text-center">Số lượng</TableHead>
                    <TableHead className="w-[120px] text-right">Tổng tiền</TableHead>
                    <TableHead className="w-[120px]">Thao tác</TableHead>
                  </TableRow>
                </TableHeader>
                <TableBody>
                  {orders.length === 0 ? (
                    <TableRow>
                      <TableCell colSpan={6} className="text-center">Không có đơn hàng nào</TableCell>
                    </TableRow>
                  ) : (
                    orders.map(order => (
                      <TableRow key={order.orderId}>
                        <TableCell className="font-medium">{order.code}</TableCell>
                        <TableCell>{order.email}</TableCell>
                        <TableCell>{getStatusBadge(order.status)}</TableCell>
                        <TableCell className="text-center">{order.totalQuantity}</TableCell>
                        <TableCell className="text-right">{formatCurrency(order.totalAmount)}</TableCell>
                        <TableCell>
                          <Button size="sm" variant="outline" onClick={() => handleOpenDetail(order.orderId)}>
                            Xem chi tiết
                          </Button>
                        </TableCell>
                      </TableRow>
                    ))
                  )}
                </TableBody>
              </Table>
              {orders.length < totalItems && (
                <div className="flex justify-center mt-4">
                  <Button onClick={handleLoadMore} disabled={loadingMore} variant="outline">
                    {loadingMore ? 'Đang tải...' : 'Xem thêm'}
                  </Button>
                </div>
              )}
              {orders.length >= totalItems && totalItems > 0 && (
                <div className="text-center text-gray-400 mt-2">Đã hiển thị tất cả đơn hàng</div>
              )}
            </>
          )}
        </CardContent>
      </Card>
      <Dialog open={modalOpen} onOpenChange={setModalOpen}>
        <DialogContent className="max-w-5xl w-full p-6">
          <DialogHeader>
            <DialogTitle>Chi tiết đơn hàng</DialogTitle>
          </DialogHeader>
          {selectedOrder && (
            <div className="space-y-4">
              <div className="flex flex-col md:flex-row md:items-center md:justify-between gap-2">
                <div>
                  <div className="font-medium">Mã đơn: {selectedOrder.code}</div>
                  <div className="text-sm text-gray-500">Email: {selectedOrder.email}</div>
                  <div className="text-sm text-gray-500">Số lượng: {selectedOrder.totalQuantity}</div>
                  <div className="text-sm text-gray-500">Tổng tiền: {formatCurrency(selectedOrder.totalAmount)}</div>
                </div>
                <div>{getStatusBadge(selectedOrder.status)}</div>
              </div>
              <div className="max-h-[60vh] overflow-y-auto p-1">
                <div className="overflow-x-auto">
                  <Table className="w-full text-sm">
                    <TableHeader>
                      <TableRow>
                        <TableHead className="px-2 py-1">Sản phẩm</TableHead>
                        <TableHead className="px-2 py-1">Thuộc tính</TableHead>
                        <TableHead className="px-2 py-1">Số lượng</TableHead>
                        <TableHead className="px-2 py-1">Giá bán</TableHead>
                        <TableHead className="px-2 py-1 hidden lg:table-cell">Thành tiền</TableHead>
                        <TableHead className="px-2 py-1 hidden lg:table-cell">Trạng thái</TableHead>
                        <TableHead className="px-2 py-1 hidden lg:table-cell">Thao tác</TableHead>
                      </TableRow>
                    </TableHeader>
                    <TableBody>
                      {selectedOrder.items.map(item => (
                        <TableRow key={item.oiId}>
                          <TableCell className="px-2 py-1">
                            <div className="flex items-center gap-2">
                              <div className="relative w-10 h-10 min-w-[40px]">
                                <Image src={item.image} alt={item.name} fill className="object-cover rounded" />
                              </div>
                              <div className="text-sm truncate">{item.name}</div>
                            </div>
                          </TableCell>
                          <TableCell className="px-2 py-1">{item.productAttribute}</TableCell>
                          <TableCell className="px-2 py-1">{item.quantity}</TableCell>
                          <TableCell className="px-2 py-1">{formatCurrency(item.finalPrice)}</TableCell>
                          <TableCell className="px-2 py-1 hidden lg:table-cell">{formatCurrency(item.totalPrice)}</TableCell>
                          <TableCell className="px-2 py-1 hidden lg:table-cell">{getItemStatusBadge(item.status)}</TableCell>
                          <TableCell className="px-2 py-1 hidden lg:table-cell">
                            {![ItemStatus.Delivered, ItemStatus.Cancelled, ItemStatus.PendingReturn, ItemStatus.ApprovedReturn, ItemStatus.RejectedReturn].includes(item.status) ? (
                              <DropdownMenu>
                                <DropdownMenuTrigger asChild>
                                  <Button variant="outline" size="sm" className="w-full">
                                    {(() => {
                                      switch (item.status) {
                                        case ItemStatus.Pending: return 'Chờ xử lý';
                                        case ItemStatus.Packed: return 'Đã đóng gói';
                                        case ItemStatus.Shipping: return 'Đang giao';
                                        case ItemStatus.Delivered: return 'Đã giao';
                                        case ItemStatus.Cancelled: return 'Đã hủy';
                                        case ItemStatus.PendingReturn: return 'Chờ trả hàng';
                                        case ItemStatus.ApprovedReturn: return 'Đã duyệt trả';
                                        case ItemStatus.RejectedReturn: return 'Từ chối trả';
                                        default: return String(item.status);
                                      }
                                    })()}
                                  </Button>
                                </DropdownMenuTrigger>
                                <DropdownMenuContent>
                                  {[
                                    ItemStatus.Pending,
                                    ItemStatus.Packed,
                                    ItemStatus.Shipping,
                                    ItemStatus.Delivered,
                                    ItemStatus.Cancelled
                                  ].filter(status => status !== item.status).map(status => (
                                    <DropdownMenuItem
                                      key={status}
                                      onClick={() => handleChangeItemStatus(item.oiId, Object.values(ItemStatus).indexOf(status))}
                                    >
                                      {(() => {
                                        switch (status) {
                                          case ItemStatus.Pending: return 'Chờ xử lý';
                                          case ItemStatus.Packed: return 'Đã đóng gói';
                                          case ItemStatus.Shipping: return 'Đang giao';
                                          case ItemStatus.Delivered: return 'Đã giao';
                                          case ItemStatus.Cancelled: return 'Đã hủy';
                                          default: return String(status);
                                        }
                                      })()}
                                    </DropdownMenuItem>
                                  ))}
                                </DropdownMenuContent>
                              </DropdownMenu>
                            ) : (
                              <span className="text-gray-400 italic">Không thao tác</span>
                            )}
                          </TableCell>
                        </TableRow>
                      ))}
                    </TableBody>
                  </Table>
                </div>
              </div>
            </div>
          )}
        </DialogContent>
      </Dialog>
    </div>
  )
}
