-- Product Category Service -------------------------------------------------------
CREATE TABLE [category] (
    [category_id] INT PRIMARY KEY IDENTITY(1,1),
    [cat_pid] INT,
    [name] NVARCHAR(255) NOT NULL,
    [description] NVARCHAR(512),
    [createddate] DATETIME NOT NULL DEFAULT GETDATE(),
    [isactived] INT NOT NULL DEFAULT 0
);

CREATE TABLE [product] (
    [product_id] INT PRIMARY KEY IDENTITY(1,1),
    [name] NVARCHAR(255) NOT NULL,
    [description] NVARCHAR(4000),
    [createddate] DATETIME NOT NULL,
    [category_id] INT,
    [isactived] INT NOT NULL DEFAULT 0,
);

CREATE TABLE [product_variant] (
    [variant_id] INT PRIMARY KEY IDENTITY(1,1),
    [product_id] INT NOT NULL,
    [price] DECIMAL(18, 2) NOT NULL,
    [stock] INT NOT NULL DEFAULT 0,
    [reserved_stock] INT NOT NULL DEFAULT 0,
    [sold_quantity] INT NOT NULL DEFAULT 0,
    [isactived] INT NOT NULL DEFAULT 1,
);

CREATE TABLE [product_detail] (
    [detail_id] INT PRIMARY KEY IDENTITY(1,1),
    [product_id] INT NOT NULL,
    [key] NVARCHAR(255) NOT NULL,
    [value] NVARCHAR(1000) NOT NULL
);

CREATE TABLE [product_image] (
    [product_image_id] INT PRIMARY KEY IDENTITY(1,1),
	[title] nvarchar(128),
	[position] int not null,
    [image_url] VARCHAR(512) NULL,
    [product_id] INT NULL,
    [variant_id] INT NULL
);

CREATE TABLE [attribute] (
    [attribute_id] INT PRIMARY KEY IDENTITY(1,1),
    [name] NVARCHAR(64) NOT NULL
);

CREATE TABLE [attribute_value] (
    [av_id] INT PRIMARY KEY IDENTITY(1,1),
    [attribute_id] INT NOT NULL,
    [value] NVARCHAR(64) NOT NULL
);

CREATE TABLE [product_attribute] (
    [variant_id] INT NOT NULL,
    [av_id] INT NOT NULL,
    PRIMARY KEY (variant_id, av_id)
);

CREATE TABLE [promotion] (
    [promotion_id] INT PRIMARY KEY IDENTITY(1,1),
    [name] NVARCHAR(128) NOT NULL,
    [description] NVARCHAR(1000),
    [discount_percentage] INT DEFAULT 0,
    [discount_amount] INT DEFAULT 0,
    [start_date] DATETIME NOT NULL,
    [end_date] DATETIME NOT NULL,
    [is_active] INT NOT NULL DEFAULT 1,
    [is_flashsale] BIT NOT NULL DEFAULT 0
);

CREATE TABLE [product_promotion] (
    [pm_id] INT PRIMARY KEY IDENTITY(1,1),
    [product_id] INT NULL,
    [variant_id] INT NULL,
    [promotion_id] INT NOT NULL
);

CREATE TABLE [product_view] (
    [pv_id] INT PRIMARY KEY IDENTITY(1,1),
    [variant_id] INT NOT NULL,
    [product_id] INT NOT NULL,
    [category_id] INT NOT NULL,
    [user_id] INT NOT NULL,
    [view] INT NOT NULL DEFAULT 0,
    [created_at] DATETIME NOT NULL,
    [updated_at] DATETIME NULL,
)

-- Order Service -------------------------------------------------------
CREATE TABLE [order] (
    [order_id] INT PRIMARY KEY IDENTITY(1,1),
    [order_code] VARCHAR(32) NOT NULL,
    [total_amount] DECIMAL(18, 2) NOT NULL,
    [ship_fee] DECIMAL(18, 2) NOT NULL,
    [order_status] INT NOT NULL DEFAULT 0,
    [payment_method] NVARCHAR(64) NOT NULL,
    [created_date] DATETIME NOT NULL DEFAULT GETDATE(),
    [updated_date] DATETIME NULL,
    [coupon_id] INT NULL,
    [ship_coupon_id] INT NULL,
    [user_id] INT NOT NULL,
    [useraddress_id] INT NOT NULL,
);

