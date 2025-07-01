"use client"

import type React from "react"
import { useState, useEffect } from "react"
import { usePathname, useRouter } from "next/navigation"
import { authService } from "@/services/auth.service"
import AuthGuard from "@/components/auth-guard"
import {
  BarChart3,
  ShoppingBag,
  Users,
  Package,
  Tag,
  Truck,
  RotateCcw,
  MessageSquare,
  Ticket,
  Calendar,
  Sparkles,
  Bell,
  Search,
  Menu,
  X,
} from "lucide-react"
import Link from "next/link"
import { cn } from "@/lib/utils"
import { Button } from "@/components/ui/button"
import { Input } from "@/components/ui/input"
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuLabel,
  DropdownMenuSeparator,
  DropdownMenuTrigger,
} from "@/components/ui/dropdown-menu"
import { Avatar, AvatarFallback, AvatarImage } from "@/components/ui/avatar"
import { Sheet, SheetContent, SheetTrigger } from "@/components/ui/sheet"
import { Suspense } from "react"

const navigation = [
  { name: "Dashboard", href: "/dashboard", icon: BarChart3 },
  { name: "Danh mục", href: "/dashboard/categories", icon: Tag },
  { name: "Sản phẩm", href: "/dashboard/products", icon: Package },
  { name: "Khuyến mãi", href: "/dashboard/discounts", icon: ShoppingBag },
  { name: "Đơn hàng", href: "/dashboard/orders", icon: Truck },
  { name: "Trả hàng", href: "/dashboard/returns", icon: RotateCcw },
  { name: "Bình luận", href: "/dashboard/comments", icon: MessageSquare },
  { name: "Người dùng", href: "/dashboard/users", icon: Users },
  { name: "Voucher", href: "/dashboard/vouchers", icon: Ticket },
  { name: "Sự kiện", href: "/dashboard/events", icon: Calendar },
  { name: "Quản lý AI", href: "/dashboard/ai", icon: Sparkles },
  { name: "Chat", href: "/dashboard/chat", icon: MessageSquare },
]

