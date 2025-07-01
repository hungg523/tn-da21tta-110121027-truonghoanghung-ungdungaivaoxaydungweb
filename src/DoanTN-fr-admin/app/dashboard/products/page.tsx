"use client"

import { useState, useEffect, useRef } from "react"
import { Tabs, TabsContent, TabsList, TabsTrigger } from "@/components/ui/tabs"
import { Button } from "@/components/ui/button"
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
import { Input } from "@/components/ui/input"
import { Textarea } from "@/components/ui/textarea"
import { Switch } from "@/components/ui/switch"
import { toast } from "sonner"
import { attributeService, Attribute, AttributeValue } from "@/services/attribute.service"
import { ChevronRight, Plus, Pencil, Trash2, Image as ImageIcon, Video, X, Wand2, Loader2, Sparkles, ChevronDown } from "lucide-react"
import Swal from 'sweetalert2'
import { productService } from "@/services/product.service"
import type { Product, ProductDetail, CreateProductRequest, CreateProductDetailRequest, UpdateProductRequest } from "@/services/product.service"
import { categoryService } from "@/services/category.service"
import type { Category } from "@/services/category.service"
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select"
import React from "react"
import { variantService } from "@/services/variant.service"
import type { ProductVariant, ProductImage, CreateVariantRequest, CreateImageRequest, UpdateVariantRequest, UpdateImageRequest } from "@/services/variant.service"
import { Badge } from "@/components/ui/badge"
import { aiService, Prompt } from "@/services/ai.service"
import { Checkbox } from "@/components/ui/checkbox"
import { cn } from "@/lib/utils"
import {
  DropdownMenu,
  DropdownMenuCheckboxItem,
  DropdownMenuContent,
  DropdownMenuLabel,
  DropdownMenuSeparator,
  DropdownMenuTrigger,
} from "@/components/ui/dropdown-menu"
import VariantReviewModal from '@/app/components/product/VariantReviewModal'
import ImageGallery from '@/app/components/product/ImageGallery'

// Cấu hình mặc định cho SweetAlert2
const swalConfig = {
  customClass: {
    container: 'swal2-container-custom'
  }
}

const permittedExtensions = ['.png', '.jpg', '.jpeg', '.pdf', '.webp', '.mp4', '.avi', '.mov', '.mkv', '.flv']
const MAX_IMAGE_SIZE = 2 * 1024 * 1024 // 2MB
const MAX_VIDEO_SIZE = 5 * 1024 * 1024 // 5MB

interface FormData {
  id?: number;
  name: string;
  description: string;
  catId: number;
  isActived: boolean;
  productId?: number;
  key?: string;
  value?: string;
}

const AttributeRow = ({ 
  attribute, 
  attributeValues, 
  onAddValue,
  onEditAttribute,
  onEditValue,
  onDeleteAttribute,
  onDeleteValue 
}: { 
  attribute: Attribute; 
  attributeValues: AttributeValue[];
  onAddValue: (attribute: Attribute) => void;
  onEditAttribute: (attribute: Attribute) => void;
  onEditValue: (value: AttributeValue) => void;
  onDeleteAttribute: (attribute: Attribute) => void;
  onDeleteValue: (value: AttributeValue) => void;
}) => {
  const [isExpanded, setIsExpanded] = useState(false)
  const values = attributeValues.filter(v => v.attributeId === attribute.id)

  return (
    <>
      <TableRow>
        <TableCell>{attribute.id}</TableCell>
        <TableCell>
          <div className="flex items-center gap-2">
            {values.length > 0 && (
              <ChevronRight
                className={`h-4 w-4 cursor-pointer transition-transform ${isExpanded ? 'rotate-90' : ''}`}
                onClick={() => setIsExpanded(!isExpanded)}
              />
            )}
            {attribute.name}
          </div>
        </TableCell>
        <TableCell>
          <div className="flex items-center gap-2">
            <Button 
              variant="outline" 
              size="sm"
              onClick={() => onAddValue(attribute)}
            >
              <Plus className="h-4 w-4 mr-1" />
              Thêm giá trị
            </Button>
            <Button 
              variant="outline" 
              size="sm"
              onClick={() => onEditAttribute(attribute)}
            >
              Sửa
            </Button>
            <Button 
              variant="destructive" 
              size="sm"
              onClick={() => onDeleteAttribute(attribute)}
            >
              Xóa
            </Button>
          </div>
        </TableCell>
      </TableRow>
      {isExpanded && values.length > 0 && (
        values.map((value) => (
          <TableRow key={value.id} className="bg-gray-50">
            <TableCell></TableCell>
            <TableCell style={{ paddingLeft: '40px' }}>
              <div className="flex items-center gap-2">
                <span className="text-gray-500">-</span>
                {value.value}
              </div>
            </TableCell>
            <TableCell>
              <div className="flex items-center gap-2">
                <Button 
                  variant="outline" 
                  size="sm"
                  onClick={() => onEditValue(value)}
                >
                  Sửa
                </Button>
                <Button 
                  variant="destructive" 
                  size="sm"
                  onClick={() => onDeleteValue(value)}
                >
                  Xóa
                </Button>
              </div>
            </TableCell>
          </TableRow>
        ))
      )}
    </>
  )
}

const ProductRow = ({ 
  product, 
  productDetails,
  onEdit,
  onAddDetail,
  onEditDetail,
  onDeleteDetail
}: { 
  product: Product; 
  productDetails: ProductDetail[];
  onEdit: (product: Product) => void;
  onAddDetail: (product: Product) => void;
  onEditDetail: (detail: { productId: number; key: string; value: string }) => void;
  onDeleteDetail: (detail: { productId: number; key: string; value: string }) => void;
}) => {
  const [isExpanded, setIsExpanded] = useState(false)
  const details = productDetails.find(d => d.productId === product.id)?.detail || []

  const formatDescription = (description: string) => {
    if (!description) return '-'
    if (description.length <= 30) return description
    return description.substring(0, 30) + '...'
  }

  return (
    <>
      <TableRow>
        <TableCell>{product.id}</TableCell>
        <TableCell>
          <div className="flex items-center gap-2">
            {details.length > 0 && (
              <ChevronRight
                className={`h-4 w-4 cursor-pointer transition-transform ${isExpanded ? 'rotate-90' : ''}`}
                onClick={() => setIsExpanded(!isExpanded)}
              />
            )}
            {product.name}
          </div>
        </TableCell>
        <TableCell>
          <div 
            className="cursor-pointer hover:text-blue-500"
            onClick={() => {
              Swal.fire({
                title: 'Mô tả chi tiết',
                html: `<div class="text-left">${product.description}</div>`,
                width: '600px',
                showCloseButton: true,
                showConfirmButton: false
              })
            }}
          >
            {formatDescription(product.description)}
          </div>
        </TableCell>
        <TableCell>
          <span className={`px-2 py-1 rounded-full text-xs font-medium ${
            product.isActived === 1 
              ? 'bg-green-100 text-green-800' 
              : 'bg-red-100 text-red-800'
          }`}>
            {product.isActived === 1 ? "Đang hoạt động" : "Không hoạt động"}
          </span>
        </TableCell>
        <TableCell>
          <div className="flex items-center gap-2">
            <Button 
              variant="outline" 
              size="sm"
              onClick={() => onAddDetail(product)}
            >
              <Plus className="h-4 w-4 mr-1" />
              Thêm chi tiết
            </Button>
            <Button 
              variant="outline" 
              size="sm"
              onClick={() => onEdit(product)}
            >
              Sửa
            </Button>
          </div>
        </TableCell>
      </TableRow>
      {isExpanded && details.length > 0 && (
        details.map((detail, index) => (
          <TableRow key={index} className="bg-gray-50">
            <TableCell></TableCell>
            <TableCell style={{ paddingLeft: '40px' }}>
              <div className="flex items-center gap-2">
                <span className="text-gray-500">-</span>
                {detail.key}
              </div>
            </TableCell>
            <TableCell>{detail.value}</TableCell>
            <TableCell></TableCell>
            <TableCell>
              <div className="flex items-center gap-2">
                <Button 
                  variant="outline" 
                  size="sm"
                  onClick={() => onEditDetail({ productId: product.id, ...detail })}
                >
                  Sửa
                </Button>
                <Button 
                  variant="destructive" 
                  size="sm"
                  onClick={() => onDeleteDetail({ productId: product.id, ...detail })}
                >
                  Xóa
                </Button>
              </div>
            </TableCell>
          </TableRow>
        ))
      )}
    </>
  )
}

