"use client"

import { useEffect, useState } from "react"
import { Button } from "@/components/ui/button"
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card"
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from "@/components/ui/table"
import { Return, ReturnStatus, ReturnType, returnService } from "@/services/return.service"
import { Badge } from "@/components/ui/badge"
import { Check, X, Clock, Package, Banknote, Smartphone, User, Phone, MapPin, ChevronDown } from "lucide-react"
import Swal from "sweetalert2"
import Image from "next/image"
import { format } from "date-fns"
import { vi } from "date-fns/locale"
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog"

const formatCurrency = (amount: number) => {
  return new Intl.NumberFormat('vi-VN', {
    style: 'currency',
    currency: 'VND'
  }).format(amount)
}

const getStatusBadge = (status: ReturnStatus) => {
  switch (status) {
    case ReturnStatus.Pending:
      return (
        <Badge variant="secondary" className="bg-yellow-100 text-yellow-800">
          <Clock className="h-3 w-3 mr-1" />
          Đang chờ
        </Badge>
      )
    case ReturnStatus.Approved:
      return (
        <Badge variant="secondary" className="bg-blue-100 text-blue-800">
          <Check className="h-3 w-3 mr-1" />
          Đã duyệt
        </Badge>
      )
    case ReturnStatus.Rejected:
      return (
        <Badge variant="secondary" className="bg-red-100 text-red-800">
          <X className="h-3 w-3 mr-1" />
          Từ chối
        </Badge>
      )
    case ReturnStatus.Completed:
      return (
        <Badge variant="secondary" className="bg-green-100 text-green-800">
          <Package className="h-3 w-3 mr-1" />
          Hoàn thành
        </Badge>
      )
  }
}

const getReturnTypeText = (type: ReturnType) => {
  switch (type) {
    case ReturnType.Bank:
      return "Chuyển khoản ngân hàng"
    case ReturnType.Momo:
      return "Ví Momo"
    case ReturnType.Other:
      return "Khác"
  }
}

const renderPaymentInfo = (returnItem: Return) => {
  switch (returnItem.returnType) {
    case ReturnType.Bank:
      return (
        <div className="space-y-1 mt-2">
          <div className="flex items-center gap-2 text-sm">
            <Banknote className="h-4 w-4 text-blue-500" />
            <div className="flex flex-col">
              <span className="font-medium">{returnItem.accountName}</span>
              <span className="text-xs text-gray-500">{returnItem.accountNumber}</span>
              <span className="text-xs text-gray-500">{returnItem.bankName}</span>
            </div>
          </div>
        </div>
      )
    case ReturnType.Momo:
      return (
        <div className="flex items-center gap-2 text-sm mt-2">
          <Smartphone className="h-4 w-4 text-pink-500" />
          <div className="flex flex-col">
            <span className="font-medium">{returnItem.phoneNumber}</span>
          </div>
        </div>
      )
    default:
      return null
  }
}