export default function DashboardLayout({
  children,
}: {
  children: React.ReactNode
}) {
  const pathname = usePathname()
  const router = useRouter()
  const [isSidebarOpen, setIsSidebarOpen] = useState(true)
  const [user, setUser] = useState<any>(null)

  useEffect(() => {
    const currentUser = authService.getCurrentUser()
    setUser(currentUser)
  }, [])

  const handleLogout = async () => {
    await authService.logout()
    router.push('/login')
  }

  return (
    <AuthGuard>
    <div className="flex h-screen overflow-hidden bg-background">
      {/* Sidebar for desktop */}
      <div
        className={cn(
          "hidden md:flex flex-col w-64 border-r transition-all duration-300",
          isSidebarOpen ? "md:w-64" : "md:w-20",
        )}
      >
        <div className="flex h-16 items-center border-b px-4">
          <Link href="/dashboard" className="flex items-center gap-2">
            <ShoppingBag className="h-6 w-6" />
            {isSidebarOpen && <span className="font-semibold">Admin Panel</span>}
          </Link>
          <Button variant="ghost" size="icon" className="ml-auto" onClick={() => setIsSidebarOpen(!isSidebarOpen)}>
            {isSidebarOpen ? <X className="h-4 w-4" /> : <Menu className="h-4 w-4" />}
          </Button>
        </div>
        <nav className="flex-1 overflow-auto py-4">
          <ul className="space-y-1 px-2">
            {navigation.map((item) => (
              <li key={item.name}>
                <Link
                  href={item.href}
                  className={cn(
                    "flex items-center gap-3 rounded-md px-3 py-2 text-sm font-medium transition-colors",
                    pathname === item.href ? "bg-primary text-primary-foreground" : "hover:bg-muted",
                  )}
                >
                  <item.icon className="h-5 w-5" />
                  {isSidebarOpen && <span>{item.name}</span>}
                </Link>
              </li>
            ))}
          </ul>
        </nav>
        <div className="border-t p-4">
          <div className="flex items-center gap-3">
            <Avatar>
                <AvatarImage src={user?.avatar} alt={user?.fullName || user?.email} />
                <AvatarFallback>{user?.fullName?.charAt(0) || user?.email?.charAt(0) || 'AD'}</AvatarFallback>
            </Avatar>
            {isSidebarOpen && (
              <div className="flex flex-col">
                  <span className="text-sm font-medium">{user?.fullName || user?.email}</span>
                  <span className="text-xs text-muted-foreground">{user?.role || 'Admin'}</span>
              </div>
            )}
          </div>
        </div>
      </div>

      {/* Mobile sidebar */}
      <Sheet>
        <SheetTrigger asChild>
          <Button variant="outline" size="icon" className="md:hidden ml-2 mt-2">
            <Menu className="h-5 w-5" />
          </Button>
        </SheetTrigger>
        <SheetContent side="left" className="w-64 p-0">
          <div className="flex h-16 items-center border-b px-4">
            <Link href="/dashboard" className="flex items-center gap-2">
              <ShoppingBag className="h-6 w-6" />
              <span className="font-semibold">Admin Panel</span>
            </Link>
          </div>
          <nav className="flex-1 overflow-auto py-4">
            <ul className="space-y-1 px-2">
              {navigation.map((item) => (
                <li key={item.name}>
                  <Link
                    href={item.href}
                    className={cn(
                      "flex items-center gap-3 rounded-md px-3 py-2 text-sm font-medium transition-colors",
                      pathname === item.href ? "bg-primary text-primary-foreground" : "hover:bg-muted",
                    )}
                  >
                    <item.icon className="h-5 w-5" />
                    <span>{item.name}</span>
                  </Link>
                </li>
              ))}
            </ul>
          </nav>
          <div className="border-t p-4">
            <div className="flex items-center gap-3">
              <Avatar>
                  <AvatarImage src={user?.avatar} alt={user?.fullName || user?.email} />
                  <AvatarFallback>{user?.fullName?.charAt(0) || user?.email?.charAt(0) || 'AD'}</AvatarFallback>
              </Avatar>
              <div className="flex flex-col">
                  <span className="text-sm font-medium">{user?.fullName || user?.email}</span>
                  <span className="text-xs text-muted-foreground">{user?.role || 'Admin'}</span>
              </div>
            </div>
          </div>
        </SheetContent>
      </Sheet>

      {/* Main content */}
      <div className="flex flex-1 flex-col overflow-hidden">
        <header className="flex h-16 items-center gap-4 border-b bg-background px-4 md:px-6">
          <div className="w-full flex-1">
            <Suspense fallback={<div>Loading search...</div>}>
              <form>
                <div className="relative">
                  <Search className="absolute left-2.5 top-2.5 h-4 w-4 text-muted-foreground" />
                  <Input
                    type="search"
                    placeholder="Search..."
                    className="w-full appearance-none bg-background pl-8 md:w-2/3 lg:w-1/3"
                  />
                </div>
              </form>
            </Suspense>
          </div>
          <DropdownMenu>
            <DropdownMenuTrigger asChild>
              <Button variant="outline" size="icon" className="relative">
                <Bell className="h-4 w-4" />
                <span className="absolute -right-1 -top-1 flex h-4 w-4 items-center justify-center rounded-full bg-primary text-[10px] text-primary-foreground">
                  3
                </span>
              </Button>
            </DropdownMenuTrigger>
            <DropdownMenuContent align="end">
              <DropdownMenuLabel>Notifications</DropdownMenuLabel>
              <DropdownMenuSeparator />
              <DropdownMenuItem>New order received</DropdownMenuItem>
              <DropdownMenuItem>Product out of stock</DropdownMenuItem>
              <DropdownMenuItem>New user registered</DropdownMenuItem>
            </DropdownMenuContent>
          </DropdownMenu>
          <DropdownMenu>
            <DropdownMenuTrigger asChild>
              <Button variant="ghost" size="icon" className="rounded-full">
                <Avatar className="h-8 w-8">
                    <AvatarImage src={user?.avatar} alt={user?.fullName || user?.email} />
                    <AvatarFallback>{user?.fullName?.charAt(0) || user?.email?.charAt(0) || 'AD'}</AvatarFallback>
                </Avatar>
              </Button>
            </DropdownMenuTrigger>
            <DropdownMenuContent align="end">
              <DropdownMenuLabel>My Account</DropdownMenuLabel>
              <DropdownMenuSeparator />
              <DropdownMenuItem>Profile</DropdownMenuItem>
              <DropdownMenuItem>Settings</DropdownMenuItem>
              <DropdownMenuSeparator />
                <DropdownMenuItem onClick={handleLogout}>Đăng xuất</DropdownMenuItem>
            </DropdownMenuContent>
          </DropdownMenu>
        </header>
        <main className="flex-1 overflow-auto p-4 md:p-6">
          <Suspense fallback={<div>Loading children...</div>}>{children}</Suspense>
        </main>
      </div>
    </div>
    </AuthGuard>
  )
}