const AttributesTab = () => {
  const [attributes, setAttributes] = useState<Attribute[]>([])
  const [attributeValues, setAttributeValues] = useState<AttributeValue[]>([])
  const [isLoading, setIsLoading] = useState(true)
  const [isCreateDialogOpen, setIsCreateDialogOpen] = useState(false)
  const [isCreateValueDialogOpen, setIsCreateValueDialogOpen] = useState(false)
  const [isEditDialogOpen, setIsEditDialogOpen] = useState(false)
  const [isEditValueDialogOpen, setIsEditValueDialogOpen] = useState(false)
  const [selectedAttribute, setSelectedAttribute] = useState<Attribute | null>(null)
  const [selectedValue, setSelectedValue] = useState<AttributeValue | null>(null)
  const [formData, setFormData] = useState({
    name: "",
  })
  const [valueFormData, setValueFormData] = useState({
    value: "",
  })

  useEffect(() => {
    loadData()
  }, [])

  const loadData = async () => {
    try {
      setIsLoading(true)
      const [attributesData, valuesData] = await Promise.all([
        attributeService.getAllAttributes(),
        attributeService.getAllAttributeValues()
      ])
      setAttributes(attributesData)
      setAttributeValues(valuesData)
    } catch (error) {
      toast.error("Không thể tải dữ liệu thuộc tính")
    } finally {
      setIsLoading(false)
    }
  }

  const handleCreateAttribute = async () => {
    try {
      if (formData.name.length > 255) {
        Swal.fire({
          icon: 'error',
          title: 'Lỗi',
          text: 'Tên thuộc tính không được vượt quá 255 ký tự'
        })
        return
      }

      await attributeService.createAttribute({
        name: formData.name
      })
      Swal.fire({
        icon: 'success',
        title: 'Thành công',
        text: 'Tạo thuộc tính thành công'
      })
      setIsCreateDialogOpen(false)
      setFormData({
        name: "",
      })
      loadData()
    } catch (error) {
      Swal.fire({
        icon: 'error',
        title: 'Lỗi',
        text: 'Không thể tạo thuộc tính'
      })
    }
  }

  const handleCreateValue = async () => {
    if (!selectedAttribute) return

    try {
      if (valueFormData.value.length > 255) {
        Swal.fire({
          icon: 'error',
          title: 'Lỗi',
          text: 'Giá trị thuộc tính không được vượt quá 255 ký tự'
        })
        return
      }

      await attributeService.createAttributeValue({
        attributeId: selectedAttribute.id,
        value: valueFormData.value
      })
      Swal.fire({
        icon: 'success',
        title: 'Thành công',
        text: 'Tạo giá trị thuộc tính thành công'
      })
      setIsCreateValueDialogOpen(false)
      setValueFormData({
        value: "",
      })
      loadData()
    } catch (error) {
      Swal.fire({
        icon: 'error',
        title: 'Lỗi',
        text: 'Không thể tạo giá trị thuộc tính'
      })
    }
  }

  const handleEditAttribute = async () => {
    if (!selectedAttribute) return

    try {
      if (formData.name.length > 255) {
        Swal.fire({
          icon: 'error',
          title: 'Lỗi',
          text: 'Tên thuộc tính không được vượt quá 255 ký tự'
        })
        return
      }

      await attributeService.updateAttribute(selectedAttribute.id, {
        name: formData.name
      })
      Swal.fire({
        icon: 'success',
        title: 'Thành công',
        text: 'Cập nhật thuộc tính thành công'
      })
      setIsEditDialogOpen(false)
      setFormData({
        name: "",
      })
      loadData()
    } catch (error) {
      Swal.fire({
        icon: 'error',
        title: 'Lỗi',
        text: 'Không thể cập nhật thuộc tính'
      })
    }
  }

  const handleEditValue = async () => {
    if (!selectedValue) return

    try {
      if (valueFormData.value.length > 255) {
        Swal.fire({
          icon: 'error',
          title: 'Lỗi',
          text: 'Giá trị thuộc tính không được vượt quá 255 ký tự'
        })
        return
      }

      await attributeService.updateAttributeValue(selectedValue.id, {
        attributeId: selectedValue.attributeId,
        value: valueFormData.value
      })
      Swal.fire({
        icon: 'success',
        title: 'Thành công',
        text: 'Cập nhật giá trị thuộc tính thành công'
      })
      setIsEditValueDialogOpen(false)
      setValueFormData({
        value: "",
      })
      loadData()
    } catch (error) {
      Swal.fire({
        icon: 'error',
        title: 'Lỗi',
        text: 'Không thể cập nhật giá trị thuộc tính'
      })
    }
  }

  const handleDeleteAttribute = async (attribute: Attribute) => {
    const result = await Swal.fire({
      title: 'Xác nhận xóa',
      text: `Bạn có chắc chắn muốn xóa thuộc tính "${attribute.name}"?`,
      icon: 'warning',
      showCancelButton: true,
      confirmButtonColor: '#d33',
      cancelButtonColor: '#3085d6',
      confirmButtonText: 'Xóa',
      cancelButtonText: 'Hủy'
    })

    if (result.isConfirmed) {
      try {
        await attributeService.deleteAttribute(attribute.id)
        Swal.fire({
          icon: 'success',
          title: 'Thành công',
          text: 'Xóa thuộc tính thành công'
        })
        loadData()
      } catch (error) {
        Swal.fire({
          icon: 'error',
          title: 'Lỗi',
          text: 'Không thể xóa thuộc tính'
        })
      }
    }
  }

  const handleDeleteValue = async (value: AttributeValue) => {
    const result = await Swal.fire({
      title: 'Xác nhận xóa',
      text: `Bạn có chắc chắn muốn xóa giá trị "${value.value}"?`,
      icon: 'warning',
      showCancelButton: true,
      confirmButtonColor: '#d33',
      cancelButtonColor: '#3085d6',
      confirmButtonText: 'Xóa',
      cancelButtonText: 'Hủy'
    })

    if (result.isConfirmed) {
      try {
        await attributeService.deleteAttributeValue(value.id)
        Swal.fire({
          icon: 'success',
          title: 'Thành công',
          text: 'Xóa giá trị thuộc tính thành công'
        })
        loadData()
      } catch (error) {
        Swal.fire({
          icon: 'error',
          title: 'Lỗi',
          text: 'Không thể xóa giá trị thuộc tính'
        })
      }
    }
  }

  const openCreateValueDialog = (attribute: Attribute) => {
    setSelectedAttribute(attribute)
    setIsCreateValueDialogOpen(true)
  }

  const openEditAttributeDialog = (attribute: Attribute) => {
    setSelectedAttribute(attribute)
    setFormData({
      name: attribute.name
    })
    setIsEditDialogOpen(true)
  }

  const openEditValueDialog = (value: AttributeValue) => {
    setSelectedValue(value)
    setValueFormData({
      value: value.value
    })
    setIsEditValueDialogOpen(true)
  }

  return (
        <div>
      <div className="flex justify-between items-center mb-6">
        <h2 className="text-xl font-semibold">Quản lý thuộc tính</h2>
        <Dialog open={isCreateDialogOpen} onOpenChange={setIsCreateDialogOpen}>
          <DialogTrigger asChild>
            <Button>Tạo thuộc tính mới</Button>
          </DialogTrigger>
          <DialogContent>
            <DialogHeader>
              <DialogTitle>Tạo thuộc tính mới</DialogTitle>
            </DialogHeader>
            <div className="space-y-4">
              <div>
                <label className="block text-sm font-medium mb-1">Tên thuộc tính</label>
                <Input
                  value={formData.name}
                  onChange={(e) => setFormData({ name: e.target.value })}
                  maxLength={255}
                />
        </div>
              <Button onClick={handleCreateAttribute}>Tạo</Button>
            </div>
          </DialogContent>
        </Dialog>
      </div>

      <Dialog open={isCreateValueDialogOpen} onOpenChange={setIsCreateValueDialogOpen}>
        <DialogContent>
          <DialogHeader>
            <DialogTitle>Thêm giá trị thuộc tính</DialogTitle>
          </DialogHeader>
          <div className="space-y-4">
            <div>
              <label className="block text-sm font-medium mb-1">Thuộc tính</label>
              <Input
                value={selectedAttribute?.name || ''}
                disabled
              />
            </div>
            <div>
              <label className="block text-sm font-medium mb-1">Giá trị</label>
              <Input
                value={valueFormData.value}
                onChange={(e) => setValueFormData({ value: e.target.value })}
                maxLength={255}
              />
            </div>
            <Button onClick={handleCreateValue}>Thêm</Button>
          </div>
        </DialogContent>
      </Dialog>

      <Dialog open={isEditDialogOpen} onOpenChange={setIsEditDialogOpen}>
        <DialogContent>
          <DialogHeader>
            <DialogTitle>Cập nhật thuộc tính</DialogTitle>
          </DialogHeader>
          <div className="space-y-4">
            <div>
              <label className="block text-sm font-medium mb-1">Tên thuộc tính</label>
              <Input
                value={formData.name}
                onChange={(e) => setFormData({ name: e.target.value })}
                maxLength={255}
              />
            </div>
            <Button onClick={handleEditAttribute}>Lưu</Button>
          </div>
        </DialogContent>
      </Dialog>

      <Dialog open={isEditValueDialogOpen} onOpenChange={setIsEditValueDialogOpen}>
        <DialogContent>
          <DialogHeader>
            <DialogTitle>Cập nhật giá trị thuộc tính</DialogTitle>
          </DialogHeader>
          <div className="space-y-4">
            <div>
              <label className="block text-sm font-medium mb-1">Thuộc tính</label>
              <Input
                value={selectedValue ? attributes.find(a => a.id === selectedValue.attributeId)?.name || '' : ''}
                disabled
              />
            </div>
            <div>
              <label className="block text-sm font-medium mb-1">Giá trị</label>
              <Input
                value={valueFormData.value}
                onChange={(e) => setValueFormData({ value: e.target.value })}
                maxLength={255}
              />
            </div>
            <Button onClick={handleEditValue}>Lưu</Button>
          </div>
        </DialogContent>
      </Dialog>

      <Table>
        <TableHeader>
          <TableRow>
            <TableHead>ID</TableHead>
            <TableHead>Tên</TableHead>
            <TableHead>Thao tác</TableHead>
          </TableRow>
        </TableHeader>
        <TableBody>
          {isLoading ? (
            <TableRow>
              <TableCell colSpan={3} className="text-center">Đang tải...</TableCell>
            </TableRow>
          ) : attributes.length === 0 ? (
            <TableRow>
              <TableCell colSpan={3} className="text-center">Không có thuộc tính nào</TableCell>
            </TableRow>
          ) : (
            attributes.map((attribute) => (
              <AttributeRow
                key={attribute.id}
                attribute={attribute}
                attributeValues={attributeValues}
                onAddValue={openCreateValueDialog}
                onEditAttribute={openEditAttributeDialog}
                onEditValue={openEditValueDialog}
                onDeleteAttribute={handleDeleteAttribute}
                onDeleteValue={handleDeleteValue}
              />
            ))
          )}
        </TableBody>
      </Table>
    </div>
  )
}

