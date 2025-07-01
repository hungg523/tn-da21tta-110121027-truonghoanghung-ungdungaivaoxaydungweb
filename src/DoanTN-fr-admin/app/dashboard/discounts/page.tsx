"use client"

import { useState, useEffect } from "react"
import { promotionService, Promotion, ProductPromotion, CreatePromotionRequest, UpdatePromotionRequest } from "@/services/promotion.service"
import { Button } from "@/components/ui/button"
import { Input } from "@/components/ui/input"
import { Textarea } from "@/components/ui/textarea"
import { Switch } from "@/components/ui/switch"
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/components/ui/table"
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
  DialogTrigger,
} from "@/components/ui/dialog"
import { toast } from "sonner"
import { Plus, Pencil, Eye, Filter, Trash2 } from "lucide-react"
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select"
import Swal from 'sweetalert2'
import { format } from "date-fns"
import { vi } from "date-fns/locale"
import { Calendar } from "@/components/ui/calendar"
import {
  Popover,
  PopoverContent,
  PopoverTrigger,
} from "@/components/ui/popover"
import { cn } from "@/lib/utils"
import { CalendarIcon } from "lucide-react"
import { variantService, ProductVariant } from "@/services/variant.service"
import { Checkbox } from "@/components/ui/checkbox"

