# AppleShop - Hệ thống Thương mại Điện tử với AI Gợi ý Sản phẩm

## 📖 Giới thiệu

AppleShop là một hệ thống thương mại điện tử hoàn chỉnh chuyên về các sản phẩm Apple, tích hợp trí tuệ nhân tạo để gợi ý sản phẩm phù hợp cho người dùng. Dự án được phát triển như một đồ án tốt nghiệp với mục tiêu xây dựng một nền tảng mua sắm trực tuyến hiện đại và thông minh.

## 🎯 Mục tiêu

- **Xây dựng hệ thống thương mại điện tử hoàn chỉnh** với các chức năng cơ bản: quản lý sản phẩm, đơn hàng, người dùng
- **Tích hợp AI/ML** để cung cấp gợi ý sản phẩm cá nhân hóa dựa trên hành vi người dùng
- **Triển khai kiến trúc microservices** với Docker để dễ dàng mở rộng và bảo trì
- **Cung cấp giao diện người dùng hiện đại** cho cả khách hàng và quản trị viên
- **Đảm bảo hiệu năng và bảo mật** trong môi trường thương mại điện tử

## 🏗️ Kiến trúc Hệ thống

Hệ thống được thiết kế theo mô hình microservices với các thành phần chính:

```
┌─────────────────┐    ┌─────────────────┐
│   Frontend      │    │  Admin Panel    │
│   (Angular)     │    │   (Next.js)     │
│   Port: 4200    │    │   Port: 4201    │
└─────────┬───────┘    └─────────┬───────┘
          │                      │
          └──────────┬───────────┘
                     │
         ┌───────────▼───────────┐
         │    Backend API        │
         │   (.NET Core API)     │
         │     Port: 7001        │
         └───────────┬───────────┘
                     │
    ┌────────────────┼────────────────┐
    │                │                │
    ▼                ▼                ▼
┌─────────┐    ┌─────────┐    ┌─────────────┐
│Database │    │ Redis   │    │ AI Service  │
│SQL Server│   │ Cache   │    │  (FastAPI)  │
│         │    │         │    │ Port: 8000  │
└─────────┘    └─────────┘    └─────────────┘
```

### Thành phần chính:

1. **Frontend (Angular 19)**
   - Giao diện khách hàng
   - Responsive design
   - PWA support
   - Material Design

2. **Admin Panel (Next.js 15)**
   - Giao diện quản trị
   - Dashboard analytics
   - Quản lý sản phẩm, đơn hàng, người dùng
   - Real-time notifications

3. **Backend API (.NET Core)**
   - RESTful API
   - Authentication & Authorization (JWT)
   - Clean Architecture
   - Entity Framework Core
   - SignalR for real-time communication

4. **AI Recommendation Service (Python FastAPI)**
   - Collaborative filtering với ALS (Alternating Least Squares)
   - Real-time recommendations
   - Model training và updating
   - Redis caching

5. **Database & Storage**
   - SQL Server cho dữ liệu chính
   - Redis cho caching và session
   - File storage cho hình ảnh

## 🛠️ Công nghệ Sử dụng

### Frontend
- **Angular 19** - Framework chính
- **Angular Material** - UI Components
- **TypeScript** - Ngôn ngữ lập trình
- **RxJS** - Reactive programming
- **Three.js & GSAP** - 3D animations

### Admin Panel
- **Next.js 15** - React framework
- **TypeScript** - Ngôn ngữ lập trình
- **Tailwind CSS** - Styling
- **Radix UI** - Component library
- **Shadcn/ui** - UI components

### Backend
- **.NET 8** - Runtime và framework
- **ASP.NET Core** - Web API framework
- **Entity Framework Core** - ORM
- **SQL Server** - Database
- **SignalR** - Real-time communication
- **JWT** - Authentication

### AI/ML Service
- **Python 3.9+** - Ngôn ngữ lập trình
- **FastAPI** - Web framework
- **Pandas & NumPy** - Data processing
- **Scikit-learn** - Machine learning
- **Implicit** - Collaborative filtering
- **Redis** - Caching

### DevOps & Infrastructure
- **Docker & Docker Compose** - Containerization
- **Nginx** - Reverse proxy
- **Git** - Version control

## 📋 Yêu cầu Hệ thống

### Phần mềm cần thiết:
- **Docker Desktop** (v4.0+)
- **Git** (v2.30+)
- **Node.js** (v18+) - Cho development
- **.NET 8 SDK** - Cho development backend
- **Python 3.9+** - Cho development AI service

### Tài nguyên hệ thống:
- **RAM**: Tối thiểu 8GB (khuyến nghị 16GB)
- **CPU**: 4 cores trở lên
- **Disk**: 10GB trống
- **Network**: Kết nối internet ổn định

## 🚀 Hướng dẫn Triển khai

### 1. Clone Repository

