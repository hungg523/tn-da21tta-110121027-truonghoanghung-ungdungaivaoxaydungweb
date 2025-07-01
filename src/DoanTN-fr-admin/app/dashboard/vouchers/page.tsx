"use client"

import { useEffect, useState } from "react"
import { Button } from "@/components/ui/button"
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card"
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from "@/components/ui/table"
import { Coupon, CouponType, CouponTypeEnum, couponService } from "@/services/coupon.service"
import { Badge } from "@/components/ui/badge"
import { Plus, Pencil, Trash2, Filter } from "lucide-react"
import Swal from "sweetalert2"
import { Tabs, TabsContent, TabsList, TabsTrigger } from "@/components/ui/tabs"
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select"
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
  DialogTrigger,
} from "@/components/ui/dialog"
import { Input } from "@/components/ui/input"
import { Label } from "@/components/ui/label"
import { Switch } from "@/components/ui/switch"
import { format } from "date-fns"
import { Calendar } from "@/components/ui/calendar"
import {
  Popover,
  PopoverContent,
  PopoverTrigger,
} from "@/components/ui/popover"
import { CalendarIcon } from "lucide-react"
import { cn } from "@/lib/utils"

const formatCurrency = (amount: number) => {
  return new Intl.NumberFormat('vi-VN', {
    style: 'currency',
    currency: 'VND'
  }).format(amount)
}

const truncateDescription = (description: string | null) => {
  if (!description) return '-'
  if (description.length <= 30) return description
  return description.substring(0, 30) + '...'
}