const ProductsTab = () => {
  const [products, setProducts] = useState<Product[]>([])
  const [productDetails, setProductDetails] = useState<ProductDetail[]>([])
  const [categories, setCategories] = useState<Category[]>([])
  const [isLoading, setIsLoading] = useState(true)
  const [isCreateDialogOpen, setIsCreateDialogOpen] = useState(false)
  const [isEditDialogOpen, setIsEditDialogOpen] = useState(false)
  const [isCreateDetailDialogOpen, setIsCreateDetailDialogOpen] = useState(false)
  const [isEditDetailDialogOpen, setIsEditDetailDialogOpen] = useState(false)
  const [selectedProduct, setSelectedProduct] = useState<Product | null>(null)
  const [selectedDetail, setSelectedDetail] = useState<{ productId: number; key: string; value: string } | null>(null)
  const [formData, setFormData] = useState<FormData>({
    name: "",
    description: "",
    catId: 0,
    isActived: true
  })
  const [detailFormData, setDetailFormData] = useState<CreateProductDetailRequest>({
    productId: 0,
    detailKey: "",
    detailValue: "",
  })
  const [isGenerating, setIsGenerating] = useState(false)
  const [isSelectPromptDialogOpen, setIsSelectPromptDialogOpen] = useState(false)
  const [prompts, setPrompts] = useState<Prompt[]>([])
  const [selectedPrompt, setSelectedPrompt] = useState<Prompt | null>(null)
  const [isEditPromptDialogOpen, setIsEditPromptDialogOpen] = useState(false)
  const [selectedPromptForEdit, setSelectedPromptForEdit] = useState<Prompt | null>(null)
  const [promptFormData, setPromptFormData] = useState({
    name: "",
    content: ""
  })
  const [isCreatePromptDialogOpen, setIsCreatePromptDialogOpen] = useState(false)
  const [newPromptFormData, setNewPromptFormData] = useState({
    name: "",
    content: ""
  })
  const [productSearchTerm, setProductSearchTerm] = useState("")

  const loadPrompts = async () => {
    try {
      const promptsData = await aiService.getAllPrompts()
      setPrompts(promptsData)
    } catch (error) {
      Swal.fire({
        icon: 'error',
        title: 'Lỗi',
        text: 'Không thể tải danh sách prompt'
      })
    }
  }

  const handleGenerateDescription = async () => {
    if (!selectedPrompt) {
      Swal.fire({
        icon: 'error',
        title: 'Lỗi',
        text: 'Vui lòng chọn prompt',
        ...swalConfig
      })
      return
    }

    try {
      setIsGenerating(true)
      const description = await aiService.generateDescription({
        prompt: selectedPrompt.content
      })
      setFormData(prev => ({
        ...prev,
        description: description
      }))
      setIsSelectPromptDialogOpen(false)
      setSelectedPrompt(null)
    } catch (error) {
      Swal.fire({
        icon: 'error',
        title: 'Lỗi',
        text: 'Không thể tạo mô tả',
        ...swalConfig
      })
    } finally {
      setIsGenerating(false)
    }
  }

  const handleEditPrompt = async () => {
    if (!selectedPromptForEdit) return

    try {
      await aiService.updatePrompt(selectedPromptForEdit.id, {
        name: promptFormData.name,
        content: promptFormData.content
      })
      Swal.fire({
        icon: 'success',
        title: 'Thành công',
        text: 'Cập nhật prompt thành công',
        ...swalConfig
      })
      setIsEditPromptDialogOpen(false)
      loadPrompts()
    } catch (error) {
      Swal.fire({
        icon: 'error',
        title: 'Lỗi',
        text: 'Không thể cập nhật prompt',
        ...swalConfig
      })
    }
  }

  const handleDeletePrompt = async (prompt: Prompt) => {
    const result = await Swal.fire({
      title: 'Xác nhận xóa',
      text: `Bạn có chắc chắn muốn xóa prompt "${prompt.name}"?`,
      icon: 'warning',
      showCancelButton: true,
      confirmButtonColor: '#d33',
      cancelButtonColor: '#3085d6',
      confirmButtonText: 'Xóa',
      cancelButtonText: 'Hủy',
      ...swalConfig
    })

    if (result.isConfirmed) {
      try {
        await aiService.deletePrompt(prompt.id)
        Swal.fire({
          icon: 'success',
          title: 'Thành công',
          text: 'Xóa prompt thành công',
          ...swalConfig
        })
        loadPrompts()
      } catch (error) {
        Swal.fire({
          icon: 'error',
          title: 'Lỗi',
          text: 'Không thể xóa prompt',
          ...swalConfig
        })
      }
    }
  }

  const openEditPromptDialog = (prompt: Prompt) => {
    setSelectedPromptForEdit(prompt)
    setPromptFormData({
      name: prompt.name || "",
      content: prompt.content
    })
    setIsEditPromptDialogOpen(true)
  }

  const renderCategoryOptions = (categories: Category[]) => {
    const renderCategory = (category: Category, level = 0) => {
      return (
        <React.Fragment key={category.id}>
          <SelectItem value={category.id.toString()}>
            {'\u00A0'.repeat(level * 2)}{level > 0 ? '└ ' : ''}{category.name}
          </SelectItem>
          {category.categories?.map(child => renderCategory(child, level + 1))}
        </React.Fragment>
      );
    };

    return categories.map(category => renderCategory(category));
  };

  useEffect(() => {
    loadData()
  }, [])

  const loadData = async () => {
    try {
      setIsLoading(true)
      const [productsData, detailsData, categoriesData] = await Promise.all([
        productService.getAllProducts(),
        productService.getAllProductDetails(),
        categoryService.getAll()
      ])
      setCategories(categoriesData || [])
      setProducts(productsData || [])
      setProductDetails(detailsData)
    } catch (error) {
      Swal.fire({
        icon: 'error',
        title: 'Lỗi',
        text: 'Không thể tải dữ liệu sản phẩm'
      })
    } finally {
      setIsLoading(false)
    }
  }

  const handleCreate = async () => {
    try {
      setIsLoading(true)
      const createRequest: CreateProductRequest = {
        name: formData.name,
        description: formData.description,
        categoryId: formData.catId,
        isActived: formData.isActived ? 1 : 0
      }

      const response = await productService.createProduct(createRequest)
      if (response.isSuccess) {
        Swal.fire({
          title: "Thành công!",
          text: "Sản phẩm đã được tạo thành công",
          icon: "success",
          ...swalConfig
        })
        loadData()
        setIsCreateDialogOpen(false)
      } else {
        throw new Error("Không thể tạo sản phẩm")
      }
    } catch (error) {
      Swal.fire({
        title: "Lỗi!",
        text: error instanceof Error ? error.message : "Không thể tạo sản phẩm",
        icon: "error",
        ...swalConfig
      })
    } finally {
      setIsLoading(false)
    }
  }

  const handleEdit = async () => {
    if (!selectedProduct) return

    try {
      const updateRequest: UpdateProductRequest = {
        name: formData.name,
        description: formData.description,
        categoryId: formData.catId,
        isActived: formData.isActived ? 1 : 0
      }

      const response = await productService.updateProduct(selectedProduct.id, updateRequest)
      if (response.isSuccess) {
        Swal.fire({
          title: "Thành công!",
          text: "Sản phẩm đã được cập nhật thành công",
          icon: "success",
          ...swalConfig
        })
        loadData()
        setIsEditDialogOpen(false)
      } else {
        throw new Error("Không thể cập nhật sản phẩm")
      }
    } catch (error) {
      Swal.fire({
        title: "Lỗi!",
        text: error instanceof Error ? error.message : "Không thể cập nhật sản phẩm",
        icon: "error",
        ...swalConfig
      })
    }
  }

  const handleCreateDetail = async () => {
    if (!selectedProduct) return

    try {
      await productService.createProductDetail({
        productId: selectedProduct.id,
        detailKey: detailFormData.detailKey,
        detailValue: detailFormData.detailValue
      })
      Swal.fire({
        icon: 'success',
        title: 'Thành công',
        text: 'Thêm chi tiết sản phẩm thành công'
      })
      setIsCreateDetailDialogOpen(false)
      setDetailFormData({
        productId: 0,
        detailKey: "",
        detailValue: "",
      })
      loadData()
    } catch (error) {
      Swal.fire({
        icon: 'error',
        title: 'Lỗi',
        text: 'Không thể thêm chi tiết sản phẩm'
      })
    }
  }

  const handleEditDetail = async () => {
    if (!selectedDetail) return

    try {
      await productService.updateProductDetail(selectedDetail.productId, {
        productId: selectedDetail.productId,
        detailKey: detailFormData.detailKey,
        detailValue: detailFormData.detailValue
      })
      Swal.fire({
        icon: 'success',
        title: 'Thành công',
        text: 'Cập nhật chi tiết sản phẩm thành công'
      })
      setIsEditDetailDialogOpen(false)
      setSelectedDetail(null)
      setDetailFormData({
        productId: 0,
        detailKey: "",
        detailValue: "",
      })
      loadData()
    } catch (error) {
      Swal.fire({
        icon: 'error',
        title: 'Lỗi',
        text: 'Không thể cập nhật chi tiết sản phẩm'
      })
    }
  }

  const handleDeleteDetail = async (detail: { productId: number; key: string; value: string }) => {
    const result = await Swal.fire({
      title: 'Xác nhận xóa',
      text: `Bạn có chắc chắn muốn xóa chi tiết "${detail.key}"?`,
      icon: 'warning',
      showCancelButton: true,
      confirmButtonColor: '#d33',
      cancelButtonColor: '#3085d6',
      confirmButtonText: 'Xóa',
      cancelButtonText: 'Hủy'
    })

    if (result.isConfirmed) {
      try {
        await productService.deleteProductDetail(detail.productId)
        Swal.fire({
          icon: 'success',
          title: 'Thành công',
          text: 'Xóa chi tiết sản phẩm thành công'
        })
        loadData()
      } catch (error) {
        Swal.fire({
          icon: 'error',
          title: 'Lỗi',
          text: 'Không thể xóa chi tiết sản phẩm'
        })
      }
    }
  }

  const openEditDialog = (product: Product) => {
    setSelectedProduct(product); // Đảm bảo selectedProduct luôn được set
    setFormData({
      id: product.id,
      name: product.name,
      description: product.description,
      catId: product.categoryId,
      isActived: product.isActived === 1
    })
    setIsEditDialogOpen(true)
  }

  const openCreateDetailDialog = (product: Product) => {
    setSelectedProduct(product)
    setDetailFormData({
      productId: product.id,
      detailKey: "",
      detailValue: "",
    })
    setIsCreateDetailDialogOpen(true)
  }

  const openEditDetailDialog = (detail: { productId: number; key: string; value: string }) => {
    setSelectedDetail(detail)
    setDetailFormData({
      productId: detail.productId,
      detailKey: detail.key,
      detailValue: detail.value,
    })
    setIsEditDetailDialogOpen(true)
  }

  const handleCreatePrompt = async () => {
    try {
      await aiService.createPrompt({
        name: newPromptFormData.name,
        content: newPromptFormData.content
      })
      Swal.fire({
        icon: 'success',
        title: 'Thành công',
        text: 'Tạo prompt thành công',
        ...swalConfig
      })
      setIsCreatePromptDialogOpen(false)
      setNewPromptFormData({
        name: "",
        content: ""
      })
      loadPrompts()
    } catch (error) {
      Swal.fire({
        icon: 'error',
        title: 'Lỗi',
        text: 'Không thể tạo prompt',
        ...swalConfig
      })
    }
  }

  return (
    <div>
      <div className="flex justify-between items-center mb-6">
        <h2 className="text-xl font-semibold">Quản lý sản phẩm</h2>
        <Dialog open={isCreateDialogOpen} onOpenChange={setIsCreateDialogOpen}>
          <DialogTrigger asChild>
            <Button>Tạo sản phẩm mới</Button>
          </DialogTrigger>
          <DialogContent>
            <DialogHeader>
              <DialogTitle>Tạo sản phẩm mới</DialogTitle>
            </DialogHeader>
            <div className="space-y-4">
              <div>
                <label className="block text-sm font-medium mb-1">Danh mục</label>
                <Select
                  value={formData.catId ? formData.catId.toString() : undefined}
                  onValueChange={(value) => setFormData({ ...formData, catId: parseInt(value) })}
                >
                  <SelectTrigger className="w-full">
                    <SelectValue placeholder="Chọn danh mục" />
                  </SelectTrigger>
                  <SelectContent>
                    <SelectItem value="0">Chọn danh mục</SelectItem>
                    {categories.map((category) => (
                      <React.Fragment key={category.id}>
                        <SelectItem value={category.id.toString()}>
                          {category.name}
                        </SelectItem>
                        {category.categories?.map((child) => (
                          <SelectItem key={child.id} value={child.id.toString()}>
                            {'\u00A0\u00A0└ '}{child.name}
                          </SelectItem>
                        ))}
                      </React.Fragment>
                    ))}
                  </SelectContent>
                </Select>
              </div>
              <div>
                <label className="block text-sm font-medium mb-1">Tên sản phẩm</label>
                <Input
                  value={formData.name}
                  onChange={(e) => setFormData({ ...formData, name: e.target.value })}
                  maxLength={255}
                />
              </div>
              <div className="space-y-2">
                <div className="flex items-center justify-between">
                  <label className="block text-sm font-medium">Mô tả</label>
                  <Button
                    type="button"
                    variant="outline"
                    size="sm"
                    onClick={() => {
                      loadPrompts()
                      setIsSelectPromptDialogOpen(true)
                    }}
                  >
                    <Sparkles className="h-4 w-4 mr-1" />
                    Tạo mô tả bằng AI
                  </Button>
                </div>
                <Textarea
                  value={formData.description}
                  onChange={(e) => setFormData({ ...formData, description: e.target.value })}
                  rows={6}
                  className="w-full"
                />
              </div>
              <div className="flex items-center space-x-2">
                <Switch
                  checked={formData.isActived}
                  onCheckedChange={(checked) => setFormData({ ...formData, isActived: checked })}
                />
                <label className="text-sm font-medium">Kích hoạt</label>
              </div>
              <Button onClick={handleCreate}>Tạo</Button>
            </div>
          </DialogContent>
        </Dialog>
      </div>

      <Dialog open={isEditDialogOpen} onOpenChange={setIsEditDialogOpen}>
        <DialogContent>
          <DialogHeader>
            <DialogTitle>Cập nhật sản phẩm</DialogTitle>
          </DialogHeader>
          <div className="space-y-4">
            <div>
              <label className="block text-sm font-medium mb-1">Danh mục</label>
              <Select
                value={formData.catId ? formData.catId.toString() : undefined}
                onValueChange={(value) => setFormData({ ...formData, catId: parseInt(value) })}
              >
                <SelectTrigger className="w-full">
                  <SelectValue placeholder="Chọn danh mục" />
                  </SelectTrigger>
                  <SelectContent>
                  <SelectItem value="0">Chọn danh mục</SelectItem>
                  {categories.map((category) => (
                    <React.Fragment key={category.id}>
                      <SelectItem value={category.id.toString()}>
                        {category.name}
                      </SelectItem>
                      {category.categories?.map((child) => (
                        <SelectItem key={child.id} value={child.id.toString()}>
                          {'\u00A0\u00A0└ '}{child.name}
                        </SelectItem>
                      ))}
                    </React.Fragment>
                  ))}
                  </SelectContent>
                </Select>
              </div>
            <div className="col-span-1">
              <label className="block text-sm font-medium mb-1">Tên sản phẩm</label>
              <Input
                value={formData.name}
                onChange={(e) => setFormData({ ...formData, name: e.target.value })}
                maxLength={255}
              />
              </div>
            <div className="space-y-2">
              <div className="flex items-center justify-between">
                <label className="block text-sm font-medium">Mô tả</label>
                <Button
                  type="button"
                  variant="outline"
                  size="sm"
                  onClick={() => {
                    loadPrompts()
                    setIsSelectPromptDialogOpen(true)
                  }}
                >
                  <Sparkles className="h-4 w-4 mr-1" />
                  Tạo mô tả bằng AI
                </Button>
              </div>
              <Textarea
                value={formData.description}
                onChange={(e) => setFormData({ ...formData, description: e.target.value })}
                rows={6}
                className="w-full"
              />
              </div>
            <div className="col-span-1 md:col-span-2 flex items-center space-x-2">
              <Switch
                checked={formData.isActived}
                onCheckedChange={(checked) => setFormData({ ...formData, isActived: checked })}
              />
              <label className="text-sm font-medium">Kích hoạt</label>
              </div>
            <div className="col-span-1 md:col-span-2">
              <Button onClick={handleEdit} className="w-full md:w-auto">Cập nhật</Button>
            </div>
          </div>
        </DialogContent>
      </Dialog>

      <Dialog open={isCreateDetailDialogOpen} onOpenChange={setIsCreateDetailDialogOpen}>
        <DialogContent>
          <DialogHeader>
            <DialogTitle>Thêm chi tiết sản phẩm</DialogTitle>
          </DialogHeader>
          <div className="space-y-4">
            <div>
              <label className="block text-sm font-medium mb-1">Sản phẩm</label>
              <Input
                value={selectedProduct?.name || ''}
                disabled
              />
            </div>
            <div>
              <label className="block text-sm font-medium mb-1">Tên thuộc tính</label>
              <Input
                value={detailFormData.detailKey}
                onChange={(e) => setDetailFormData({ ...detailFormData, detailKey: e.target.value })}
              />
            </div>
            <div>
              <label className="block text-sm font-medium mb-1">Giá trị</label>
              <Input
                value={detailFormData.detailValue}
                onChange={(e) => setDetailFormData({ ...detailFormData, detailValue: e.target.value })}
              />
            </div>
            <Button onClick={handleCreateDetail}>Thêm</Button>
          </div>
        </DialogContent>
      </Dialog>

      <Dialog open={isEditDetailDialogOpen} onOpenChange={setIsEditDetailDialogOpen}>
        <DialogContent>
          <DialogHeader>
            <DialogTitle>Cập nhật chi tiết sản phẩm</DialogTitle>
          </DialogHeader>
          <div className="space-y-4">
            <div>
              <label className="block text-sm font-medium mb-1">Sản phẩm</label>
              <Input
                value={selectedProduct?.name || ''}
                disabled
              />
            </div>
            <div>
              <label className="block text-sm font-medium mb-1">Tên thuộc tính</label>
              <Input
                value={detailFormData.detailKey}
                onChange={(e) => setDetailFormData({ ...detailFormData, detailKey: e.target.value })}
              />
            </div>
            <div>
              <label className="block text-sm font-medium mb-1">Giá trị</label>
              <Input
                value={detailFormData.detailValue}
                onChange={(e) => setDetailFormData({ ...detailFormData, detailValue: e.target.value })}
              />
            </div>
            <Button onClick={handleEditDetail}>Cập nhật</Button>
          </div>
        </DialogContent>
      </Dialog>

      <Table>
        <TableHeader>
          <TableRow>
            <TableHead>ID</TableHead>
            <TableHead>Tên</TableHead>
            <TableHead>Mô tả</TableHead>
            <TableHead>Trạng thái</TableHead>
            <TableHead>Thao tác</TableHead>
          </TableRow>
        </TableHeader>
        <TableBody>
          {isLoading ? (
            <TableRow>
              <TableCell colSpan={5} className="text-center">Đang tải...</TableCell>
            </TableRow>
          ) : products.length === 0 ? (
            <TableRow>
              <TableCell colSpan={5} className="text-center">Không có sản phẩm nào</TableCell>
            </TableRow>
          ) : (
            products.map((product) => (
              <ProductRow
                key={product.id}
                product={product}
                productDetails={productDetails}
                onEdit={openEditDialog}
                onAddDetail={openCreateDetailDialog}
                onEditDetail={openEditDetailDialog}
                onDeleteDetail={handleDeleteDetail}
              />
            ))
          )}
        </TableBody>
      </Table>

      <Dialog open={isSelectPromptDialogOpen} onOpenChange={setIsSelectPromptDialogOpen}>
        <DialogContent className="max-w-3xl">
          <DialogHeader>
            <DialogTitle>Chọn prompt</DialogTitle>
          </DialogHeader>
          <div className="space-y-4">
            <div className="flex justify-end">
              <Button
                variant="outline"
                onClick={() => setIsCreatePromptDialogOpen(true)}
              >
                <Plus className="h-4 w-4 mr-1" />
                Thêm prompt mới
              </Button>
            </div>
            <div className="max-h-[300px] overflow-y-auto">
              {prompts.map((prompt) => (
                <div
                  key={prompt.id}
                  className={`p-4 rounded-lg cursor-pointer mb-2 ${
                    selectedPrompt?.id === prompt.id
                      ? 'bg-blue-100 border border-blue-500'
                      : 'hover:bg-gray-100 border border-gray-200'
                  }`}
                  onClick={() => setSelectedPrompt(prompt)}
                >
                  <div className="flex justify-between items-start">
                    <div className="font-medium">{prompt.name || "Không có tên"}</div>
                    <div className="flex gap-2">
                      <Button
                        variant="outline"
                        size="sm"
                        onClick={(e) => {
                          e.stopPropagation()
                          openEditPromptDialog(prompt)
                        }}
                      >
                        <Pencil className="h-4 w-4" />
                      </Button>
                      <Button
                        variant="destructive"
                        size="sm"
                        onClick={(e) => {
                          e.stopPropagation()
                          handleDeletePrompt(prompt)
                        }}
                      >
                        <Trash2 className="h-4 w-4" />
                      </Button>
                    </div>
                  </div>
                  <div className="text-sm text-gray-500 mt-1">
                    {prompt.content.length > 100
                      ? prompt.content.substring(0, 100) + "..."
                      : prompt.content}
                  </div>
                </div>
              ))}
            </div>
            <div className="flex justify-end gap-2">
              <Button
                variant="outline"
                onClick={() => {
                  setIsSelectPromptDialogOpen(false)
                  setSelectedPrompt(null)
                }}
              >
                Hủy
              </Button>
              <Button 
                onClick={handleGenerateDescription}
                disabled={isGenerating}
              >
                {isGenerating ? (
                  <>
                    <Loader2 className="mr-2 h-4 w-4 animate-spin" />
                    Đang tạo mô tả...
                  </>
                ) : (
                  'Tạo mô tả'
                )}
              </Button>
            </div>
          </div>
          </DialogContent>
        </Dialog>

      <Dialog open={isEditPromptDialogOpen} onOpenChange={setIsEditPromptDialogOpen}>
        <DialogContent>
          <DialogHeader>
            <DialogTitle>Chỉnh sửa prompt</DialogTitle>
          </DialogHeader>
          <div className="space-y-4">
            <div>
              <label className="block text-sm font-medium mb-1">Tên prompt</label>
              <Input
                value={promptFormData.name}
                onChange={(e) => setPromptFormData({ ...promptFormData, name: e.target.value })}
                placeholder="Nhập tên prompt (không bắt buộc)"
              />
      </div>
            <div>
              <label className="block text-sm font-medium mb-1">Nội dung prompt</label>
              <Textarea
                value={promptFormData.content}
                onChange={(e) => setPromptFormData({ ...promptFormData, content: e.target.value })}
                rows={6}
                className="w-full"
                placeholder="Nhập nội dung prompt"
              />
            </div>
            <div className="flex justify-end gap-2">
              <Button
                variant="outline"
                onClick={() => setIsEditPromptDialogOpen(false)}
              >
                Hủy
              </Button>
              <Button onClick={handleEditPrompt}>
                Lưu
              </Button>
            </div>
          </div>
        </DialogContent>
      </Dialog>

      <Dialog open={isCreatePromptDialogOpen} onOpenChange={setIsCreatePromptDialogOpen}>
        <DialogContent>
          <DialogHeader>
            <DialogTitle>Thêm prompt mới</DialogTitle>
          </DialogHeader>
          <div className="space-y-4">
            <div>
              <label className="block text-sm font-medium mb-1">Tên prompt</label>
          <Input
                value={newPromptFormData.name}
                onChange={(e) => setNewPromptFormData({ ...newPromptFormData, name: e.target.value })}
                placeholder="Nhập tên prompt (không bắt buộc)"
              />
            </div>
            <div>
              <label className="block text-sm font-medium mb-1">Nội dung prompt</label>
              <Textarea
                value={newPromptFormData.content}
                onChange={(e) => setNewPromptFormData({ ...newPromptFormData, content: e.target.value })}
                rows={6}
            className="w-full"
                placeholder="Nhập nội dung prompt"
              />
            </div>
            <div className="flex justify-end gap-2">
              <Button
                variant="outline"
                onClick={() => setIsCreatePromptDialogOpen(false)}
              >
                Hủy
              </Button>
              <Button onClick={handleCreatePrompt}>
                Thêm
          </Button>
        </div>
          </div>
        </DialogContent>
      </Dialog>
    </div>
  )
}

