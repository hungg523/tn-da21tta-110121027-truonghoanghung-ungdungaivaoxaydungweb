"use client"

import { useEffect, useState } from "react"
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card"
import { dashboardService, NewUserStatistic, TopSpendingUser, TopProduct, ProductStatisticType, OrderStatistic, ReturnStatistic, TimeUnit, SummaryStatistic } from "@/services/dashboard.service"
import { format } from "date-fns"
import { vi } from "date-fns/locale"
import {
  LineChart,
  Line,
  XAxis,
  YAxis,
  CartesianGrid,
  Tooltip,
  ResponsiveContainer,
  Legend,
  BarChart,
  Bar,
} from "recharts"
import { Users, TrendingUp, DollarSign, Mail, ShoppingBag, Star, Eye, Heart, ShoppingCart } from "lucide-react"
import { DatePickerWithRange } from "@/components/ui/date-range-picker"
import { DateRange } from "react-day-picker"
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/select"
import { Avatar, AvatarFallback, AvatarImage } from "@/components/ui/avatar"
import { Button } from "@/components/ui/button"
import { Tabs, TabsContent, TabsList, TabsTrigger } from "@/components/ui/tabs"

// Hàm gộp user mới theo ngày
function groupNewUsersByDate(users: NewUserStatistic[]) {
  const grouped: Record<string, number> = {};
  users.forEach((user: NewUserStatistic) => {
    const date = format(new Date(user.date), 'yyyy-MM-dd');
    grouped[date] = (grouped[date] || 0) + user.newUserCount;
  });
  return Object.entries(grouped).map(([date, count]) => ({
    date,
    newUserCount: count
  }));
}

