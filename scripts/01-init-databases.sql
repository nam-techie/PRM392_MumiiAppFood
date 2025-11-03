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

-- Users table (theo schema mới)
CREATE TABLE IF NOT EXISTS users (
    id INT PRIMARY KEY AUTO_INCREMENT,
    email VARCHAR(255) UNIQUE NOT NULL,
    password VARCHAR(255) NOT NULL,
    fullname VARCHAR(255) NOT NULL,
    role VARCHAR(50) DEFAULT 'User',
    is_active BOOLEAN DEFAULT true,
    login_method VARCHAR(50) DEFAULT 'Email',
    google_id VARCHAR(255),
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    INDEX idx_email (email),
    INDEX idx_is_active (is_active),
    INDEX idx_google_id (google_id)
);

-- Profiles table
CREATE TABLE IF NOT EXISTS profiles (
    id INT PRIMARY KEY AUTO_INCREMENT,
    user_id INT NOT NULL,
    gender VARCHAR(20),
    avatar VARCHAR(500),
    phone_number VARCHAR(20),
    address TEXT,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    FOREIGN KEY (user_id) REFERENCES users(id) ON DELETE CASCADE,
    INDEX idx_user_id (user_id)
);

-- Notifications table
CREATE TABLE IF NOT EXISTS notifications (
    id INT PRIMARY KEY AUTO_INCREMENT,
    user_id INT NOT NULL,
    title VARCHAR(255) NOT NULL,
    content TEXT NOT NULL,
    is_read BOOLEAN DEFAULT false,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (user_id) REFERENCES users(id) ON DELETE CASCADE,
    INDEX idx_user_id (user_id),
    INDEX idx_is_read (is_read),
    INDEX idx_created_at (created_at)
);

-- Sử dụng database mumii_discovery
USE mumii_discovery;

-- Restaurants table (theo schema mới)
CREATE TABLE IF NOT EXISTS restaurants (
    id INT PRIMARY KEY AUTO_INCREMENT,
    partner_id INT NOT NULL,
    name VARCHAR(255) NOT NULL,
    address TEXT NOT NULL,
    longitude DOUBLE,
    latitude DOUBLE,
    description TEXT,
    avg_price DOUBLE,
    rating FLOAT DEFAULT 0,
    status VARCHAR(50) DEFAULT 'Active',
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    INDEX idx_partner_id (partner_id),
    INDEX idx_location (latitude, longitude),
    INDEX idx_rating (rating),
    INDEX idx_status (status),
    INDEX idx_created_at (created_at)
);

-- Restaurant Images table
CREATE TABLE IF NOT EXISTS restaurant_images (
    id INT PRIMARY KEY AUTO_INCREMENT,
    restaurant_id INT NOT NULL,
    image_url VARCHAR(500) NOT NULL,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (restaurant_id) REFERENCES restaurants(id) ON DELETE CASCADE,
    INDEX idx_restaurant_id (restaurant_id)
);

-- Reviews table
CREATE TABLE IF NOT EXISTS reviews (
    id INT PRIMARY KEY AUTO_INCREMENT,
    user_id INT NOT NULL,
    restaurant_id INT NOT NULL,
    rating INT NOT NULL CHECK (rating >= 1 AND rating <= 5),
    comment TEXT NOT NULL,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (restaurant_id) REFERENCES restaurants(id) ON DELETE CASCADE,
    INDEX idx_user_id (user_id),
    INDEX idx_restaurant_id (restaurant_id),
    INDEX idx_rating (rating),
    INDEX idx_created_at (created_at)
);

-- Favorites table
CREATE TABLE IF NOT EXISTS favorites (
    id INT PRIMARY KEY AUTO_INCREMENT,
    user_id INT NOT NULL,
    restaurant_id INT NOT NULL,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (restaurant_id) REFERENCES restaurants(id) ON DELETE CASCADE,
    UNIQUE KEY unique_favorite (user_id, restaurant_id),
    INDEX idx_user_id (user_id),
    INDEX idx_restaurant_id (restaurant_id),
    INDEX idx_created_at (created_at)
);

-- Sử dụng database mumii_social
USE mumii_social;

-- Moods table
CREATE TABLE IF NOT EXISTS moods (
    id INT PRIMARY KEY AUTO_INCREMENT,
    name VARCHAR(50) NOT NULL,
    description TEXT,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    INDEX idx_name (name)
);

