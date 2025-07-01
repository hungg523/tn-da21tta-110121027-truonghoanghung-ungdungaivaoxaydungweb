# Hệ thống Đề xuất Sản phẩm

Hệ thống đề xuất sản phẩm sử dụng Collaborative Filtering (ALS) dựa trên tương tác của người dùng.

## Cài đặt

1. Cài đặt các thư viện cần thiết:
```bash
pip install -r requirements.txt
```

2. Tạo file `.env` với các biến môi trường:
```
DB_USER=your_db_user
DB_PASSWORD=your_db_password
DB_SERVER=your_db_server
DB_NAME=your_db_name
MODEL_PATH=models/als_model_latest.pkl
```

## Cấu trúc thư mục

```
recommendations/
├── api/                 # API endpoints
├── data/               # Data loading utilities
├── models/             # Model implementation
├── scripts/            # Training scripts
├── dashboard/          # Admin dashboard
└── requirements.txt    # Dependencies
```

## Sử dụng

1. Huấn luyện model:
```bash
python scripts/train_model.py
```

2. Chạy API server:
```bash
uvicorn api.main:app --reload
```

3. Chạy dashboard:
```bash
python dashboard/app.py
```

## API Endpoints

### Đề xuất sản phẩm
- `POST /recommend`: Đề xuất sản phẩm cho user
  ```json
  {
    "n_items": 10
  }
  ```

### Metrics
- `GET /metrics/user-activity`: Thống kê hoạt động của user
- `GET /metrics/product-activity`: Thống kê hoạt động của sản phẩm
- `GET /metrics/model-performance`: Đánh giá hiệu suất model

### Admin
- `GET /admin/blacklist`: Lấy danh sách sản phẩm trong blacklist
- `POST /admin/blacklist`: Thêm sản phẩm vào blacklist
- `DELETE /admin/blacklist`: Xóa sản phẩm khỏi blacklist

### Quản lý training model

- `POST /admin/train-model`: Train lại model ngay lập tức, trả về thông tin file model mới.
- `POST /admin/schedule-train`: Đặt lịch train model tự động (body: cron expression, ví dụ: '0 2 * * *' để train lúc 2h sáng mỗi ngày).
- `GET /admin/schedule-train`: Xem lịch train model tự động.
- `DELETE /admin/schedule-train`: Hủy lịch train model tự động.

### Quản lý file model

- `GET /admin/models`: Lấy danh sách các file model đã train.
- `DELETE /admin/models/{filename}`: Xóa file model.
- `POST /admin/models/use`: Chọn file model để sử dụng (body: filename).

## Dashboard

Dashboard admin có các tính năng:
- Theo dõi hoạt động của user và sản phẩm
- Đánh giá hiệu suất model
- Quản lý blacklist
- A/B testing

## Cập nhật model

Model được cập nhật tự động hàng ngày/tuần thông qua cron job:

```bash
# Cập nhật hàng ngày
0 0 * * * cd /path/to/recommendations && python scripts/train_model.py
```

## Đánh giá model

Model được đánh giá dựa trên các metrics:
- Precision@K
- Recall@K
- MAP@K (Mean Average Precision)

## Blacklist

Sản phẩm có thể bị thêm vào blacklist nếu:
- Không còn được bán
- Chất lượng đề xuất kém
- Vi phạm chính sách 