```bash
git clone <repository-url>
cd TN
```

### 2. Triển khai với Docker (Khuyến nghị)

#### 2.1. Khởi động toàn bộ hệ thống:

```bash
cd src/DoanTN-docker
docker-compose up -d
```

#### 2.2. Kiểm tra trạng thái services:

```bash
docker-compose ps
```

#### 2.3. Truy cập các dịch vụ:

- **Frontend (Khách hàng)**: http://localhost:4200
- **Admin Panel**: http://localhost:4201
- **Backend API**: http://localhost:7001
- **AI Recommendation**: http://localhost:8000

### 3. Development Setup (Tùy chọn)

#### 3.1. Backend (.NET)

```bash
cd src/DoanTN-backend
dotnet restore
dotnet build
dotnet run --project src/AppleShop.API
```

#### 3.2. Frontend (Angular)

```bash
cd src/DoanTN-frontend/app
npm install
ng serve
```

#### 3.3. Admin Panel (Next.js)

```bash
cd src/DoanTN-fr-admin
npm install
npm run dev
```

#### 3.4. AI Service (Python)

```bash
cd src/DoanTN-AI/recommendations
pip install -r requirements.txt
uvicorn api.main:app --reload --port 8000
```

## 🔧 Cấu hình

### Environment Variables

Tạo file `.env` trong thư mục `src/DoanTN-docker/`:

```env
# Database
DB_CONNECTION_STRING=Server=sqlserver;Database=AppleShopDB;User Id=sa;Password=YourPassword;
REDIS_CONNECTION_STRING=redis:6379

# JWT
JWT_SECRET=YourJWTSecretKey
JWT_ISSUER=AppleShop
JWT_AUDIENCE=AppleShopUsers

# AI Service
AI_SERVICE_URL=http://ai-recommendation:8000
MODEL_UPDATE_INTERVAL=24

# File Storage
UPLOAD_PATH=/app/wwwroot/assets
MAX_FILE_SIZE=10485760
```

## 📊 Tính năng Chính

### Khách hàng:
- ✅ Đăng ký/Đăng nhập
- ✅ Duyệt sản phẩm với bộ lọc
- ✅ Gợi ý sản phẩm cá nhân hóa
- ✅ Giỏ hàng và thanh toán
- ✅ Theo dõi đơn hàng
- ✅ Đánh giá sản phẩm
- ✅ Chat support

### Quản trị viên:
- ✅ Dashboard analytics
- ✅ Quản lý sản phẩm/danh mục
- ✅ Quản lý đơn hàng
- ✅ Quản lý người dùng
- ✅ Báo cáo doanh thu
- ✅ Cấu hình khuyến mãi
- ✅ Theo dõi hiệu suất AI

### AI Recommendation:
- ✅ Collaborative filtering
- ✅ Content-based filtering
- ✅ Real-time recommendations
- ✅ A/B testing support
- ✅ Performance monitoring

## 🧪 Testing

### Backend Testing:
```bash
cd src/DoanTN-backend
dotnet test
```

### Frontend Testing:
```bash
cd src/DoanTN-frontend/app
ng test
```

### AI Service Testing:
```bash
cd src/DoanTN-AI/recommendations
python -m pytest tests/
```

## 📖 API Documentation

- **Backend API**: http://localhost:7001/swagger

## 🔍 Monitoring & Logs

```bash
# Xem logs của tất cả services
docker-compose logs -f

# Xem logs của service cụ thể
docker-compose logs -f frontend
docker-compose logs -f api-backend
docker-compose logs -f ai-recommendation
```

## 🤝 Đóng góp

1. Fork repository
2. Tạo feature branch (`git checkout -b feature/AmazingFeature`)
3. Commit changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to branch (`git push origin feature/AmazingFeature`)
5. Mở Pull Request

## 📝 License

Dự án này được phát triển cho mục đích học tập và nghiên cứu.

## 👥 Tác giả

- **Họ tên**: Trương Hoàng Hưng
- **MSSV**: 110121027
- **Email**: hoanghung52304@gmail.com
- **Trường**: Trường Kỹ thuật và Công nghệ - Đại học Trà Vinh

## 📞 Liên hệ

Nếu có bất kỳ câu hỏi hoặc góp ý nào, vui lòng liên hệ qua:
- Email: hoanghung52304@gmail.com

---

## 🔄 Cập nhật và Bảo trì

### Cập nhật Docker Images:
```bash
docker-compose pull
docker-compose up -d
```

### Backup Database:
```bash
docker exec sqlserver /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P YourPassword -Q "BACKUP DATABASE AppleShopDB TO DISK = '/backup/AppleShopDB.bak'"
```

### Monitoring Resources:
```bash
docker stats
```

---

*Cảm ơn bạn đã quan tâm đến dự án AppleShop! 🍎*