-- Posts table (theo schema mới)
CREATE TABLE IF NOT EXISTS posts (
    id INT PRIMARY KEY AUTO_INCREMENT,
    partner_id INT NOT NULL,
    restaurant_id INT,
    title VARCHAR(255) NOT NULL,
    content TEXT NOT NULL,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    INDEX idx_partner_id (partner_id),
    INDEX idx_restaurant_id (restaurant_id),
    INDEX idx_created_at (created_at)
);

-- Post_Mood table (many-to-many relationship)
CREATE TABLE IF NOT EXISTS post_moods (
    post_id INT NOT NULL,
    mood_id INT NOT NULL,
    PRIMARY KEY (post_id, mood_id),
    FOREIGN KEY (post_id) REFERENCES posts(id) ON DELETE CASCADE,
    FOREIGN KEY (mood_id) REFERENCES moods(id) ON DELETE CASCADE,
    INDEX idx_post_id (post_id),
    INDEX idx_mood_id (mood_id)
);

-- Insert sample data

-- Sample users
USE mumii_auth;
INSERT IGNORE INTO users (id, email, password, fullname, role, login_method) VALUES
(1, 'admin@mumii.com', '$2b$10$example_hashed_password', 'Administrator', 'Admin', 'Email'),
(2, 'partner@mumii.com', '$2b$10$example_hashed_password', 'Restaurant Partner', 'Partner', 'Email'),
(3, 'user@mumii.com', '$2b$10$example_hashed_password', 'Regular User', 'User', 'Email');

-- Sample profiles
INSERT IGNORE INTO profiles (user_id, gender, avatar, phone_number, address) VALUES
(1, 'Male', 'https://example.com/admin_avatar.jpg', '0901234567', 'Hà Nội, Việt Nam'),
(2, 'Female', 'https://example.com/partner_avatar.jpg', '0987654321', 'TP.HCM, Việt Nam'),
(3, 'Male', 'https://example.com/user_avatar.jpg', '0912345678', 'Đà Nẵng, Việt Nam');

-- Sample restaurants
USE mumii_discovery;
INSERT IGNORE INTO restaurants (partner_id, name, address, latitude, longitude, avg_price, rating, description, status) VALUES
(2, 'Phở Hà Nội', '123 Phố Cổ, Hoàn Kiếm, Hà Nội', 21.0285, 105.8542, 50000, 4.5, 'Phở bò truyền thống Hà Nội', 'Active'),
(2, 'Bún Chả Obama', '1 Lê Văn Hưu, Hai Bà Trưng, Hà Nội', 21.0285, 105.8542, 80000, 4.8, 'Bún chả nổi tiếng từ chuyến thăm của Obama', 'Active'),
(2, 'Cơm Tấm Sài Gòn', '123 Nguyễn Văn Cừ, Quận 1, TP.HCM', 10.7769, 106.7009, 45000, 4.3, 'Cơm tấm sườn nướng truyền thống', 'Active');

-- Sample restaurant images
INSERT IGNORE INTO restaurant_images (restaurant_id, image_url) VALUES
(1, 'https://example.com/pho1.jpg'),
(1, 'https://example.com/pho2.jpg'),
(2, 'https://example.com/buncha1.jpg'),
(3, 'https://example.com/comtam1.jpg');

-- Sample moods
USE mumii_social;
INSERT IGNORE INTO moods (name, description) VALUES
('Happy', 'Cảm thấy vui vẻ, hạnh phúc'),
('Satisfied', 'Cảm thấy hài lòng với món ăn'),
('Excited', 'Hào hứng, phấn khích'),
('Hungry', 'Cảm thấy đói bụng'),
('Disappointed', 'Cảm thấy thất vọng');

-- Sample posts
INSERT IGNORE INTO posts (partner_id, restaurant_id, title, content) VALUES
(2, 1, 'Phở Hà Nội Truyền Thống', 'Hôm nay giới thiệu món phở bò truyền thống với nước dùng được ninh từ xương trong 12 tiếng! 🍜'),
(2, 2, 'Bún Chả Đặc Biệt', 'Khám phá hương vị bún chả đặc biệt tại quán chúng tôi! 👍'),
(2, 3, 'Cơm Tấm Ngon Miệng', 'Cơm tấm sườn nướng với nước mắm pha đặc biệt của gia đình 🍽️');

-- Sample post moods
INSERT IGNORE INTO post_moods (post_id, mood_id) VALUES
(1, 2), -- Post 1 có mood Satisfied
(2, 3), -- Post 2 có mood Excited
(3, 1); -- Post 3 có mood Happy
