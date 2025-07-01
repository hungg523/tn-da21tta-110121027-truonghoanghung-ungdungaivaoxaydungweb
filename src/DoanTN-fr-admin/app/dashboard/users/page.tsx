"use client"

import { useState, useEffect } from "react"
import { userService, User } from "@/services/user.service"
import { Button } from "@/components/ui/button"
import { Input } from "@/components/ui/input"
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/components/ui/table"
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select"
import { Avatar, AvatarFallback, AvatarImage } from "@/components/ui/avatar"
import { toast } from "sonner"
import Swal from 'sweetalert2'
import { Search, RefreshCw, ChevronDown } from "lucide-react"
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card"

export default function UsersPage() {
  const [users, setUsers] = useState<User[]>([])
  const [isLoading, setIsLoading] = useState(true)
  const [searchEmail, setSearchEmail] = useState("")
  const [selectedRole, setSelectedRole] = useState<string>("all")
  const [selectedStatus, setSelectedStatus] = useState<string>("all")
  const [skip, setSkip] = useState(0)
  const [take, setTake] = useState(25)
  const [totalItems, setTotalItems] = useState(0)
  const [loadingMore, setLoadingMore] = useState(false)

  useEffect(() => {
    setSkip(0)
    loadUsers(true, 0, take)
  }, [selectedRole, selectedStatus, take])

  const loadUsers = async (reset = true, customSkip?: number, customTake?: number) => {
    try {
      if (reset) setIsLoading(true)
      const role = selectedRole !== "all" ? parseInt(selectedRole) : undefined
      const isActived = selectedStatus !== "all" ? parseInt(selectedStatus) : undefined
      const data = await userService.getAll(role, isActived, customSkip ?? 0, customTake ?? take)
      if (reset) {
        setUsers(data.items)
      } else {
        setUsers(prev => [...prev, ...data.items])
      }
      setTotalItems(data.totalItems)
    } catch (error) {
      Swal.fire({
        icon: 'error',
        title: 'Lỗi',
        text: 'Không thể tải danh sách người dùng'
      })
      setUsers([])
    } finally {
      if (reset) setIsLoading(false)
      setLoadingMore(false)
    }
  }

  const handleLoadMore = () => {
    setLoadingMore(true)
    const newSkip = skip + take
    setSkip(newSkip)
    loadUsers(false, newSkip, take)
  }

  const handleSearch = async () => {
    if (!searchEmail) {
      loadUsers()
      return
    }

    try {
      setIsLoading(true)
      const data = await userService.search(searchEmail)
      setUsers(data)
    } catch (error) {
      Swal.fire({
        icon: 'error',
        title: 'Lỗi',
        text: 'Không thể tìm kiếm người dùng'
      })
    } finally {
      setIsLoading(false)
    }
  }

  const handleChangeStatus = async (user: User) => {
    try {
      if (user.role === "Admin") {
        Swal.fire({
          icon: 'error',
          title: 'Lỗi',
          text: 'Không thể chặn tài khoản Admin'
        })
        return
      }

      const newStatus = user.isActived === 1 ? 2 : 1
      const result = await Swal.fire({
        title: 'Xác nhận',
        text: newStatus === 2 ? 'Bạn có chắc muốn chặn người dùng này?' : 'Bạn có chắc muốn bỏ chặn người dùng này?',
        icon: 'warning',
        showCancelButton: true,
        confirmButtonText: 'Xác nhận',
        cancelButtonText: 'Hủy'
      })

      if (result.isConfirmed) {
        await userService.changeStatus(user.id, { isActived: newStatus })
        Swal.fire({
          icon: 'success',
          title: 'Thành công',
          text: newStatus === 2 ? 'Đã chặn người dùng' : 'Đã bỏ chặn người dùng'
        })
        loadUsers()
      }
    } catch (error) {
      Swal.fire({
        icon: 'error',
        title: 'Lỗi',
        text: 'Không thể thay đổi trạng thái người dùng'
      })
    }
  }

  const getStatusText = (status: number) => {
    switch (status) {
      case 0:
        return "Chưa kích hoạt"
      case 1:
        return "Đã kích hoạt"
      case 2:
        return "Đã bị chặn"
      default:
        return "Không xác định"
    }
  }

  const getStatusColor = (status: number) => {
    switch (status) {
      case 0:
        return "bg-yellow-100 text-yellow-800"
      case 1:
        return "bg-green-100 text-green-800"
      case 2:
        return "bg-red-100 text-red-800"
      default:
        return "bg-gray-100 text-gray-800"
    }
  }

  const getRoleColor = (role: string) => {
    return role === "Admin" ? "bg-purple-100 text-purple-800" : "bg-blue-100 text-blue-800"
  }

  return (
    <div className="p-6 space-y-6">
      <div className="flex justify-between items-center flex-wrap gap-2">
        <div>
          <h1 className="text-3xl font-bold tracking-tight">Quản lý người dùng</h1>
          <p className="text-muted-foreground mt-1">
            Quản lý và theo dõi tất cả người dùng trong hệ thống
          </p>
        </div>
        <div className="flex items-center gap-2">
          <Button variant="outline" onClick={() => loadUsers(true, 0, take)} className="gap-2">
            <RefreshCw className="h-4 w-4" />
            Làm mới
          </Button>
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
      <div className="text-sm text-gray-500">Tổng số người dùng: {totalItems}</div>

      <Card>
        <CardHeader>
          <CardTitle>Bộ lọc tìm kiếm</CardTitle>
        </CardHeader>
        <CardContent>
          <div className="flex flex-col md:flex-row gap-4 items-end">
            <div className="flex-1 relative">
              <Search className="absolute left-3 top-1/2 transform -translate-y-1/2 h-4 w-4 text-muted-foreground" />
              <Input
                placeholder="Tìm kiếm theo email..."
                value={searchEmail}
                onChange={(e) => setSearchEmail(e.target.value)}
                onKeyDown={(e) => e.key === 'Enter' && handleSearch()}
                className="pl-9"
              />
            </div>
            <div className="flex flex-col gap-1 w-[180px]">
              <label htmlFor="role-select" className="text-xs text-muted-foreground font-medium">Vai trò</label>
              <Select value={selectedRole} onValueChange={setSelectedRole}>
                <SelectTrigger id="role-select" className="w-full">
                  <SelectValue placeholder="Chọn quyền" />
                </SelectTrigger>
                <SelectContent>
                  <SelectItem value="all">Tất cả</SelectItem>
                  <SelectItem value="0">User</SelectItem>
                  <SelectItem value="1">Admin</SelectItem>
                </SelectContent>
              </Select>
            </div>
            <div className="flex flex-col gap-1 w-[180px]">
              <label htmlFor="status-select" className="text-xs text-muted-foreground font-medium">Trạng thái</label>
              <Select value={selectedStatus} onValueChange={setSelectedStatus}>
                <SelectTrigger id="status-select" className="w-full">
                  <SelectValue placeholder="Chọn trạng thái" />
                </SelectTrigger>
                <SelectContent>
                  <SelectItem value="all">Tất cả</SelectItem>
                  <SelectItem value="0">Chưa kích hoạt</SelectItem>
                  <SelectItem value="1">Đã kích hoạt</SelectItem>
                  <SelectItem value="2">Đã bị chặn</SelectItem>
                </SelectContent>
              </Select>
            </div>
            <Button onClick={handleSearch} className="gap-2 h-10 mt-5 md:mt-0">
              <Search className="h-4 w-4" />
              Tìm kiếm
            </Button>
          </div>
        </CardContent>
      </Card>

      <Card>
        <CardContent className="p-0">
          <div className="relative overflow-x-auto">
        <Table>
          <TableHeader>
            <TableRow>
                  <TableHead className="w-[80px]">ID</TableHead>
                  <TableHead className="w-[100px]">Avatar</TableHead>
                  <TableHead>Tên</TableHead>
                  <TableHead>Email</TableHead>
                  <TableHead className="w-[100px]">Vai trò</TableHead>
                  <TableHead className="w-[120px]">Trạng thái</TableHead>
                  <TableHead className="w-[100px] text-center">Đơn hàng</TableHead>
                  <TableHead className="w-[120px]">Ngày tạo</TableHead>
                  <TableHead className="w-[100px] text-center">Thao tác</TableHead>
            </TableRow>
          </TableHeader>
          <TableBody>
                {isLoading ? (
                  <TableRow>
                    <TableCell colSpan={9} className="text-center py-8">
                      <div className="flex items-center justify-center gap-2">
                        <RefreshCw className="h-4 w-4 animate-spin" />
                        <span>Đang tải...</span>
                      </div>
                    </TableCell>
                  </TableRow>
                ) : users.length === 0 ? (
                  <TableRow>
                    <TableCell colSpan={9} className="text-center py-8 text-muted-foreground">
                      Không có người dùng nào
                    </TableCell>
                  </TableRow>
                ) : (
                  users.map((user) => (
              <TableRow key={user.id}>
                      <TableCell className="font-medium">{user.id}</TableCell>
                <TableCell>
                        <Avatar className="h-8 w-8">
                          <AvatarImage src={user.avatar || undefined} />
                          <AvatarFallback className="text-sm">
                            {user.userName.charAt(0).toUpperCase()}
                      </AvatarFallback>
                    </Avatar>
                      </TableCell>
                      <TableCell className="font-medium">{user.userName}</TableCell>
                      <TableCell className="text-muted-foreground">{user.email}</TableCell>
                      <TableCell>
                        <span className={`px-2 py-1 rounded-full text-xs font-medium ${getRoleColor(user.role)}`}>
                          {user.role}
                        </span>
                </TableCell>
                <TableCell>
                        <span className={`px-2 py-1 rounded-full text-xs font-medium ${getStatusColor(user.isActived)}`}>
                          {getStatusText(user.isActived)}
                        </span>
                </TableCell>
                      <TableCell className="text-center">{user.totalOrders}</TableCell>
                      <TableCell className="text-muted-foreground">
                        {new Date(user.createdAt).toLocaleDateString()}
                      </TableCell>
                      <TableCell className="text-center">
                        {user.role !== "Admin" && (
                          <Button
                            variant="outline"
                            size="sm"
                            onClick={() => handleChangeStatus(user)}
                            className={user.isActived === 2 ? "text-green-600 hover:text-green-700" : "text-red-600 hover:text-red-700"}
                          >
                            {user.isActived === 2 ? "Bỏ chặn" : "Chặn"}
                  </Button>
                        )}
                </TableCell>
              </TableRow>
                  ))
                )}
          </TableBody>
        </Table>
        {users.length < totalItems && (
          <div className="flex justify-center mt-4">
            <Button onClick={handleLoadMore} disabled={loadingMore} variant="outline">
              {loadingMore ? 'Đang tải...' : 'Xem thêm'}
            </Button>
          </div>
        )}
        {users.length >= totalItems && totalItems > 0 && (
          <div className="text-center text-gray-400 mt-2">Đã hiển thị tất cả người dùng</div>
        )}
      </div>
        </CardContent>
      </Card>
    </div>
  )
}
