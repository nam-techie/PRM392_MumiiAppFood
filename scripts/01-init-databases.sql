-- Mumii Database Initialization Script
-- Tạo các databases cho microservices

-- Tạo databases
CREATE DATABASE IF NOT EXISTS mumii_auth;
CREATE DATABASE IF NOT EXISTS mumii_discovery;
CREATE DATABASE IF NOT EXISTS mumii_social;

-- Grant permissions
GRANT ALL PRIVILEGES ON mumii_auth.* TO 'root'@'%';
GRANT ALL PRIVILEGES ON mumii_discovery.* TO 'root'@'%';
GRANT ALL PRIVILEGES ON mumii_social.* TO 'root'@'%';
FLUSH PRIVILEGES;

-- Sử dụng database mumii_auth để tạo tables
USE mumii_auth;

-- Accounts table
CREATE TABLE IF NOT EXISTS accounts (
    id VARCHAR(36) PRIMARY KEY,
    email VARCHAR(255) UNIQUE NOT NULL,
    password_hash VARCHAR(255) NOT NULL,
    display_name VARCHAR(100) NOT NULL,
    avatar_url VARCHAR(500),
    role ENUM('User', 'Admin') DEFAULT 'User',
    is_active BOOLEAN DEFAULT true,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    INDEX idx_email (email),
    INDEX idx_is_active (is_active)
);

-- Sử dụng database mumii_discovery
USE mumii_discovery;

-- Restaurants table
CREATE TABLE IF NOT EXISTS restaurants (
    id VARCHAR(36) PRIMARY KEY,
    name VARCHAR(255) NOT NULL,
    address TEXT NOT NULL,
    latitude DECIMAL(10,8),
    longitude DECIMAL(11,8),
    region VARCHAR(100),
    avg_price DECIMAL(10,2),
    rating DECIMAL(2,1) DEFAULT 0,
    description TEXT,
    image_urls JSON,
    tags JSON,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    is_deleted BOOLEAN DEFAULT false,
    INDEX idx_location (latitude, longitude),
    INDEX idx_region (region),
    INDEX idx_rating (rating),
    INDEX idx_created_at (created_at)
);

-- Sử dụng database mumii_social
USE mumii_social;

-- Posts table
CREATE TABLE IF NOT EXISTS posts (
    id VARCHAR(36) PRIMARY KEY,
    account_id VARCHAR(36) NOT NULL,
    content TEXT NOT NULL,
    mood VARCHAR(50),
    image_urls JSON,
    restaurant_id VARCHAR(36),
    reaction_count INT DEFAULT 0,
    comment_count INT DEFAULT 0,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    is_deleted BOOLEAN DEFAULT false,
    INDEX idx_account_id (account_id),
    INDEX idx_restaurant_id (restaurant_id),
    INDEX idx_mood (mood),
    INDEX idx_created_at (created_at),
    INDEX idx_is_deleted (is_deleted)
);

-- Comments table
CREATE TABLE IF NOT EXISTS comments (
    id VARCHAR(36) PRIMARY KEY,
    post_id VARCHAR(36) NOT NULL,
    account_id VARCHAR(36) NOT NULL,
    content TEXT NOT NULL,
    parent_comment_id VARCHAR(36),
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    is_deleted BOOLEAN DEFAULT false,
    INDEX idx_post_id (post_id),
    INDEX idx_account_id (account_id),
    INDEX idx_parent_comment_id (parent_comment_id),
    INDEX idx_created_at (created_at),
    FOREIGN KEY (post_id) REFERENCES posts(id) ON DELETE CASCADE,
    FOREIGN KEY (parent_comment_id) REFERENCES comments(id) ON DELETE CASCADE
);

-- Reactions table
CREATE TABLE IF NOT EXISTS reactions (
    id VARCHAR(36) PRIMARY KEY,
    post_id VARCHAR(36) NOT NULL,
    account_id VARCHAR(36) NOT NULL,
    type ENUM('LIKE', 'LOVE', 'WOW') NOT NULL,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    UNIQUE KEY unique_reaction (post_id, account_id),
    INDEX idx_post_id (post_id),
    INDEX idx_account_id (account_id),
    INDEX idx_type (type),
    FOREIGN KEY (post_id) REFERENCES posts(id) ON DELETE CASCADE
);

-- Insert sample data

-- Sample restaurants
USE mumii_discovery;
INSERT IGNORE INTO restaurants (id, name, address, latitude, longitude, region, avg_price, rating, description, image_urls, tags) VALUES
(UUID(), 'Phở Hà Nội', '123 Phố Cổ, Hoàn Kiếm, Hà Nội', 21.0285, 105.8542, 'HaNoi', 50000, 4.5, 'Phở bò truyền thống Hà Nội', '["https://example.com/pho1.jpg"]', '["vietnamese", "pho", "beef"]'),
(UUID(), 'Bún Chả Obama', '1 Lê Văn Hưu, Hai Bà Trưng, Hà Nội', 21.0285, 105.8542, 'HaNoi', 80000, 4.8, 'Bún chả nổi tiếng từ chuyến thăm của Obama', '["https://example.com/buncha1.jpg"]', '["vietnamese", "buncha", "grilled"]'),
(UUID(), 'Cơm Tấm Sài Gòn', '123 Nguyễn Văn Cừ, Quận 1, TP.HCM', 10.7769, 106.7009, 'HoChiMinh', 45000, 4.3, 'Cơm tấm sườn nướng truyền thống', '["https://example.com/comtam1.jpg"]', '["vietnamese", "rice", "grilled"]'),
(UUID(), 'Bánh Mì Huynh Hoa', '26 Lê Thị Riêng, Quận 1, TP.HCM', 10.7769, 106.7009, 'HoChiMinh', 25000, 4.7, 'Bánh mì thập cẩm nổi tiếng', '["https://example.com/banhmi1.jpg"]', '["vietnamese", "sandwich", "street_food"]'),
(UUID(), 'Mì Quảng Bà Mua', '45 Trần Cao Vân, Đà Nẵng', 16.0471, 108.2068, 'DaNang', 35000, 4.4, 'Mì quảng đặc sản Đà Nẵng', '["https://example.com/miquang1.jpg"]', '["vietnamese", "noodles", "seafood"]');

-- Sample posts
USE mumii_social;
INSERT IGNORE INTO posts (id, account_id, content, mood, image_urls, restaurant_id, reaction_count, comment_count) VALUES
(UUID(), 'admin-id', 'Hôm nay ăn phở ngon quá! 🍜', 'SATISFIED', '["https://example.com/post1.jpg"]', NULL, 5, 2),
(UUID(), 'admin-id', 'Khám phá quán bún chả mới, recommended! 👍', 'EXCITED', '["https://example.com/post2.jpg"]', NULL, 8, 3),
(UUID(), 'admin-id', 'Đói quá, không biết ăn gì giờ... 🤔', 'HUNGRY', '[]', NULL, 3, 1);
