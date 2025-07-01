import sys
import os
import glob
from fastapi import FastAPI, HTTPException, BackgroundTasks, Depends, Request
from fastapi.security import HTTPBearer, HTTPAuthorizationCredentials
from fastapi.middleware.cors import CORSMiddleware
from fastapi.responses import StreamingResponse, JSONResponse
from pydantic import BaseModel
from typing import List, Optional
import pandas as pd
from datetime import datetime, timedelta
import json
from apscheduler.schedulers.background import BackgroundScheduler
import jwt
from jwt.exceptions import InvalidTokenError
import redis
from functools import lru_cache
import asyncio
from queue import Queue
import threading

from models.als_model import ALSRecommender
from data.data_loader import load_user_interactions, load_product_data, load_user_data

app = FastAPI(title="Product Recommendation API")

# Cấu hình Redis
redis_client = None
REDIS_RETRY_INTERVAL = 900  # 15 phút
last_redis_error = None

def get_redis_client():
    """Lấy Redis client với xử lý lỗi"""
    global redis_client, last_redis_error
    
    if redis_client is None:
        try:
            redis_client = redis.Redis(
                host=os.getenv('REDIS_HOST', '127.0.0.1'),
                port=int(os.getenv('REDIS_PORT', 6379)),
                db=int(os.getenv('REDIS_DB', 0)),
                decode_responses=True,
                socket_timeout=5,
                socket_connect_timeout=5
            )
            # Test kết nối
            redis_client.ping()
            last_redis_error = None
        except Exception as e:
            print(f"Redis connection error: {str(e)}")
            last_redis_error = datetime.now()
            return None
            
    return redis_client

def safe_redis_operation(operation, *args, **kwargs):
    """Thực hiện thao tác Redis an toàn với xử lý lỗi"""
    client = get_redis_client()
    if client is None:
        return None
        
    try:
        return operation(client, *args, **kwargs)
    except Exception as e:
        print(f"Redis operation error: {str(e)}")
        global redis_client
        redis_client = None
        return None

# Cache time to live (TTL) - 1 giờ
CACHE_TTL = 3600
# Cache time to live cho metrics - 5 phút
METRICS_CACHE_TTL = 300

# Cấu hình CORS
origins = [
    "http://localhost:4200",
    "http://localhost:4201",
    "http://localhost:8000",
    "http://thh.id.vn",
    "https://thh.id.vn",
    "http://admin.thh.id.vn",
    "https://admin.thh.id.vn"
]

app.add_middleware(
    CORSMiddleware,
    allow_origins=["*"],  # Cho phép tất cả origins trong môi trường development
    allow_credentials=True,
    allow_methods=["GET", "POST", "PUT", "DELETE", "OPTIONS"],
    allow_headers=["*"],
    expose_headers=["*"]
)

security = HTTPBearer()

# Thêm cấu hình JWT
JWT_SECRET = "This is a simple token to test the gateway"  # Thay thế bằng secret key của bạn
JWT_ALGORITHM = "HS256"

# Queue để lưu trữ progress
training_progress = Queue()

def training_progress_callback(progress: float, message: str):
    """Callback function để cập nhật tiến trình training"""
    training_progress.put({
        "progress": progress,
        "message": message,
        "timestamp": datetime.now().isoformat()
    })

async def training_stream():
    """Generator function để stream tiến trình training"""
    while True:
        try:
            # Lấy progress từ queue
            progress_data = training_progress.get(timeout=1)
            yield f"data: {json.dumps(progress_data)}\n\n"
        except:
            # Nếu không có data mới, gửi heartbeat
            yield "data: {\"type\": \"heartbeat\"}\n\n"
            await asyncio.sleep(1)

async def get_current_user(request: Request, credentials: HTTPAuthorizationCredentials = Depends(security)) -> int:
    try:
        # Log toàn bộ headers
        # print("All headers:", dict(request.headers))
        # print("Authorization header:", request.headers.get("authorization"))
        
        token = credentials.credentials
        # print("JWT Token:", token)
        
        # Điều chỉnh options để khớp với .NET
        payload = jwt.decode(
            token, 
            JWT_SECRET, 
            algorithms=[JWT_ALGORITHM],
            options={
                'verify_iss': False,  # ValidateIssuer = false
                'verify_aud': False,  # ValidateAudience = false
                'verify_exp': False,  # ValidateLifetime = false
                'verify_signature': True  # ValidateIssuerSigningKey = true
            }
        )
        # print("JWT Payload:", payload)
        
        user_id = int(payload.get("nameid"))
        if user_id is None:
            raise HTTPException(status_code=401, detail="Invalid token payload")
        return user_id
    except InvalidTokenError as e:
        # print("InvalidTokenError:", str(e))
        raise HTTPException(status_code=401, detail="Invalid token")
    except Exception as e:
        # print("Error processing token:", str(e))
        raise HTTPException(status_code=401, detail=str(e))

