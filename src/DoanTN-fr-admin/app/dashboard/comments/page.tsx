"use client"

import { useEffect, useState } from 'react'
import { ReviewReport, ReportStatus, reviewReportService } from '@/services/review-report.service'
import { Button } from '@/components/ui/button'
import { Check, X, Image as ImageIcon, MessageSquare, User } from 'lucide-react'
import Swal from 'sweetalert2'
import { Badge } from '@/components/ui/badge'
import { Avatar, AvatarFallback, AvatarImage } from '@/components/ui/avatar'
import { format } from 'date-fns'
import { vi } from 'date-fns/locale'
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
} from '@/components/ui/dialog'
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from '@/components/ui/table'
import { Star } from 'lucide-react'

export default function CommentsPage() {
  const [reports, setReports] = useState<ReviewReport[]>([])
  const [isLoading, setIsLoading] = useState(true)
  const [selectedReview, setSelectedReview] = useState<ReviewReport | null>(null)

  const truncateText = (text: string, maxLength: number) => {
    if (text.length <= maxLength) return text
    return text.substring(0, maxLength) + '...'
  }

  const loadData = async () => {
    try {
      setIsLoading(true)
      const data = await reviewReportService.getAllReports()
      setReports(data)
    } catch (error) {
      Swal.fire({
        icon: 'error',
        title: 'Lỗi',
        text: 'Không thể tải danh sách báo cáo'
      })
    } finally {
      setIsLoading(false)
    }
  }

  const handleReport = async (reportId: number, status: number) => {
    try {
      const result = await Swal.fire({
        title: status === 1 ? 'Xác nhận báo cáo?' : 'Từ chối báo cáo?',
        text: status === 1 
          ? 'Bạn có chắc chắn muốn xác nhận báo cáo này?' 
          : 'Bạn có chắc chắn muốn từ chối báo cáo này?',
        icon: 'question',
        showCancelButton: true,
        confirmButtonText: 'Xác nhận',
        cancelButtonText: 'Hủy',
        confirmButtonColor: status === 1 ? '#3085d6' : '#d33',
      })

      if (result.isConfirmed) {
        await reviewReportService.handleReport(reportId, status)
        await loadData()
        Swal.fire({
          icon: 'success',
          title: 'Thành công',
          text: status === 1 
            ? 'Đã xác nhận báo cáo thành công' 
            : 'Đã từ chối báo cáo thành công'
        })
      }
    } catch (error) {
      Swal.fire({
        icon: 'error',
        title: 'Lỗi',
        text: 'Không thể xử lý báo cáo'
      })
    }
  }

  useEffect(() => {
    loadData()
  }, [])

  const getStatusBadge = (status: ReportStatus) => {
    switch (status) {
      case ReportStatus.Pending:
        return <Badge variant="secondary" className="bg-yellow-100 text-yellow-800">Chờ xử lý</Badge>
      case ReportStatus.Resolved:
        return <Badge variant="secondary" className="bg-green-100 text-green-800">Đã xử lý</Badge>
      case ReportStatus.Rejected:
        return <Badge variant="destructive">Đã từ chối</Badge>
    }
  }

  return (
    <div className="space-y-4">
      <div className="flex items-center justify-between">
        <h1 className="text-3xl font-bold">Quản lý báo cáo bình luận</h1>
      </div>

      <div className="rounded-md border">
        <Table>
          <TableHeader>
            <TableRow>
              <TableHead className="w-[100px]">ID</TableHead>
              <TableHead className="w-[200px]">Người bình luận</TableHead>
              <TableHead className="w-[300px]">Nội dung</TableHead>
              <TableHead className="w-[150px]">Hình ảnh</TableHead>
              <TableHead className="w-[100px]">Đánh giá</TableHead>
              <TableHead className="w-[100px]">Số báo cáo</TableHead>
              <TableHead className="w-[150px]">Thời gian</TableHead>
              <TableHead className="w-[100px]">Trạng thái</TableHead>
              <TableHead className="w-[150px]">Thao tác</TableHead>
            </TableRow>
          </TableHeader>
          <TableBody>
            {reports.map((report) => (
              <TableRow key={report.id}>
                <TableCell>{report.id}</TableCell>
                <TableCell>
                  <div className="flex items-center gap-2">
                    <Avatar>
                      <AvatarImage src={report.review.user.avatar} />
                      <AvatarFallback>{report.review.user.username[0]}</AvatarFallback>
                    </Avatar>
                    <span>{report.review.user.username}</span>
                  </div>
                </TableCell>
                <TableCell>
                  <div 
                    className="text-sm font-medium cursor-pointer hover:text-blue-500 transition-colors"
                    onClick={() => setSelectedReview(report)}
                  >
                    {truncateText(report.review.comment, 20)}
                  </div>
                </TableCell>
                <TableCell>
                  {report.review.images.length > 0 && (
                    <div className="flex items-center gap-2">
                      <ImageIcon className="h-4 w-4" />
                      <span>{report.review.images.length} ảnh</span>
                    </div>
                  )}
                </TableCell>
                <TableCell>
                  <div className="flex items-center gap-1">
                    <Star className="h-4 w-4 fill-yellow-400 text-yellow-400" />
                    <span>{report.review.rating}</span>
                  </div>
                </TableCell>
                <TableCell>{report.totalReports}</TableCell>
                <TableCell>
                  {format(new Date(report.review.createdAt), 'dd/MM/yyyy HH:mm', { locale: vi })}
                </TableCell>
                <TableCell>{getStatusBadge(report.status)}</TableCell>
                <TableCell>
                  <div className="flex items-center gap-2">
                    <Button
                      variant="outline"
                      size="sm"
                      onClick={() => handleReport(report.id, 1)}
                      disabled={report.status !== ReportStatus.Pending}
                    >
                      <Check className="h-4 w-4 mr-1" />
                      Xác nhận
                    </Button>
                    <Button
                      variant="destructive"
                      size="sm"
                      onClick={() => handleReport(report.id, 2)}
                      disabled={report.status !== ReportStatus.Pending}
                    >
                      <X className="h-4 w-4 mr-1" />
                      Từ chối
                    </Button>
                  </div>
                </TableCell>
              </TableRow>
            ))}
          </TableBody>
        </Table>
      </div>

      <Dialog open={!!selectedReview} onOpenChange={() => setSelectedReview(null)}>
        <DialogContent className="max-w-3xl">
          <DialogHeader>
            <DialogTitle>Chi tiết bình luận</DialogTitle>
          </DialogHeader>
          {selectedReview && (
            <div className="space-y-4">
              <div className="flex items-center gap-4">
                <Avatar className="h-12 w-12">
                  <AvatarImage src={selectedReview.review.user.avatar} />
                  <AvatarFallback>{selectedReview.review.user.username[0]}</AvatarFallback>
                </Avatar>
                <div>
                  <div className="font-medium">{selectedReview.review.user.username}</div>
                  <div className="text-sm text-gray-500">
                    {format(new Date(selectedReview.review.createdAt), 'dd/MM/yyyy HH:mm', { locale: vi })}
                  </div>
                </div>
              </div>

              <div className="flex items-center gap-2">
                <div className="flex items-center gap-1">
                  <Star className="h-5 w-5 fill-yellow-400 text-yellow-400" />
                  <span className="font-medium">{selectedReview.review.rating}</span>
                </div>
                <Badge variant="outline">{selectedReview.review.rating} sao</Badge>
              </div>

              <div className="p-4 bg-gray-50 rounded-lg max-w-[600px] break-words overflow-x-auto">
                <div className="text-sm whitespace-pre-wrap break-words">{selectedReview.review.comment}</div>
              </div>

              {selectedReview.review.images.length > 0 && (
                <div className="grid grid-cols-3 gap-4">
                  {selectedReview.review.images.map((image, index) => (
                    <div key={index} className="aspect-square rounded-lg overflow-hidden">
                      <img
                        src={image.imageUrl}
                        alt={`Hình ảnh ${index + 1}`}
                        className="w-full h-full object-cover"
                      />
                    </div>
                  ))}
                </div>
              )}

              {selectedReview.review.reply && (
                <div className="space-y-2">
                  <div className="font-medium">Phản hồi từ cửa hàng:</div>
                  <div className="p-4 bg-gray-50 rounded-lg">
                    <div className="text-sm whitespace-pre-wrap">{selectedReview.review.reply.content}</div>
                  </div>
                </div>
              )}

              <div className="space-y-2">
                <div className="font-medium">Danh sách báo cáo:</div>
                <div className="space-y-2 max-h-72 overflow-y-auto">
                  {selectedReview.reportUsers.map((report, index) => (
                    <div key={index} className="p-3 bg-gray-50 rounded-lg">
                      <div className="flex items-center gap-2">
                        <Avatar className="h-8 w-8">
                          <AvatarImage src={report.user.avatar} />
                          <AvatarFallback>{report.user.username[0]}</AvatarFallback>
                        </Avatar>
                        <div>
                          <div className="font-medium">{report.user.username}</div>
                          <div className="text-sm text-gray-500">
                            {format(new Date(report.createdAt), 'dd/MM/yyyy HH:mm', { locale: vi })}
                          </div>
                        </div>
                      </div>
                      <div className="mt-2 text-sm">
                        <span className="font-medium">Lý do: </span>
                        {report.reason}
                      </div>
                    </div>
                  ))}
                </div>
              </div>
            </div>
          )}
        </DialogContent>
      </Dialog>
    </div>
  )
} 