export default function DiscountsPage() {
  const [promotions, setPromotions] = useState<Promotion[]>([])
  const [selectedPromotionProducts, setSelectedPromotionProducts] = useState<{
    pmId: number;
    variantId: number;
    variantName: string;
  }[]>([])
  const [isLoading, setIsLoading] = useState(true)
  const [isCreateDialogOpen, setIsCreateDialogOpen] = useState(false)
  const [isEditDialogOpen, setIsEditDialogOpen] = useState(false)
  const [isViewProductsDialogOpen, setIsViewProductsDialogOpen] = useState(false)
  const [selectedPromotion, setSelectedPromotion] = useState<Promotion | null>(null)
  const [formData, setFormData] = useState<CreatePromotionRequest>({
    name: "",
    description: "",
    discountAmount: null,
    discountPercentage: 0,
    startDate: new Date().toISOString(),
    endDate: new Date().toISOString(),
    isActived: 1,
    isFlashSale: false
  })
  const [discountType, setDiscountType] = useState<'amount' | 'percentage'>('percentage')
  const [filterStatus, setFilterStatus] = useState<'all' | 'active' | 'inactive'>('all')
  const [selectedDescription, setSelectedDescription] = useState<string | null>(null)
  const [isDescriptionDialogOpen, setIsDescriptionDialogOpen] = useState(false)
  const [isAddProductDialogOpen, setIsAddProductDialogOpen] = useState(false)
  const [availableVariants, setAvailableVariants] = useState<ProductVariant[]>([])
  const [selectedVariantIds, setSelectedVariantIds] = useState<number[]>([])
  const [isLoadingVariants, setIsLoadingVariants] = useState(false)
  const [isUpdateProductDialogOpen, setIsUpdateProductDialogOpen] = useState(false)
  const [selectedProduct, setSelectedProduct] = useState<{
    pmId: number;
    variantId: number;
    variantName: string;
  } | null>(null)

  useEffect(() => {
    loadData()
  }, [])

  const loadData = async () => {
    try {
      setIsLoading(true)
      const promotionsData = await promotionService.getAllPromotions()
      setPromotions(promotionsData)
    } catch (error) {
      Swal.fire({
        icon: 'error',
        title: 'Lỗi',
        text: 'Không thể tải dữ liệu khuyến mãi'
      })
    } finally {
      setIsLoading(false)
    }
  }

  const handleCreate = async () => {
    try {
      if (!formData.name) {
        Swal.fire({
          icon: 'error',
          title: 'Lỗi',
          text: 'Vui lòng nhập tên khuyến mãi'
        })
        return
      }

      if (discountType === 'amount') {
        if (formData.discountAmount === null || formData.discountAmount < 0) {
          Swal.fire({
            icon: 'error',
            title: 'Lỗi',
            text: 'Vui lòng nhập số tiền giảm lớn hơn 0'
          })
          return
        }
      } else {
        if (formData.discountPercentage < 0 || formData.discountPercentage > 100) {
          Swal.fire({
            icon: 'error',
            title: 'Lỗi',
            text: 'Vui lòng nhập phần trăm giảm từ 1 đến 100'
          })
          return
        }
      }

      const startDate = new Date(formData.startDate)
      const endDate = new Date(formData.endDate)
      if (endDate <= startDate) {
        Swal.fire({
          icon: 'error',
          title: 'Lỗi',
          text: 'Ngày kết thúc phải sau ngày bắt đầu'
        })
        return
      }

      await promotionService.createPromotion({
        ...formData,
        discountAmount: discountType === 'amount' ? formData.discountAmount : null,
        discountPercentage: discountType === 'percentage' ? formData.discountPercentage : 0
      })
      
      Swal.fire({
        icon: 'success',
        title: 'Thành công',
        text: 'Tạo khuyến mãi thành công'
      })
      setIsCreateDialogOpen(false)
      setFormData({
        name: "",
        description: "",
        discountAmount: null,
        discountPercentage: 0,
        startDate: new Date().toISOString(),
        endDate: new Date().toISOString(),
        isActived: 1,
        isFlashSale: false
      })
      loadData()
    } catch (error) {
      Swal.fire({
        icon: 'error',
        title: 'Lỗi',
        text: 'Không thể tạo khuyến mãi'
      })
    }
  }

  const handleEdit = async () => {
    if (!selectedPromotion) return

    try {
      if (!formData.name) {
        Swal.fire({
          icon: 'error',
          title: 'Lỗi',
          text: 'Vui lòng nhập tên khuyến mãi'
        })
        return
      }

      if (discountType === 'amount' && (!formData.discountAmount || formData.discountAmount <= 0)) {
        Swal.fire({
          icon: 'error',
          title: 'Lỗi',
          text: 'Vui lòng nhập số tiền giảm hợp lệ'
        })
        return
      }

      if (discountType === 'percentage' && (formData.discountPercentage <= 0 || formData.discountPercentage > 100)) {
        Swal.fire({
          icon: 'error',
          title: 'Lỗi',
          text: 'Vui lòng nhập phần trăm giảm hợp lệ (1-100)'
        })
        return
      }

      const startDate = new Date(formData.startDate)
      const endDate = new Date(formData.endDate)
      if (endDate <= startDate) {
        Swal.fire({
          icon: 'error',
          title: 'Lỗi',
          text: 'Ngày kết thúc phải sau ngày bắt đầu'
        })
        return
      }

      await promotionService.updatePromotion(selectedPromotion.promotionId, {
        ...formData,
        discountAmount: discountType === 'amount' ? formData.discountAmount : null,
        discountPercentage: discountType === 'percentage' ? formData.discountPercentage : 0
      })
      
      Swal.fire({
        icon: 'success',
        title: 'Thành công',
        text: 'Cập nhật khuyến mãi thành công'
      })
      setIsEditDialogOpen(false)
      setSelectedPromotion(null)
      setFormData({
        name: "",
        description: "",
        discountAmount: null,
        discountPercentage: 0,
        startDate: new Date().toISOString(),
        endDate: new Date().toISOString(),
        isActived: 1,
        isFlashSale: false
      })
      loadData()
    } catch (error) {
      Swal.fire({
        icon: 'error',
        title: 'Lỗi',
        text: 'Không thể cập nhật khuyến mãi'
      })
    }
  }

  const handleToggleStatus = async (promotion: Promotion) => {
    if (promotion.isActived === 0) {
      Swal.fire({
        icon: 'info',
        title: 'Thông báo',
        text: 'Khuyến mãi này đã được tắt trạng thái'
      })
      return
    }

    const result = await Swal.fire({
      title: 'Xác nhận tắt trạng thái',
      text: 'Bạn có chắc chắn muốn tắt trạng thái khuyến mãi này?',
      icon: 'warning',
      showCancelButton: true,
      confirmButtonColor: '#d33',
      cancelButtonColor: '#3085d6',
      confirmButtonText: 'Tắt',
      cancelButtonText: 'Hủy'
    })

    if (result.isConfirmed) {
      try {
        await promotionService.updatePromotion(promotion.promotionId, {
          ...promotion,
          isActived: 0
        })
        Swal.fire({
          icon: 'success',
          title: 'Thành công',
          text: 'Đã tắt trạng thái khuyến mãi'
        })
        loadData()
      } catch (error) {
        Swal.fire({
          icon: 'error',
          title: 'Lỗi',
          text: 'Không thể tắt trạng thái khuyến mãi'
        })
      }
    }
  }

  const openEditDialog = (promotion: Promotion) => {
    setSelectedPromotion(promotion)
    setFormData({
      name: promotion.name,
      description: promotion.description,
      discountAmount: promotion.discountAmount,
      discountPercentage: promotion.discountPercentage,
      startDate: promotion.startDate,
      endDate: promotion.endDate,
      isActived: promotion.isActived,
      isFlashSale: promotion.isFlashSale
    })
    setDiscountType(promotion.discountAmount !== null ? 'amount' : 'percentage')
    setIsEditDialogOpen(true)
  }

  const openViewProductsDialog = (promotion: Promotion) => {
    setSelectedPromotion(promotion)
    setSelectedPromotionProducts(promotion.promotions)
    setIsViewProductsDialogOpen(true)
  }

  const formatCurrency = (amount: number) => {
    return new Intl.NumberFormat('vi-VN', {
      style: 'currency',
      currency: 'VND'
    }).format(amount)
  }

  const renderDiscountValue = (promotion: Promotion) => {
    if (promotion.discountAmount !== null) {
      return formatCurrency(promotion.discountAmount)
    }
    return `${promotion.discountPercentage}%`
  }

  const renderPromotionForm = () => (
    <div className="space-y-4">
      <div>
        <label className="block text-sm font-medium mb-1">Tên khuyến mãi</label>
        <Input
          value={formData.name}
          onChange={(e) => setFormData({ ...formData, name: e.target.value })}
          placeholder="Nhập tên khuyến mãi"
        />
      </div>
      <div>
        <label className="block text-sm font-medium mb-1">Mô tả</label>
        <Textarea
          value={formData.description}
          onChange={(e) => setFormData({ ...formData, description: e.target.value })}
          placeholder="Nhập mô tả khuyến mãi"
        />
      </div>
      <div>
        <label className="block text-sm font-medium mb-1">Loại giảm giá</label>
        <Select
          value={discountType}
          onValueChange={(value: 'amount' | 'percentage') => setDiscountType(value)}
        >
          <SelectTrigger>
            <SelectValue placeholder="Chọn loại giảm giá" />
          </SelectTrigger>
          <SelectContent>
            <SelectItem value="percentage">Phần trăm (%)</SelectItem>
            <SelectItem value="amount">Số tiền (VNĐ)</SelectItem>
          </SelectContent>
        </Select>
      </div>
      {discountType === 'percentage' ? (
        <div>
          <label className="block text-sm font-medium mb-1">Phần trăm giảm giá</label>
          <Input
            type="number"
            value={formData.discountPercentage}
            onChange={(e) => setFormData({ ...formData, discountPercentage: parseFloat(e.target.value) })}
            placeholder="Nhập phần trăm giảm giá"
            min="0"
            max="100"
          />
        </div>
      ) : (
        <div>
          <label className="block text-sm font-medium mb-1">Số tiền giảm giá</label>
          <Input
            type="number"
            value={formData.discountAmount || ''}
            onChange={(e) => setFormData({ ...formData, discountAmount: parseFloat(e.target.value) })}
            placeholder="Nhập số tiền giảm giá"
            min="0"
          />
        </div>
      )}
      <div>
        <label className="block text-sm font-medium mb-1">Ngày bắt đầu</label>
        <Popover>
          <PopoverTrigger asChild>
            <Button
              variant="outline"
              className={cn(
                "w-full justify-start text-left font-normal",
                !formData.startDate && "text-muted-foreground"
              )}
            >
              <CalendarIcon className="mr-2 h-4 w-4" />
              {formData.startDate ? format(new Date(formData.startDate), "PPP", { locale: vi }) : "Chọn ngày"}
            </Button>
          </PopoverTrigger>
          <PopoverContent className="w-auto p-0">
            <Calendar
              mode="single"
              selected={new Date(formData.startDate)}
              onSelect={(date) => setFormData({ ...formData, startDate: date?.toISOString() || new Date().toISOString() })}
              initialFocus
            />
          </PopoverContent>
        </Popover>
      </div>
      <div>
        <label className="block text-sm font-medium mb-1">Ngày kết thúc</label>
        <Popover>
          <PopoverTrigger asChild>
            <Button
              variant="outline"
              className={cn(
                "w-full justify-start text-left font-normal",
                !formData.endDate && "text-muted-foreground"
              )}
            >
              <CalendarIcon className="mr-2 h-4 w-4" />
              {formData.endDate ? format(new Date(formData.endDate), "PPP", { locale: vi }) : "Chọn ngày"}
            </Button>
          </PopoverTrigger>
          <PopoverContent className="w-auto p-0">
            <Calendar
              mode="single"
              selected={new Date(formData.endDate)}
              onSelect={(date) => setFormData({ ...formData, endDate: date?.toISOString() || new Date().toISOString() })}
              initialFocus
            />
          </PopoverContent>
        </Popover>
      </div>
      <div className="flex items-center space-x-2">
        <Switch
          checked={formData.isActived === 1}
          onCheckedChange={(checked) => setFormData({ ...formData, isActived: checked ? 1 : 0 })}
        />
        <label className="text-sm font-medium">Kích hoạt</label>
      </div>
      <div className="flex items-center space-x-2">
        <Switch
          checked={formData.isFlashSale}
          onCheckedChange={(checked) => setFormData({ ...formData, isFlashSale: checked })}
        />
        <label className="text-sm font-medium">Flash Sale</label>
      </div>
    </div>
  )

  const truncateDescription = (description: string | null) => {
    if (!description) return '-'
    if (description.length <= 30) return description
    return description.substring(0, 30) + '...'
  }

  const filteredPromotions = promotions.filter(promotion => {
    if (filterStatus === 'all') return true
    if (filterStatus === 'active') return promotion.isActived === 1
    if (filterStatus === 'inactive') return promotion.isActived === 0
    return true
  })

  const loadAvailableVariants = async (promotionId: number) => {
    try {
      setIsLoadingVariants(true)
      const allVariants = await variantService.getAll()
      
      const usedVariantIds = new Set(
        promotions.flatMap(p => 
          p.promotions.map(pp => pp.variantId)
        )
      )
      
      setAvailableVariants(
        allVariants.filter(v => !usedVariantIds.has(v.variantId))
      )
    } catch (error) {
      Swal.fire({
        icon: 'error',
        title: 'Lỗi',
        text: 'Không thể tải danh sách sản phẩm'
      })
    } finally {
      setIsLoadingVariants(false)
    }
  }

  const handleAddProducts = async () => {
    if (!selectedPromotion || selectedVariantIds.length === 0) return

    try {
      await promotionService.createProductPromotion({
        variantIds: selectedVariantIds,
        promotionId: selectedPromotion.promotionId
      })
      
      Swal.fire({
        icon: 'success',
        title: 'Thành công',
        text: 'Thêm sản phẩm vào khuyến mãi thành công'
      })
      setIsAddProductDialogOpen(false)
      setSelectedVariantIds([])
      loadData()
    } catch (error) {
      Swal.fire({
        icon: 'error',
        title: 'Lỗi',
        text: 'Không thể thêm sản phẩm vào khuyến mãi'
      })
    }
  }

  const handleRemoveProduct = async (pmId: number) => {
    const result = await Swal.fire({
      title: 'Xác nhận xóa',
      text: 'Bạn có chắc chắn muốn xóa sản phẩm này khỏi khuyến mãi?',
      icon: 'warning',
      showCancelButton: true,
      confirmButtonColor: '#d33',
      cancelButtonColor: '#3085d6',
      confirmButtonText: 'Xóa',
      cancelButtonText: 'Hủy'
    })

    if (result.isConfirmed) {
      try {
        await promotionService.deleteProductPromotion(pmId)
        Swal.fire({
          icon: 'success',
          title: 'Thành công',
          text: 'Xóa sản phẩm khỏi khuyến mãi thành công'
        })
        loadData()
      } catch (error) {
        Swal.fire({
          icon: 'error',
          title: 'Lỗi',
          text: 'Không thể xóa sản phẩm khỏi khuyến mãi'
        })
      }
    }
  }

  const openAddProductDialog = () => {
    if (!selectedPromotion) return
    loadAvailableVariants(selectedPromotion.promotionId)
    setIsAddProductDialogOpen(true)
  }

  const handleUpdateProduct = (product: {
    pmId: number;
    variantId: number;
    variantName: string;
  }) => {
    setSelectedProduct(product)
    setIsUpdateProductDialogOpen(true)
  }

  const handleUpdateProductSubmit = async () => {
    if (!selectedProduct || !selectedPromotion || selectedVariantIds.length !== 1) return

    try {
      await promotionService.updateProductPromotion(selectedProduct.pmId, {
        variantId: selectedVariantIds[0],
        promotionId: selectedPromotion.promotionId
      })
      
      Swal.fire({
        icon: 'success',
        title: 'Thành công',
        text: 'Cập nhật sản phẩm khuyến mãi thành công'
      })
      setIsUpdateProductDialogOpen(false)
      setSelectedVariantIds([])
      setSelectedProduct(null)
      loadData()
    } catch (error) {
      Swal.fire({
        icon: 'error',
        title: 'Lỗi',
        text: 'Không thể cập nhật sản phẩm khuyến mãi'
      })
    }
  }

  return (
    <div className="container mx-auto py-10">
      <div className="flex justify-between items-center mb-6">
        <h2 className="text-xl font-semibold">Quản lý khuyến mãi</h2>
        <div className="flex items-center gap-4">
          <Select
            value={filterStatus}
            onValueChange={(value: 'all' | 'active' | 'inactive') => setFilterStatus(value)}
          >
            <SelectTrigger className="w-[150px]">
              <SelectValue placeholder="Trạng thái" />
            </SelectTrigger>
            <SelectContent>
              <SelectItem value="all">Tất cả</SelectItem>
              <SelectItem value="active">Đang hoạt động</SelectItem>
              <SelectItem value="inactive">Không hoạt động</SelectItem>
            </SelectContent>
          </Select>
          <Dialog open={isCreateDialogOpen} onOpenChange={setIsCreateDialogOpen}>
            <DialogTrigger asChild>
              <Button>
                <Plus className="h-4 w-4 mr-1" />
                Tạo khuyến mãi
              </Button>
            </DialogTrigger>
            <DialogContent>
              <DialogHeader>
                <DialogTitle>Tạo khuyến mãi mới</DialogTitle>
              </DialogHeader>
              {renderPromotionForm()}
              <Button onClick={handleCreate}>Tạo</Button>
            </DialogContent>
          </Dialog>
        </div>
      </div>

      <Table>
        <TableHeader>
          <TableRow>
            <TableHead>ID</TableHead>
            <TableHead>Tên</TableHead>
            <TableHead>Mô tả</TableHead>
            <TableHead>Giảm giá</TableHead>
            <TableHead>Thời gian</TableHead>
            <TableHead>Trạng thái</TableHead>
            <TableHead>Flash Sale</TableHead>
            <TableHead>Thao tác</TableHead>
          </TableRow>
        </TableHeader>
        <TableBody>
          {isLoading ? (
            <TableRow>
              <TableCell colSpan={8} className="text-center">Đang tải...</TableCell>
            </TableRow>
          ) : promotions.length === 0 ? (
            <TableRow>
              <TableCell colSpan={8} className="text-center">Không có khuyến mãi nào</TableCell>
            </TableRow>
          ) : (
            promotions
              .filter(promotion => {
                if (filterStatus === 'all') return true
                if (filterStatus === 'active') return promotion.isActived === 1
                if (filterStatus === 'inactive') return promotion.isActived === 0
                return true
              })
              .map((promotion) => (
                <TableRow key={promotion.promotionId}>
                  <TableCell>{promotion.promotionId}</TableCell>
                  <TableCell>{promotion.name}</TableCell>
                  <TableCell>
                    <div
                      className="cursor-pointer hover:text-blue-500"
                      onClick={() => {
                        setSelectedDescription(promotion.description)
                        setIsDescriptionDialogOpen(true)
                      }}
                    >
                      {truncateDescription(promotion.description)}
                    </div>
                  </TableCell>
                  <TableCell>{renderDiscountValue(promotion)}</TableCell>
                  <TableCell>
                    {format(new Date(promotion.startDate), "dd/MM/yyyy", { locale: vi })} - {format(new Date(promotion.endDate), "dd/MM/yyyy", { locale: vi })}
                  </TableCell>
                  <TableCell>
                    <span className={`px-2 py-1 rounded-full text-xs font-medium ${
                      promotion.isActived === 1 
                        ? 'bg-green-100 text-green-800' 
                        : 'bg-red-100 text-red-800'
                    }`}>
                      {promotion.isActived === 1 ? "Đang hoạt động" : "Không hoạt động"}
                    </span>
                  </TableCell>
                  <TableCell>
                    <span className={`px-2 py-1 rounded-full text-xs font-medium ${
                      promotion.isFlashSale
                        ? 'bg-yellow-100 text-yellow-800' 
                        : 'bg-gray-100 text-gray-800'
                    }`}>
                      {promotion.isFlashSale ? "Flash Sale" : "Thường"}
                    </span>
                  </TableCell>
                  <TableCell>
                    <div className="flex items-center gap-2">
                      <Button
                        variant="outline"
                        size="sm"
                        onClick={() => openEditDialog(promotion)}
                      >
                        <Pencil className="h-4 w-4 mr-1" />
                        Sửa
                      </Button>
                      <Button
                        variant="outline"
                        size="sm"
                        onClick={() => openViewProductsDialog(promotion)}
                      >
                        <Eye className="h-4 w-4 mr-1" />
                        Sản phẩm
                      </Button>
                      <Button
                        variant="outline"
                        size="sm"
                        className={cn(
                          "flex items-center",
                          promotion.isActived === 1
                            ? "text-red-600 hover:text-red-700 hover:bg-red-50"
                            : "text-gray-400 cursor-not-allowed"
                        )}
                        onClick={() => handleToggleStatus(promotion)}
                        disabled={promotion.isActived === 0}
                      >
                        <Trash2 className="h-4 w-4 mr-1" />
                        {promotion.isActived === 1 ? "Tắt" : "Đã tắt"}
                      </Button>
                    </div>
                  </TableCell>
                </TableRow>
              ))
          )}
        </TableBody>
      </Table>

      <Dialog open={isDescriptionDialogOpen} onOpenChange={setIsDescriptionDialogOpen}>
        <DialogContent>
          <DialogHeader>
            <DialogTitle>Mô tả chi tiết</DialogTitle>
          </DialogHeader>
          <div className="whitespace-pre-wrap text-gray-700">
            {selectedDescription || '-'}
          </div>
          <div className="flex justify-end">
            <Button variant="outline" onClick={() => setIsDescriptionDialogOpen(false)}>
              Đóng
            </Button>
          </div>
        </DialogContent>
      </Dialog>

      <Dialog open={isUpdateProductDialogOpen} onOpenChange={setIsUpdateProductDialogOpen}>
        <DialogContent className="max-w-4xl">
          <DialogHeader>
            <DialogTitle>Cập nhật sản phẩm khuyến mãi</DialogTitle>
          </DialogHeader>
          <div className="space-y-4">
            {isLoadingVariants ? (
              <div className="text-center py-4">Đang tải danh sách sản phẩm...</div>
            ) : (
              <>
                <Table>
                  <TableHeader>
                    <TableRow>
                      <TableHead className="w-[50px]">
                        <Checkbox
                          checked={selectedVariantIds.length === 1}
                          onCheckedChange={(checked) => {
                            setSelectedVariantIds(checked ? [availableVariants[0]?.variantId] : [])
                          }}
                        />
                      </TableHead>
                      <TableHead>Tên sản phẩm</TableHead>
                      <TableHead>Biến thể</TableHead>
                      <TableHead>Giá</TableHead>
                    </TableRow>
                  </TableHeader>
                  <TableBody>
                    {availableVariants.length === 0 ? (
                      <TableRow>
                        <TableCell colSpan={4} className="text-center">
                          Không có sản phẩm nào khả dụng
                        </TableCell>
                      </TableRow>
                    ) : (
                      availableVariants.map((variant) => (
                        <TableRow key={variant.variantId}>
                          <TableCell>
                            <Checkbox
                              checked={selectedVariantIds.includes(variant.variantId)}
                              onCheckedChange={(checked) => {
                                setSelectedVariantIds(
                                  checked
                                    ? [...selectedVariantIds, variant.variantId]
                                    : selectedVariantIds.filter(
                                        (id) => id !== variant.variantId
                                      )
                                )
                              }}
                            />
                          </TableCell>
                          <TableCell>{variant.name}</TableCell>
                          <TableCell>
                            {variant.productsAttributes
                              .map((attr) => attr.value)
                              .join(" - ")}
                          </TableCell>
                          <TableCell>{formatCurrency(variant.price)}</TableCell>
                        </TableRow>
                      ))
                    )}
                  </TableBody>
                </Table>
                <div className="flex justify-end gap-2">
                  <Button
                    variant="outline"
                    onClick={() => {
                      setIsUpdateProductDialogOpen(false)
                      setSelectedVariantIds([])
                      setSelectedProduct(null)
                    }}
                  >
                    Hủy
                  </Button>
                  <Button
                    onClick={handleUpdateProductSubmit}
                    disabled={selectedVariantIds.length !== 1}
                  >
                    Cập nhật
                  </Button>
                </div>
              </>
            )}
          </div>
        </DialogContent>
      </Dialog>

      <Dialog open={isEditDialogOpen} onOpenChange={setIsEditDialogOpen}>
        <DialogContent>
          <DialogHeader>
            <DialogTitle>Cập nhật khuyến mãi</DialogTitle>
          </DialogHeader>
          {renderPromotionForm()}
          <Button onClick={handleEdit}>Cập nhật</Button>
        </DialogContent>
      </Dialog>

      <Dialog open={isViewProductsDialogOpen} onOpenChange={setIsViewProductsDialogOpen}>
        <DialogContent className="max-w-4xl">
          <DialogHeader>
            <DialogTitle>Danh sách sản phẩm áp dụng khuyến mãi</DialogTitle>
          </DialogHeader>
          <div className="space-y-4">
            <div className="flex justify-end">
              <Button onClick={openAddProductDialog}>
                <Plus className="h-4 w-4 mr-1" />
                Thêm sản phẩm
              </Button>
            </div>
            <div className="max-h-[400px] overflow-y-auto">
            <Table>
              <TableHeader>
                <TableRow>
                  <TableHead>ID</TableHead>
                  <TableHead>Biến thể</TableHead>
                  <TableHead>Thao tác</TableHead>
                </TableRow>
              </TableHeader>
              <TableBody>
                {selectedPromotionProducts.length === 0 ? (
                  <TableRow>
                    <TableCell colSpan={3} className="text-center">
                      Chưa có sản phẩm nào được áp dụng
                    </TableCell>
                  </TableRow>
                ) : (
                  selectedPromotionProducts.map((product) => (
                    <TableRow key={product.pmId}>
                      <TableCell>{product.pmId}</TableCell>
                      <TableCell>{product.variantName}</TableCell>
                      <TableCell>
                        <div className="flex items-center gap-2">
                          <Button
                            variant="destructive"
                            size="sm"
                            onClick={() => handleRemoveProduct(product.pmId)}
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
            </div>
          </div>
        </DialogContent>
      </Dialog>

      <Dialog open={isAddProductDialogOpen} onOpenChange={setIsAddProductDialogOpen}>
        <DialogContent className="max-w-4xl">
          <DialogHeader>
            <DialogTitle>Thêm sản phẩm vào khuyến mãi</DialogTitle>
          </DialogHeader>
          <div className="space-y-4">
            {isLoadingVariants ? (
              <div className="text-center py-4">Đang tải danh sách sản phẩm...</div>
            ) : (
              <>
                <div className="max-h-[400px] overflow-y-auto">
                  <Table>
                    <TableHeader>
                      <TableRow>
                        <TableHead className="w-[50px]">
                          <Checkbox
                            checked={
                              availableVariants.length > 0 &&
                              selectedVariantIds.length === availableVariants.length
                            }
                            onCheckedChange={(checked) => {
                              setSelectedVariantIds(
                                checked
                                  ? availableVariants.map((v) => v.variantId)
                                  : []
                              )
                            }}
                          />
                        </TableHead>
                        <TableHead>Tên sản phẩm</TableHead>
                        <TableHead>Biến thể</TableHead>
                        <TableHead>Giá</TableHead>
                      </TableRow>
                    </TableHeader>
                    <TableBody>
                      {availableVariants.length === 0 ? (
                        <TableRow>
                          <TableCell colSpan={4} className="text-center">
                            Không có sản phẩm nào khả dụng
                          </TableCell>
                        </TableRow>
                      ) : (
                        availableVariants.map((variant) => (
                          <TableRow key={variant.variantId}>
                            <TableCell>
                              <Checkbox
                                checked={selectedVariantIds.includes(variant.variantId)}
                                onCheckedChange={(checked) => {
                                  setSelectedVariantIds(
                                    checked
                                      ? [...selectedVariantIds, variant.variantId]
                                      : selectedVariantIds.filter(
                                          (id) => id !== variant.variantId
                                        )
                                  )
                                }}
                              />
                            </TableCell>
                            <TableCell>{variant.name}</TableCell>
                            <TableCell>
                              {variant.productsAttributes
                                .map((attr) => attr.value)
                                .join(" - ")}
                            </TableCell>
                            <TableCell>{formatCurrency(variant.price)}</TableCell>
                          </TableRow>
                        ))
                      )}
                    </TableBody>
                  </Table>
                </div>
                <div className="flex justify-end gap-2">
                  <Button
                    variant="outline"
                    onClick={() => {
                      setIsAddProductDialogOpen(false)
                      setSelectedVariantIds([])
                    }}
                  >
                    Hủy
                  </Button>
                  <Button
                    onClick={handleAddProducts}
                    disabled={selectedVariantIds.length === 0}
                  >
                    Thêm {selectedVariantIds.length} sản phẩm
                  </Button>
                </div>
              </>
            )}
          </div>
        </DialogContent>
      </Dialog>
    </div>
  )
}