# Biến toàn cục lưu model và test_df đang sử dụng
recommender = None
current_test_df = None

# Load model khi khởi động
print("Loading recommendation model...")
try:
    # Tìm file model mới nhất
    model_dir = os.path.join(os.path.dirname(os.path.dirname(os.path.abspath(__file__))), "models")
    model_files = [f for f in os.listdir(model_dir) if f.startswith("als_model_")]
    if not model_files:
        print("No model found, training new model...")
        from scripts.train_model import train_and_evaluate
        recommender, metrics = train_and_evaluate()
        current_test_df = None
    else:
        latest_model = max(model_files)
        model_path = os.path.join(model_dir, latest_model)
        print(f"Loading model from {model_path}")
        recommender, current_test_df = ALSRecommender.load_model(model_path)
    print("Model loaded successfully!")
except Exception as e:
    print(f"Error loading model: {str(e)}")
    recommender = None
    current_test_df = None

scheduler = BackgroundScheduler()
scheduler.start()
train_job_id = 'auto_train_model'

class RecommendationRequest(BaseModel):
    n_items: Optional[int] = 10

class RecommendationResponse(BaseModel):
    variant_id: int
    score: float
    product_name: str
    description: str
    price: float
    discount_price: float
    stock: int
    reserved_stock: int
    actual_stock: int
    average_rating: float
    sold_quantity: int
    total_reviews: int
    images: List[dict]
    details: List[dict]

@app.get("/")
def read_root():
    return {"message": "Product Recommendation API"}

# Cache cho product data
@lru_cache(maxsize=1)
def get_cached_product_data():
    return load_product_data()

@app.post("/recommend", response_model=List[RecommendationResponse])
async def get_recommendations(request: RecommendationRequest, user_id: int = Depends(get_current_user)):
    if recommender is None:
        raise HTTPException(status_code=500, detail="Model not loaded")
        
    try:
        # Kiểm tra cache
        cache_key = f"recommendations:{user_id}:{request.n_items}"
        cached_result = safe_redis_operation(lambda client, key: client.get(key), cache_key)
        
        if cached_result:
            print(f"Cache hit for user {user_id}")
            return json.loads(cached_result)
            
        print(f"Cache miss for user {user_id}")
        
        # Lấy recommendations
        recommendations = recommender.recommend(user_id, request.n_items)
        if not recommendations:
            raise HTTPException(status_code=404, detail="No recommendations found for this user")
            
        # Load product data từ cache
        products_df = get_cached_product_data()
        
        # Debug: In ra tên các cột
        print("Available columns:", products_df.columns.tolist())
        
        # Format response
        response = []
        for variant_id, score in recommendations:
            try:
                # Tìm sản phẩm trong database
                product = products_df[products_df['variant_id'] == variant_id]
                if product.empty:
                    print(f"Product {variant_id} not found in database")
                    continue
                    
                product = product.iloc[0]
                
                # Parse JSON fields
                try:
                    images = json.loads(product['images']) if isinstance(product['images'], str) else product['images']
                except:
                    images = []
                    
                try:
                    details = json.loads(product['details']) if isinstance(product['details'], str) else product['details']
                except:
                    details = []
                
                # Tạo response object
                response_item = RecommendationResponse(
                    variant_id=int(variant_id),
                    score=float(score),
                    product_name=str(product['name']),
                    description=str(product.get('description', '')),
                    price=float(product['price']),
                    discount_price=float(product['discount_price']),
                    stock=int(product['stock']),
                    reserved_stock=int(product.get('reserved_stock', 0)),
                    actual_stock=int(product.get('actual_stock', 0)),
                    sold_quantity=int(product.get('sold_quantity', 0)),
                    average_rating=float(product['average_rating']),
                    total_reviews=int(product['total_reviews']),
                    images=images,
                    details=details
                )
                response.append(response_item)
                
            except Exception as e:
                print(f"Error processing product {variant_id}: {str(e)}")
                continue
        
        if not response:
            raise HTTPException(status_code=404, detail="No valid products found in recommendations")
            
        # Lưu vào cache
        safe_redis_operation(
            lambda client, key, value, ttl: client.setex(key, ttl, value),
            cache_key,
            json.dumps([item.dict() for item in response]),
            CACHE_TTL
        )
            
        return response
        
    except HTTPException as he:
        raise he
    except Exception as e:
        print(f"Error in get_recommendations: {str(e)}")
        raise HTTPException(status_code=500, detail=str(e))

