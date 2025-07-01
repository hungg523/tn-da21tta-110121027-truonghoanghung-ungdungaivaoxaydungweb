"use client"

import { useState, useEffect } from "react"
import { categoryService, Category, CreateCategoryRequest, UpdateCategoryRequest } from "@/services/category.service"
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
import { ChevronRight } from "lucide-react"
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select"
import Swal from 'sweetalert2'

const CategoryRow = ({ category, level = 0, onEdit }: { category: Category; level?: number; onEdit: (category: Category) => void }) => {
  const [isExpanded, setIsExpanded] = useState(false)
  const hasChildren = category.categories && category.categories.length > 0

  return (
    <>
      <TableRow>
        <TableCell>{category.id}</TableCell>
        <TableCell style={{ paddingLeft: `${level * 20}px` }}>
          <div className="flex items-center gap-2">
            {hasChildren && (
              <ChevronRight
                className={`h-4 w-4 cursor-pointer transition-transform ${isExpanded ? 'rotate-90' : ''}`}
                onClick={() => setIsExpanded(!isExpanded)}
              />
            )}
            {category.name}
          </div>
        </TableCell>
        <TableCell>{category.description || '-'}</TableCell>
        <TableCell>
          <span className={`px-2 py-1 rounded-full text-xs font-medium ${
            category.isActived === 1 
              ? 'bg-green-100 text-green-800' 
              : 'bg-red-100 text-red-800'
          }`}>
            {category.isActived === 1 ? "Đang hoạt động" : "Không hoạt động"}
          </span>
        </TableCell>
        <TableCell>{category.createdAt ? new Date(category.createdAt).toLocaleDateString() : '-'}</TableCell>
        <TableCell>
          <Button variant="outline" onClick={() => onEdit(category)}>
            Sửa
          </Button>
        </TableCell>
      </TableRow>
      {isExpanded && hasChildren && category.categories && (
        <>
          {category.categories.map((child) => (
            <CategoryRow
              key={child.id}
              category={child}
              level={level + 1}
              onEdit={onEdit}
            />
          ))}
        </>
      )}
    </>
  )
}