CREATE TABLE [order_item] (
    [oi_id] INT PRIMARY KEY IDENTITY(1,1),
	[order_id] INT NOT NULL,
    [variant_id] INT NOT NULL,
    [quantity] INT NOT NULL,
    [original_price] DECIMAL(18,2) NOT NULL, -- Giá tại thời điểm được mua
    [final_price] DECIMAL(18,2) NULL, -- Giá sau khi áp Voucher (nếu có)
    [total_price] DECIMAL(18, 2) NOT NULL,
    [item_status] INT NOT NULL DEFAULT 0,
    [is_review] BIT NOT NULL DEFAULT 0,
);

CREATE TABLE [payment] (
    [payment_id] INT PRIMARY KEY IDENTITY(1,1),
    [order_id] INT NOT NULL,
    [payment_method] INT NOT NULL DEFAULT 0,  -- momo, vnpay, cod...
    [amount] DECIMAL(18, 2) NOT NULL,
    [status] INT NOT NULL DEFAULT 0,         -- 0: pending, 1: success, 2: failed
    [transaction_code] VARCHAR(100) NULL,    -- Mã giao dịch từ cổng thanh toán (momo, vnpay...)
    [created_at] DATETIME NOT NULL DEFAULT GETDATE(),
    [updated_at] DATETIME NULL
);

CREATE TABLE [transaction] (
    [transaction_id] INT PRIMARY KEY IDENTITY(1,1),
    [order_id] INT NOT NULL,
    [payment_gateway] NVARCHAR(50) NOT NULL, -- Ví dụ: VNPay, Momo, COD...
    [transaction_code] VARCHAR(100) NULL, -- Mã giao dịch từ cổng thanh toán
    [amount] DECIMAL(18, 2) NOT NULL,
    [status] INT NOT NULL DEFAULT 0, -- Pending, Success, Failed
    [created_at] DATETIME NOT NULL DEFAULT GETDATE(),
    [updated_at] DATETIME NULL
);

CREATE TABLE [return] (
    [return_id] INT PRIMARY KEY IDENTITY(1,1),
    [oi_id] INT NOT NULL,
    [user_id] INT NOT NULL,
    [reason] NVARCHAR(255) NOT NULL,
    [quantity] INT NOT NULL,
    [refund_amount] DECIMAL(18, 2) NOT NULL,
    [status] INT NOT NULL DEFAULT 0, -- Pending, Approved, Rejected, Refunded
    [created_at] DATETIME NOT NULL DEFAULT GETDATE(),
    [processed_at] DATETIME NULL,
    [account_name] NVARCHAR(100) NULL,
    [account_number] VARCHAR(50) NULL,
    [bank_name] NVARCHAR(100) NULL,
    [phone_number] NVARCHAR(20) NULL,
    [return_type] NVARCHAR(20) NULL,
    [url] VARCHAR(128) NULL,
);

-- Cart Service -------------------------------------------------------
CREATE TABLE [cart] (
    [cart_id] INT PRIMARY KEY IDENTITY(1,1),
    [user_id] INT NOT NULL,
    [created_date] DATETIME NOT NULL DEFAULT GETDATE(),
    [updated_date] DATETIME NULL,
);

CREATE TABLE [cart_item] (
    [ci_id] INT PRIMARY KEY IDENTITY(1,1),
    [cart_id] INT NOT NULL,
    [variant_id] INT NOT NULL,
    [quantity] INT NOT NULL,
    [unit_price] DECIMAL(18, 2) NOT NULL,
    [total_price] DECIMAL(18, 2) NOT NULL,
);

CREATE TABLE [wish_list] (
    [wish_id] INT PRIMARY KEY IDENTITY(1,1),
    [user_id] INT NOT NULL,  -- liên kết đến người dùng
    [variant_id] INT NOT NULL,  -- sản phẩm yêu thích
    [added_date] DATETIME NOT NULL,  -- ngày thêm vào danh sách yêu thích
    [is_actived] BIT NOT NULL DEFAULT 1,  -- trạng thái kích hoạt (có thể xóa sản phẩm ra khỏi danh sách yêu thích)
);