@app.get("/metrics/user-activity")
async def get_user_activity_metrics():
    """Lấy metrics về hoạt động của user"""
    try:
        # Kiểm tra cache
        cache_key = "metrics:user-activity"
        cached_result = safe_redis_operation(lambda client, key: client.get(key), cache_key)
        
        if cached_result:
            print("Cache hit for user activity metrics")
            return JSONResponse(
                content=json.loads(cached_result),
                headers={
                    "Access-Control-Allow-Origin": "*",
                    "Access-Control-Allow-Methods": "GET, OPTIONS",
                    "Access-Control-Allow-Headers": "*"
                }
            )
            
        print("Cache miss for user activity metrics")
        
        # Load dữ liệu
        try:
            interactions_df = load_user_interactions()
            users_df = load_user_data()
        except Exception as e:
            print(f"Error loading data: {str(e)}")
            return JSONResponse(
                content={"error": "Failed to load data", "detail": str(e)},
                status_code=500,
                headers={
                    "Access-Control-Allow-Origin": "*",
                    "Access-Control-Allow-Methods": "GET, OPTIONS",
                    "Access-Control-Allow-Headers": "*"
                }
            )
        
        # Kiểm tra dữ liệu trống
        if interactions_df.empty or users_df.empty:
            return JSONResponse(
                content={"error": "No data available"},
                status_code=404,
                headers={
                    "Access-Control-Allow-Origin": "*",
                    "Access-Control-Allow-Methods": "GET, OPTIONS",
                    "Access-Control-Allow-Headers": "*"
                }
            )
          # Tính số tương tác theo user
        user_interactions = interactions_df.groupby('user_id').size()
        
        # Tính thời gian hoạt động gần nhất
        latest_interactions = interactions_df.groupby('user_id')['created_at'].max()
        
        # Phân loại user dựa trên toàn bộ user trong hệ thống
        all_user_ids = set(users_df['user_id'].unique())
        users_with_interactions = set(latest_interactions.index)
        users_without_interactions = all_user_ids - users_with_interactions
        
        # Active users: có tương tác trong 30 ngày gần đây
        active_users = latest_interactions[latest_interactions > datetime.now() - timedelta(days=30)].index
        
        # Inactive users: có tương tác nhưng không hoạt động trong 30 ngày gần đây
        inactive_users_with_history = latest_interactions[latest_interactions <= datetime.now() - timedelta(days=30)].index
        
        # Dormant users: user đã đăng ký nhưng chưa có tương tác nào
        dormant_users = users_without_interactions
        
        result = {
            "total_users": int(len(users_df)),
            "active_users": int(len(active_users)),  # User có tương tác trong 30 ngày
            "inactive_users": int(len(inactive_users_with_history)),  # User có tương tác cũ nhưng không hoạt động 30 ngày gần đây
            "dormant_users": int(len(dormant_users)),  # User chưa bao giờ có tương tác
            "users_with_interactions": int(len(users_with_interactions)),  # Tổng user đã từng có tương tác
            "avg_interactions_per_user": float(user_interactions.mean()) if not user_interactions.empty else 0,
            "max_interactions_per_user": int(user_interactions.max()) if not user_interactions.empty else 0,
            "min_interactions_per_user": int(user_interactions.min()) if not user_interactions.empty else 0,
            # Thêm thông tin để verify
            "verification": {
                "total_check": f"{len(active_users)} + {len(inactive_users_with_history)} + {len(dormant_users)} = {len(active_users) + len(inactive_users_with_history) + len(dormant_users)} (should equal {len(users_df)})"
            }
        }
          # Lưu vào cache
        safe_redis_operation(
            lambda client, key, value, ttl: client.setex(key, ttl, value),
            cache_key,
            json.dumps(result),
            METRICS_CACHE_TTL
        )
        
        return JSONResponse(
            content=result,
            headers={
                "Access-Control-Allow-Origin": "*",
                "Access-Control-Allow-Methods": "GET, OPTIONS",
                "Access-Control-Allow-Headers": "*"
            }
        )
    except Exception as e:
        print(f"Unexpected error in get_user_activity_metrics: {str(e)}")
        return JSONResponse(
            content={"error": "Internal server error", "detail": str(e)},
            status_code=500,
            headers={
                "Access-Control-Allow-Origin": "*",
                "Access-Control-Allow-Methods": "GET, OPTIONS",
                "Access-Control-Allow-Headers": "*"
            }
        )

