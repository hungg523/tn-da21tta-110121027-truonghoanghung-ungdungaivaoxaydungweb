import React, { useState, useEffect } from 'react'
import { Dialog, DialogContent, DialogHeader, DialogTitle } from '@/components/ui/dialog'
import { Avatar, AvatarImage, AvatarFallback } from '@/components/ui/avatar'
import { Button } from '@/components/ui/button'
import { Star } from 'lucide-react'
import { format } from 'date-fns'
import { vi } from 'date-fns/locale'
import Swal from 'sweetalert2'
import { Review } from '@/services/review.service'
import { reviewService } from '@/services/review.service'
import { VariantReviews } from '@/services/review.service'

interface VariantReviewModalProps {
  open: boolean
  onOpenChange: (open: boolean) => void
  variantId: number
}

export default function VariantReviewModal({ open, onOpenChange, variantId }: VariantReviewModalProps) {
  const [reviews, setReviews] = useState<Review[]>([])
  const [isLoading, setIsLoading] = useState(false)
  const [starFilter, setStarFilter] = useState<number | null>(null)
  const [selectedReview, setSelectedReview] = useState<Review | null>(null)
  const [replyTexts, setReplyTexts] = useState<{ [reviewId: number]: string }>({})
  const [replyingId, setReplyingId] = useState<number | null>(null)

  const truncateText = (text: string | null) => {
    if (!text) return '-'
    if (text.length <= 20) return text
    return text.substring(0, 20) + '...'
  }

  const loadReviews = async () => {
    if (!variantId) return
    try {
      setIsLoading(true)
      const data: VariantReviews = await reviewService.getAllReviews(variantId, starFilter || undefined)
      setReviews(data.reviews)
    } catch (error) {
      Swal.fire({ icon: 'error', title: 'Lỗi', text: 'Không thể tải bình luận' })
    } finally {
      setIsLoading(false)
    }
  }

  useEffect(() => {
    if (open) loadReviews()
    // eslint-disable-next-line
  }, [open, variantId, starFilter])

  const handleReply = async (reviewId: number) => {
    const replyText = replyTexts[reviewId] || ""
    if (!replyText.trim()) return
    if (replyText.length > 500) {
      Swal.fire({ icon: 'warning', title: 'Tối đa 500 ký tự!' })
      return
    }
    try {
      setReplyingId(reviewId)
      await reviewService.replyReview(reviewId, replyText)
      setReplyTexts((prev) => ({ ...prev, [reviewId]: "" }))
      setReplyingId(null)
      await loadReviews()
      Swal.fire({ icon: 'success', title: 'Đã gửi phản hồi!' })
    } catch (error) {
      Swal.fire({ icon: 'error', title: 'Lỗi', text: 'Không thể gửi phản hồi' })
    }
  }

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="max-w-3xl">
        <DialogHeader>
          <DialogTitle>Bình luận của biến thể</DialogTitle>
        </DialogHeader>
        <div className="flex items-center gap-2 mb-4">
          <span>Lọc theo sao:</span>
          {[1,2,3,4,5].map(star => (
            <Button
              key={star}
              size="sm"
              variant={starFilter === star ? "default" : "outline"}
              onClick={() => setStarFilter(starFilter === star ? null : star)}
            >
              <Star className="h-4 w-4 text-yellow-400 fill-yellow-400 mr-1" />
              {star}
            </Button>
          ))}
        </div>
        <div className="space-y-4 max-h-[500px] overflow-y-auto">
          {isLoading ? (
            <div className="text-center py-4">Đang tải...</div>
          ) : reviews.length === 0 ? (
            <div className="text-center py-4">Không có bình luận nào</div>
          ) : (
            reviews.map(review => (
              <div key={review.reviewId} className="p-4 border rounded-lg bg-gray-50">
                <div className="flex items-center gap-2 mb-2">
                  <Avatar className="h-8 w-8">
                    <AvatarImage src={review.user.avatar} />
                    <AvatarFallback>{review.user.username[0]}</AvatarFallback>
                  </Avatar>
                  <div>
                    <div className="font-medium">{review.user.username}</div>
                    <div className="text-xs text-gray-500">{format(new Date(review.createdAt), 'dd/MM/yyyy HH:mm', { locale: vi })}</div>
                  </div>
                  <div className="ml-auto flex items-center gap-1">
                    <Star className="h-4 w-4 text-yellow-400 fill-yellow-400" />
                    <span className="font-medium">{review.rating}</span>
                  </div>
                </div>
                <div className="mb-2">
                  <span className="text-sm">
                    {truncateText(review.comment)}
                    {review.comment.length > 20 && (
                      <Button variant="link" size="sm" className="px-1" onClick={() => setSelectedReview(review)}>
                        Xem chi tiết
                      </Button>
                    )}
                  </span>
                </div>
                {review.images.length > 0 && (
                  <div className="flex gap-2 mb-2">
                    {review.images.map((img, idx) => (
                      img.imageUrl.endsWith('.mp4') ? (
                        <video key={idx} src={img.imageUrl} controls className="w-24 h-24 rounded object-cover" />
                      ) : (
                        <img key={idx} src={img.imageUrl} alt="review-img" className="w-24 h-24 rounded object-cover" />
                      )
                    ))}
                  </div>
                )}
                {review.reply ? (
                  <div className="mt-2 p-2 bg-blue-50 rounded">
                    <div className="text-xs text-gray-500 mb-1">Phản hồi từ shop:</div>
                    <div className="text-sm whitespace-pre-wrap">{review.reply.replyMessage}</div>
                  </div>
                ) : (
                  <div className="mt-2">
                    <textarea
                      className="w-full border rounded p-2 text-sm"
                      rows={2}
                      maxLength={500}
                      placeholder="Nhập phản hồi (tối đa 500 ký tự)"
                      value={replyTexts[review.reviewId] || ""}
                      onChange={e => setReplyTexts(prev => ({ ...prev, [review.reviewId]: e.target.value }))}
                      disabled={replyingId !== null && replyingId !== review.reviewId}
                    />
                    <div className="flex justify-end mt-1">
                      <Button
                        size="sm"
                        disabled={replyingId !== null && replyingId !== review.reviewId || !(replyTexts[review.reviewId] && replyTexts[review.reviewId].trim())}
                        onClick={() => handleReply(review.reviewId)}
                      >
                        Gửi phản hồi
                      </Button>
                    </div>
                  </div>
                )}
              </div>
            ))
          )}
        </div>
        <Dialog open={!!selectedReview} onOpenChange={() => setSelectedReview(null)}>
          <DialogContent className="max-w-[500px]">
            <DialogHeader>
              <DialogTitle>Chi tiết bình luận</DialogTitle>
            </DialogHeader>
            {selectedReview && (
              <div className="space-y-2">
                <div className="font-medium">{selectedReview.user.username}</div>
                <div className="text-xs text-gray-500">{format(new Date(selectedReview.createdAt), 'dd/MM/yyyy HH:mm', { locale: vi })}</div>
                <div className="whitespace-pre-wrap break-words break-all text-gray-700">{selectedReview.comment}</div>
                {selectedReview.images.length > 0 && (
                  <div className="flex gap-2">
                    {selectedReview.images.map((img, idx) => (
                      img.imageUrl.endsWith('.mp4') ? (
                        <video key={idx} src={img.imageUrl} controls className="w-24 h-24 rounded object-cover" />
                      ) : (
                        <img key={idx} src={img.imageUrl} alt="review-img" className="w-24 h-24 rounded object-cover" />
                      )
                    ))}
                  </div>
                )}
              </div>
            )}
            <div className="flex justify-end">
              <Button variant="outline" onClick={() => setSelectedReview(null)}>
                Đóng
              </Button>
            </div>
          </DialogContent>
        </Dialog>
      </DialogContent>
    </Dialog>
  )
} 