export default function VouchersPage() {
  const [coupons, setCoupons] = useState<{
    couponOrder: Coupon[]
    couponShip: Coupon[]
  }>({ couponOrder: [], couponShip: [] })
  const [couponTypes, setCouponTypes] = useState<CouponType[]>([])
  const [isLoading, setIsLoading] = useState(true)
  const [activeTab, setActiveTab] = useState("order")
  const [filterStatus, setFilterStatus] = useState<'all' | 'active' | 'inactive'>('all')
  const [isCreateDialogOpen, setIsCreateDialogOpen] = useState(false)
  const [isEditDialogOpen, setIsEditDialogOpen] = useState(false)
  const [selectedCoupon, setSelectedCoupon] = useState<Coupon | null>(null)
  const [formData, setFormData] = useState({
    code: '',
    description: '',
    discountPercentage: 0,
    discountAmount: 0,
    minOrderValue: 0,
    maxDiscountAmount: 0,
    maxUsage: 0,
    maxUsagePerUser: 0,
    isVip: false,
    userSpecific: false,
    terms: '',
    ctId: 0,
    isActived: true,
    startDate: new Date(),
    endDate: new Date()
  })

  const loadData = async () => {
    try {
      setIsLoading(true)
      const [couponsData, typesData] = await Promise.all([
        couponService.getAllCoupons(filterStatus === 'all' ? undefined : filterStatus === 'active'),
        couponService.getAllCouponTypes()
      ])
      setCoupons(couponsData)
      setCouponTypes(typesData)
    } catch (error) {
      Swal.fire({
        icon: 'error',
        title: 'Lỗi',
        text: 'Không thể tải danh sách voucher'
      })
    } finally {
      setIsLoading(false)
    }
  }

  useEffect(() => {
    loadData()
  }, [filterStatus])

  const handleCreate = async () => {
    try {
      await couponService.createCoupon({
        ...formData,
        startDate: formData.startDate.toISOString(),
        endDate: formData.endDate.toISOString()
      })
      Swal.fire({
        icon: 'success',
        title: 'Thành công',
        text: 'Tạo voucher thành công'
      })
      setIsCreateDialogOpen(false)
      loadData()
    } catch (error) {
      Swal.fire({
        icon: 'error',
        title: 'Lỗi',
        text: 'Không thể tạo voucher'
      })
    }
  }

  const handleEdit = async () => {
    if (!selectedCoupon) return
    try {
      await couponService.updateCoupon(selectedCoupon.id, {
        ...formData,
        startDate: formData.startDate.toISOString(),
        endDate: formData.endDate.toISOString()
      })
      Swal.fire({
        icon: 'success',
        title: 'Thành công',
        text: 'Cập nhật voucher thành công'
      })
      setIsEditDialogOpen(false)
      loadData()
    } catch (error) {
      Swal.fire({
        icon: 'error',
        title: 'Lỗi',
        text: 'Không thể cập nhật voucher'
      })
    }
  }

  const handleDelete = async (id: number) => {
    const result = await Swal.fire({
      title: 'Bạn có chắc chắn?',
      text: "Bạn sẽ không thể hoàn tác hành động này!",
      icon: 'warning',
      showCancelButton: true,
      confirmButtonColor: '#3085d6',
      cancelButtonColor: '#d33',
      confirmButtonText: 'Xóa',
      cancelButtonText: 'Hủy'
    })

    if (result.isConfirmed) {
      try {
        await couponService.deleteCoupon(id)
        Swal.fire({
          icon: 'success',
          title: 'Thành công',
          text: 'Xóa voucher thành công'
        })
        loadData()
      } catch (error) {
        Swal.fire({
          icon: 'error',
          title: 'Lỗi',
          text: 'Không thể xóa voucher'
        })
      }
    }
  }

  const openEditDialog = (coupon: Coupon) => {
    setSelectedCoupon(coupon)
    setFormData({
      code: coupon.code,
      description: coupon.description,
      discountPercentage: coupon.discountPercentage,
      discountAmount: coupon.discountAmount,
      minOrderValue: coupon.minOrderValue,
      maxDiscountAmount: coupon.maxDiscountAmount,
      maxUsage: coupon.maxUsage,
      maxUsagePerUser: coupon.maxUsagePerUser,
      isVip: coupon.isVip,
      userSpecific: coupon.userSpecific,
      terms: coupon.term,
      ctId: couponTypes.find(t => t.typeName === coupon.couponType)?.typeId || 0,
      isActived: coupon.isAvtived,
      startDate: new Date(coupon.startDate),
      endDate: new Date(coupon.endDate)
    })
    setIsEditDialogOpen(true)
  }

  const renderCouponTable = (coupons: Coupon[]) => (
    <Table>
      <TableHeader>
        <TableRow>
          <TableHead>Mã</TableHead>
          <TableHead>Mô tả</TableHead>
          <TableHead>Giảm giá</TableHead>
          <TableHead>Đơn tối thiểu</TableHead>
          <TableHead>Giảm tối đa</TableHead>
          <TableHead>Đã dùng</TableHead>
          <TableHead>Thời gian</TableHead>
          <TableHead>Thông tin</TableHead>
          <TableHead>Trạng thái</TableHead>
          <TableHead>Thao tác</TableHead>
        </TableRow>
      </TableHeader>
      <TableBody>
        {coupons.length === 0 ? (
          <TableRow>
            <TableCell colSpan={10} className="text-center">
              Không có voucher nào
            </TableCell>
          </TableRow>
        ) : (
          coupons.map((coupon) => (
            <TableRow key={coupon.id}>
              <TableCell>
                <div className="font-medium">{coupon.code}</div>
                {coupon.isVip && (
                  <Badge variant="default" className="mt-1 bg-gradient-to-r from-purple-500 to-pink-500 text-white border-0">
                    VIP
                  </Badge>
                )}
              </TableCell>
              <TableCell>
                <div 
                  className="cursor-pointer hover:text-blue-500"
                  onClick={() => {
                    Swal.fire({
                      title: 'Mô tả chi tiết',
                      html: `<div class="text-left">${coupon.description}</div>`,
                      width: '600px',
                      showCloseButton: true,
                      showConfirmButton: false
                    })
                  }}
                >
                  {truncateDescription(coupon.description)}
                </div>
              </TableCell>
              <TableCell>
                {coupon.discountPercentage > 0 
                  ? `${coupon.discountPercentage}%` 
                  : formatCurrency(coupon.discountAmount)}
              </TableCell>
              <TableCell>{formatCurrency(coupon.minOrderValue)}</TableCell>
              <TableCell>
                {coupon.maxDiscountAmount > 0 
                  ? formatCurrency(coupon.maxDiscountAmount)
                  : '-'}
              </TableCell>
              <TableCell>
                {coupon.timesUsed}/{coupon.maxUsage}
              </TableCell>
              <TableCell>
                <div className="text-sm">
                  <div>Bắt đầu: {new Date(coupon.startDate).toLocaleDateString('vi-VN')}</div>
                  <div>Kết thúc: {new Date(coupon.endDate).toLocaleDateString('vi-VN')}</div>
                </div>
              </TableCell>
              <TableCell>
                <div className="space-y-1">
                  <div className="text-sm">
                    <span className="font-medium">Lượt dùng/người:</span> {coupon.maxUsagePerUser}
                  </div>
                  <div className="text-sm">
                    <span className="font-medium">Dành riêng:</span> {coupon.userSpecific ? 'Có' : 'Không'}
                  </div>
                  <div 
                    className="text-sm cursor-pointer hover:text-blue-500"
                    onClick={() => {
                      Swal.fire({
                        title: 'Điều khoản',
                        html: `<div class="text-left">${coupon.term}</div>`,
                        width: '600px',
                        showCloseButton: true,
                        showConfirmButton: false
                      })
                    }}
                  >
                    <span className="font-medium">Điều khoản:</span> {truncateDescription(coupon.term)}
                  </div>
                </div>
              </TableCell>
              <TableCell>
                <span className={`px-2 py-1 rounded-full text-xs font-medium ${
                  coupon.isAvtived 
                    ? 'bg-green-100 text-green-800' 
                    : 'bg-red-100 text-red-800'
                }`}>
                  {coupon.isAvtived ? "Đang hoạt động" : "Không hoạt động"}
                </span>
              </TableCell>
              <TableCell>
                <div className="flex items-center gap-2">
                  <Button
                    variant="outline"
                    size="sm"
                    onClick={() => openEditDialog(coupon)}
                  >
                    <Pencil className="h-4 w-4 mr-1" />
                    Sửa
                  </Button>
                  <Button
                    variant="destructive"
                    size="sm"
                    onClick={() => handleDelete(coupon.id)}
                  >
                    <Trash2 className="h-4 w-4 mr-1" />
                    Xóa
                  </Button>
                </div>
              </TableCell>
            </TableRow>
          ))
        )}
      </TableBody>
    </Table>
  )

  return (
    <div className="space-y-4">
      <div className="flex justify-between items-center">
        <h1 className="text-3xl font-bold">Quản lý Voucher</h1>
        <div className="flex items-center gap-4">
          <div className="flex items-center gap-2">
            <Filter className="w-4 h-4 text-gray-500" />
            <Select
              value={filterStatus}
              onValueChange={(value: 'all' | 'active' | 'inactive') => setFilterStatus(value)}
            >
              <SelectTrigger className="w-[180px]">
                <SelectValue placeholder="Lọc theo trạng thái" />
              </SelectTrigger>
              <SelectContent>
                <SelectItem value="all">Tất cả</SelectItem>
                <SelectItem value="active">Đang hoạt động</SelectItem>
                <SelectItem value="inactive">Không hoạt động</SelectItem>
              </SelectContent>
            </Select>
          </div>
          <Dialog open={isCreateDialogOpen} onOpenChange={setIsCreateDialogOpen}>
            <DialogTrigger asChild>
              <Button>
                <Plus className="h-4 w-4 mr-2" />
                Thêm Voucher
              </Button>
            </DialogTrigger>
            <DialogContent className="max-w-xl max-h-[90vh] overflow-y-auto">
              <DialogHeader>
                <DialogTitle>Thêm Voucher Mới</DialogTitle>
              </DialogHeader>
              <form
                onSubmit={e => {
                  e.preventDefault();
                  handleCreate();
                }}
              >
                <div className="grid grid-cols-2 gap-4">
                  <div>
                    <Label htmlFor="code">Mã Voucher</Label>
                    <Input id="code" value={formData.code} onChange={(e) => setFormData({ ...formData, code: e.target.value })} />
                  </div>
                  <div>
                    <Label htmlFor="ctId">Loại Voucher</Label>
                    <Select
                      value={formData.ctId.toString()}
                      onValueChange={(value) => setFormData({ ...formData, ctId: parseInt(value) })}
                    >
                      <SelectTrigger>
                        <SelectValue placeholder="Chọn loại voucher" />
                      </SelectTrigger>
                      <SelectContent>
                        {couponTypes.map((type) => (
                          <SelectItem key={type.typeId} value={type.typeId.toString()}>
                            {type.typeName}
                          </SelectItem>
                        ))}
                      </SelectContent>
                    </Select>
                  </div>
                  <div>
                    <Label htmlFor="discountPercentage">Phần trăm giảm giá (%)</Label>
                    <Input
                      id="discountPercentage"
                      type="number"
                      value={formData.discountPercentage}
                      onChange={(e) => setFormData({ ...formData, discountPercentage: parseFloat(e.target.value) })}
                    />
                  </div>
                  <div>
                    <Label htmlFor="discountAmount">Số tiền giảm giá</Label>
                    <Input
                      id="discountAmount"
                      type="number"
                      value={formData.discountAmount}
                      onChange={(e) => setFormData({ ...formData, discountAmount: parseFloat(e.target.value) })}
                    />
                  </div>
                  <div>
                    <Label htmlFor="minOrderValue">Đơn tối thiểu</Label>
                    <Input
                      id="minOrderValue"
                      type="number"
                      value={formData.minOrderValue}
                      onChange={(e) => setFormData({ ...formData, minOrderValue: parseFloat(e.target.value) })}
                    />
                  </div>
                  <div>
                    <Label htmlFor="maxDiscountAmount">Giảm tối đa</Label>
                    <Input
                      id="maxDiscountAmount"
                      type="number"
                      value={formData.maxDiscountAmount}
                      onChange={(e) => setFormData({ ...formData, maxDiscountAmount: parseFloat(e.target.value) })}
                    />
                  </div>
                  <div>
                    <Label htmlFor="maxUsage">Số lượt sử dụng tối đa</Label>
                    <Input
                      id="maxUsage"
                      type="number"
                      value={formData.maxUsage}
                      onChange={(e) => setFormData({ ...formData, maxUsage: parseInt(e.target.value) })}
                    />
                  </div>
                  <div>
                    <Label htmlFor="maxUsagePerUser">Số lượt sử dụng/người</Label>
                    <Input
                      id="maxUsagePerUser"
                      type="number"
                      value={formData.maxUsagePerUser}
                      onChange={(e) => setFormData({ ...formData, maxUsagePerUser: parseInt(e.target.value) })}
                    />
                  </div>
                  <div>
                    <Label htmlFor="startDate">Ngày bắt đầu</Label>
                    <Popover>
                      <PopoverTrigger asChild>
                        <Button
                          variant={"outline"}
                          className={cn(
                            "w-full justify-start text-left font-normal",
                            !formData.startDate && "text-muted-foreground"
                          )}
                        >
                          <CalendarIcon className="mr-2 h-4 w-4" />
                          {formData.startDate ? format(formData.startDate, "PPP") : <span>Chọn ngày</span>}
                        </Button>
                      </PopoverTrigger>
                      <PopoverContent className="w-auto p-0">
                        <Calendar
                          mode="single"
                          selected={formData.startDate}
                          onSelect={(date) => date && setFormData({ ...formData, startDate: date })}
                          initialFocus
                        />
                      </PopoverContent>
                    </Popover>
                  </div>
                  <div>
                    <Label htmlFor="endDate">Ngày kết thúc</Label>
                    <Popover>
                      <PopoverTrigger asChild>
                        <Button
                          variant={"outline"}
                          className={cn(
                            "w-full justify-start text-left font-normal",
                            !formData.endDate && "text-muted-foreground"
                          )}
                        >
                          <CalendarIcon className="mr-2 h-4 w-4" />
                          {formData.endDate ? format(formData.endDate, "PPP") : <span>Chọn ngày</span>}
                        </Button>
                      </PopoverTrigger>
                      <PopoverContent className="w-auto p-0">
                        <Calendar
                          mode="single"
                          selected={formData.endDate}
                          onSelect={(date) => date && setFormData({ ...formData, endDate: date })}
                          initialFocus
                        />
                      </PopoverContent>
                    </Popover>
                  </div>
                  <div className="col-span-2">
                    <Label htmlFor="description">Mô tả</Label>
                    <Input
                      id="description"
                      value={formData.description}
                      onChange={(e) => setFormData({ ...formData, description: e.target.value })}
                    />
                  </div>
                  <div className="col-span-2">
                    <Label htmlFor="terms">Điều khoản</Label>
                    <Input
                      id="terms"
                      value={formData.terms}
                      onChange={(e) => setFormData({ ...formData, terms: e.target.value })}
                    />
                  </div>
                  <div className="col-span-2 flex items-center gap-6 mt-2">
                    <div className="flex items-center gap-2">
                      <Switch
                        id="isVip"
                        checked={formData.isVip}
                        onCheckedChange={(checked) => setFormData({ ...formData, isVip: checked })}
                      />
                      <Label htmlFor="isVip">VIP</Label>
                    </div>
                    <div className="flex items-center gap-2">
                      <Switch
                        id="userSpecific"
                        checked={formData.userSpecific}
                        onCheckedChange={(checked) => setFormData({ ...formData, userSpecific: checked })}
                      />
                      <Label htmlFor="userSpecific">Dành riêng</Label>
                    </div>
                    <div className="flex items-center gap-2">
                      <Switch
                        id="isActived"
                        checked={formData.isActived}
                        onCheckedChange={(checked) => setFormData({ ...formData, isActived: checked })}
                      />
                      <Label htmlFor="isActived">Hoạt động</Label>
                    </div>
                  </div>
                </div>
                <div className="flex justify-end gap-2 mt-6">
                  <Button variant="outline" type="button" onClick={() => setIsCreateDialogOpen(false)}>
                    Hủy
                  </Button>
                  <Button type="submit">Tạo</Button>
                </div>
              </form>
            </DialogContent>
          </Dialog>
        </div>
      </div>

      <Tabs defaultValue="order" className="space-y-4" onValueChange={setActiveTab}>
        <TabsList>
          <TabsTrigger value="order">Voucher Giảm Giá Đơn Hàng</TabsTrigger>
          <TabsTrigger value="ship">Voucher Giảm Giá Vận Chuyển</TabsTrigger>
        </TabsList>
        <TabsContent value="order">
          <Card>
            <CardContent className="pt-6">
              {isLoading ? (
                <div className="text-center py-4">Đang tải dữ liệu...</div>
              ) : (
                renderCouponTable(coupons.couponOrder)
              )}
            </CardContent>
          </Card>
        </TabsContent>
        <TabsContent value="ship">
          <Card>
            <CardContent className="pt-6">
              {isLoading ? (
                <div className="text-center py-4">Đang tải dữ liệu...</div>
              ) : (
                renderCouponTable(coupons.couponShip)
              )}
            </CardContent>
          </Card>
        </TabsContent>
      </Tabs>

      <Dialog open={isEditDialogOpen} onOpenChange={setIsEditDialogOpen}>
        <DialogContent className="max-w-xl max-h-[90vh] overflow-y-auto">
          <DialogHeader>
            <DialogTitle>Cập nhật Voucher</DialogTitle>
          </DialogHeader>
          <form
            onSubmit={e => {
              e.preventDefault();
              handleEdit();
            }}
          >
            <div className="grid grid-cols-2 gap-4">
              <div>
                <Label htmlFor="edit-code">Mã Voucher</Label>
                <Input
                  id="edit-code"
                  value={formData.code}
                  onChange={(e) => setFormData({ ...formData, code: e.target.value })}
                />
              </div>
              <div>
                <Label htmlFor="edit-ctId">Loại Voucher</Label>
                <Select
                  value={formData.ctId.toString()}
                  onValueChange={(value) => setFormData({ ...formData, ctId: parseInt(value) })}
                >
                  <SelectTrigger>
                    <SelectValue placeholder="Chọn loại voucher" />
                  </SelectTrigger>
                  <SelectContent>
                    {couponTypes.map((type) => (
                      <SelectItem key={type.typeId} value={type.typeId.toString()}>
                        {type.typeName}
                      </SelectItem>
                    ))}
                  </SelectContent>
                </Select>
              </div>
              <div>
                <Label htmlFor="edit-discountPercentage">Phần trăm giảm giá (%)</Label>
                <Input
                  id="edit-discountPercentage"
                  type="number"
                  value={formData.discountPercentage}
                  onChange={(e) => setFormData({ ...formData, discountPercentage: parseFloat(e.target.value) })}
                />
              </div>
              <div>
                <Label htmlFor="edit-discountAmount">Số tiền giảm giá</Label>
                <Input
                  id="edit-discountAmount"
                  type="number"
                  value={formData.discountAmount}
                  onChange={(e) => setFormData({ ...formData, discountAmount: parseFloat(e.target.value) })}
                />
              </div>
              <div>
                <Label htmlFor="edit-minOrderValue">Đơn tối thiểu</Label>
                <Input
                  id="edit-minOrderValue"
                  type="number"
                  value={formData.minOrderValue}
                  onChange={(e) => setFormData({ ...formData, minOrderValue: parseFloat(e.target.value) })}
                />
              </div>
              <div>
                <Label htmlFor="edit-maxDiscountAmount">Giảm tối đa</Label>
                <Input
                  id="edit-maxDiscountAmount"
                  type="number"
                  value={formData.maxDiscountAmount}
                  onChange={(e) => setFormData({ ...formData, maxDiscountAmount: parseFloat(e.target.value) })}
                />
              </div>
              <div>
                <Label htmlFor="edit-maxUsage">Số lượt sử dụng tối đa</Label>
                <Input
                  id="edit-maxUsage"
                  type="number"
                  value={formData.maxUsage}
                  onChange={(e) => setFormData({ ...formData, maxUsage: parseInt(e.target.value) })}
                />
              </div>
              <div>
                <Label htmlFor="edit-maxUsagePerUser">Số lượt sử dụng/người</Label>
                <Input
                  id="edit-maxUsagePerUser"
                  type="number"
                  value={formData.maxUsagePerUser}
                  onChange={(e) => setFormData({ ...formData, maxUsagePerUser: parseInt(e.target.value) })}
                />
              </div>
              <div>
                <Label htmlFor="edit-startDate">Ngày bắt đầu</Label>
                <Popover>
                  <PopoverTrigger asChild>
                    <Button
                      variant={"outline"}
                      className={cn(
                        "w-full justify-start text-left font-normal",
                        !formData.startDate && "text-muted-foreground"
                      )}
                    >
                      <CalendarIcon className="mr-2 h-4 w-4" />
                      {formData.startDate ? format(formData.startDate, "PPP") : <span>Chọn ngày</span>}
                    </Button>
                  </PopoverTrigger>
                  <PopoverContent className="w-auto p-0">
                    <Calendar
                      mode="single"
                      selected={formData.startDate}
                      onSelect={(date) => date && setFormData({ ...formData, startDate: date })}
                      initialFocus
                    />
                  </PopoverContent>
                </Popover>
              </div>
              <div>
                <Label htmlFor="edit-endDate">Ngày kết thúc</Label>
                <Popover>
                  <PopoverTrigger asChild>
                    <Button
                      variant={"outline"}
                      className={cn(
                        "w-full justify-start text-left font-normal",
                        !formData.endDate && "text-muted-foreground"
                      )}
                    >
                      <CalendarIcon className="mr-2 h-4 w-4" />
                      {formData.endDate ? format(formData.endDate, "PPP") : <span>Chọn ngày</span>}
                    </Button>
                  </PopoverTrigger>
                  <PopoverContent className="w-auto p-0">
                    <Calendar
                      mode="single"
                      selected={formData.endDate}
                      onSelect={(date) => date && setFormData({ ...formData, endDate: date })}
                      initialFocus
                    />
                  </PopoverContent>
                </Popover>
              </div>
              <div className="col-span-2">
                <Label htmlFor="edit-description">Mô tả</Label>
                <Input
                  id="edit-description"
                  value={formData.description}
                  onChange={(e) => setFormData({ ...formData, description: e.target.value })}
                />
              </div>
              <div className="col-span-2">
                <Label htmlFor="edit-terms">Điều khoản</Label>
                <Input
                  id="edit-terms"
                  value={formData.terms}
                  onChange={(e) => setFormData({ ...formData, terms: e.target.value })}
                />
              </div>
              <div className="col-span-2 flex items-center gap-6 mt-2">
                <div className="flex items-center gap-2">
                  <Switch
                    id="edit-isVip"
                    checked={formData.isVip}
                    onCheckedChange={(checked) => setFormData({ ...formData, isVip: checked })}
                  />
                  <Label htmlFor="edit-isVip">VIP</Label>
                </div>
                <div className="flex items-center gap-2">
                  <Switch
                    id="edit-userSpecific"
                    checked={formData.userSpecific}
                    onCheckedChange={(checked) => setFormData({ ...formData, userSpecific: checked })}
                  />
                  <Label htmlFor="edit-userSpecific">Dành riêng</Label>
                </div>
                <div className="flex items-center gap-2">
                  <Switch
                    id="edit-isActived"
                    checked={formData.isActived}
                    onCheckedChange={(checked) => setFormData({ ...formData, isActived: checked })}
                  />
                  <Label htmlFor="edit-isActived">Hoạt động</Label>
                </div>
              </div>
            </div>
            <div className="flex justify-end gap-2 mt-6">
              <Button variant="outline" type="button" onClick={() => setIsEditDialogOpen(false)}>
                Hủy
              </Button>
              <Button type="submit">Cập nhật</Button>
            </div>
          </form>
        </DialogContent>
      </Dialog>
    </div>
  )
} 