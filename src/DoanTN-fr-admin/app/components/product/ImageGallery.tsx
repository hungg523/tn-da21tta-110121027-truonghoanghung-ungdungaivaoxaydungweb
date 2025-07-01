import { useState, useRef, useEffect } from 'react'
import { Button } from "@/components/ui/button"
import { Pencil, Trash2 } from "lucide-react"
import { ProductImage } from "@/services/variant.service"

interface ImageGalleryProps {
  images: ProductImage[];
  onEdit?: (image: ProductImage) => void;
  onDelete?: (imageId: number) => void;
}

const ImageGallery = ({ images, onEdit, onDelete }: ImageGalleryProps) => {
  const [currentIndex, setCurrentIndex] = useState(0)
  const [showModal, setShowModal] = useState(false)
  const modalRef = useRef<HTMLDivElement>(null)

  useEffect(() => {
    const handleClickOutside = (event: MouseEvent) => {
      if (modalRef.current && !modalRef.current.contains(event.target as Node)) {
        setShowModal(false)
      }
    }

    if (showModal) {
      document.addEventListener('mousedown', handleClickOutside)
    }

    return () => {
      document.removeEventListener('mousedown', handleClickOutside)
    }
  }, [showModal])

  if (images.length === 0) return <div>-</div>

  const visibleImages = images.slice(0, 3)
  const totalImages = images.length

  const handlePrev = () => {
    setCurrentIndex((prev) => (prev > 0 ? prev - 1 : totalImages - 1))
  }

  const handleNext = () => {
    setCurrentIndex((prev) => (prev < totalImages - 1 ? prev + 1 : 0))
  }

  return (
    <>
      <div className="flex gap-2">
        {visibleImages.map((image, index) => (
          <div key={image.id} className="relative group">
            <img
              src={image.url}
              alt={image.title}
              className="w-10 h-10 object-cover rounded cursor-pointer"
              onClick={() => {
                setCurrentIndex(index)
                setShowModal(true)
              }}
            />
            {(onEdit || onDelete) && (
              <div className="absolute inset-0 bg-black bg-opacity-50 opacity-0 group-hover:opacity-100 flex items-center justify-center gap-2">
                {onEdit && (
                  <Button
                    variant="ghost"
                    size="icon"
                    onClick={(e) => {
                      e.stopPropagation()
                      onEdit(image)
                    }}
                  >
                    <Pencil className="h-4 w-4 text-white" />
                  </Button>
                )}
                {onDelete && (
                  <Button
                    variant="ghost"
                    size="icon"
                    onClick={(e) => {
                      e.stopPropagation()
                      onDelete(image.id)
                    }}
                  >
                    <Trash2 className="h-4 w-4 text-white" />
                  </Button>
                )}
              </div>
            )}
          </div>
        ))}
        {totalImages > 3 && (
          <div 
            className="w-10 h-10 flex items-center justify-center bg-gray-100 rounded cursor-pointer"
            onClick={() => setShowModal(true)}
          >
            +{totalImages - 3}
          </div>
        )}
      </div>

      {showModal && (
        <div className="fixed inset-0 bg-black bg-opacity-75 flex items-center justify-center z-50">
          <div ref={modalRef} className="bg-white p-4 rounded-lg max-w-4xl w-full">
            <div className="flex justify-between items-center mb-4">
              <h3 className="text-lg font-semibold">Hình ảnh sản phẩm</h3>
              <div className="flex items-center gap-2">
                {(onEdit || onDelete) && (
                  <div className="flex items-center gap-2 mr-4">
                    {onEdit && (
                      <Button
                        variant="outline"
                        size="sm"
                        onClick={() => {
                          onEdit(images[currentIndex])
                          setShowModal(false)
                        }}
                      >
                        <Pencil className="h-4 w-4 mr-1" />
                        Chỉnh sửa
                      </Button>
                    )}
                    {onDelete && (
                      <Button
                        variant="destructive"
                        size="sm"
                        onClick={() => {
                          onDelete(images[currentIndex].id)
                          setShowModal(false)
                        }}
                      >
                        <Trash2 className="h-4 w-4 mr-1" />
                        Xóa
                      </Button>
                    )}
                  </div>
                )}
                <button 
                  onClick={() => setShowModal(false)}
                  className="text-gray-500 hover:text-gray-700"
                >
                  ✕
                </button>
              </div>
            </div>
            <div className="relative">
              <img
                src={images[currentIndex].url}
                alt={images[currentIndex].title}
                className="w-full h-96 object-contain"
              />
              <button
                onClick={handlePrev}
                className="absolute left-0 top-1/2 transform -translate-y-1/2 bg-white p-2 rounded-full shadow-md"
              >
                ←
              </button>
              <button
                onClick={handleNext}
                className="absolute right-0 top-1/2 transform -translate-y-1/2 bg-white p-2 rounded-full shadow-md"
              >
                →
              </button>
            </div>
            <div className="flex justify-center gap-2 mt-4">
              {images.map((_, index) => (
                <button
                  key={index}
                  onClick={() => setCurrentIndex(index)}
                  className={`w-2 h-2 rounded-full ${
                    index === currentIndex ? 'bg-blue-500' : 'bg-gray-300'
                  }`}
                />
              ))}
            </div>
          </div>
        </div>
      )}
    </>
  )
}

export default ImageGallery 