export default function ReturnsPage() {
  const [returns, setReturns] = useState<Return[]>([])
  const [isLoading, setIsLoading] = useState(true)
  const [selectedImage, setSelectedImage] = useState<string | null>(null)
  const [selectedCustomer, setSelectedCustomer] = useState<Return | null>(null)
  const [selectedPayment, setSelectedPayment] = useState<Return | null>(null)
  const [selectedReason, setSelectedReason] = useState<Return | null>(null)
  const [skip, setSkip] = useState(0)
  const [take, setTake] = useState(25)
  const [totalItems, setTotalItems] = useState(0)
  const [loadingMore, setLoadingMore] = useState(false)

  const loadData = async (reset = true, customSkip?: number, customTake?: number) => {
    try {
      if (reset) setIsLoading(true)
      const data = await returnService.getAllReturns(customSkip ?? 0, customTake ?? take)
      if (reset) {
        setReturns(data.items)
      } else {
        setReturns(prev => [...prev, ...data.items])
      }
      setTotalItems(data.totalItems)
    } catch (error) {
      Swal.fire({
        icon: 'error',
        title: 'Lỗi',
        text: 'Không thể tải danh sách đơn trả hàng'
      })
    } finally {
      if (reset) setIsLoading(false)
      setLoadingMore(false)
    }
  }

  useEffect(() => {
    setSkip(0)
    loadData(true, 0, take)
  }, [take])

  const handleLoadMore = () => {
    setLoadingMore(true)
    const newSkip = skip + take
    setSkip(newSkip)
    loadData(false, newSkip, take)
  }

  const handleChangeStatus = async (returnId: number, status: number) => {
    try {
      const result = await Swal.fire({
        title: status === 1 ? 'Duyệt đơn trả hàng?' : 'Từ chối đơn trả hàng?',
        text: status === 1 
          ? 'Bạn có chắc chắn muốn duyệt đơn trả hàng này?' 
          : 'Bạn có chắc chắn muốn từ chối đơn trả hàng này?',
        icon: 'question',
        showCancelButton: true,
        confirmButtonText: 'Xác nhận',
        cancelButtonText: 'Hủy',
        confirmButtonColor: status === 1 ? '#3085d6' : '#d33',
      })

      if (result.isConfirmed) {
        await returnService.changeStatus(returnId, status)
        await loadData()
        Swal.fire({
          icon: 'success',
          title: 'Thành công',
          text: status === 1 
            ? 'Đã duyệt đơn trả hàng thành công' 
            : 'Đã từ chối đơn trả hàng thành công'
        })
      }
    } catch (error) {
      Swal.fire({
        icon: 'error',
        title: 'Lỗi',
        text: 'Không thể cập nhật trạng thái đơn trả hàng'
      })
    }
  }

  const truncateText = (text: string, maxLength: number) => {
    if (text.length <= maxLength) return text
    return text.substring(0, maxLength) + '...'
  }

  return (
    <div className="space-y-4">
      <div className="flex flex-wrap justify-between items-center gap-2">
        <h1 className="text-3xl font-bold">Quản lý Trả hàng</h1>
        <div className="flex items-center gap-2">
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
      <div className="text-sm text-gray-500">Tổng số đơn trả hàng: {totalItems}</div>
      <Card>
        <CardContent className="pt-6">
          {isLoading ? (
            <div className="text-center py-4">Đang tải dữ liệu...</div>
          ) : (
            <>
              <Table>
                <TableHeader>
                  <TableRow>
                    <TableHead className="w-[100px]">Mã đơn</TableHead>
                    <TableHead className="w-[200px]">Sản phẩm</TableHead>
                    <TableHead className="w-[150px]">Thông tin khách hàng</TableHead>
                    <TableHead className="w-[200px]">Lý do</TableHead>
                    <TableHead className="w-[120px]">Hình thức trả hàng</TableHead>
                    <TableHead className="w-[80px] text-center">Số lượng</TableHead>
                    <TableHead className="w-[120px] text-right">Tiền hoàn</TableHead>
                    <TableHead className="w-[150px]">Thời gian</TableHead>
                    <TableHead className="w-[100px]">Trạng thái</TableHead>
                    <TableHead className="w-[150px]">Thao tác</TableHead>
                  </TableRow>
                </TableHeader>
                <TableBody>
                  {returns.length === 0 ? (
                    <TableRow>
                      <TableCell colSpan={10} className="text-center">
                        Không có đơn trả hàng nào
                      </TableCell>
                    </TableRow>
                  ) : (
                    returns.map((returnItem) => (
                      <TableRow key={returnItem.returnId}>
                        <TableCell className="font-medium">{returnItem.orderCode}</TableCell>
                        <TableCell>
                          <div className="flex items-center gap-2">
                            <div 
                              className="relative w-10 h-10 cursor-pointer hover:opacity-80 transition-opacity"
                              onClick={() => setSelectedImage(returnItem.imageUrl)}
                            >
                              <Image
                                src={returnItem.imageUrl}
                                alt={returnItem.name}
                                fill
                                className="object-cover rounded-md"
                              />
                            </div>
                            <div className="text-sm">{returnItem.name}</div>
                          </div>
                        </TableCell>
                        <TableCell>
                          <div 
                            className="text-sm cursor-pointer hover:text-blue-500 transition-colors"
                            onClick={() => setSelectedCustomer(returnItem)}
                          >
                            {truncateText(returnItem.userAddresses.fullName, 15)}
                          </div>
                        </TableCell>
                        <TableCell>
                          <div 
                            className="text-sm font-medium cursor-pointer hover:text-blue-500 transition-colors"
                            onClick={() => setSelectedReason(returnItem)}
                          >
                            {truncateText(returnItem.reason, 30)}
                          </div>
                        </TableCell>
                        <TableCell>
                          <div 
                            className="cursor-pointer hover:text-blue-500 transition-colors"
                            onClick={() => setSelectedPayment(returnItem)}
                          >
                            <Badge variant="outline" className="w-fit">
                              {getReturnTypeText(returnItem.returnType)}
                            </Badge>
                          </div>
                        </TableCell>
                        <TableCell className="text-center">{returnItem.quantity}</TableCell>
                        <TableCell className="text-right">{formatCurrency(returnItem.refundAmount)}</TableCell>
                        <TableCell>
                          <div className="space-y-1">
                            <div className="flex items-center gap-1 text-sm">
                              <Clock className="h-3 w-3 text-gray-500" />
                              <span className="text-gray-500">
                                {format(new Date(returnItem.createdAt), 'dd/MM/yyyy HH:mm', { locale: vi })}
                              </span>
                            </div>
                            {returnItem.processedAt && (
                              <div className="flex items-center gap-1 text-sm">
                                <Check className="h-3 w-3 text-green-500" />
                                <span className="text-gray-500">
                                  {format(new Date(returnItem.processedAt), 'dd/MM/yyyy HH:mm', { locale: vi })}
                                </span>
                              </div>
                            )}
                          </div>
                        </TableCell>
                        <TableCell>{getStatusBadge(returnItem.status)}</TableCell>
                        <TableCell>
                          <div className="flex items-center gap-2">
                            <Button
                              variant="outline"
                              size="sm"
                              onClick={() => handleChangeStatus(returnItem.returnId, 1)}
                              disabled={returnItem.status !== ReturnStatus.Pending}
                            >
                              <Check className="h-4 w-4 mr-1" />
                              Duyệt
                            </Button>
                            <Button
                              variant="destructive"
                              size="sm"
                              onClick={() => handleChangeStatus(returnItem.returnId, 2)}
                              disabled={returnItem.status !== ReturnStatus.Pending}
                            >
                              <X className="h-4 w-4 mr-1" />
                              Từ chối
                            </Button>
                          </div>
                        </TableCell>
                      </TableRow>
                    ))
                  )}
                </TableBody>
              </Table>
              {returns.length < totalItems && (
                <div className="flex justify-center mt-4">
                  <Button onClick={handleLoadMore} disabled={loadingMore} variant="outline">
                    {loadingMore ? 'Đang tải...' : 'Xem thêm'}
                  </Button>
                </div>
              )}
              {returns.length >= totalItems && totalItems > 0 && (
                <div className="text-center text-gray-400 mt-2">Đã hiển thị tất cả đơn trả hàng</div>
              )}
            </>
          )}
        </CardContent>
      </Card>

      <Dialog open={!!selectedImage} onOpenChange={() => setSelectedImage(null)}>
        <DialogContent className="max-w-3xl">
          <DialogHeader>
            <DialogTitle>Xem ảnh sản phẩm</DialogTitle>
          </DialogHeader>
          {selectedImage && (
            <div className="relative w-full h-[500px]">
              <Image
                src={selectedImage}
                alt="Product image"
                fill
                className="object-contain"
              />
            </div>
          )}
        </DialogContent>
      </Dialog>

      <Dialog open={!!selectedCustomer} onOpenChange={() => setSelectedCustomer(null)}>
        <DialogContent>
          <DialogHeader>
            <DialogTitle>Thông tin khách hàng</DialogTitle>
          </DialogHeader>
          {selectedCustomer && (
            <div className="space-y-4">
              <div className="flex items-center gap-2">
                <User className="h-5 w-5 text-blue-500" />
                <div>
                  <div className="font-medium">Họ tên</div>
                  <div className="text-gray-500">{selectedCustomer.userAddresses.fullName}</div>
                </div>
              </div>
              <div className="flex items-center gap-2">
                <Phone className="h-5 w-5 text-blue-500" />
                <div>
                  <div className="font-medium">Số điện thoại</div>
                  <div className="text-gray-500">{selectedCustomer.userAddresses.phoneNumber}</div>
                </div>
              </div>
              <div className="flex items-center gap-2">
                <MapPin className="h-5 w-5 text-blue-500" />
                <div>
                  <div className="font-medium">Địa chỉ</div>
                  <div className="text-gray-500">{selectedCustomer.userAddresses.address}</div>
                </div>
              </div>
            </div>
          )}
        </DialogContent>
      </Dialog>

      <Dialog open={!!selectedPayment} onOpenChange={() => setSelectedPayment(null)}>
        <DialogContent>
          <DialogHeader>
            <DialogTitle>Thông tin thanh toán</DialogTitle>
          </DialogHeader>
          {selectedPayment && (
            <div className="space-y-4">
              <div className="flex items-center gap-2">
                <Badge variant="outline" className="w-fit">
                  {getReturnTypeText(selectedPayment.returnType)}
                </Badge>
              </div>
              {selectedPayment.returnType === ReturnType.Bank && (
                <div className="space-y-2">
                  <div className="flex items-center gap-2">
                    <Banknote className="h-5 w-5 text-blue-500" />
                    <div>
                      <div className="font-medium">Tên tài khoản</div>
                      <div className="text-gray-500">{selectedPayment.accountName}</div>
                    </div>
                  </div>
                  <div className="flex items-center gap-2">
                    <Banknote className="h-5 w-5 text-blue-500" />
                    <div>
                      <div className="font-medium">Số tài khoản</div>
                      <div className="text-gray-500">{selectedPayment.accountNumber}</div>
                    </div>
                  </div>
                  <div className="flex items-center gap-2">
                    <Banknote className="h-5 w-5 text-blue-500" />
                    <div>
                      <div className="font-medium">Ngân hàng</div>
                      <div className="text-gray-500">{selectedPayment.bankName}</div>
                    </div>
                  </div>
                </div>
              )}
              {selectedPayment.returnType === ReturnType.Momo && (
                <div className="flex items-center gap-2">
                  <Smartphone className="h-5 w-5 text-pink-500" />
                  <div>
                    <div className="font-medium">Số điện thoại</div>
                    <div className="text-gray-500">{selectedPayment.phoneNumber}</div>
                  </div>
                </div>
              )}
            </div>
          )}
        </DialogContent>
      </Dialog>

      <Dialog open={!!selectedReason} onOpenChange={() => setSelectedReason(null)}>
        <DialogContent className="max-w-3xl">
          <DialogHeader>
            <DialogTitle>Lý do trả hàng</DialogTitle>
          </DialogHeader>
          {selectedReason && (
            <div className="space-y-4">
              <div className="p-4 bg-gray-50 rounded-lg">
                <div className="text-sm whitespace-pre-wrap">{selectedReason.reason}</div>
              </div>
              {selectedReason.imageUrl && (
                <div className="space-y-2">
                  <div className="font-medium">Minh chứng:</div>
                  <div className="relative w-full h-[400px] rounded-lg overflow-hidden">
                    {selectedReason.imageUrl.toLowerCase().endsWith('.mp4') ? (
                      <video 
                        src={selectedReason.imageUrl} 
                        controls 
                        className="w-full h-full object-contain"
                      />
                    ) : (
                      <Image
                        src={selectedReason.imageUrl}
                        alt="Minh chứng trả hàng"
                        fill
                        className="object-contain"
                      />
                    )}
                  </div>
                </div>
              )}
            </div>
          )}
        </DialogContent>
      </Dialog>
    </div>
  )
} 