export default function CategoriesPage() {
  const [categories, setCategories] = useState<Category[]>([])
  const [isLoading, setIsLoading] = useState(true)
  const [isCreateDialogOpen, setIsCreateDialogOpen] = useState(false)
  const [isEditDialogOpen, setIsEditDialogOpen] = useState(false)
  const [selectedCategory, setSelectedCategory] = useState<Category | null>(null)
  const [formData, setFormData] = useState<CreateCategoryRequest>({
    catPid: 0,
    name: "",
    description: "",
    isActived: 1,
  })

  useEffect(() => {
    loadCategories()
  }, [])

  const loadCategories = async () => {
    try {
      setIsLoading(true)
      const data = await categoryService.getAll()
      if (Array.isArray(data)) {
        setCategories(data)
      } else {
        setCategories([])
        Swal.fire({
          icon: 'error',
          title: 'Lỗi',
          text: 'Dữ liệu danh mục không hợp lệ'
        })
      }
    } catch (error) {
      Swal.fire({
        icon: 'error',
        title: 'Lỗi',
        text: 'Không thể tải danh sách danh mục'
      })
      setCategories([])
    } finally {
      setIsLoading(false)
    }
  }

  const handleCreate = async () => {
    try {
      if (formData.name.length > 255) {
        Swal.fire({
          icon: 'error',
          title: 'Lỗi',
          text: 'Tên danh mục không được vượt quá 255 ký tự'
        })
        return
      }
      if (formData.description.length > 500) {
        Swal.fire({
          icon: 'error',
          title: 'Lỗi',
          text: 'Mô tả không được vượt quá 500 ký tự'
        })
        return
      }

      await categoryService.create(formData)
      Swal.fire({
        icon: 'success',
        title: 'Thành công',
        text: 'Tạo danh mục thành công'
      })
      setIsCreateDialogOpen(false)
      setFormData({
        catPid: 0,
        name: "",
        description: "",
        isActived: 1,
      })
      loadCategories()
    } catch (error) {
      Swal.fire({
        icon: 'error',
        title: 'Lỗi',
        text: 'Không thể tạo danh mục'
      })
    }
  }

  const handleEdit = async () => {
    if (!selectedCategory) return

    try {
      if (formData.name.length > 255) {
        Swal.fire({
          icon: 'error',
          title: 'Lỗi',
          text: 'Tên danh mục không được vượt quá 255 ký tự'
        })
        return
      }
      if (formData.description.length > 500) {
        Swal.fire({
          icon: 'error',
          title: 'Lỗi',
          text: 'Mô tả không được vượt quá 500 ký tự'
        })
        return
      }

      await categoryService.update(selectedCategory.id, formData)
      Swal.fire({
        icon: 'success',
        title: 'Thành công',
        text: 'Cập nhật danh mục thành công'
      })
      setIsEditDialogOpen(false)
      setSelectedCategory(null)
      setFormData({
        catPid: 0,
        name: "",
        description: "",
        isActived: 1,
      })
      loadCategories()
    } catch (error) {
      Swal.fire({
        icon: 'error',
        title: 'Lỗi',
        text: 'Không thể cập nhật danh mục'
      })
    }
  }

  const openEditDialog = (category: Category) => {
    setSelectedCategory(category)
    setFormData({
      catPid: category.catPid || 0,
      name: category.name,
      description: category.description || "",
      isActived: category.isActived || 1,
    })
    setIsEditDialogOpen(true)
  }

  return (
    <div className="container mx-auto py-10">
      <div className="flex justify-between items-center mb-6">
        <h1 className="text-2xl font-bold">Quản lý danh mục</h1>
        <Dialog open={isCreateDialogOpen} onOpenChange={setIsCreateDialogOpen}>
          <DialogTrigger asChild>
            <Button>Tạo mới</Button>
          </DialogTrigger>
          <DialogContent>
            <DialogHeader>
              <DialogTitle>Tạo danh mục mới</DialogTitle>
            </DialogHeader>
            <div className="space-y-4">
              <div>
                <label className="block text-sm font-medium mb-1">Danh mục cha</label>
                <Select
                  value={formData.catPid.toString()}
                  onValueChange={(value) => setFormData({ ...formData, catPid: parseInt(value) })}
                >
                  <SelectTrigger>
                    <SelectValue placeholder="Chọn danh mục cha" />
                  </SelectTrigger>
                  <SelectContent>
                    <SelectItem value="0">Không có danh mục cha</SelectItem>
                    {categories.map((category) => (
                      <SelectItem key={category.id} value={category.id.toString()}>
                        {category.name}
                      </SelectItem>
                    ))}
                  </SelectContent>
                </Select>
              </div>
              <div>
                <label className="block text-sm font-medium mb-1">Tên danh mục</label>
                <Input
                  value={formData.name}
                  onChange={(e) => setFormData({ ...formData, name: e.target.value })}
                  maxLength={255}
                />
              </div>
              <div>
                <label className="block text-sm font-medium mb-1">Mô tả</label>
                <Textarea
                  value={formData.description}
                  onChange={(e) => setFormData({ ...formData, description: e.target.value })}
                  maxLength={500}
                />
              </div>
              <div className="flex items-center space-x-2">
                <Switch
                  checked={formData.isActived === 1}
                  onCheckedChange={(checked) => setFormData({ ...formData, isActived: checked ? 1 : 0 })}
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
            <DialogTitle>Cập nhật danh mục</DialogTitle>
          </DialogHeader>
              <div className="space-y-4">
            <div>
              <label className="block text-sm font-medium mb-1">Danh mục cha</label>
              <Select
                value={formData.catPid.toString()}
                onValueChange={(value) => setFormData({ ...formData, catPid: parseInt(value) })}
              >
                <SelectTrigger>
                  <SelectValue placeholder="Chọn danh mục cha" />
                </SelectTrigger>
                <SelectContent>
                  <SelectItem value="0">Không có danh mục cha</SelectItem>
                  {categories
                    .filter(category => category.id !== selectedCategory?.id)
                    .map((category) => (
                      <SelectItem key={category.id} value={category.id.toString()}>
                      {category.name}
                      </SelectItem>
                    ))}
                </SelectContent>
              </Select>
                    </div>
            <div>
              <label className="block text-sm font-medium mb-1">Tên danh mục</label>
              <Input
                value={formData.name}
                onChange={(e) => setFormData({ ...formData, name: e.target.value })}
                maxLength={255}
              />
                  </div>
            <div>
              <label className="block text-sm font-medium mb-1">Mô tả</label>
              <Textarea
                value={formData.description}
                onChange={(e) => setFormData({ ...formData, description: e.target.value })}
                maxLength={500}
              />
              </div>
            <div className="flex items-center space-x-2">
              <Switch
                checked={formData.isActived === 1}
                onCheckedChange={(checked) => setFormData({ ...formData, isActived: checked ? 1 : 0 })}
              />
              <label className="text-sm font-medium">Kích hoạt</label>
        </div>
            <Button onClick={handleEdit}>Cập nhật</Button>
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
            <TableHead>Ngày tạo</TableHead>
            <TableHead>Thao tác</TableHead>
                </TableRow>
              </TableHeader>
              <TableBody>
          {isLoading ? (
            <TableRow>
              <TableCell colSpan={6} className="text-center">Đang tải...</TableCell>
            </TableRow>
          ) : categories.length === 0 ? (
            <TableRow>
              <TableCell colSpan={6} className="text-center">Không có danh mục nào</TableCell>
                  </TableRow>
          ) : (
            categories.map((category) => (
              <CategoryRow
                key={category.id}
                category={category}
                onEdit={openEditDialog}
              />
            ))
          )}
              </TableBody>
            </Table>
    </div>
  )
}