CREATE TABLE [coupon] (
    [coupon_id] INT PRIMARY KEY IDENTITY(1,1),
    [code] VARCHAR(10) NOT NULL UNIQUE,
    [description] NVARCHAR(512),
    [discount_percentage] INT NOT NULL DEFAULT 0,  -- Giảm theo phần trăm
    [discount_amount] DECIMAL(18, 2) NOT NULL DEFAULT 0,  -- Giảm cố định
    [max_discount_amount] DECIMAL(18, 2) NULL,  -- Giảm tối đa nếu giảm theo phần trăm
    [min_order_value] DECIMAL(18, 2) NOT NULL DEFAULT 0,  -- Giá trị đơn tối thiểu để áp dụng
    [times_used] INT NOT NULL DEFAULT 0,  -- Tổng số lần voucher đã được sử dụng
    [max_usage] INT NULL,  -- Tổng số lần voucher có thể sử dụng (null = không giới hạn)
    [max_usage_per_user] INT NULL,  -- Số lần 1 user có thể dùng (null = không giới hạn)
    [is_vip] BIT NOT NULL DEFAULT 0,  -- 1: Chỉ dành cho VIP, 0: Ai cũng có thể nhận
    [user_specific] BIT NOT NULL DEFAULT 1,  -- Tất cả voucher phải được nhận trước
    [ct_id] INT NOT NULL,
    [start_date] DATETIME NOT NULL,
    [end_date] DATETIME NOT NULL,
    [created_date] DATETIME NOT NULL DEFAULT GETDATE(),
    [terms_conditions] NVARCHAR(MAX) NULL,  -- Điều khoản sử dụng voucher
    [is_actived] BIT NOT NULL DEFAULT 1
);

CREATE TABLE [coupon_type] (
    [ct_id] INT PRIMARY KEY IDENTITY(1,1),
    [type_name] INT NOT NULL,  -- Tên loại voucher (VD: Giảm giá sản phẩm, Miễn phí vận chuyển)
    [description] NVARCHAR(255) NULL  -- Mô tả loại voucher
);

CREATE TABLE [user_coupon] (
    [uc_id] INT PRIMARY KEY IDENTITY(1,1),
    [user_id] INT NOT NULL,
    [coupon_id] INT NOT NULL,
    [is_used] BIT NOT NULL DEFAULT 0,  -- Đánh dấu voucher đã sử dụng hay chưa
    [times_used] INT NOT NULL DEFAULT 0,  -- Số lần user đã dùng voucher này
    [claimed_at] DATETIME NOT NULL DEFAULT GETDATE()
);

-- User Service -------------------------------------------------------
CREATE TABLE [user] (
    [user_id] INT PRIMARY KEY IDENTITY(1,1),
    [username] NVARCHAR(255) NOT NULL,
    [email] VARCHAR(255) NOT NULL,
    [password] VARCHAR(255) NULL,
    [gender] NVARCHAR(32) NULL,
    [date_of_birth] DATETIME NULL,
    [image_url] varchar(255),
    [role] INT NOT NULL DEFAULT 0,
    [otp] VARCHAR(6) NULL,
    [otp_attempt] INT DEFAULT 0,
    [otp_expiration] DATETIME NULL,
    [createddate] DATETIME NOT NULL DEFAULT GETDATE(),
    [last_login] DATETIME,
    [isactived] INT NOT NULL DEFAULT 0
);

CREATE TABLE [useraddress] (
    [useraddress_id] INT PRIMARY KEY IDENTITY(1,1),
    [firstname] NVARCHAR(255) NOT NULL,
    [lastname] NVARCHAR(255) NOT NULL,
    [addressline] NVARCHAR(255) NOT NULL,
    [phonenumber] VARCHAR(50),
    [province] NVARCHAR(100) NOT NULL,
    [district] NVARCHAR(100) NOT NULL,
    [ward] NVARCHAR(100) NOT NULL,
    [user_id] INT NOT NULL,
    [default] BIT NOT NULL DEFAULT 0,
);

-- Authentication Service -------------------------------------------------------
CREATE TABLE [usertoken] (
    [usertoken_id] INT PRIMARY KEY IDENTITY(1,1),
    [user_id] INT NOT NULL,
    [refreshtoken] VARCHAR(1000) NULL,
    [issued_at] DATETIME NOT NULL DEFAULT GETDATE(),
    [expiration] DATETIME NOT NULL,
    [revoked_at] DATETIME,
    [isactived] INT NOT NULL DEFAULT 0
);

-- Review Service -------------------------------------------------------
CREATE TABLE [review] (
    [review_id] INT PRIMARY KEY IDENTITY(1,1),
    [user_id] INT NOT NULL, -- Người đánh giá
    [variant_id] INT NOT NULL, -- Sản phẩm được đánh giá
    [oi_id] INT NOT NULL,
    [rating] INT NOT NULL, -- Số sao (1-5)
    [comment] NVARCHAR(500) NULL, -- Nội dung bình luận
    [created_at] DATETIME NOT NULL DEFAULT GETDATE(),
    [is_flagged] BIT NOT NULL DEFAULT 0,
    [is_deleted] BIT NOT NULL DEFAULT 0
);