@app.get("/metrics/product-activity")
async def get_product_activity_metrics():
    """Lấy metrics về hoạt động của sản phẩm"""
    # Kiểm tra cache
    cache_key = "metrics:product-activity"
    cached_result = safe_redis_operation(lambda client, key: client.get(key), cache_key)
    
    if cached_result:
        print("Cache hit for product activity metrics")
        return json.loads(cached_result)
        
    print("Cache miss for product activity metrics")
    
    interactions_df = load_user_interactions()
    products_df = load_product_data()
    
    # Tính số tương tác theo sản phẩm
    product_interactions = interactions_df.groupby('variant_id').size()
    
    # Phân loại sản phẩm
    popular_products = product_interactions[product_interactions > product_interactions.mean()].index
    unpopular_products = product_interactions[product_interactions <= product_interactions.mean()].index
    
    result = {
        "total_products": int(len(products_df)),
        "popular_products": int(len(popular_products)),
        "unpopular_products": int(len(unpopular_products)),
        "avg_interactions_per_product": float(product_interactions.mean()),
        "max_interactions_per_product": int(product_interactions.max()),
        "min_interactions_per_product": int(product_interactions.min())
    }
    
    # Lưu vào cache
    safe_redis_operation(
        lambda client, key, value, ttl: client.setex(key, ttl, value),
        cache_key,
        json.dumps(result),
        METRICS_CACHE_TTL
    )
    
    return result

@app.get("/metrics/model-performance")
async def get_model_performance():
    """Lấy metrics về hiệu suất của mô hình đang sử dụng"""
    global recommender, current_test_df
    if recommender is None:
        raise HTTPException(status_code=500, detail="Model not loaded")
    
    if current_test_df is None:
        # Nếu không có test data, trả về thông báo hoặc tạo test data mới
        return {
            "status": "no_test_data",
            "message": "No test data available. Please retrain the model to generate performance metrics.",
            "recommendation": "Use POST /admin/train-model to train a new model with test data"
        }
    
    # Kiểm tra cache
    cache_key = "metrics:model-performance"
    cached_result = safe_redis_operation(lambda client, key: client.get(key), cache_key)
    if cached_result:
        print("Cache hit for model performance metrics")
        return json.loads(cached_result)
    print("Cache miss for model performance metrics")
    
    try:
        metrics = recommender.evaluate(current_test_df)
        safe_redis_operation(
            lambda client, key, value, ttl: client.setex(key, ttl, value),
            cache_key,
            json.dumps(metrics),
            METRICS_CACHE_TTL
        )
        return metrics
    except Exception as e:
        print(f"Error evaluating model: {str(e)}")
        raise HTTPException(status_code=500, detail=f"Error evaluating model: {str(e)}")

@app.post("/admin/blacklist")
def add_to_blacklist(variant_id: int):
    """Thêm sản phẩm vào blacklist"""
    # TODO: Implement blacklist functionality
    pass

@app.delete("/admin/blacklist")
def remove_from_blacklist(variant_id: int):
    """Xóa sản phẩm khỏi blacklist"""
    # TODO: Implement blacklist functionality
    pass

@app.get("/admin/blacklist")
def get_blacklist():
    """Lấy danh sách sản phẩm trong blacklist"""
    # TODO: Implement blacklist functionality
    pass