const ProductVariantsTab = () => {
  const [variants, setVariants] = useState<ProductVariant[]>([])
  const [categories, setCategories] = useState<Category[]>([])
  const [products, setProducts] = useState<Product[]>([])
  const [attributes, setAttributes] = useState<Attribute[]>([])
  const [attributeValues, setAttributeValues] = useState<AttributeValue[]>([])
  const [isLoading, setIsLoading] = useState(true)
  const [selectedCategory, setSelectedCategory] = useState<number | undefined>()
  const [isActived, setIsActived] = useState<number | undefined>(1)
  const [isCreateDialogOpen, setIsCreateDialogOpen] = useState(false)
  const [isEditDialogOpen, setIsEditDialogOpen] = useState(false)
  const [isImageDialogOpen, setIsImageDialogOpen] = useState(false)
  const [selectedVariant, setSelectedVariant] = useState<ProductVariant | null>(null)
  const [selectedVariantForImage, setSelectedVariantForImage] = useState<ProductVariant | null>(null)
  const [formData, setFormData] = useState<CreateVariantRequest>({
    productId: 0,
    price: 0,
    stock: 0,
    isActived: 1,
    avIds: []
  })
  const [newImage, setNewImage] = useState<CreateImageRequest>({
    title: '',
    imageData: '',
    position: 0,
    variantId: 0
  })
  const fileInputRef = useRef<HTMLInputElement>(null)
  const [isEditImageDialogOpen, setIsEditImageDialogOpen] = useState(false)
  const [selectedImage, setSelectedImage] = useState<ProductImage | null>(null)
  const [editImageData, setEditImageData] = useState<UpdateImageRequest>({
    title: '',
    imageData: '',
    position: 0,
    variantId: 0
  })
  const [selectedCounts, setSelectedCounts] = useState({
    color: 0,
    storage: 0
  })
  const [isReviewModalOpen, setIsReviewModalOpen] = useState(false)
  const [selectedVariantForReview, setSelectedVariantForReview] = useState<number | null>(null)
  const [searchTerms, setSearchTerms] = useState<{ [key: number]: string }>({})
  const [productSearchTerm, setProductSearchTerm] = useState("")
  // Thêm state phân trang
  const [skip, setSkip] = useState(0)
  const [take, setTake] = useState(25)
  const [total, setTotal] = useState(0)
  const scrollRef = useRef<HTMLDivElement>(null)
  const [searchName, setSearchName] = useState("")
  const [searchNameDebounced, setSearchNameDebounced] = useState("")
  // Infinite scroll lắng nghe window, chống gọi loadData liên tục
  const isFetchingRef = useRef(false)

  // Debounce searchName
  useEffect(() => {
    const handler = setTimeout(() => {
      setSearchNameDebounced(searchName)
    }, 400)
    return () => clearTimeout(handler)
  }, [searchName])

  useEffect(() => {
    setSkip(0)
    setVariants([])
    loadData(0, take, true)
  }, [selectedCategory, isActived, take, searchNameDebounced])

  // Sửa loadData trả về dữ liệu vừa load (để setSkip đúng)
  const loadData = async (
    skipParam = skip,
    takeParam = take,
    reset = false
  ): Promise<{ productVariants: ProductVariant[]; totalItems: number } | null> => {
    try {
      setIsLoading(true)
      let variantRes: { productVariants: ProductVariant[]; totalItems: number }, categoriesData, productsData, attributesData, valuesData
      if (searchNameDebounced.trim() !== "") {
        variantRes = await variantService.search(searchNameDebounced, skipParam, takeParam)
        ;[categoriesData, productsData, attributesData, valuesData] = await Promise.all([
          categoryService.getAll(),
          productService.getAllProducts(),
          attributeService.getAllAttributes(),
          attributeService.getAllAttributeValues()
        ])
      } else {
        const res = await Promise.all([
          variantService.getAll({
            categoryId: selectedCategory,
            isActived: isActived,
            skip: skipParam,
            take: takeParam
          }),
          categoryService.getAll(),
          productService.getAllProducts(),
          attributeService.getAllAttributes(),
          attributeService.getAllAttributeValues()
        ])
        variantRes = res[0]
        categoriesData = res[1]
        productsData = res[2]
        attributesData = res[3]
        valuesData = res[4]
      }
      setCategories(categoriesData || [])
      setProducts(productsData || [])
      setAttributes(attributesData || [])
      setAttributeValues(valuesData || [])
      setTotal(variantRes.totalItems)
      if (reset) {
        setVariants(variantRes.productVariants)
      } else {
        setVariants(prev => [...prev, ...variantRes.productVariants])
      }
      return variantRes
    } catch (error) {
      Swal.fire({
        icon: 'error',
        title: 'Lỗi',
        text: 'Không thể tải dữ liệu biến thể sản phẩm'
      })
      return null
    } finally {
      setIsLoading(false)
    }
  }

  const handleCreate = async () => {
    try {
      await variantService.createVariant(formData)
      Swal.fire({
        icon: 'success',
        title: 'Thành công',
        text: 'Tạo biến thể thành công'
      })
      setIsCreateDialogOpen(false)
      setFormData({
        productId: 0,
        price: 0,
        stock: 0,
        isActived: 1,
        avIds: []
      })
      loadData()
    } catch (error) {
      Swal.fire({
        icon: 'error',
        title: 'Lỗi',
        text: 'Không thể tạo biến thể'
      })
    }
  }

  const handleEdit = async () => {
    if (!selectedVariant) return

    // Nếu chưa chọn sản phẩm (vẫn là placeholder), truyền productId là null
    const productIdToSend = formData.productId === 0 ? null : formData.productId

    // Nếu chưa chọn thuộc tính nào (avIds rỗng), truyền null
    const avIdsToSend = formData.avIds.length === 0 ? null : formData.avIds

    try {
      const updateRequest: UpdateVariantRequest = {
        productId: productIdToSend,
        price: formData.price,
        stock: formData.stock,
        isActived: formData.isActived,
        avIds: avIdsToSend
      }

      const response = await variantService.updateVariant(selectedVariant.variantId, updateRequest)
      if (response.isSuccess) {
        Swal.fire({
          icon: 'success',
          title: 'Thành công',
          text: 'Cập nhật biến thể thành công'
        })
        setIsEditDialogOpen(false)
        loadData()
      }
    } catch (error) {
      Swal.fire({
        icon: 'error',
        title: 'Lỗi',
        text: 'Không thể cập nhật biến thể'
      })
    }
  }

  const handleDelete = async (variantId: number) => {
    const result = await Swal.fire({
      title: 'Xác nhận xóa',
      text: 'Bạn có chắc chắn muốn xóa biến thể này?',
      icon: 'warning',
      showCancelButton: true,
      confirmButtonColor: '#d33',
      cancelButtonColor: '#3085d6',
      confirmButtonText: 'Xóa',
      cancelButtonText: 'Hủy',
      ...swalConfig
    })

    if (result.isConfirmed) {
      try {
        await variantService.deleteVariant(variantId)
        Swal.fire({
          icon: 'success',
          title: 'Thành công',
          text: 'Xóa biến thể thành công',
          ...swalConfig
        })
        loadData()
      } catch (error) {
        Swal.fire({
          icon: 'error',
          title: 'Lỗi',
          text: 'Không thể xóa biến thể',
          ...swalConfig
        })
      }
    }
  }

  const handleFileChange = async (e: React.ChangeEvent<HTMLInputElement>) => {
    const file = e.target.files?.[0]
    if (!file) return

    const extension = '.' + file.name.split('.').pop()?.toLowerCase()
    if (!permittedExtensions.includes(extension)) {
      Swal.fire({
        icon: 'error',
        title: 'Lỗi',
        text: 'Định dạng file không được hỗ trợ'
      })
      return
    }

    const isVideo = ['.mp4', '.avi', '.mov', '.mkv', '.flv'].includes(extension)
    const maxSize = isVideo ? MAX_VIDEO_SIZE : MAX_IMAGE_SIZE

    if (file.size > maxSize) {
      Swal.fire({
        icon: 'error',
        title: 'Lỗi',
        text: `Kích thước file quá lớn. Tối đa ${isVideo ? '5MB' : '2MB'}`
      })
      return
    }

    try {
      const base64 = await convertFileToBase64(file)
      const base64Data = base64.split(',')[1]
      
      if (!base64Data) {
        throw new Error("Không thể mã hóa file")
      }

      // Cập nhật state với base64 đầy đủ để hiển thị preview
      setNewImage(prev => ({
        ...prev,
        title: file.name,
        imageData: base64 // Giữ nguyên base64 đầy đủ để hiển thị
      }))

      // Cập nhật state với base64 data để gửi lên server
      setEditImageData(prev => ({
        ...prev,
        title: file.name,
        imageData: base64 // Chỉ lấy phần data để gửi lên server
      }))
    } catch (error) {
      console.error("Error converting file to base64:", error)
      Swal.fire({
        icon: 'error',
        title: 'Lỗi',
        text: 'Không thể mã hóa file'
      })
    }
  }

  const convertFileToBase64 = (file: File): Promise<string> => {
    return new Promise((resolve, reject) => {
      const reader = new FileReader()
      reader.readAsDataURL(file)
      reader.onload = () => {
        if (typeof reader.result === 'string') {
          resolve(reader.result)
        } else {
          reject(new Error("Không thể đọc file"))
        }
      }
      reader.onerror = error => reject(error)
    })
  }

  const handleCreateImage = async () => {
    if (!selectedVariantForImage) return

    try {
      if (!newImage.imageData) {
        Swal.fire({
          icon: 'error',
          title: 'Lỗi',
          text: 'Vui lòng chọn file trước'
        })
        return
      }

      Swal.fire({
        title: 'Đang xử lý',
        text: 'Đang tải lên file...',
        allowOutsideClick: false,
        didOpen: () => {
          Swal.showLoading()
        }
      })

      await variantService.createImage({
        ...newImage,
        variantId: selectedVariantForImage.variantId
      })

      Swal.close()
      Swal.fire({
        icon: 'success',
        title: 'Thành công',
        text: 'Đã thêm hình ảnh/video'
      })
      setIsImageDialogOpen(false)
      loadData()
    } catch (error) {
      console.error("Error creating image:", error)
      Swal.fire({
        icon: 'error',
        title: 'Lỗi',
        text: 'Không thể thêm hình ảnh/video'
      })
    }
  }

  const handleDeleteImage = async (imageId: number) => {
    const result = await Swal.fire({
      title: 'Xác nhận xóa',
      text: 'Bạn có chắc chắn muốn xóa hình ảnh/video này?',
      icon: 'warning',
      showCancelButton: true,
      confirmButtonColor: '#d33',
      cancelButtonColor: '#3085d6',
      confirmButtonText: 'Xóa',
      cancelButtonText: 'Hủy'
    })

    if (result.isConfirmed) {
      try {
        await variantService.deleteImage(imageId)
        Swal.fire({
          icon: 'success',
          title: 'Thành công',
          text: 'Đã xóa hình ảnh/video'
        })
        loadData()
      } catch (error) {
        console.error("Error deleting image:", error)
        Swal.fire({
          icon: 'error',
          title: 'Lỗi',
          text: 'Không thể xóa hình ảnh/video'
        })
      }
    }
  }

  const openCreateDialog = () => {
    setFormData({
      productId: 0,
      price: 0,
      stock: 0,
      isActived: 1,
      avIds: []
    })
    setIsCreateDialogOpen(true)
  }

  const openEditDialog = (variant: ProductVariant) => {
    setSelectedVariant(variant)
    setFormData({
      productId: variant.productId || 0,
      price: variant.price,
      stock: variant.stock,
      isActived: variant.isActived,
      avIds: variant.productsAttributes.map(attr => attr.avId)
    })
    setIsEditDialogOpen(true)
  }

  const openImageDialog = (variant: ProductVariant) => {
    setSelectedVariantForImage(variant)
    setNewImage({
      title: '',
      imageData: '',
      position: 0,
      variantId: variant.variantId
    })
    setIsImageDialogOpen(true)
  }

  const formatPrice = (price: number) => {
    return new Intl.NumberFormat('vi-VN', {
      style: 'currency',
      currency: 'VND'
    }).format(price)
  }

  const formatRating = (rating: number) => {
    return rating.toFixed(1)
  }

  const formatDescription = (description: string) => {
    if (!description) return '-'
    if (description.length <= 30) return description
    return description.substring(0, 30) + '...'
  }

  const renderCategoryOptions = (categories: Category[]) => {
    const renderCategory = (category: Category, level = 0) => {
      return (
        <React.Fragment key={category.id}>
          <SelectItem value={category.id.toString()}>
            {'\u00A0'.repeat(level * 2)}{level > 0 ? '└ ' : ''}{category.name}
          </SelectItem>
          {category.categories?.map(child => renderCategory(child, level + 1))}
        </React.Fragment>
      );
    };

    return categories.map(category => renderCategory(category));
  };

  const handleEditImage = async () => {
    if (!selectedImage || !selectedVariant) return

    try {
      const updateData: UpdateImageRequest = {
        title: editImageData.title,
        position: editImageData.position,
        variantId: selectedVariant.variantId,
        imageData: editImageData.imageData || null // Nếu không có file mới thì gửi null
      }

      await variantService.updateImage(selectedImage.id, updateData)
      Swal.fire({
        icon: 'success',
        title: 'Thành công',
        text: 'Đã cập nhật hình ảnh/video'
      })
      setIsEditImageDialogOpen(false)
      loadData()
    } catch (error) {
      console.error("Error updating image:", error)
      Swal.fire({
        icon: 'error',
        title: 'Lỗi',
        text: 'Không thể cập nhật hình ảnh/video'
      })
    }
  }

  const openEditImageDialog = (variant: ProductVariant, image: ProductImage) => {
    setSelectedVariant(variant)
    setSelectedImage(image)
    setEditImageData({
      title: image.title,
      imageData: null, // Không cần cập nhật lại imageData khi chỉ sửa title và position
      position: image.position,
      variantId: variant.variantId
    })
    setIsEditImageDialogOpen(true)
  }

  const getAttributeType = (attributeName: string): 'color' | 'storage' | 'other' => {
    const lowerName = attributeName.toLowerCase()
    if (lowerName.includes('màu')) return 'color'
    if (lowerName.includes('dung lượng')) return 'storage'
    return 'other'
  }

  const updateSelectedCount = (type: 'color' | 'storage', increment: boolean) => {
    if (type === 'color' || type === 'storage') {
      setSelectedCounts(prev => ({
        ...prev,
        [type]: increment ? prev[type] + 1 : prev[type] - 1
      }))
    }
  }

  const handleToggleStatus = async (variant: ProductVariant) => {
    if (variant.isActived === 0) {
      Swal.fire({
        icon: 'info',
        title: 'Thông báo',
        text: 'Biến thể này đã được tắt trạng thái'
      })
      return
    }

    const result = await Swal.fire({
      title: 'Xác nhận tắt trạng thái',
      text: 'Bạn có chắc chắn muốn tắt trạng thái biến thể này?',
      icon: 'warning',
      showCancelButton: true,
      confirmButtonColor: '#d33',
      cancelButtonColor: '#3085d6',
      confirmButtonText: 'Tắt',
      cancelButtonText: 'Hủy',
      ...swalConfig
    })

    if (result.isConfirmed) {
      try {
        const updateRequest: UpdateVariantRequest = {
          productId: variant.productId || 0,
          price: variant.price,
          stock: variant.stock,
          isActived: 0,
          avIds: variant.productsAttributes.map((attr: any) => attr.avId)
        }

        await variantService.updateVariant(variant.variantId, updateRequest)
        Swal.fire({
          icon: 'success',
          title: 'Thành công',
          text: 'Đã tắt trạng thái biến thể',
          ...swalConfig
        })
        loadData()
      } catch (error) {
        Swal.fire({
          icon: 'error',
          title: 'Lỗi',
          text: 'Không thể tắt trạng thái biến thể',
          ...swalConfig
        })
      }
    }
  }

  useEffect(() => {
    if (isEditDialogOpen && selectedVariant) {
      // Đếm lại số lượng thuộc tính đã chọn cho từng loại
      let color = 0, storage = 0;
      selectedVariant.productsAttributes.forEach(attr => {
        const attribute = attributes.find(a => a.id === attr.attributeId);
        if (attribute) {
          const type = getAttributeType(attribute.name);
          if (type === 'color') color++;
          if (type === 'storage') storage++;
        }
      });
      setSelectedCounts({ color, storage });
      // Đồng bộ lại avIds nếu cần
      setFormData(prev => ({
        ...prev,
        avIds: selectedVariant.productsAttributes.map(attr => attr.avId)
      }));
    }
  }, [isEditDialogOpen, selectedVariant, attributes]);

  return (
    <div>
      <div className="flex justify-between items-center mb-6">
        <h2 className="text-xl font-semibold">Quản lý biến thể sản phẩm</h2>
        <div className="flex items-center gap-4">
          <Input
            placeholder="Tìm kiếm tên biến thể..."
            value={searchName}
            onChange={e => setSearchName(e.target.value)}
            className="w-[250px]"
          />
          <Select
            value={selectedCategory?.toString() || "all"}
            onValueChange={(value) => setSelectedCategory(value === "all" ? undefined : parseInt(value))}
          >
            <SelectTrigger className="w-[200px]">
              <SelectValue placeholder="Chọn danh mục" />
            </SelectTrigger>
            <SelectContent>
              <SelectItem value="all">Tất cả</SelectItem>
              {renderCategoryOptions(categories)}
            </SelectContent>
          </Select>
          <Select
            value={isActived?.toString() || "all"}
            onValueChange={(value) => setIsActived(value === "all" ? undefined : parseInt(value))}
          >
            <SelectTrigger className="w-[150px]">
              <SelectValue placeholder="Trạng thái" />
            </SelectTrigger>
            <SelectContent>
              <SelectItem value="all">Tất cả</SelectItem>
              <SelectItem value="1">Đang hoạt động</SelectItem>
              <SelectItem value="0">Không hoạt động</SelectItem>
            </SelectContent>
          </Select>
          <Select value={take.toString()} onValueChange={v => setTake(Number(v))}>
            <SelectTrigger className="w-[120px]">
              <SelectValue placeholder="Số dòng/trang" />
            </SelectTrigger>
            <SelectContent>
              <SelectItem value="25">25</SelectItem>
              <SelectItem value="50">50</SelectItem>
              <SelectItem value="75">75</SelectItem>
              <SelectItem value="100">100</SelectItem>
            </SelectContent>
          </Select>
          <Button onClick={openCreateDialog}>
            <Plus className="h-4 w-4 mr-1" />
            Thêm biến thể
              </Button>
        </div>
      </div>
      <div>
        <Table>
          <TableHeader>
            <TableRow>
              <TableHead>ID</TableHead>
              <TableHead>Sản phẩm</TableHead>
              <TableHead>Giá</TableHead>
              <TableHead>Tồn kho</TableHead>
              <TableHead>Hình ảnh</TableHead>
              <TableHead>Trạng thái</TableHead>
              <TableHead>Thao tác</TableHead>
              <TableHead>Bình luận</TableHead>
            </TableRow>
          </TableHeader>
          <TableBody>
            {variants.map((variant) => (
              <TableRow key={variant.variantId}>
                <TableCell>{variant.variantId}</TableCell>
                <TableCell>{variant.name}</TableCell>
                <TableCell>{formatPrice(variant.price)}</TableCell>
                <TableCell>
                  <div>
                    <div>Tổng: {variant.stock}</div>
                    <div>Đã đặt: {variant.reservedStock}</div>
                    <div>Còn lại: {variant.actualStock}</div>
                  </div>
                </TableCell>
                <TableCell>
                  <div className="flex flex-col gap-2">
                    <ImageGallery 
                      images={variant.images} 
                      onEdit={(image) => openEditImageDialog(variant, image)}
                      onDelete={handleDeleteImage}
                    />
                    <Button
                      variant="outline"
                      size="sm"
                      onClick={() => openImageDialog(variant)}
                    >
                      <Plus className="h-4 w-4 mr-1" />
                      Thêm hình ảnh
                    </Button>
                  </div>
                </TableCell>
                <TableCell>
                  <span className={`px-2 py-1 rounded-full text-xs font-medium ${
                    variant.isActived === 1 
                      ? 'bg-green-100 text-green-800' 
                      : 'bg-red-100 text-red-800'
                  }`}>
                    {variant.isActived === 1 ? "Đang hoạt động" : "Không hoạt động"}
                  </span>
                </TableCell>
                <TableCell>
                  <div className="flex items-center gap-2">
                    <Button
                      variant="outline"
                      size="sm"
                      onClick={() => openEditDialog(variant)}
                    >
                      <Pencil className="h-4 w-4 mr-1" />
                      Sửa
                    </Button>
                    <Button
                      variant="outline"
                      size="sm"
                      className={cn(
                        "flex items-center",
                        variant.isActived === 1
                          ? "text-red-600 hover:text-red-700 hover:bg-red-50"
                          : "text-gray-400 cursor-not-allowed"
                      )}
                      onClick={() => handleToggleStatus(variant)}
                      disabled={variant.isActived === 0}
                    >
                      <Trash2 className="h-4 w-4 mr-1" />
                      {variant.isActived === 1 ? "Tắt" : "Đã tắt"}
                    </Button>
                  </div>
                </TableCell>
                <TableCell>
                  <div className="flex items-center gap-2">
                    <Button
                      variant="outline"
                      size="sm"
                      onClick={() => {
                        setSelectedVariantForReview(variant.variantId)
                        setIsReviewModalOpen(true)
                      }}
                    >
                      Xem bình luận
                    </Button>
                  </div>
                </TableCell>
              </TableRow>
            ))}
            {/* Loading khi đang tải thêm (không phải lần đầu) */}
            {isLoading && variants.length > 0 && (
              <TableRow>
                <TableCell colSpan={8} className="text-center text-gray-500 py-4">
                  <span className="inline-flex items-center gap-2">
                    <svg className="animate-spin h-5 w-5 text-gray-400" viewBox="0 0 24 24"><circle className="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="4" fill="none" /><path className="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8v8z" /></svg>
                    Đang tải thêm...
                  </span>
                </TableCell>
              </TableRow>
            )}
            {/* Nút Xem thêm */}
            {!isLoading && variants.length > 0 && variants.length < total && (
              <TableRow>
                <TableCell colSpan={8} className="text-center py-4">
                  <button
                    className="px-4 py-2 rounded bg-gray-100 hover:bg-gray-200 text-gray-700 font-medium border border-gray-300"
                    onClick={() => {
                      if (!isFetchingRef.current) {
                        isFetchingRef.current = true
                        loadData(variants.length, take).then((res) => {
                          setSkip(variants.length + (res?.productVariants?.length || 0))
                        }).finally(() => {
                          isFetchingRef.current = false
                        })
                      }
                    }}
                  >
                    Xem thêm
                  </button>
                </TableCell>
              </TableRow>
            )}
            {/* Khi không có dữ liệu */}
            {!isLoading && variants.length === 0 && (
              <TableRow>
                <TableCell colSpan={8} className="text-center">Không có biến thể nào</TableCell>
              </TableRow>
            )}
            {/* Khi đã tải hết dữ liệu */}
            {!isLoading && variants.length > 0 && variants.length >= total && (
              <TableRow>
                <TableCell colSpan={8} className="text-center text-gray-400 py-4">Đã tải hết tất cả biến thể</TableCell>
              </TableRow>
            )}
          </TableBody>
        </Table>
        <VariantReviewModal
          open={isReviewModalOpen}
          onOpenChange={setIsReviewModalOpen}
          variantId={selectedVariantForReview || 0}
        />
        {/* Các Dialog giữ nguyên vị trí cũ */}
      </div>

      <Dialog open={isCreateDialogOpen} onOpenChange={setIsCreateDialogOpen}>
        <DialogContent className="max-h-[80vh] overflow-y-auto">
          <DialogHeader>
            <DialogTitle>Tạo biến thể mới</DialogTitle>
          </DialogHeader>
          <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
            <div className="col-span-1">
              <label className="block text-sm font-medium mb-1">Sản phẩm</label>
              <DropdownMenu>
                <DropdownMenuTrigger asChild>
                  <Button variant="outline" className="w-full justify-between">
                    {products.find(p => p.id === formData.productId)?.name || "Chọn sản phẩm"}
                    <ChevronDown className="ml-2 h-4 w-4" />
                  </Button>
                </DropdownMenuTrigger>
                <DropdownMenuContent className="w-56 max-h-[300px] overflow-y-auto">
                  <DropdownMenuLabel>Sản phẩm</DropdownMenuLabel>
                  <DropdownMenuSeparator />
                  <div className="px-2 pb-2">
                    <Input
                      placeholder="Tìm kiếm..."
                      value={productSearchTerm}
                      onChange={(e) => setProductSearchTerm(e.target.value)}
                      className="h-8"
                    />
                  </div>
                  {products
                    .filter(p => !productSearchTerm || p.name.toLowerCase().includes(productSearchTerm.toLowerCase()))
                    .map(product => (
                      <DropdownMenuCheckboxItem
                        key={product.id}
                        checked={formData.productId === product.id}
                        onCheckedChange={(checked) => {
                          if (checked) {
                            setFormData({ ...formData, productId: product.id })
                          }
                        }}
                      >
                        {product.name}
                      </DropdownMenuCheckboxItem>
                    ))}
                </DropdownMenuContent>
              </DropdownMenu>
            </div>
            <div className="col-span-1">
              <label className="block text-sm font-medium mb-1">Giá</label>
              <Input
                type="number"
                value={formData.price}
                onChange={(e) => setFormData({ ...formData, price: parseFloat(e.target.value) })}
              />
            </div>
            <div className="col-span-1">
              <label className="block text-sm font-medium mb-1">Số lượng</label>
              <Input
                type="number"
                value={formData.stock}
                onChange={(e) => setFormData({ ...formData, stock: parseInt(e.target.value) })}
              />
            </div>
            <div className="col-span-1 md:col-span-2">
              <label className="block text-sm font-medium mb-1">Thuộc tính</label>
              <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                {attributes.map((attribute) => (
                  <div key={attribute.id} className="mb-4">
                    <label className="block text-sm font-medium mb-1">{attribute.name}</label>
                    <div className="space-y-2">
                      <DropdownMenu>
                        <DropdownMenuTrigger asChild>
                          <Button variant="outline" className="w-full justify-between">
                            Chọn {attribute.name}
                            <ChevronDown className="ml-2 h-4 w-4" />
                          </Button>
                        </DropdownMenuTrigger>
                        <DropdownMenuContent className="w-56 max-h-[300px] overflow-y-auto">
                          <DropdownMenuLabel>{attribute.name}</DropdownMenuLabel>
                          <DropdownMenuSeparator />
                          <div className="px-2 pb-2">
                            <Input
                              placeholder="Tìm kiếm..."
                              value={searchTerms[attribute.id] || ''}
                              onChange={(e) => setSearchTerms({ ...searchTerms, [attribute.id]: e.target.value })}
                              className="h-8"
                            />
                          </div>
                          {attributeValues
                            .filter(v => v.attributeId === attribute.id)
                            .filter(v => !searchTerms[attribute.id] || v.value.toLowerCase().includes(searchTerms[attribute.id].toLowerCase()))
                            .map(value => {
                              const attributeType = getAttributeType(attribute.name)
                              const isChecked = formData.avIds.includes(value.id)
                              const otherType = attributeType === 'color' ? 'storage' : 'color'
                              const canSelect = isChecked || attributeType === 'other' || 
                                (selectedCounts[otherType] === 0 || selectedCounts[attributeType === 'color' ? 'color' : 'storage'] < selectedCounts[otherType])

                              return (
                                <DropdownMenuCheckboxItem
                                  key={value.id}
                                  checked={isChecked}
                                  disabled={!canSelect && !isChecked}
                                  onCheckedChange={(checked) => {
                                    if (checked) {
                                      updateSelectedCount(attributeType === 'color' ? 'color' : 'storage', true)
                                      setFormData({
                                        ...formData,
                                        avIds: [...formData.avIds, value.id]
                                      })
                                    } else {
                                      updateSelectedCount(attributeType === 'color' ? 'color' : 'storage', false)
                                      setFormData({
                                        ...formData,
                                        avIds: formData.avIds.filter(id => id !== value.id)
                                      })
                                    }
                                  }}
                                >
                                  {value.value}
                                </DropdownMenuCheckboxItem>
                              )
                            })}
                        </DropdownMenuContent>
                      </DropdownMenu>
                      <div className="flex flex-wrap gap-2">
                        {attributeValues
                          .filter(v => v.attributeId === attribute.id && formData.avIds.includes(v.id))
                          .map(value => (
                            <Badge 
                              key={value.id} 
                              variant="secondary"
                              className="pl-2 h-7"
                            >
                              {value.value}
                              <button
                                className="ml-1 hover:bg-muted rounded-full"
                                onClick={() => {
                                  const attributeType = getAttributeType(attribute.name)
                                  updateSelectedCount(attributeType === 'color' ? 'color' : 'storage', false)
                                  setFormData({
                                    ...formData,
                                    avIds: formData.avIds.filter(id => id !== value.id)
                                  })
                                }}
                              >
                                <X className="h-4 w-4" />
                              </button>
                            </Badge>
                          ))}
                      </div>
                    </div>
                  </div>
                ))}
              </div>
            </div>
            <div className="col-span-1 md:col-span-2 flex items-center space-x-2">
              <Switch
                checked={formData.isActived === 1}
                onCheckedChange={(checked) => setFormData({ ...formData, isActived: checked ? 1 : 0 })}
              />
              <label className="text-sm font-medium">Kích hoạt</label>
            </div>
            <div className="col-span-1 md:col-span-2">
              <Button onClick={handleCreate} className="w-full md:w-auto">Tạo</Button>
            </div>
          </div>
        </DialogContent>
      </Dialog>

      <Dialog open={isEditDialogOpen} onOpenChange={setIsEditDialogOpen}>
        <DialogContent className="max-h-[80vh] overflow-y-auto">
          <DialogHeader>
            <DialogTitle>Cập nhật biến thể</DialogTitle>
          </DialogHeader>
          <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
            <div className="col-span-1">
              <label className="block text-sm font-medium mb-1">Sản phẩm</label>
              <DropdownMenu>
                <DropdownMenuTrigger asChild>
                  <Button variant="outline" className="w-full justify-between">
                    {products.find(p => p.id === formData.productId)?.name || "Chọn sản phẩm"}
                    <ChevronDown className="ml-2 h-4 w-4" />
                  </Button>
                </DropdownMenuTrigger>
                <DropdownMenuContent className="w-56 max-h-[300px] overflow-y-auto">
                  <DropdownMenuLabel>Sản phẩm</DropdownMenuLabel>
                  <DropdownMenuSeparator />
                  <div className="px-2 pb-2">
                    <Input
                      placeholder="Tìm kiếm..."
                      value={productSearchTerm}
                      onChange={(e) => setProductSearchTerm(e.target.value)}
                      className="h-8"
                    />
                  </div>
                  {products
                    .filter(p => !productSearchTerm || p.name.toLowerCase().includes(productSearchTerm.toLowerCase()))
                    .map(product => (
                      <DropdownMenuCheckboxItem
                        key={product.id}
                        checked={formData.productId === product.id}
                        onCheckedChange={(checked) => {
                          if (checked) {
                            setFormData({ ...formData, productId: product.id })
                          }
                        }}
                      >
                        {product.name}
                      </DropdownMenuCheckboxItem>
                    ))}
                </DropdownMenuContent>
              </DropdownMenu>
            </div>
            <div className="col-span-1">
              <label className="block text-sm font-medium mb-1">Giá</label>
              <Input
                type="number"
                value={formData.price}
                onChange={(e) => setFormData({ ...formData, price: parseFloat(e.target.value) })}
              />
            </div>
            <div className="col-span-1">
              <label className="block text-sm font-medium mb-1">Số lượng</label>
              <Input
                type="number"
                value={formData.stock}
                onChange={(e) => setFormData({ ...formData, stock: parseInt(e.target.value) })}
              />
            </div>
            <div className="col-span-1 md:col-span-2">
              <label className="block text-sm font-medium mb-1">Thuộc tính</label>
              <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                {attributes.map((attribute) => (
                  <div key={attribute.id} className="mb-4">
                    <label className="block text-sm font-medium mb-1">{attribute.name}</label>
                    <div className="space-y-2">
                      <DropdownMenu>
                        <DropdownMenuTrigger asChild>
                          <Button variant="outline" className="w-full justify-between">
                            Chọn {attribute.name}
                            <ChevronDown className="ml-2 h-4 w-4" />
                          </Button>
                        </DropdownMenuTrigger>
                        <DropdownMenuContent className="w-56 max-h-[300px] overflow-y-auto">
                          <DropdownMenuLabel>{attribute.name}</DropdownMenuLabel>
                          <DropdownMenuSeparator />
                          <div className="px-2 pb-2">
                            <Input
                              placeholder="Tìm kiếm..."
                              value={searchTerms[attribute.id] || ''}
                              onChange={(e) => setSearchTerms({ ...searchTerms, [attribute.id]: e.target.value })}
                              className="h-8"
                            />
                          </div>
                          {attributeValues
                            .filter(v => v.attributeId === attribute.id)
                            .filter(v => !searchTerms[attribute.id] || v.value.toLowerCase().includes(searchTerms[attribute.id].toLowerCase()))
                            .map(value => {
                              const attributeType = getAttributeType(attribute.name)
                              const isChecked = formData.avIds.includes(value.id)
                              const otherType = attributeType === 'color' ? 'storage' : 'color'
                              const canSelect = isChecked || attributeType === 'other' || 
                                (selectedCounts[otherType] === 0 || selectedCounts[attributeType === 'color' ? 'color' : 'storage'] < selectedCounts[otherType])

                              return (
                                <DropdownMenuCheckboxItem
                                  key={value.id}
                                  checked={isChecked}
                                  disabled={!canSelect && !isChecked}
                                  onCheckedChange={(checked) => {
                                    if (checked) {
                                      updateSelectedCount(attributeType === 'color' ? 'color' : 'storage', true)
                                      setFormData({
                                        ...formData,
                                        avIds: [...formData.avIds, value.id]
                                      })
                                    } else {
                                      updateSelectedCount(attributeType === 'color' ? 'color' : 'storage', false)
                                      setFormData({
                                        ...formData,
                                        avIds: formData.avIds.filter(id => id !== value.id)
                                      })
                                    }
                                  }}
                                >
                                  {value.value}
                                </DropdownMenuCheckboxItem>
                              )
                            })}
                        </DropdownMenuContent>
                      </DropdownMenu>
                      <div className="flex flex-wrap gap-2">
                        {attributeValues
                          .filter(v => v.attributeId === attribute.id && formData.avIds.includes(v.id))
                          .map(value => (
                            <Badge 
                              key={value.id} 
                              variant="secondary"
                              className="pl-2 h-7"
                            >
                              {value.value}
                              <button
                                className="ml-1 hover:bg-muted rounded-full"
                                onClick={() => {
                                  const attributeType = getAttributeType(attribute.name)
                                  updateSelectedCount(attributeType === 'color' ? 'color' : 'storage', false)
                                  setFormData({
                                    ...formData,
                                    avIds: formData.avIds.filter(id => id !== value.id)
                                  })
                                }}
                              >
                                <X className="h-4 w-4" />
                              </button>
                            </Badge>
                          ))}
                      </div>
                    </div>
                  </div>
                ))}
              </div>
            </div>
            <div className="col-span-1 md:col-span-2 flex items-center space-x-2">
              <Switch
                checked={formData.isActived === 1}
                onCheckedChange={(checked) => setFormData({ ...formData, isActived: checked ? 1 : 0 })}
              />
              <label className="text-sm font-medium">Kích hoạt</label>
            </div>
            <div className="col-span-1 md:col-span-2">
              <Button onClick={handleEdit} className="w-full md:w-auto">Cập nhật</Button>
            </div>
          </div>
        </DialogContent>
      </Dialog>

      <Dialog open={isImageDialogOpen} onOpenChange={setIsImageDialogOpen}>
        <DialogContent>
          <DialogHeader>
            <DialogTitle>Thêm hình ảnh/video</DialogTitle>
          </DialogHeader>
          <div className="space-y-4">
            <div>
              <label className="block text-sm font-medium mb-1">Chọn file</label>
              <div className="flex items-center space-x-2">
                <Input
                  type="file"
                  ref={fileInputRef}
                  onChange={handleFileChange}
                  accept={permittedExtensions.join(',')}
                  className="hidden"
                />
                <Button
                  variant="outline"
                  onClick={() => fileInputRef.current?.click()}
                >
                  Chọn file
          </Button>
                {newImage.imageData && (
                  <span className="text-sm text-gray-500">
                    {newImage.title}
                  </span>
                )}
              </div>
              {newImage.imageData && (
                <div className="mt-4">
                  <img 
                    src={newImage.imageData} 
                    alt="Preview" 
                    className="max-w-full h-auto max-h-[200px] object-contain"
                  />
                </div>
              )}
              <p className="text-sm text-gray-500 mt-1">
                Hỗ trợ: PNG, JPG, JPEG, PDF, WEBP, MP4, AVI, MOV, MKV, FLV
                <br />
                Kích thước tối đa: 2MB cho ảnh, 5MB cho video
              </p>
            </div>
            <div>
              <label className="block text-sm font-medium mb-1">Vị trí</label>
              <Input
                type="number"
                value={newImage.position}
                onChange={(e) => setNewImage({ ...newImage, position: Number(e.target.value) })}
              />
            </div>
            <Button onClick={handleCreateImage}>Thêm</Button>
          </div>
        </DialogContent>
      </Dialog>

      <Dialog open={isEditImageDialogOpen} onOpenChange={setIsEditImageDialogOpen}>
        <DialogContent>
          <DialogHeader>
            <DialogTitle>Cập nhật hình ảnh/video</DialogTitle>
          </DialogHeader>
          <div className="space-y-4">
            <div>
              <label className="block text-sm font-medium mb-1">Chọn file mới (nếu muốn thay đổi)</label>
              <div className="flex items-center space-x-2">
                <Input
                  type="file"
                  ref={fileInputRef}
                  onChange={handleFileChange}
                  accept={permittedExtensions.join(',')}
                  className="hidden"
                />
                <Button
                  variant="outline"
                  onClick={() => fileInputRef.current?.click()}
                >
                  Chọn file
          </Button>
                {editImageData.imageData && (
                  <span className="text-sm text-gray-500">
                    {editImageData.title}
                  </span>
                )}
        </div>
              {editImageData.imageData && (
                <div className="mt-4">
                  <img 
                    src={editImageData.imageData} 
                    alt="Preview" 
                    className="max-w-full h-auto max-h-[200px] object-contain"
                  />
                </div>
              )}
              <p className="text-sm text-gray-500 mt-1">
                Hỗ trợ: PNG, JPG, JPEG, PDF, WEBP, MP4, AVI, MOV, MKV, FLV
                <br />
                Kích thước tối đa: 2MB cho ảnh, 5MB cho video
              </p>
      </div>
            <div>
              <label className="block text-sm font-medium mb-1">Tiêu đề</label>
              <Input
                value={editImageData.title}
                onChange={(e) => setEditImageData({ ...editImageData, title: e.target.value })}
              />
            </div>
            <div>
              <label className="block text-sm font-medium mb-1">Vị trí</label>
              <Input
                type="number"
                value={editImageData.position}
                onChange={(e) => setEditImageData({ ...editImageData, position: Number(e.target.value) })}
              />
            </div>
            <Button onClick={handleEditImage}>Cập nhật</Button>
          </div>
        </DialogContent>
      </Dialog>
    </div>
  )
}