CREATE TABLE [review_media] (
    [media_id] INT PRIMARY KEY IDENTITY(1,1),
    [review_id] INT NOT NULL,
    [media_url] VARCHAR(128) NOT NULL
);

CREATE TABLE [review_reply] (
    [reply_id] INT PRIMARY KEY IDENTITY(1,1),
    [review_id] INT NOT NULL, -- Đánh giá cần phản hồi
    [user_id] INT NOT NULL, -- Quản trị viên phản hồi
    [reply_text] NVARCHAR(500) NOT NULL, -- Nội dung phản hồi
    [created_at] DATETIME NOT NULL DEFAULT GETDATE()
);

CREATE TABLE [review_report] (
    [report_id] INT PRIMARY KEY IDENTITY(1,1),
    [review_id] INT NOT NULL UNIQUE,  -- Mỗi review chỉ có 1 báo cáo
    [status] INT NOT NULL DEFAULT 0,  -- 0: Chờ xử lý, 1: Đã xử lý, 2: Từ chối
    [total_reports] INT NOT NULL DEFAULT 1, -- Số lượng người báo cáo
    [created_at] DATETIME NOT NULL DEFAULT GETDATE()
);

CREATE TABLE [review_report_users] (
    [rru_id] INT PRIMARY KEY IDENTITY(1,1),
    [report_id] INT NOT NULL,
    [user_id] INT NOT NULL,
    [reason] NVARCHAR(300) NOT NULL,  -- Lý do báo cáo
    [created_at] DATETIME NOT NULL DEFAULT GETDATE()
);

CREATE TABLE [banner] (
    [banner_id] INT PRIMARY KEY IDENTITY(1,1), -- Mã định danh banner
    [title] NVARCHAR(255) NOT NULL, -- Tiêu đề banner
    [description] NVARCHAR(1000) NULL, -- Mô tả banner (tuỳ chọn)
    [image_url] VARCHAR(128) NULL, -- Đường dẫn hình ảnh
    [link] NVARCHAR(500) NULL, -- Liên kết khi người dùng nhấp vào banner
    [start_date] DATETIME NOT NULL, -- Ngày bắt đầu hiển thị
    [end_date] DATETIME NOT NULL, -- Ngày kết thúc hiển thị
    [status] INT NOT NULL DEFAULT 1, -- Trạng thái (1: Hiển thị, 0: Ẩn)
    [position] INT NOT NULL DEFAULT 0 -- Vị trí banner (tuỳ chọn)
);

CREATE TABLE [ai_prompt] (
    [prompt_id] INT PRIMARY KEY IDENTITY(1,1),
    [name] NVARCHAR(128) NULL,
    [content] NVARCHAR(1000) NOT NULL,
    [created_at] DATETIME NOT NULL
)

CREATE TABLE [user_interaction] (
    [interaction_id] INT PRIMARY KEY IDENTITY(1,1),
    [user_id] INT NOT NULL,
    [variant_id] INT NOT NULL,
    [interaction_type] VARCHAR(20) NOT NULL, -- 'click', 'add_to_cart', 'purchase'
    [interaction_value] FLOAT NOT NULL, -- Giá trị tương tác 
    [created_at] DATETIME NOT NULL
)

CREATE TABLE [spin_history] (
    [spin_id] INT PRIMARY KEY IDENTITY(1,1),
    [user_id] INT NULL,
    [coupon_id] INT NULL,
    [spin_date] DATE NOT NULL,
    [created_at] DATETIME NOT NULL,
);

CREATE TABLE [conversations] (
    [conversation_id] INT PRIMARY KEY IDENTITY(1,1),
    [user_id] INT NULL, -- Người dùng trò chuyện
    [status] VARCHAR(64) NOT NULL, -- pending, active, resolved
    [is_bot_handled] BIT NOT NULL DEFAULT 0, -- Đã xử lý xong bằng bot
    [created_at] DATETIME NOT NULL
)

CREATE TABLE [chat_messages] (
    [chat_id] INT PRIMARY KEY IDENTITY(1,1),
    [conversation_id] INT NOT NULL,
    [sender_type] VARCHAR(32) NOT NULL, -- user, admin, bot
    [sender_id] INT NULL, -- Ai gửi (nếu cần)
    [message] NVARCHAR(2000) NULL, -- Nội dung
    [time_stamp] DATETIME NOT NULL, 
    [is_from_bot] BIT NULL, -- Phân biệt bot/người (tuỳ chọn)
)