@app.post("/admin/train-model")
async def train_model_now():
    """Bắt đầu training model và trả về response ngay lập tức"""
    # Kiểm tra xem có đang training không
    if not training_progress.empty():
        raise HTTPException(status_code=400, detail="Training is already in progress")
    
    # Xóa cache cũ
    clear_all_cache()
      # Tạo thread mới để training
    def train_thread():
        try:
            from scripts.train_model import train_and_evaluate
            # Truyền callback function vào hàm training
            recommender_new, metrics = train_and_evaluate(progress_callback=training_progress_callback)
            
            # Tự động load model mới sau khi training xong
            try:
                # Tìm file model mới nhất
                model_dir = os.path.join(os.path.dirname(os.path.dirname(os.path.abspath(__file__))), "models")
                model_files = [f for f in os.listdir(model_dir) if f.startswith("als_model_")]
                if model_files:
                    latest_model = max(model_files)
                    model_path = os.path.join(model_dir, latest_model)
                    global recommender, current_test_df
                    recommender, current_test_df = ALSRecommender.load_model(model_path)
                    training_progress.put({
                        "progress": 100,
                        "message": "Model loaded successfully after training",
                        "timestamp": datetime.now().isoformat()
                    })
            except Exception as load_error:
                training_progress.put({
                    "progress": 95,
                    "message": f"Training completed but failed to load new model: {str(load_error)}",
                    "timestamp": datetime.now().isoformat()
                })
            
            # Thông báo hoàn thành
            training_progress.put({
                "progress": 100,
                "message": "Training completed successfully",
                "metrics": metrics,
                "timestamp": datetime.now().isoformat()
            })
        except Exception as e:
            # Thông báo lỗi
            training_progress.put({
                "progress": 0,
                "message": f"Training failed: {str(e)}",
                "error": True,
                "timestamp": datetime.now().isoformat()
            })
    
    # Bắt đầu training trong thread riêng
    thread = threading.Thread(target=train_thread)
    thread.start()
    redis_client.flushdb()
    # Trả về response ngay lập tức
    return {
        "message": "Đã nhận yêu cầu train",
        "status": "started",
        "timestamp": datetime.now().isoformat()
    }

@app.get("/admin/train-model/stream")
async def stream_training_progress():
    """Stream tiến trình training"""
    return StreamingResponse(
        training_stream(),
        media_type="text/event-stream"
    )

@app.get("/admin/training-status")
async def get_training_status():
    """Lấy trạng thái training hiện tại"""
    if training_progress.empty():
        return {"status": "idle"}
    
    try:
        # Lấy progress mới nhất
        latest_progress = training_progress.queue[-1]
        return {
            "status": "training",
            "progress": latest_progress
        }
    except:
        return {"status": "idle"}

@app.post("/admin/schedule-train")
def schedule_train(cron: str):
    # cron: ví dụ '0 2 * * *' (2h sáng mỗi ngày)
    from apscheduler.triggers.cron import CronTrigger
    scheduler.remove_job(train_job_id, jobstore=None) if scheduler.get_job(train_job_id) else None
    scheduler.add_job(
        lambda: __import__('scripts.train_model').train_model.train_and_evaluate(),
        CronTrigger.from_crontab(cron),
        id=train_job_id
    )
    return {"message": f"Scheduled training with cron: {cron}"}

@app.get("/admin/schedule-train")
def get_schedule_train():
    job = scheduler.get_job(train_job_id)
    if job:
        return {"next_run_time": str(job.next_run_time), "cron": str(job.trigger)}
    return {"message": "No schedule"}

@app.delete("/admin/schedule-train")
def delete_schedule_train():
    scheduler.remove_job(train_job_id, jobstore=None) if scheduler.get_job(train_job_id) else None
    return {"message": "Schedule removed"}

@app.get("/admin/models")
def list_models():
    model_files = glob.glob("models/als_model_*.pkl")
    return [{"filename": os.path.basename(f), "created": os.path.getctime(f), "size": os.path.getsize(f)} for f in model_files]

@app.delete("/admin/models/{filename}")
def delete_model(filename: str):
    path = os.path.join("models", filename)
    if os.path.exists(path):
        os.remove(path)
        return {"message": "Deleted"}
    return {"error": "File not found"}

@app.post("/admin/models/use")
def use_model(filename: str):
    path = os.path.join("models", filename)
    if os.path.exists(path):
        global recommender, current_test_df
        recommender, current_test_df = ALSRecommender.load_model(path)
        redis_client.flushdb()
        return {"message": f"Model {filename} loaded"}
    return {"error": "File not found"}

@app.delete("/admin/cache/{user_id}")
def clear_user_cache(user_id: int):
    """Xóa cache của một user cụ thể"""
    pattern = f"recommendations:{user_id}:*"
    keys = safe_redis_operation(lambda client, pattern: client.keys(pattern), pattern)
    if keys:
        safe_redis_operation(lambda client, *keys: client.delete(*keys), *keys)
    return {"message": f"Cache cleared for user {user_id}"}

@app.delete("/admin/cache")
def clear_all_cache():
    """Xóa toàn bộ cache"""
    safe_redis_operation(lambda client: client.flushdb())
    return {"message": "All cache cleared"}

@app.delete("/admin/cache/metrics")
def clear_metrics_cache():
    """Xóa cache của metrics"""
    pattern = "metrics:*"
    keys = safe_redis_operation(lambda client, pattern: client.keys(pattern), pattern)
    if keys:
        safe_redis_operation(lambda client, *keys: client.delete(*keys), *keys)
    return {"message": "Metrics cache cleared"} 