export default function DashboardPage() {
  const [newUsers, setNewUsers] = useState<NewUserStatistic[]>([])
  const [topSpendingUsers, setTopSpendingUsers] = useState<TopSpendingUser[]>([])
  const [topProducts, setTopProducts] = useState<TopProduct[]>([])
  const [isLoading, setIsLoading] = useState(true)
  const [dateRange, setDateRange] = useState<DateRange | undefined>(undefined)
  const [topCount, setTopCount] = useState<number>(5)
  const [productType, setProductType] = useState<ProductStatisticType>('best-selling')
  const [tab, setTab] = useState<string>('overview')
  const [orderStats, setOrderStats] = useState<OrderStatistic[]>([])
  const [returnStats, setReturnStats] = useState<ReturnStatistic[]>([])
  const [orderTimeUnit, setOrderTimeUnit] = useState<TimeUnit>('day')
  const [returnTimeUnit, setReturnTimeUnit] = useState<TimeUnit>('day')
  const [orderDateRange, setOrderDateRange] = useState<DateRange | undefined>(undefined)
  const [returnDateRange, setReturnDateRange] = useState<DateRange | undefined>(undefined)
  const [summaryStats, setSummaryStats] = useState<SummaryStatistic[]>([])
  const [summaryTimeUnit, setSummaryTimeUnit] = useState<TimeUnit>('day')
  const [summaryDateRange, setSummaryDateRange] = useState<DateRange | undefined>(undefined)

  useEffect(() => {
    if (tab === 'overview') {
      loadData()
      loadSummaryStats()
    }
    if (tab === 'orders') loadOrderStats()
    if (tab === 'returns') loadReturnStats()
    // eslint-disable-next-line
  }, [tab, dateRange, topCount, productType, orderTimeUnit, orderDateRange, returnTimeUnit, returnDateRange, summaryTimeUnit, summaryDateRange])

  const loadData = async () => {
    try {
      setIsLoading(true)
      const [newUsersData, topSpendingData, topProductsData] = await Promise.all([
        dashboardService.getNewUsers(dateRange?.from, dateRange?.to),
        dashboardService.getTopSpendingUsers(topCount, dateRange?.from, dateRange?.to),
        dashboardService.getTopProducts(productType, dateRange?.from, dateRange?.to)
      ])
      setNewUsers(newUsersData)
      setTopSpendingUsers(topSpendingData)
      setTopProducts(topProductsData)
    } catch (error) {
      console.error('Error loading dashboard data:', error)
    } finally {
      setIsLoading(false)
    }
  }

  const loadOrderStats = async () => {
    try {
      setIsLoading(true)
      const stats = await dashboardService.getOrderStatistics(
        orderTimeUnit,
        orderDateRange?.from,
        orderDateRange?.to
      )
      setOrderStats(stats)
    } catch (error) {
      console.error('Error loading order stats:', error)
    } finally {
      setIsLoading(false)
    }
  }

  const loadReturnStats = async () => {
    try {
      setIsLoading(true)
      const stats = await dashboardService.getReturnStatistics(
        returnTimeUnit,
        returnDateRange?.from,
        returnDateRange?.to
      )
      setReturnStats(stats)
    } catch (error) {
      console.error('Error loading return stats:', error)
    } finally {
      setIsLoading(false)
    }
  }

  const loadSummaryStats = async () => {
    try {
      setIsLoading(true)
      const stats = await dashboardService.getSummaryStatistics(
        summaryTimeUnit,
        summaryDateRange?.from,
        summaryDateRange?.to
      )
      setSummaryStats(stats)
    } catch (error) {
      console.error('Error loading summary stats:', error)
    } finally {
      setIsLoading(false)
    }
  }

  const formatCurrency = (amount: number) => {
    return new Intl.NumberFormat('vi-VN', {
      style: 'currency',
      currency: 'VND'
    }).format(amount)
  }

  const handleResetDateRange = () => {
    setDateRange(undefined)
  }

  const getProductTypeIcon = (type: ProductStatisticType) => {
    switch (type) {
      case 'best-selling':
        return <ShoppingBag className="h-4 w-4" />
      case 'most-viewed':
        return <Eye className="h-4 w-4" />
      case 'most-carted':
        return <ShoppingCart className="h-4 w-4" />
      case 'most-wished':
        return <Heart className="h-4 w-4" />
      case 'highest-rated':
        return <Star className="h-4 w-4" />
    }
  }

  const getProductTypeLabel = (type: ProductStatisticType) => {
    switch (type) {
      case 'best-selling':
        return 'Bán chạy nhất'
      case 'most-viewed':
        return 'Xem nhiều nhất'
      case 'most-carted':
        return 'Thêm giỏ nhiều nhất'
      case 'most-wished':
        return 'Yêu thích nhất'
      case 'highest-rated':
        return 'Đánh giá cao nhất'
    }
  }

  // Tính toán tổng/ trung bình cho các chỉ số nổi bật
  const totalViews = summaryStats.reduce((sum, s) => sum + s.totalViews, 0)
  const totalOrders = summaryStats.reduce((sum, s) => sum + s.totalOrders, 0)
  const avgCartToOrderRate = summaryStats.length ? summaryStats.reduce((sum, s) => sum + s.cartToOrderRate, 0) / summaryStats.length : 0
  const totalCustomers = summaryStats.reduce((sum, s) => sum + s.totalCustomers, 0)
  const avgReturningCustomerRate = summaryStats.length ? summaryStats.reduce((sum, s) => sum + s.returningCustomerRate, 0) / summaryStats.length : 0
  const totalReturningCustomers = summaryStats.reduce((sum, s) => sum + s.returningCustomers, 0)

  const groupedNewUsers = groupNewUsersByDate(newUsers);

  return (
    <Tabs value={tab} onValueChange={setTab} className="space-y-4">
      <TabsList className="mb-4">
        <TabsTrigger value="overview">Tổng quan</TabsTrigger>
        <TabsTrigger value="orders">Thống kê đơn hàng</TabsTrigger>
        <TabsTrigger value="returns">Thống kê trả hàng</TabsTrigger>
      </TabsList>
      <TabsContent value="overview">
        <div className="space-y-4">
          <div className="flex items-center justify-between">
            <h1 className="text-3xl font-bold">Dashboard</h1>
            <div className="flex items-center gap-4">
              <div className="flex items-center gap-2">
                <DatePickerWithRange 
                  date={dateRange}
                  onDateChange={setDateRange}
                />
                {/* <Button 
                  variant="outline" 
                  onClick={handleResetDateRange}
                  className={!dateRange ? "bg-primary text-primary-foreground" : ""}
                >
                  Từ trước đến nay
                </Button> */}
              </div>
              <Select
                value={topCount.toString()}
                onValueChange={(value) => setTopCount(parseInt(value))}
              >
                <SelectTrigger className="w-[180px]">
                  <SelectValue placeholder="Chọn số lượng top" />
                </SelectTrigger>
                <SelectContent>
                  <SelectItem value="5">Top 5</SelectItem>
                  <SelectItem value="10">Top 10</SelectItem>
                  <SelectItem value="15">Top 15</SelectItem>
                  <SelectItem value="20">Top 20</SelectItem>
                </SelectContent>
              </Select>
            </div>
      </div>

          <div className="grid gap-4 md:grid-cols-2 lg:grid-cols-5">
        <Card>
          <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
                <CardTitle className="text-sm font-medium">Người dùng mới</CardTitle>
                <Users className="h-4 w-4 text-muted-foreground" />
          </CardHeader>
          <CardContent>
                <div className="text-2xl font-bold">{newUsers.length}</div>
                <p className="text-xs text-muted-foreground">
                  Tổng số người dùng mới
                </p>
          </CardContent>
        </Card>

        <Card>
          <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
                <CardTitle className="text-sm font-medium">Tổng chi tiêu</CardTitle>
          </CardHeader>
          <CardContent>
                <div className="text-2xl font-bold">
                  {formatCurrency(topSpendingUsers.reduce((sum, user) => sum + user.totalSpending, 0))}
            </div>
                <p className="text-xs text-muted-foreground">
                  Tổng chi tiêu của top người dùng
                </p>
          </CardContent>
        </Card>

        <Card>
          <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
                <CardTitle className="text-sm font-medium">Tổng đơn hàng</CardTitle>
                <TrendingUp className="h-4 w-4 text-muted-foreground" />
          </CardHeader>
          <CardContent>
                <div className="text-2xl font-bold">
                  {topSpendingUsers.reduce((sum, user) => sum + user.totalOrders, 0)}
            </div>
                <p className="text-xs text-muted-foreground">
                  Tổng số đơn hàng của top người dùng
                </p>
          </CardContent>
        </Card>

        <Card>
          <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
                <CardTitle className="text-sm font-medium">Tổng lượt xem</CardTitle>
              </CardHeader>
              <CardContent>
                <div className="text-2xl font-bold">{totalViews}</div>
                <p className="text-xs text-muted-foreground">Tổng cộng</p>
              </CardContent>
            </Card>

            <Card>
              <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
                <CardTitle className="text-sm font-medium">Tổng đơn hàng</CardTitle>
              </CardHeader>
              <CardContent>
                <div className="text-2xl font-bold">{totalOrders}</div>
                <p className="text-xs text-muted-foreground">Tổng cộng</p>
              </CardContent>
            </Card>
          </div>

          <div className="grid gap-4 md:grid-cols-2">
            <Card>
              <CardHeader>
                <CardTitle>Người dùng mới theo thời gian</CardTitle>
              </CardHeader>
              <CardContent>
                <div className="h-[300px]">
                  <ResponsiveContainer width="100%" height="100%">
                    <LineChart data={groupedNewUsers} margin={{ top: 10, right: 30, left: 0, bottom: 0 }}>
                      <CartesianGrid strokeDasharray="3 3" />
                      <XAxis 
                        dataKey="date" 
                        tickFormatter={date => format(new Date(date), 'dd/MM', { locale: vi })}
                        minTickGap={20}
                        interval="preserveStartEnd"
                      />
                      <YAxis allowDecimals={false} />
                      <Tooltip 
                        labelFormatter={date => format(new Date(date), 'dd/MM/yyyy', { locale: vi })}
                        formatter={(value) => [`${value} người`, 'Số người dùng mới']}
                      />
                      <Legend />
                      <Line 
                        type="monotone" 
                        dataKey="newUserCount" 
                        stroke="#4f46e5" 
                        strokeWidth={3}
                        dot={{ r: 5, stroke: '#4f46e5', strokeWidth: 2, fill: '#fff' }}
                        activeDot={{ r: 7 }}
                        name="Số người dùng mới"
                      />
                    </LineChart>
                  </ResponsiveContainer>
                </div>
              </CardContent>
            </Card>

            <Card>
              <CardHeader>
                <CardTitle>Top người dùng chi tiêu nhiều nhất</CardTitle>
              </CardHeader>
              <CardContent>
                <div className="space-y-4">
                  {topSpendingUsers.map((user, index) => (
                    <div key={user.userId} className="flex items-center gap-4 p-4 border rounded-lg">
                      <div className="flex-shrink-0">
                        <Avatar className="h-12 w-12">
                          <AvatarImage src={user.avatar || undefined} alt={user.userName || ''} />
                          <AvatarFallback>{user.userName ? user.userName.charAt(0) : null}</AvatarFallback>
                        </Avatar>
                    </div>
                      <div className="flex-1 min-w-0">
                        <div className="flex items-center gap-2">
                          <p className="text-sm font-medium truncate">{user.userName}</p>
                          <span className="text-xs text-muted-foreground">#{index + 1}</span>
                    </div>
                        <div className="flex items-center gap-2 text-sm text-muted-foreground">
                          <Mail className="h-4 w-4" />
                          <span className="truncate">{user.email}</span>
                  </div>
                    </div>
                      <div className="flex flex-col items-end gap-1">
                        <div className="flex items-center gap-1 text-sm font-medium">
                          <span>{formatCurrency(user.totalSpending)}</span>
                    </div>
                        <div className="flex items-center gap-1 text-sm text-muted-foreground">
                          <ShoppingBag className="h-4 w-4" />
                          <span>{user.totalOrders} đơn hàng</span>
                  </div>
                    </div>
                    </div>
                  ))}
                </div>
              </CardContent>
            </Card>
          </div>

          <Card>
            <CardHeader>
              <CardTitle>Top sản phẩm</CardTitle>
              <div className="flex items-center justify-between">
                <Select
                  value={productType}
                  onValueChange={(value) => setProductType(value as ProductStatisticType)}
                >
                  <SelectTrigger className="w-[200px]">
                    <SelectValue placeholder="Chọn loại thống kê" />
                  </SelectTrigger>
                  <SelectContent>
                    <SelectItem value="best-selling">Bán chạy nhất</SelectItem>
                    <SelectItem value="most-viewed">Xem nhiều nhất</SelectItem>
                    <SelectItem value="most-carted">Thêm giỏ nhiều nhất</SelectItem>
                    <SelectItem value="most-wished">Yêu thích nhất</SelectItem>
                    <SelectItem value="highest-rated">Đánh giá cao nhất</SelectItem>
                  </SelectContent>
                </Select>
              </div>
            </CardHeader>
            <CardContent>
              <div className="overflow-x-auto">
                <table className="min-w-full text-sm">
                  <thead>
                    <tr className="border-b">
                      <th className="px-2 py-2 text-left">#</th>
                      <th className="px-2 py-2 text-left">Ảnh</th>
                      <th className="px-2 py-2 text-left">Tên sản phẩm</th>
                      <th className="px-2 py-2 text-right whitespace-nowrap">Giá KM</th>
                      <th className="px-2 py-2 text-right whitespace-nowrap">Giá gốc</th>
                      <th className="px-2 py-2 text-center">Số lượng</th>
                      <th className="px-2 py-2 text-center">Đánh giá TB</th>
                      <th className="px-2 py-2 text-center">Số lượt đánh giá</th>
                    </tr>
                  </thead>
                  <tbody>
                    {topProducts.map((product, index) => (
                      <tr key={product.variantId} className="border-b hover:bg-muted">
                        <td className="px-2 py-2 font-bold text-center">{index + 1}</td>
                        <td className="px-2 py-2">
                          <img src={product.image} alt={product.name} className="w-12 h-12 object-cover rounded" />
                        </td>
                        <td className="px-2 py-2">{product.name}</td>
                        <td className="px-2 py-2 text-right whitespace-nowrap font-semibold text-primary">
                          {formatCurrency(product.discountPrice)}
                        </td>
                        <td className="px-2 py-2 text-right whitespace-nowrap text-muted-foreground line-through">
                          {formatCurrency(product.price)}
                        </td>
                        <td className="px-2 py-2 text-center">
                          {product.totalQuantity}
                        </td>
                        <td className="px-2 py-2 text-center">
                          <span className="flex items-center justify-center gap-1">
                            <Star className="h-4 w-4 fill-yellow-400 text-yellow-400" />
                            {product.averageRating.toFixed(1)}
                          </span>
                        </td>
                        <td className="px-2 py-2 text-center">{product.totalReviews}</td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>
            </CardContent>
          </Card>

          <div className="flex items-center gap-4 justify-end">
            <Select value={summaryTimeUnit} onValueChange={v => setSummaryTimeUnit(v as TimeUnit)}>
              <SelectTrigger className="w-[120px]">
                <SelectValue placeholder="Đơn vị thời gian" />
              </SelectTrigger>
              <SelectContent>
                <SelectItem value="day">Ngày</SelectItem>
                <SelectItem value="month">Tháng</SelectItem>
                <SelectItem value="year">Năm</SelectItem>
              </SelectContent>
            </Select>
            <DatePickerWithRange date={summaryDateRange} onDateChange={setSummaryDateRange} />
          </div>

          <div className="grid gap-4 md:grid-cols-2">
            <Card>
              <CardHeader>
                <CardTitle>Lượt xem, Đơn hàng, Thêm giỏ theo thời gian</CardTitle>
              </CardHeader>
              <CardContent>
                <div className="h-[300px]">
                  <ResponsiveContainer width="100%" height="100%">
                    <LineChart data={summaryStats} margin={{ top: 10, right: 30, left: 0, bottom: 0 }}>
                      <CartesianGrid strokeDasharray="3 3" />
                      <XAxis dataKey="date" tickFormatter={date => format(new Date(date), 'dd/MM')} minTickGap={20} interval="preserveStartEnd" />
                      <YAxis allowDecimals={false} />
                      <Tooltip labelFormatter={date => format(new Date(date), 'dd/MM/yyyy')} />
                      <Legend />
                      <Line type="monotone" dataKey="totalViews" stroke="#6366f1" name="Lượt xem" strokeWidth={2} dot={{ r: 4 }} />
                      <Line type="monotone" dataKey="totalOrders" stroke="#22c55e" name="Đơn hàng" strokeWidth={2} dot={{ r: 4 }} />
                      <Line type="monotone" dataKey="totalCartItems" stroke="#f59e42" name="Thêm giỏ" strokeWidth={2} dot={{ r: 4 }} />
                    </LineChart>
                  </ResponsiveContainer>
                </div>
              </CardContent>
            </Card>
            <Card>
              <CardHeader>
                <CardTitle>Tỉ lệ chuyển đổi & khách quay lại</CardTitle>
              </CardHeader>
              <CardContent>
                <div className="h-[300px]">
                  <ResponsiveContainer width="100%" height="100%">
                    <BarChart data={summaryStats} margin={{ top: 10, right: 30, left: 0, bottom: 0 }}>
                      <CartesianGrid strokeDasharray="3 3" />
                      <XAxis dataKey="date" tickFormatter={date => format(new Date(date), 'dd/MM')} minTickGap={20} interval="preserveStartEnd" />
                      <YAxis allowDecimals={false} />
                      <Tooltip labelFormatter={date => format(new Date(date), 'dd/MM/yyyy')} />
                      <Legend />
                      <Bar dataKey="cartToOrderRate" fill="#6366f1" name="Cart → Order (%)" />
                      <Bar dataKey="returningCustomerRate" fill="#22c55e" name="Khách quay lại (%)" />
                    </BarChart>
                  </ResponsiveContainer>
                </div>
              </CardContent>
            </Card>
          </div>

          <Card>
            <CardHeader>
              <CardTitle>Bảng chi tiết tổng quan</CardTitle>
            </CardHeader>
            <CardContent>
              <div className="overflow-x-auto">
                <table className="min-w-full text-sm">
                  <thead>
                    <tr className="border-b">
                      <th className="px-2 py-2 text-left">Ngày</th>
                      <th className="px-2 py-2 text-center">Lượt xem</th>
                      <th className="px-2 py-2 text-center">Thêm giỏ</th>
                      <th className="px-2 py-2 text-center">Đơn hàng</th>
                      <th className="px-2 py-2 text-center">Tỉ lệ Cart→Order</th>
                      <th className="px-2 py-2 text-center">Khách mới</th>
                      <th className="px-2 py-2 text-center">Khách quay lại</th>
                      <th className="px-2 py-2 text-center">Tỉ lệ quay lại</th>
                    </tr>
                  </thead>
                  <tbody>
                    {summaryStats.map((row, idx) => (
                      <tr key={idx} className="border-b hover:bg-muted">
                        <td className="px-2 py-2">{format(new Date(row.date), 'dd/MM/yyyy')}</td>
                        <td className="px-2 py-2 text-center">{row.totalViews}</td>
                        <td className="px-2 py-2 text-center">{row.totalCartItems}</td>
                        <td className="px-2 py-2 text-center">{row.totalOrders}</td>
                        <td className="px-2 py-2 text-center">{row.cartToOrderRate.toFixed(0)}%</td>
                        <td className="px-2 py-2 text-center">{row.totalCustomers}</td>
                        <td className="px-2 py-2 text-center">{row.returningCustomers}</td>
                        <td className="px-2 py-2 text-center">{row.returningCustomerRate.toFixed(0)}%</td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>
            </CardContent>
          </Card>
        </div>
      </TabsContent>
      <TabsContent value="orders">
        <Card>
          <CardHeader>
            <div className="flex items-center justify-between">
              <CardTitle>Thống kê đơn hàng</CardTitle>
              <div className="flex gap-2 items-center">
                <Select value={orderTimeUnit} onValueChange={v => setOrderTimeUnit(v as TimeUnit)}>
                  <SelectTrigger className="w-[120px]">
                    <SelectValue placeholder="Đơn vị thời gian" />
                  </SelectTrigger>
                  <SelectContent>
                    <SelectItem value="day">Ngày</SelectItem>
                    <SelectItem value="month">Tháng</SelectItem>
                    <SelectItem value="year">Năm</SelectItem>
                  </SelectContent>
                </Select>
                <DatePickerWithRange date={orderDateRange} onDateChange={setOrderDateRange} />
              </div>
            </div>
          </CardHeader>
          <CardContent>
            <div className="h-[450px] mb-6">
              <ResponsiveContainer width="100%" height="100%">
                <LineChart data={orderStats} margin={{ top: 20, right: 60, left: 20, bottom: 20 }}>
                  <CartesianGrid strokeDasharray="3 3" stroke="#e5e7eb" />
                  <XAxis 
                    dataKey="date" 
                    tickFormatter={date => format(new Date(date), orderTimeUnit === 'day' ? 'dd/MM' : orderTimeUnit === 'month' ? 'MM/yyyy' : 'yyyy')}
                    tick={{ fill: '#6b7280' }}
                    axisLine={{ stroke: '#e5e7eb' }}
                  />
                  <YAxis 
                    yAxisId="left" 
                    tickFormatter={v => v} 
                    label={{ value: 'Đơn', angle: -90, position: 'insideLeft', offset: 10, style: { fill: '#6b7280' } }}
                    tick={{ fill: '#6b7280' }}
                    axisLine={{ stroke: '#e5e7eb' }}
                  />
                  <YAxis 
                    yAxisId="right" 
                    orientation="right" 
                    tickFormatter={v => formatCurrency(v)}
                    tick={{ fill: '#6b7280' }}
                    axisLine={{ stroke: '#e5e7eb' }}
                  />
                  <Tooltip 
                    formatter={(value: any, name: string) => name === 'Tổng doanh thu' ? formatCurrency(Number(value)) : value} 
                    labelFormatter={date => format(new Date(date), 'dd/MM/yyyy')}
                    contentStyle={{ 
                      backgroundColor: 'white',
                      border: '1px solid #e5e7eb',
                      borderRadius: '6px',
                      boxShadow: '0 2px 4px rgba(0,0,0,0.1)'
                    }}
                  />
                  <Legend 
                    verticalAlign="top" 
                    height={36}
                    wrapperStyle={{ paddingBottom: '20px' }}
                  />
                  <Line 
                    yAxisId="left" 
                    type="monotone" 
                    dataKey="totalOrders" 
                    stroke="#4f46e5" 
                    name="Tổng đơn" 
                    strokeWidth={3}
                    dot={{ r: 5, stroke: '#4f46e5', strokeWidth: 2, fill: '#fff' }}
                    activeDot={{ r: 7 }}
                  />
                  <Line 
                    yAxisId="right" 
                    type="monotone" 
                    dataKey="totalRevenue" 
                    stroke="#10b981" 
                    name="Tổng doanh thu" 
                    strokeWidth={3}
                    dot={{ r: 5, stroke: '#10b981', strokeWidth: 2, fill: '#fff' }}
                    activeDot={{ r: 7 }}
                  />
                </LineChart>
              </ResponsiveContainer>
            </div>
            <div className="overflow-x-auto">
              <table className="min-w-full text-sm">
                <thead>
                  <tr className="border-b">
                    <th className="px-2 py-2 text-left">Ngày</th>
                    <th className="px-2 py-2 text-right whitespace-nowrap">Tổng doanh thu</th>
                    <th className="px-2 py-2 text-center">Tổng đơn</th>
                    <th className="px-2 py-2 text-center">Thành công</th>
                    <th className="px-2 py-2 text-center">Thất bại</th>
                    <th className="px-2 py-2 text-center">Tỉ lệ thành công</th>
                  </tr>
                </thead>
                <tbody>
                  {orderStats.map((stat, idx) => (
                    <tr key={idx} className="border-b hover:bg-muted">
                      <td className="px-2 py-2">{format(new Date(stat.date), orderTimeUnit === 'day' ? 'dd/MM/yyyy' : orderTimeUnit === 'month' ? 'MM/yyyy' : 'yyyy')}</td>
                      <td className="px-2 py-2 text-right whitespace-nowrap">{formatCurrency(stat.totalRevenue)}</td>
                      <td className="px-2 py-2 text-center">{stat.totalOrders}</td>
                      <td className="px-2 py-2 text-center">{stat.successfulOrders}</td>
                      <td className="px-2 py-2 text-center">{stat.failedOrders}</td>
                      <td className="px-2 py-2 text-center">{stat.successRate.toFixed(2)}%</td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          </CardContent>
        </Card>
      </TabsContent>
      <TabsContent value="returns">
        <Card>
          <CardHeader>
            <div className="flex items-center justify-between">
              <CardTitle>Thống kê trả hàng</CardTitle>
              <div className="flex gap-2 items-center">
                <Select value={returnTimeUnit} onValueChange={v => setReturnTimeUnit(v as TimeUnit)}>
                  <SelectTrigger className="w-[120px]">
                    <SelectValue placeholder="Đơn vị thời gian" />
                  </SelectTrigger>
                  <SelectContent>
                    <SelectItem value="day">Ngày</SelectItem>
                    <SelectItem value="month">Tháng</SelectItem>
                    <SelectItem value="year">Năm</SelectItem>
                  </SelectContent>
                </Select>
                <DatePickerWithRange date={returnDateRange} onDateChange={setReturnDateRange} />
              </div>
            </div>
          </CardHeader>
          <CardContent>
            <div className="h-[450px] mb-6">
              <ResponsiveContainer width="100%" height="100%">
                <LineChart data={returnStats} margin={{ top: 20, right: 60, left: 20, bottom: 20 }}>
                  <CartesianGrid strokeDasharray="3 3" stroke="#e5e7eb" />
                  <XAxis 
                    dataKey="date" 
                    tickFormatter={date => format(new Date(date), returnTimeUnit === 'day' ? 'dd/MM' : returnTimeUnit === 'month' ? 'MM/yyyy' : 'yyyy')}
                    tick={{ fill: '#6b7280' }}
                    axisLine={{ stroke: '#e5e7eb' }}
                  />
                  <YAxis 
                    yAxisId="left" 
                    tickFormatter={v => v} 
                    label={{ value: 'Lượt trả', angle: -90, position: 'insideLeft', offset: 10, style: { fill: '#6b7280' } }}
                    tick={{ fill: '#6b7280' }}
                    axisLine={{ stroke: '#e5e7eb' }}
                  />
                  <YAxis 
                    yAxisId="right" 
                    orientation="right" 
                    tickFormatter={v => formatCurrency(v)}
                    tick={{ fill: '#6b7280' }}
                    axisLine={{ stroke: '#e5e7eb' }}
                  />
                  <Tooltip 
                    formatter={(value: any, name: string) => name === 'Tổng tiền hoàn' ? formatCurrency(Number(value)) : value} 
                    labelFormatter={date => format(new Date(date), 'dd/MM/yyyy')}
                    contentStyle={{ 
                      backgroundColor: 'white',
                      border: '1px solid #e5e7eb',
                      borderRadius: '6px',
                      boxShadow: '0 2px 4px rgba(0,0,0,0.1)'
                    }}
                  />
                  <Legend 
                    verticalAlign="top" 
                    height={36}
                    wrapperStyle={{ paddingBottom: '20px' }}
                  />
                  <Line 
                    yAxisId="left" 
                    type="monotone" 
                    dataKey="totalReturns" 
                    stroke="#ef4444" 
                    name="Tổng trả hàng" 
                    strokeWidth={3}
                    dot={{ r: 5, stroke: '#ef4444', strokeWidth: 2, fill: '#fff' }}
                    activeDot={{ r: 7 }}
                  />
                  <Line 
                    yAxisId="right" 
                    type="monotone" 
                    dataKey="totalRefundAmount" 
                    stroke="#f59e0b" 
                    name="Tổng tiền hoàn" 
                    strokeWidth={3}
                    dot={{ r: 5, stroke: '#f59e0b', strokeWidth: 2, fill: '#fff' }}
                    activeDot={{ r: 7 }}
                  />
                </LineChart>
              </ResponsiveContainer>
            </div>
            <div className="overflow-x-auto">
              <table className="min-w-full text-sm">
                <thead>
                  <tr className="border-b">
                    <th className="px-2 py-2 text-left">Ngày</th>
                    <th className="px-2 py-2 text-center">Tổng trả hàng</th>
                    <th className="px-2 py-2 text-right whitespace-nowrap">Tổng tiền hoàn</th>
                    <th className="px-2 py-2 text-left">Lý do trả hàng phổ biến</th>
                  </tr>
                </thead>
                <tbody>
                  {returnStats.map((stat, idx) => (
                    <tr key={idx} className="border-b hover:bg-muted">
                      <td className="px-2 py-2">{format(new Date(stat.date), returnTimeUnit === 'day' ? 'dd/MM/yyyy' : returnTimeUnit === 'month' ? 'MM/yyyy' : 'yyyy')}</td>
                      <td className="px-2 py-2 text-center">{stat.totalReturns}</td>
                      <td className="px-2 py-2 text-right whitespace-nowrap">{formatCurrency(stat.totalRefundAmount)}</td>
                      <td className="px-2 py-2">
                        {stat.topReasons && stat.topReasons.length > 0 ? (
                          <ul className="list-disc pl-4">
                            {stat.topReasons.map((reason, i) => (
                              <li key={i}>{reason.reason} ({reason.count} lần, {reason.percentage.toFixed(2)}%)</li>
                            ))}
                          </ul>
                        ) : (
                          <span className="text-muted-foreground">Không có dữ liệu</span>
                        )}
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
              </div>
            </CardContent>
          </Card>
        </TabsContent>
      </Tabs>
  )
}