export default function ProductsPage() {
  const [selectedTab, setSelectedTab] = useState("products")
  const [loading, setLoading] = useState(true)
  const [products, setProducts] = useState<Product[]>([])
  const [categories, setCategories] = useState<Category[]>([])
  const [variants, setVariants] = useState<ProductVariant[]>([])
  const [isVariantDialogOpen, setIsVariantDialogOpen] = useState(false)
  const [selectedProductForVariant, setSelectedProductForVariant] = useState<Product | null>(null)
  const [newVariant, setNewVariant] = useState<CreateVariantRequest>({
    productId: 0,
    price: 0,
    stock: 0,
    isActived: 1,
    avIds: []
  })
  const [isImageDialogOpen, setIsImageDialogOpen] = useState(false)
  const [selectedVariantForImage, setSelectedVariantForImage] = useState<ProductVariant | null>(null)
  const [newImage, setNewImage] = useState<CreateImageRequest>({
    title: '',
    imageData: '',
    position: 0,
    variantId: 0
  })
  const fileInputRef = useRef<HTMLInputElement>(null)

  useEffect(() => {
    loadData()
  }, [])

  const loadData = async () => {
    try {
      setLoading(true)
      const [productsData, categoriesData, variantsRes] = await Promise.all([
        productService.getAllProducts(),
        categoryService.getAll(),
        variantService.getAll()
      ])
      setProducts(productsData || [])
      setCategories(categoriesData || [])
      setVariants(variantsRes.productVariants)
      // Nếu muốn, có thể setTotal(variantsRes.totalItems)
    } catch (error) {
      console.error("Error loading data:", error)
      Swal.fire({
        title: "Lỗi",
        text: "Không thể tải dữ liệu",
        icon: "error",
        confirmButtonText: "OK",
        ...swalConfig
      })
    } finally {
      setLoading(false)
    }
  }

  const handleCreateVariant = async () => {
    if (!selectedProductForVariant) return

    try {
      await variantService.createVariant({
        ...newVariant,
        productId: selectedProductForVariant.id
      })
      Swal.fire({
        title: "Thành công",
        text: "Đã tạo biến thể mới",
        icon: "success",
        confirmButtonText: "OK",
        ...swalConfig
      })
      setIsVariantDialogOpen(false)
      loadData()
    } catch (error) {
      console.error("Error creating variant:", error)
      Swal.fire({
        title: "Lỗi",
        text: "Không thể tạo biến thể",
        icon: "error",
        confirmButtonText: "OK",
        ...swalConfig
      })
    }
  }

  const handleDeleteVariant = async (variantId: number) => {
    const result = await Swal.fire({
      title: "Xác nhận",
      text: "Bạn có chắc chắn muốn xóa biến thể này?",
      icon: "warning",
      showCancelButton: true,
      confirmButtonColor: '#d33',
      cancelButtonColor: '#3085d6',
      confirmButtonText: "Xóa",
      cancelButtonText: "Hủy",
      ...swalConfig
    })

    if (result.isConfirmed) {
      try {
        await variantService.deleteVariant(variantId)
        Swal.fire({
          title: "Thành công",
          text: "Đã xóa biến thể",
          icon: "success",
          confirmButtonText: "OK",
          ...swalConfig
        })
        loadData()
      } catch (error) {
        Swal.fire({
          icon: 'error',
          title: 'Lỗi',
          text: 'Không thể xóa biến thể',
          ...swalConfig
        })
      }
    }
  }

  const handleFileChange = async (e: React.ChangeEvent<HTMLInputElement>) => {
    const file = e.target.files?.[0]
    if (!file) return

    const extension = '.' + file.name.split('.').pop()?.toLowerCase()
    if (!permittedExtensions.includes(extension)) {
      Swal.fire({
        icon: 'error',
        title: 'Lỗi',
        text: 'Định dạng file không được hỗ trợ'
      })
      return
    }

    const isVideo = ['.mp4', '.avi', '.mov', '.mkv', '.flv'].includes(extension)
    const maxSize = isVideo ? MAX_VIDEO_SIZE : MAX_IMAGE_SIZE

    if (file.size > maxSize) {
      Swal.fire({
        icon: 'error',
        title: 'Lỗi',
        text: `Kích thước file quá lớn. Tối đa ${isVideo ? '5MB' : '2MB'}`
      })
      return
    }

    try {
      const base64 = await convertFileToBase64(file)
      const base64Data = base64.split(',')[1]
      
      if (!base64Data) {
        throw new Error("Không thể mã hóa file")
      }

      setNewImage(prev => ({
        ...prev,
        title: file.name,
        imageData: base64Data
      }))
    } catch (error) {
      console.error("Error converting file to base64:", error)
      Swal.fire({
        icon: 'error',
        title: 'Lỗi',
        text: 'Không thể mã hóa file'
      })
    }
  }

  const convertFileToBase64 = (file: File): Promise<string> => {
    return new Promise((resolve, reject) => {
      const reader = new FileReader()
      reader.readAsDataURL(file)
      reader.onload = () => {
        if (typeof reader.result === 'string') {
          resolve(reader.result)
        } else {
          reject(new Error("Không thể đọc file"))
        }
      }
      reader.onerror = error => reject(error)
    })
  }

  const handleCreateImage = async () => {
    if (!selectedVariantForImage) return

    try {
      if (!newImage.imageData) {
        Swal.fire({
          icon: 'error',
          title: 'Lỗi',
          text: 'Vui lòng chọn file trước'
        })
        return
      }

      Swal.fire({
        title: 'Đang xử lý',
        text: 'Đang tải lên file...',
        allowOutsideClick: false,
        didOpen: () => {
          Swal.showLoading()
        }
      })

      await variantService.createImage({
        ...newImage,
        variantId: selectedVariantForImage.variantId
      })

      Swal.close()
      Swal.fire({
        icon: 'success',
        title: 'Thành công',
        text: 'Đã thêm hình ảnh/video'
      })
      setIsImageDialogOpen(false)
      loadData()
    } catch (error) {
      console.error("Error creating image:", error)
      Swal.fire({
        icon: 'error',
        title: 'Lỗi',
        text: 'Không thể thêm hình ảnh/video'
      })
    }
  }

  const handleDeleteImage = async (imageId: number) => {
    const result = await Swal.fire({
      title: 'Xác nhận',
      text: 'Bạn có chắc chắn muốn xóa hình ảnh/video này?',
      icon: 'warning',
      showCancelButton: true,
      confirmButtonColor: '#d33',
      cancelButtonColor: '#3085d6',
      confirmButtonText: 'Xóa',
      cancelButtonText: 'Hủy'
    })

    if (result.isConfirmed) {
      try {
        await variantService.deleteImage(imageId)
        Swal.fire({
          title: 'Thành công',
          text: 'Đã xóa hình ảnh/video'
        })
        loadData()
      } catch (error) {
        console.error("Error deleting image:", error)
        Swal.fire({
          icon: 'error',
          title: 'Lỗi',
          text: 'Không thể xóa hình ảnh/video'
        })
      }
    }
  }

  return (
    <div className="container mx-auto py-10">
      <h1 className="text-2xl font-bold mb-6">Quản lý sản phẩm</h1>
      
      <Tabs defaultValue="products" className="w-full">
        <TabsList className="grid w-full grid-cols-3">
          <TabsTrigger value="products">Sản phẩm</TabsTrigger>
          <TabsTrigger value="attributes">Thuộc tính</TabsTrigger>
          <TabsTrigger value="variants">Biến thể</TabsTrigger>
        </TabsList>
        
        <TabsContent value="products">
          <ProductsTab />
        </TabsContent>
        
        <TabsContent value="attributes">
          <AttributesTab />
        </TabsContent>
        
        <TabsContent value="variants">
          <ProductVariantsTab />
        </TabsContent>
      </Tabs>
    </div>
  )
}
