import pandas as pd
import sqlalchemy
from sqlalchemy import create_engine
from dotenv import load_dotenv
import os
import requests
import json
import numpy as np
from datetime import datetime, timedelta

load_dotenv()

def get_db_connection():
    """Tạo kết nối đến database"""
    try:
        connection_string = f"mssql+pyodbc://{os.getenv('DB_USER')}:{os.getenv('DB_PASSWORD')}@{os.getenv('DB_SERVER')}/{os.getenv('DB_NAME')}?driver=FreeTDS"
        engine = create_engine(connection_string)
        # Test connection
        engine.connect()
        return engine
    except Exception as e:
        print(f"Database connection failed: {str(e)}")
        return None

def generate_sample_interactions():
    """Tạo dữ liệu tương tác mẫu khi không có database"""
    print("Generating sample interaction data...")
    
    # Tạo dữ liệu mẫu
    np.random.seed(42)  # Để có kết quả nhất quán
    
    n_users = 100
    n_items = 50
    n_interactions = 2000
    
    # Chỉ 80% users có interaction để mô phỏng thực tế
    active_users = int(n_users * 0.8)  # 80 users có interaction
    
    # Tạo user_ids và variant_ids, chỉ sử dụng 80 users đầu
    user_ids = np.random.randint(1, active_users + 1, n_interactions)
    variant_ids = np.random.randint(1, n_items + 1, n_interactions)
    
    # Tạo interaction_type và interaction_value
    interaction_types = np.random.choice(['view', 'cart', 'purchase'], n_interactions, p=[0.7, 0.2, 0.1])
    interaction_values = []
    
    for itype in interaction_types:
        if itype == 'view':
            interaction_values.append(1)
        elif itype == 'cart':
            interaction_values.append(2)
        else:  # purchase
            interaction_values.append(3)
      # Tạo created_at với một số interactions cũ để test inactive users
    created_at = []
    
    for i in range(n_interactions):
        user_id = user_ids[i]
        
        # 60% users sẽ có interaction trong 30 ngày (active)
        # 20% users sẽ có interaction cũ hơn 30 ngày (inactive)  
        # 20% users không có interaction (dormant - đã handle ở trên)
        
        if user_id <= int(active_users * 0.75):  # 75% của active users = 60 users
            # Active users: interactions trong 30 ngày gần đây
            days_ago = np.random.randint(0, 31)
        else:
            # Inactive users: interactions cũ hơn 30 ngày
            days_ago = np.random.randint(31, 90)
            
        created_at.append(datetime.now() - timedelta(
            days=days_ago,
            hours=np.random.randint(0, 24),
            minutes=np.random.randint(0, 60)
        ))
    
    return pd.DataFrame({
        'user_id': user_ids,
        'variant_id': variant_ids,
        'interaction_type': interaction_types,
        'interaction_value': interaction_values,
        'created_at': created_at
    })

def load_user_interactions():
    """Đọc dữ liệu tương tác người dùng"""
    try:
        engine = get_db_connection()
        if engine is None:
            return generate_sample_interactions()
            
        query = """
        SELECT 
            user_id,
            variant_id,
            interaction_type,
            interaction_value,
            created_at
        FROM user_interaction
        """
        return pd.read_sql(query, engine)
    except Exception as e:
        print(f"Error loading user interactions from database: {str(e)}")
        print("Falling back to sample data...")
        return generate_sample_interactions()

def load_product_data():
    """Đọc thông tin sản phẩm từ API"""
    try:
        base_url = os.getenv('BASE_URL', 'http://localhost:7001')
        url = f"{base_url}/product-variant/get-all?isActived=1"
        response = requests.get(url, verify=False)
        if response.status_code == 200:
            data = response.json()
            if data.get('isSuccess'):
                products = data.get('data', {}).get('productVariants', [])
                
                # Chuyển đổi dữ liệu thành DataFrame
                df = pd.DataFrame([{
                    'variant_id': p['variantId'],
                    'product_id': p['productId'],
                    'category_id': p['categoryId'],
                    'name': p['name'],
                    'description': p['description'],
                    'price': p['price'],
                    'discount_price': p['discountPrice'],
                    'stock': p['stock'],
                    'reserved_stock': p['reservedStock'],
                    'actual_stock': p['actualStock'],
                    'sold_quantity': p['soldQuantity'],
                    'total_reviews': p['totalReviews'],
                    'average_rating': p['averageRating'],
                    'is_actived': p['isActived'],
                    'images': json.dumps(p['images']),
                    'details': json.dumps(p['details'])
                } for p in products])
                
                return df
    except Exception as e:
        print(f"Error loading product data: {str(e)}")
    return pd.DataFrame()

def generate_sample_user_data():
    """Tạo dữ liệu user mẫu khi không có database"""
    print("Generating sample user data...")
    
    np.random.seed(42)
    n_users = 100
    
    return pd.DataFrame({
        'user_id': range(1, n_users + 1),
        'username': [f'user_{i}' for i in range(1, n_users + 1)],
        'email': [f'user{i}@example.com' for i in range(1, n_users + 1)],
        'gender': np.random.choice(['Male', 'Female', 'Other'], n_users),
        'date_of_birth': [datetime.now() - timedelta(days=np.random.randint(6570, 25550)) for _ in range(n_users)],  # 18-70 years old
        'image_url': [f'https://example.com/avatar_{i}.jpg' for i in range(1, n_users + 1)],
        'role': ['User'] * n_users,
        'created_at': [datetime.now() - timedelta(days=np.random.randint(1, 365)) for _ in range(n_users)],
        'last_login': [datetime.now() - timedelta(days=np.random.randint(0, 30)) for _ in range(n_users)],
        'isactived': [True] * n_users
    })

def load_user_data():
    """Đọc thông tin người dùng"""
    try:
        engine = get_db_connection()
        if engine is None:
            return generate_sample_user_data()
            
        query = """
        SELECT 
            user_id,
            username,
            email,
            gender,
            date_of_birth,
            image_url,
            role,
            createddate as created_at,
            last_login,
            isactived
        FROM [user]
        """
        return pd.read_sql(query, engine)
    except Exception as e:
        print(f"Error loading user data from database: {str(e)}")
        print("Falling back to sample data...")
        return generate_sample_user_data()