--Index
CREATE INDEX IX_category_cat_pid ON category(cat_pid);
CREATE INDEX IX_category_isactived ON category(isactived);
CREATE INDEX IX_product_category_id ON product(category_id);
CREATE INDEX IX_product_isactived ON product(isactived);
CREATE INDEX IX_product_createddate ON product(createddate);
CREATE INDEX IX_variant_product_id ON product_variant(product_id);
CREATE INDEX IX_variant_isactived ON product_variant(isactived);
CREATE INDEX IX_detail_product_id ON product_detail(product_id);
CREATE INDEX IX_detail_key ON product_detail([key]);
CREATE INDEX IX_image_product_id ON product_image(product_id);
CREATE INDEX IX_image_variant_id ON product_image(variant_id);
CREATE INDEX IX_av_attribute_id ON attribute_value(attribute_id);
CREATE INDEX IX_pa_variant_id ON product_attribute(variant_id);
CREATE INDEX IX_pa_av_id ON product_attribute(av_id);
CREATE INDEX IX_pp_promotion_id ON product_promotion(promotion_id);
CREATE INDEX IX_pp_product_id ON product_promotion(product_id);
CREATE INDEX IX_pp_variant_id ON product_promotion(variant_id);
CREATE INDEX IX_order_user_id ON [order]([user_id]);
CREATE INDEX IX_order_created_date ON [order](created_date);
CREATE INDEX IX_order_coupon_id ON [order](coupon_id);
CREATE INDEX IX_order_status ON [order](order_status);
CREATE INDEX IX_oi_order_id ON order_item(order_id);
CREATE INDEX IX_oi_variant_id ON order_item(variant_id);
CREATE INDEX IX_payment_order_id ON payment(order_id);
CREATE INDEX IX_transaction_order_id ON [transaction](order_id);
CREATE INDEX IX_return_oi_id ON [return](oi_id);
CREATE INDEX IX_return_user_id ON [return]([user_id]);
CREATE INDEX IX_cart_user_id ON cart([user_id]);
CREATE INDEX IX_ci_cart_id ON cart_item(cart_id);
CREATE INDEX IX_ci_variant_id ON cart_item(variant_id);
CREATE INDEX IX_wish_user_id ON wish_list([user_id]);
CREATE INDEX IX_wish_variant_id ON wish_list(variant_id);
CREATE INDEX IX_uc_user_id ON user_coupon([user_id]);
CREATE INDEX IX_uc_coupon_id ON user_coupon(coupon_id);
CREATE INDEX IX_useraddress_user_id ON useraddress([user_id]);
CREATE INDEX IX_token_user_id ON usertoken([user_id]);
CREATE INDEX IX_review_user_id ON review([user_id]);
CREATE INDEX IX_review_variant_id ON review(variant_id);
CREATE INDEX IX_media_review_id ON review_media(review_id);
CREATE INDEX IX_reply_review_id ON review_reply(review_id);
CREATE INDEX IX_reply_user_id ON review_reply([user_id]);
CREATE INDEX IX_rru_report_id ON review_report_users(report_id);
CREATE INDEX IX_rru_user_id ON review_report_users([user_id]);

--Proceduce, Trigger, View-------------------------------------------------
CREATE PROCEDURE sp_UpdateOrderStatus
    @OrderId INT
AS
BEGIN
    SET NOCOUNT ON;

    WITH OrderItemStates AS (
        SELECT 
            oi.order_id,
            COUNT(DISTINCT oi.item_status) AS DistinctStatusCount,
            MIN(oi.item_status) AS MinStatus,
            MAX(oi.item_status) AS MaxStatus
        FROM order_item oi
        WHERE oi.order_id = @OrderId
        GROUP BY oi.order_id
    )
    UPDATE o
    SET order_status =
        CASE 
            WHEN s.DistinctStatusCount > 1 THEN 5
            WHEN s.MinStatus = 0 THEN 0
            WHEN s.MinStatus = 1 THEN 1
            WHEN s.MinStatus = 2 THEN 3
            WHEN s.MinStatus = 3 THEN 4
            WHEN s.MinStatus = 4 THEN 6
            WHEN s.MinStatus = 5 THEN 5
            WHEN s.MinStatus = 6 THEN 7
            WHEN s.MinStatus = 7 THEN 5
            ELSE 0
        END
    FROM [order] o
    INNER JOIN OrderItemStates s ON o.order_id = s.order_id;
END
