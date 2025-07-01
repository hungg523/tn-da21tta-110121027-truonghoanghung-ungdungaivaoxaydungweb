import numpy as np
import pandas as pd
from scipy.sparse import csr_matrix
from implicit.als import AlternatingLeastSquares
from implicit.evaluation import precision_at_k, mean_average_precision_at_k
import joblib
import os
from datetime import datetime

def recall_at_k(model, train_user_items, test_user_items, K=10):
    """Tính recall@k cho model"""
    n_users = train_user_items.shape[0]
    recall = 0.0
    
    for user_id in range(n_users):
        # Lấy các item đã tương tác trong test set
        test_items = test_user_items[user_id].indices
        
        if len(test_items) == 0:
            continue
            
        # Lấy top K item được đề xuất
        recommended = model.recommend(user_id, train_user_items[user_id], N=K)
        recommended_items = [item for item, _ in recommended]
        
        # Tính recall
        hits = len(set(recommended_items) & set(test_items))
        recall += hits / len(test_items)
    
    return recall / n_users

class ALSRecommender:
    def __init__(self, factors=50, regularization=0.01, iterations=30):
        self.model = AlternatingLeastSquares(
            factors=factors,
            regularization=regularization,
            iterations=iterations
        )
        self.user_mapping = None
        self.item_mapping = None
        self.reverse_user_mapping = None
        self.reverse_item_mapping = None
        self.interaction_matrix = None
        
    def _create_mappings(self, df):
        """Tạo mapping giữa user_id/item_id và index"""
        # Sắp xếp user_id và item_id để đảm bảo thứ tự
        unique_users = sorted(df['user_id'].unique())
        unique_items = sorted(df['variant_id'].unique())
        
        # Tạo mapping từ 0 đến n-1
        self.user_mapping = {id: i for i, id in enumerate(unique_users)}
        self.item_mapping = {id: i for i, id in enumerate(unique_items)}
        self.reverse_user_mapping = {v: k for k, v in self.user_mapping.items()}
        self.reverse_item_mapping = {v: k for k, v in self.item_mapping.items()}
        
    def _create_interaction_matrix(self, df):
        """Tạo ma trận tương tác user-item"""
        user_indices = [self.user_mapping[user] for user in df['user_id']]
        item_indices = [self.item_mapping[item] for item in df['variant_id']]
        values = df['interaction_value'].values
        
        self.interaction_matrix = csr_matrix(
            (values, (user_indices, item_indices)),
            shape=(len(self.user_mapping), len(self.item_mapping))
        )
        
    def fit(self, df):
        """Huấn luyện model"""
        self._create_mappings(df)
        self._create_interaction_matrix(df)
        self.model.fit(self.interaction_matrix)
        
    def recommend(self, user_id, n_items=10):
        """Đề xuất sản phẩm cho user"""
        if user_id not in self.user_mapping:
            return []
            
        user_idx = self.user_mapping[user_id]
        recommendations = self.model.recommend(
            user_idx,
            self.interaction_matrix[user_idx],
            N=n_items
        )
        
        # Thư viện implicit trả về tuple (items, scores)
        items, scores = recommendations
        return [(self.reverse_item_mapping[item], score) 
                for item, score in zip(items, scores)]
                
    def evaluate(self, test_df, k=10):
        """Đánh giá model"""
        # Lọc test_df để chỉ giữ lại các user và item đã có trong train set
        test_df = test_df[
            (test_df['user_id'].isin(self.user_mapping.keys())) & 
            (test_df['variant_id'].isin(self.item_mapping.keys()))
        ]
        
        if len(test_df) == 0:
            print("Warning: No valid test data after filtering")
            return {
                'precision@k': 0.0,
                'recall@k': 0.0,
                'map@k': 0.0
            }
        
        # Tính toán metrics cho từng user
        total_precision = 0.0
        total_recall = 0.0
        total_map = 0.0
        valid_users = 0
        
        for user_id in test_df['user_id'].unique():
            # Lấy các item thực tế user đã tương tác trong test set
            actual_items = set(test_df[test_df['user_id'] == user_id]['variant_id'])
            
            if len(actual_items) == 0:
                continue
            
            # Lấy các item được đề xuất
            recommended = self.recommend(user_id, n_items=k)
            recommended_items = set(item for item, _ in recommended)
            
            # Tính precision
            if len(recommended_items) > 0:
                hits = len(recommended_items & actual_items)
                precision = hits / len(recommended_items)
                total_precision += precision
                
                # Tính recall
                recall = hits / len(actual_items)
                total_recall += recall
                
                # Tính MAP
                ap = 0.0
                hits = 0
                for i, item in enumerate(recommended_items):
                    if item in actual_items:
                        hits += 1
                        ap += hits / (i + 1)
                if hits > 0:
                    ap /= hits
                    total_map += ap
                
                valid_users += 1
        
        if valid_users == 0:
            return {
                'precision@k': 0.0,
                'recall@k': 0.0,
                'map@k': 0.0
            }
        
        return {
            'precision@k': total_precision / valid_users,
            'recall@k': total_recall / valid_users,
            'map@k': total_map / valid_users
        }
        
    def _create_test_matrix(self, test_df):
        """Tạo ma trận test"""
        # Chỉ sử dụng các user và item đã có trong mapping
        user_indices = [self.user_mapping[user] for user in test_df['user_id']]
        item_indices = [self.item_mapping[item] for item in test_df['variant_id']]
        values = test_df['interaction_value'].values
        
        return csr_matrix(
            (values, (user_indices, item_indices)),
            shape=(len(self.user_mapping), len(self.item_mapping))
        )
        
    def save_model(self, path='models', test_df=None):
        """Lưu model và dữ liệu test nếu có"""
        if not os.path.exists(path):
            os.makedirs(path)
        timestamp = datetime.now().strftime('%Y%m%d_%H%M%S')
        model_path = os.path.join(path, f'als_model_{timestamp}.pkl')
        model_data = {
            'model': self.model,
            'user_mapping': self.user_mapping,
            'item_mapping': self.item_mapping,
            'reverse_user_mapping': self.reverse_user_mapping,
            'reverse_item_mapping': self.reverse_item_mapping,
            'interaction_matrix': self.interaction_matrix,
            'test_df': test_df  # Lưu thêm test_df
        }
        joblib.dump(model_data, model_path)
        return model_path
        
    @classmethod
    def load_model(cls, model_path):
        """Load model đã lưu và trả về cả test_df nếu có"""
        model_data = joblib.load(model_path)
        recommender = cls()
        recommender.model = model_data['model']
        recommender.user_mapping = model_data['user_mapping']
        recommender.item_mapping = model_data['item_mapping']
        recommender.reverse_user_mapping = model_data['reverse_user_mapping']
        recommender.reverse_item_mapping = model_data['reverse_item_mapping']
        recommender.interaction_matrix = model_data['interaction_matrix']
        test_df = model_data.get('test_df', None)
        return recommender, test_df 