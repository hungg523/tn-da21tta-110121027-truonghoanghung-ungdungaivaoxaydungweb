'use client';
import { useEffect, useState, useRef } from 'react';
import { eventService, Banner, CreateBannerRequest, UpdateBannerRequest } from '@/services/event.service';
import { Button } from '../../../components/ui/button';
import { Input } from '../../../components/ui/input';
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/components/ui/select';
import { Dialog, DialogContent, DialogHeader, DialogTitle, DialogTrigger } from '../../../components/ui/dialog';
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from '@/components/ui/table';
import { Switch } from '@/components/ui/switch';
import Swal from 'sweetalert2'
import { toast } from 'sonner';
import { Plus, Pencil, Trash2 } from 'lucide-react';

const permittedExtensions = [
  '.png', '.jpg', '.jpeg', '.pdf', '.webp',
  '.mp4', '.avi', '.mov', '.mkv', '.flv'
];

function getFileExtension(name: string) {
  return name.substring(name.lastIndexOf('.')).toLowerCase();
}

export default function BannerPage() {
  const [banners, setBanners] = useState<Banner[]>([]);
  const [isLoading, setIsLoading] = useState(false);
  const [isDialogOpen, setIsDialogOpen] = useState(false);
  const [isEdit, setIsEdit] = useState(false);
  const [selectedBanner, setSelectedBanner] = useState<Banner | null>(null);
  const [formData, setFormData] = useState<CreateBannerRequest>({
    title: '',
    description: '',
    imageData: '',
    link: '',
    status: 1,
    position: 1,
  });
  const [file, setFile] = useState<File | null>(null);
  const [preview, setPreview] = useState<string | null>(null);
  const [filterStatus, setFilterStatus] = useState<number | undefined>(undefined);
  const [filterPosition, setFilterPosition] = useState<number | undefined>(undefined);
  const fileInputRef = useRef<HTMLInputElement>(null);

  const loadData = async () => {
    setIsLoading(true);
    try {
      const data = await eventService.getAllBanner(filterStatus, filterPosition);
      setBanners(data);
    } catch (e) {
      toast.error('Không thể tải danh sách banner');
    } finally {
      setIsLoading(false);
    }
  };

  useEffect(() => {
    loadData();
  }, [filterStatus, filterPosition]);

  const handleOpenCreate = () => {
    setIsEdit(false);
    setFormData({ title: '', description: '', imageData: '', link: '', status: 1, position: 1 });
    setFile(null);
    setPreview(null);
    setIsDialogOpen(true);
  };

  const handleOpenEdit = (banner: Banner) => {
    setIsEdit(true);
    setSelectedBanner(banner);
    setFormData({
      title: banner.title,
      description: banner.description,
      imageData: '',
      link: banner.link,
      status: banner.status,
      position: banner.position,
    });
    setFile(null);
    setPreview(banner.url);
    setIsDialogOpen(true);
  };

  const handleDelete = async (id: number) => {
    if (!window.confirm('Bạn có chắc chắn muốn xóa banner này?')) return;
    try {
      await eventService.deleteBanner(id);
      toast.success('Đã xóa banner');
      loadData();
    } catch (e) {
      toast.error('Xóa banner thất bại');
    }
  };

  const handleFileChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const f = e.target.files?.[0];
    if (!f) return;
    const ext = getFileExtension(f.name);
    if (!permittedExtensions.includes(ext)) {
      toast.error('Định dạng file không được phép');
      return;
    }
    if ([".png", ".jpg", ".jpeg", ".webp"].includes(ext) && f.size > 2 * 1024 * 1024) {
      toast.error('Ảnh vượt quá 2MB');
      return;
    }
    if ([".mp4", ".avi", ".mov", ".mkv", ".flv"].includes(ext) && f.size > 5 * 1024 * 1024) {
      toast.error('Video vượt quá 5MB');
      return;
    }
    setFile(f);
    const reader = new FileReader();
    reader.onload = (ev) => {
      setFormData(fd => ({ ...fd, imageData: ev.target?.result as string }));
      setPreview(ev.target?.result as string);
    };
    reader.readAsDataURL(f);
  };

  const handleSubmit = async () => {
    try {
      if (isEdit && selectedBanner) {
        const updateData: any = {
          title: formData.title || selectedBanner.title,
          description: formData.description || selectedBanner.description,
          link: formData.link || selectedBanner.link,
          status: typeof formData.status === 'number' ? formData.status : selectedBanner.status,
          position: typeof formData.position === 'number' ? formData.position : selectedBanner.position,
        };
        if (file) {
          updateData.imageData = formData.imageData || null;
          updateData.file = file;
        } else {
          updateData.imageData = formData.imageData || null;
        }
        await eventService.updateBanner(selectedBanner.id, updateData);
        setIsDialogOpen(false);
        toast.success('Cập nhật banner thành công');
        loadData();
      } else {
        await eventService.createBanner({ ...formData, file: file || undefined });
        setIsDialogOpen(false);
        toast.success('Tạo banner thành công');
        loadData();
      }
    } catch (e: any) {
      toast.error(e.message || 'Lưu banner thất bại');
    }
  };

  return (
    <div className="container mx-auto py-10">
      <div className="mb-6">
        <h2 className="text-2xl font-bold mb-4">Quản lý Banner/Sự kiện</h2>
        <div className="flex flex-wrap items-end gap-4">
          <div>
            <label className="block text-sm font-medium mb-1">Trạng thái</label>
            <Select
              value={filterStatus === undefined ? 'all' : String(filterStatus)}
              onValueChange={v => setFilterStatus(v === 'all' ? undefined : Number(v))}
            >
              <SelectTrigger className="w-[150px]">
                <SelectValue placeholder="Tất cả trạng thái" />
              </SelectTrigger>
              <SelectContent>
                <SelectItem value="all">Tất cả trạng thái</SelectItem>
                <SelectItem value="1">Mở</SelectItem>
                <SelectItem value="0">Tắt</SelectItem>
              </SelectContent>
            </Select>
          </div>
          <div>
            <label className="block text-sm font-medium mb-1">Vị trí</label>
            <Input
              type="number"
              className="w-[120px]"
              placeholder="Vị trí"
              value={filterPosition ?? ''}
              onChange={e => setFilterPosition(e.target.value === '' ? undefined : Number(e.target.value))}
            />
          </div>
          <div className="flex-1" />
          <Button onClick={handleOpenCreate} className="h-10 mt-6">
            <Plus className="h-4 w-4 mr-1" /> Thêm banner
          </Button>
        </div>
      </div>
      <Table>
        <TableHeader>
          <TableRow>
            <TableHead>ID</TableHead>
            <TableHead>Tiêu đề</TableHead>
            <TableHead>Mô tả</TableHead>
            <TableHead>Ảnh/Video</TableHead>
            <TableHead>Link</TableHead>
            <TableHead>Vị trí</TableHead>
            <TableHead>Trạng thái</TableHead>
            <TableHead>Thao tác</TableHead>
          </TableRow>
        </TableHeader>
        <TableBody>
          {isLoading ? (
            <TableRow>
              <TableCell colSpan={7} className="text-center">Đang tải...</TableCell>
            </TableRow>
          ) : banners.length === 0 ? (
            <TableRow>
              <TableCell colSpan={7} className="text-center">Không có banner nào</TableCell>
            </TableRow>
          ) : (
            banners.map(banner => (
              <TableRow key={banner.id}>
                <TableCell>{banner.id}</TableCell>
                <TableCell>{banner.title}</TableCell>
                <TableCell>
                  <span
                    className="cursor-pointer hover:text-blue-500"
                    onClick={() => {
                      if (!banner.description) return;
                      Swal.fire({
                        title: 'Mô tả chi tiết',
                        html: `<div class=\"text-left\">${banner.description}</div>`,
                        width: '600px',
                        showCloseButton: true,
                        showConfirmButton: false
                      })
                    }}
                  >
                    {banner.description
                      ? banner.description.length > 10
                        ? banner.description.substring(0, 10) + '...'
                        : banner.description
                      : '-'}
                  </span>
                </TableCell>
                <TableCell>
                  {banner.url ? (
                    banner.url.startsWith('data:video') || /\.(mp4|avi|mov|mkv|flv)$/i.test(banner.url) ? (
                      <video src={banner.url} controls className="w-32 h-20 rounded" />
                    ) : banner.url.startsWith('data:image') || /\.(png|jpg|jpeg|webp)$/i.test(banner.url) ? (
                      <img src={banner.url} alt={banner.title} className="w-20 h-10 object-cover rounded" />
                    ) : (
                      <span className="text-xs text-muted-foreground">Không hỗ trợ</span>
                    )
                  ) : (
                    <span className="text-xs text-muted-foreground">Chưa có ảnh/video</span>
                  )}
                </TableCell>
                <TableCell>
                  <a href={banner.link} target="_blank" rel="noopener noreferrer" className="text-blue-600 underline">
                    {banner.link}
                  </a>
                </TableCell>
                <TableCell>{banner.position}</TableCell>
                <TableCell>
                  <Switch
                    checked={banner.status === 1}
                    onCheckedChange={async (checked) => {
                      try {
                        await eventService.updateBanner(banner.id, {
                          title: banner.title || null,
                          description: banner.description || null,
                          link: banner.link || null,
                          status: checked ? 1 : 0,
                          position: banner.position ?? null,
                          imageData: null,
                        });
                        toast.success(checked ? 'Đã bật banner' : 'Đã tắt banner');
                        loadData();
                      } catch {
                        toast.error('Cập nhật trạng thái thất bại');
                      }
                    }}
                  />
                  <span className="ml-2 text-sm">{banner.status === 1 ? 'Mở' : 'Tắt'}</span>
                </TableCell>
                <TableCell>
                  <Button variant="outline" size="sm" onClick={() => handleOpenEdit(banner)}>
                    <Pencil className="h-4 w-4 mr-1" /> Sửa
                  </Button>
                </TableCell>
              </TableRow>
            ))
          )}
        </TableBody>
      </Table>
      <Dialog open={isDialogOpen} onOpenChange={setIsDialogOpen}>
        <DialogContent>
          <DialogHeader>
            <DialogTitle>{isEdit ? 'Cập nhật banner' : 'Thêm banner mới'}</DialogTitle>
          </DialogHeader>
          <div className="space-y-4">
            <div>
              <label className="block text-sm font-medium mb-1">Tên banner</label>
              <Input value={formData.title} onChange={e => setFormData({ ...formData, title: e.target.value })} />
            </div>
            <div>
              <label className="block text-sm font-medium mb-1">Mô tả</label>
              <Input value={formData.description} onChange={e => setFormData({ ...formData, description: e.target.value })} />
            </div>
            <div>
              <label className="block text-sm font-medium mb-1">Chọn ảnh/video (2MB cho ảnh, 5MB cho video)</label>
              <input
                id="file-upload"
                ref={fileInputRef}
                type="file"
                accept={permittedExtensions.join(",")}
                onChange={handleFileChange}
                className="hidden"
              />
              <Button type="button" variant="outline" onClick={() => fileInputRef.current?.click()}>
                Chọn tệp
              </Button>
              <span className="ml-2 text-sm text-gray-600">
                {file ? file.name : preview ? 'Đã có tệp' : 'Chưa chọn tệp'}
              </span>
              {preview && (preview.startsWith('data:video') || /\.(mp4|avi|mov|mkv|flv)$/i.test(file?.name || preview) ? (
                <video src={preview} controls className="w-32 h-20 mt-2 rounded" />
              ) : preview.startsWith('data:image') || /\.(png|jpg|jpeg|webp)$/i.test(file?.name || preview) ? (
                <img src={preview} alt="preview" className="w-32 h-16 mt-2 object-cover rounded" />
              ) : null)}
            </div>
            <div>
              <label className="block text-sm font-medium mb-1">Link chuyển hướng</label>
              <Input value={formData.link} onChange={e => setFormData({ ...formData, link: e.target.value })} />
            </div>
            <div>
              <label className="block text-sm font-medium mb-1">Vị trí</label>
              <Input type="number" value={formData.position ?? ''} onChange={e => setFormData({ ...formData, position: Number(e.target.value) })} />
            </div>
            <div className="flex items-center space-x-2">
              <Switch checked={formData.status === 1} onCheckedChange={checked => setFormData({ ...formData, status: checked ? 1 : 0 })} />
              <span className="text-sm">{formData.status === 1 ? 'Mở' : 'Tắt'}</span>
            </div>
            <Button onClick={handleSubmit}>{isEdit ? 'Cập nhật' : 'Tạo mới'}</Button>
          </div>
        </DialogContent>
      </Dialog>
    </div>
  );
} 