import sys
import os
from datetime import datetime, timedelta

# Thêm thư mục gốc vào PYTHONPATH
sys.path.append(os.path.dirname(os.path.dirname(os.path.abspath(__file__))))

import pandas as pd
from sklearn.model_selection import train_test_split
from models.als_model import ALSRecommender
from data.data_loader import load_user_interactions

def train_and_evaluate(progress_callback=None):
    """Train và đánh giá model"""
    try:
        # Load data
        if progress_callback:
            progress_callback(0, "Loading data...")
        interactions_df = load_user_interactions()
        
        # Lọc dữ liệu
        if progress_callback:
            progress_callback(10, "Filtering data...")
        # Lọc theo số lượng tương tác của sản phẩm
        item_counts = interactions_df['variant_id'].value_counts()
        valid_items = item_counts[item_counts >= 3].index
        filtered_df = interactions_df[interactions_df['variant_id'].isin(valid_items)]
        
        # Lọc theo số lượng tương tác của người dùng
        user_counts = filtered_df['user_id'].value_counts()
        valid_users = user_counts[user_counts >= 2].index
        filtered_df = filtered_df[filtered_df['user_id'].isin(valid_users)]
        
        if progress_callback:
            progress_callback(15, "After filtering:")
            progress_callback(15, f"Number of interactions: {len(filtered_df)}")
            progress_callback(15, f"Number of unique users: {filtered_df['user_id'].nunique()}")
            progress_callback(15, f"Number of unique items: {filtered_df['variant_id'].nunique()}")
        
        # Chia train/test ngẫu nhiên
        if progress_callback:
            progress_callback(20, "\nSplitting data...")
        
        # Đảm bảo mỗi user có ít nhất 1 tương tác trong train và test
        train_users = []
        test_users = []
        
        for user in filtered_df['user_id'].unique():
            user_interactions = filtered_df[filtered_df['user_id'] == user]
            if len(user_interactions) >= 2:  # Cần ít nhất 2 tương tác để chia
                train_size = max(1, int(len(user_interactions) * 0.8))  # Ít nhất 1 tương tác trong train
                train_users.append(user_interactions.iloc[:train_size])
                test_users.append(user_interactions.iloc[train_size:])
        
        train_df = pd.concat(train_users)
        test_df = pd.concat(test_users)
        
        if progress_callback:
            progress_callback(25, f"Train set size: {len(train_df)}")
            progress_callback(25, f"Test set size: {len(test_df)}")
            progress_callback(25, f"Number of unique users in train: {train_df['user_id'].nunique()}")
            progress_callback(25, f"Number of unique items in train: {train_df['variant_id'].nunique()}")
        
        # Train model
        if progress_callback:
            progress_callback(30, "\nTraining model...")
        recommender = ALSRecommender(factors=10, regularization=0.01, iterations=30)
        recommender.fit(train_df)
        
        # Đánh giá model
        if progress_callback:
            progress_callback(70, "\nEvaluating model...")
        metrics = recommender.evaluate(test_df)
        if progress_callback:
            progress_callback(75, "\nModel Performance:")
            progress_callback(75, f"Precision@K: {metrics['precision@k']:.4f}")
            progress_callback(75, f"Recall@K: {metrics['recall@k']:.4f}")
            progress_callback(75, f"MAP@K: {metrics['map@k']:.4f}")
        
        # Lưu model
        if progress_callback:
            progress_callback(90, "\nSaving model...")
        model_path = recommender.save_model(test_df=test_df)
        if progress_callback:
            progress_callback(95, f"Model saved to: {model_path}")
        
        if progress_callback:
            progress_callback(100, "Training completed!")
            
        return recommender, metrics
        
    except Exception as e:
        if progress_callback:
            progress_callback(0, f"Error: {str(e)}")
        raise e

if __name__ == "__main__":
    train_and_evaluate() 