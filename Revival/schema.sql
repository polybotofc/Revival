-- ============================================
-- Roblox Revival - MySQL Database Schema
-- Target Roblox Client: Version 0.338.0.202976 (May 2018)
-- Generated for Entity Framework Core 8 with Pomelo MySQL Provider
-- ============================================

-- Create database
CREATE DATABASE IF NOT EXISTS Revival 
    CHARACTER SET utf8mb4 
    COLLATE utf8mb4_unicode_ci;

USE Revival;

-- ============================================
-- USERS TABLE
-- ============================================
CREATE TABLE IF NOT EXISTS Users (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    Username VARCHAR(50) NOT NULL,
    Email VARCHAR(255) NOT NULL,
    PasswordHash VARCHAR(255) NOT NULL,
    DisplayName VARCHAR(100) NULL,
    Description VARCHAR(1000) NULL,
    Location VARCHAR(100) NULL,
    Website VARCHAR(100) NULL,
    CreatedAt DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    UpdatedAt DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6) ON UPDATE CURRENT_TIMESTAMP(6),
    LastLoginAt DATETIME(6) NULL,
    Birthdate DATETIME NULL,
    IsBanned TINYINT(1) NOT NULL DEFAULT 0,
    BanReason VARCHAR(500) NULL,
    AvatarAssetId INT NULL,
    LoginCount INT NOT NULL DEFAULT 0,
    Reputation INT NOT NULL DEFAULT 0,
    TotalFriends INT NOT NULL DEFAULT 0,
    FollowerCount INT NOT NULL DEFAULT 0,
    FollowingCount INT NOT NULL DEFAULT 0,
    PostCount INT NOT NULL DEFAULT 0,
    EmailVerified TINYINT(1) NOT NULL DEFAULT 0,
    EmailVerificationToken VARCHAR(255) NULL,
    PasswordResetToken VARCHAR(255) NULL,
    PasswordResetExpiresAt DATETIME NULL,
    FailedLoginAttempts INT NOT NULL DEFAULT 0,
    LockoutEnd DATETIME NULL,
    
    UNIQUE INDEX IX_Users_Username (Username),
    UNIQUE INDEX IX_Users_Email (Email),
    UNIQUE INDEX IX_Users_EmailVerificationToken (EmailVerificationToken),
    UNIQUE INDEX IX_Users_PasswordResetToken (PasswordResetToken)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- ============================================
-- SESSIONS TABLE
-- ============================================
CREATE TABLE IF NOT EXISTS Sessions (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    SessionToken VARCHAR(128) NOT NULL,
    UserId INT NOT NULL,
    IpAddress VARCHAR(45) NULL,
    UserAgent VARCHAR(512) NULL,
    DeviceType VARCHAR(100) NULL,
    CreatedAt DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    ExpiresAt DATETIME(6) NOT NULL,
    LastAccessedAt DATETIME(6) NULL,
    IsActive TINYINT(1) NOT NULL DEFAULT 1,
    IsPersistent TINYINT(1) NOT NULL DEFAULT 0,
    Status VARCHAR(50) NULL DEFAULT 'Active',
    
    UNIQUE INDEX IX_Sessions_SessionToken (SessionToken),
    INDEX IX_Sessions_UserId (UserId),
    INDEX IX_Sessions_ExpiresAt (ExpiresAt),
    INDEX IX_Sessions_IsActive_ExpiresAt (IsActive, ExpiresAt),
    
    CONSTRAINT FK_Sessions_Users_UserId 
        FOREIGN KEY (UserId) 
        REFERENCES Users(Id) 
        ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- ============================================
-- GAMES TABLE
-- ============================================
CREATE TABLE IF NOT EXISTS Games (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    Name VARCHAR(100) NOT NULL,
    Description VARCHAR(2000) NULL,
    CreatorId INT NOT NULL,
    ThumbnailUrl VARCHAR(255) NULL,
    IconUrl VARCHAR(255) NULL,
    TotalPlays BIGINT NOT NULL DEFAULT 0,
    MonthlyPlays BIGINT NOT NULL DEFAULT 0,
    WeeklyPlays BIGINT NOT NULL DEFAULT 0,
    ActivePlayers INT NOT NULL DEFAULT 0,
    MaxPlayers INT NOT NULL DEFAULT 50,
    IsPublic TINYINT(1) NOT NULL DEFAULT 1,
    IsActive TINYINT(1) NOT NULL DEFAULT 1,
    IsFeatured TINYINT(1) NOT NULL DEFAULT 0,
    Votes INT NOT NULL DEFAULT 0,
    Likes INT NOT NULL DEFAULT 0,
    Dislikes INT NOT NULL DEFAULT 0,
    Rating DECIMAL(3,2) NOT NULL DEFAULT 0.00,
    Genre VARCHAR(50) NULL,
    PlayingMode VARCHAR(50) NULL,
    MinPlayers INT NOT NULL DEFAULT 1,
    CreatedAt DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    UpdatedAt DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6) ON UPDATE CURRENT_TIMESTAMP(6),
    PublishedAt DATETIME(6) NULL,
    Version INT NOT NULL DEFAULT 1,
    VisitCount BIGINT NOT NULL DEFAULT 0,
    
    INDEX IX_Games_Name (Name),
    INDEX IX_Games_CreatorId (CreatorId),
    INDEX IX_Games_IsPublic (IsPublic),
    INDEX IX_Games_IsFeatured (IsFeatured),
    INDEX IX_Games_CreatedAt (CreatedAt),
    INDEX IX_Games_TotalPlays (TotalPlays),
    INDEX IX_Games_IsPublic_IsActive (IsPublic, IsActive),
    
    CONSTRAINT FK_Games_Users_CreatorId 
        FOREIGN KEY (CreatorId) 
        REFERENCES Users(Id) 
        ON DELETE RESTRICT
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- ============================================
-- PLACES TABLE
-- ============================================
CREATE TABLE IF NOT EXISTS Places (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    Name VARCHAR(100) NOT NULL,
    GameId INT NOT NULL,
    Description VARCHAR(2000) NULL,
    FilePath VARCHAR(500) NOT NULL,
    MaxPlayers INT NOT NULL DEFAULT 50,
    MinPlayers INT NOT NULL DEFAULT 1,
    CurrentPlayers INT NOT NULL DEFAULT 0,
    IsActive TINYINT(1) NOT NULL DEFAULT 1,
    IsPrimary TINYINT(1) NOT NULL DEFAULT 0,
    IsCopyable TINYINT(1) NOT NULL DEFAULT 1,
    Version INT NOT NULL DEFAULT 1,
    TotalPlays BIGINT NOT NULL DEFAULT 0,
    PlayingMode VARCHAR(50) NULL,
    Score INT NOT NULL DEFAULT 0,
    CreatedAt DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    UpdatedAt DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6) ON UPDATE CURRENT_TIMESTAMP(6),
    
    INDEX IX_Places_GameId (GameId),
    INDEX IX_Places_IsActive (IsActive),
    
    CONSTRAINT FK_Places_Games_GameId 
        FOREIGN KEY (GameId) 
        REFERENCES Games(Id) 
        ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- ============================================
-- ASSETS TABLE
-- ============================================
CREATE TABLE IF NOT EXISTS Assets (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    Name VARCHAR(100) NOT NULL,
    Type INT NOT NULL DEFAULT 99,
    FilePath VARCHAR(500) NOT NULL,
    Description VARCHAR(2000) NULL,
    FileSize BIGINT NOT NULL DEFAULT 0,
    ContentType VARCHAR(100) NULL,
    Hash VARCHAR(64) NULL,
    CreatorId INT NULL,
    IsPublic TINYINT(1) NOT NULL DEFAULT 1,
    IsApproved TINYINT(1) NOT NULL DEFAULT 1,
    IsForSale TINYINT(1) NOT NULL DEFAULT 0,
    Price DECIMAL(10,2) NOT NULL DEFAULT 0.00,
    SalesCount INT NOT NULL DEFAULT 0,
    Version INT NOT NULL DEFAULT 1,
    DownloadCount INT NOT NULL DEFAULT 0,
    Category VARCHAR(50) NULL,
    Tags VARCHAR(100) NULL,
    CreatedAt DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    UpdatedAt DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6) ON UPDATE CURRENT_TIMESTAMP(6),
    
    INDEX IX_Assets_Name (Name),
    INDEX IX_Assets_Type (Type),
    INDEX IX_Assets_CreatorId (CreatorId),
    INDEX IX_Assets_IsPublic (IsPublic),
    INDEX IX_Assets_Hash (Hash),
    
    CONSTRAINT FK_Assets_Users_CreatorId 
        FOREIGN KEY (CreatorId) 
        REFERENCES Users(Id) 
        ON DELETE SET NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- ============================================
-- GAME SERVERS TABLE
-- ============================================
CREATE TABLE IF NOT EXISTS GameServers (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    ServerId VARCHAR(36) NOT NULL,
    PlaceId INT NOT NULL,
    GameId INT NOT NULL,
    CreatorId INT NOT NULL,
    Host VARCHAR(45) NOT NULL,
    Port INT NOT NULL DEFAULT 53640,
    TicketToken VARCHAR(64) NULL,
    CurrentPlayers INT NOT NULL DEFAULT 0,
    MaxPlayers INT NOT NULL DEFAULT 50,
    Status VARCHAR(20) NOT NULL DEFAULT 'Starting',
    RCCJobId VARCHAR(100) NULL,
    RCCServerUrl VARCHAR(255) NULL,
    CPUUsage INT NOT NULL DEFAULT 0,
    MemoryUsage BIGINT NOT NULL DEFAULT 0,
    Region VARCHAR(50) NULL,
    MachineId VARCHAR(50) NULL,
    IsReserved TINYINT(1) NOT NULL DEFAULT 0,
    ReservedPlayerCount INT NOT NULL DEFAULT 0,
    CreatedAt DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    UpdatedAt DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6) ON UPDATE CURRENT_TIMESTAMP(6),
    StartedAt DATETIME(6) NULL,
    ExpiresAt DATETIME(6) NULL,
    Ping INT NOT NULL DEFAULT 0,
    
    UNIQUE INDEX IX_GameServers_ServerId (ServerId),
    INDEX IX_GameServers_PlaceId (PlaceId),
    INDEX IX_GameServers_GameId (GameId),
    INDEX IX_GameServers_RCCJobId (RCCJobId),
    INDEX IX_GameServers_Status (Status),
    INDEX IX_GameServers_Status_CurrentPlayers_MaxPlayers (Status, CurrentPlayers, MaxPlayers),
    INDEX IX_GameServers_ExpiresAt (ExpiresAt),
    
    CONSTRAINT FK_GameServers_Places_PlaceId 
        FOREIGN KEY (PlaceId) 
        REFERENCES Places(Id) 
        ON DELETE CASCADE,
    CONSTRAINT FK_GameServers_Games_GameId 
        FOREIGN KEY (GameId) 
        REFERENCES Games(Id) 
        ON DELETE CASCADE,
    CONSTRAINT FK_GameServers_Users_CreatorId 
        FOREIGN KEY (CreatorId) 
        REFERENCES Users(Id) 
        ON DELETE RESTRICT
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- ============================================
-- GAME SERVER PLAYERS TABLE
-- ============================================
CREATE TABLE IF NOT EXISTS GameServerPlayers (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    ServerId INT NOT NULL,
    UserId INT NOT NULL,
    DisplayName VARCHAR(50) NULL,
    MembershipType VARCHAR(50) NULL,
    JoinedAt DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    LeftAt DATETIME(6) NULL,
    Ping INT NOT NULL DEFAULT 0,
    Status VARCHAR(50) NULL DEFAULT 'Playing',
    Score INT NOT NULL DEFAULT 0,
    PlayTimeSeconds BIGINT NOT NULL DEFAULT 0,
    
    INDEX IX_GameServerPlayers_ServerId (ServerId),
    INDEX IX_GameServerPlayers_UserId (UserId),
    INDEX IX_GameServerPlayers_JoinedAt (JoinedAt),
    
    CONSTRAINT FK_GameServerPlayers_GameServers_ServerId 
        FOREIGN KEY (ServerId) 
        REFERENCES GameServers(Id) 
        ON DELETE CASCADE,
    CONSTRAINT FK_GameServerPlayers_Users_UserId 
        FOREIGN KEY (UserId) 
        REFERENCES Users(Id) 
        ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- ============================================
-- STORED PROCEDURES
-- ============================================

-- Procedure to clean up expired sessions
DELIMITER //
CREATE PROCEDURE CleanupExpiredSessions()
BEGIN
    DELETE FROM Sessions WHERE ExpiresAt < NOW() OR IsActive = 0;
END //
DELIMITER ;

-- Procedure to clean up expired game servers
DELIMITER //
CREATE PROCEDURE CleanupExpiredServers()
BEGIN
    UPDATE GameServers 
    SET Status = 'Expired' 
    WHERE ExpiresAt < NOW() AND Status IN ('Running', 'Starting');
END //
DELIMITER ;

-- Procedure to update game statistics
DELIMITER //
CREATE PROCEDURE UpdateGameStatistics(IN p_GameId INT)
BEGIN
    UPDATE Games g
    SET 
        g.ActivePlayers = (
            SELECT COALESCE(SUM(gs.CurrentPlayers), 0) 
            FROM GameServers gs 
            WHERE gs.GameId = p_GameId AND gs.Status = 'Running'
        )
    WHERE g.Id = p_GameId;
END //
DELIMITER ;

-- ============================================
-- VIEWS
-- ============================================

-- View for active game servers with game info
CREATE OR REPLACE VIEW ActiveGameServers AS
SELECT 
    gs.Id,
    gs.ServerId,
    gs.PlaceId,
    gs.GameId,
    g.Name AS GameName,
    g.Description AS GameDescription,
    gs.Host,
    gs.Port,
    gs.CurrentPlayers,
    gs.MaxPlayers,
    gs.Status,
    gs.CreatedAt,
    gs.ExpiresAt,
    u.Username AS CreatorName
FROM GameServers gs
INNER JOIN Games g ON gs.GameId = g.Id
INNER JOIN Users u ON gs.CreatorId = u.Id
WHERE gs.Status IN ('Running', 'Starting');

-- View for user statistics
CREATE OR REPLACE VIEW UserStatistics AS
SELECT 
    u.Id,
    u.Username,
    u.DisplayName,
    u.CreatedAt,
    u.LastLoginAt,
    u.LoginCount,
    u.Reputation,
    COUNT(DISTINCT g.Id) AS TotalGames,
    COALESCE(SUM(g.TotalPlays), 0) AS TotalGamePlays
FROM Users u
LEFT JOIN Games g ON u.Id = g.CreatorId
GROUP BY u.Id;

-- ============================================
-- INDEXES FOR PERFORMANCE
-- ============================================

-- Additional composite indexes for common queries
CREATE INDEX IX_Games_CreatedAt_TotalPlays ON Games(CreatedAt, TotalPlays DESC);
CREATE INDEX IX_Places_GameId_IsActive ON Places(GameId, IsActive);
CREATE INDEX IX_Assets_Type_IsPublic ON Assets(Type, IsPublic);

-- ============================================
-- TRIGGERS
-- ============================================

DELIMITER //
CREATE TRIGGER UpdatePlacePlayerCount
AFTER INSERT ON GameServerPlayers
FOR EACH ROW
BEGIN
    UPDATE Places SET CurrentPlayers = CurrentPlayers + 1 WHERE Id = NEW.ServerId;
END //

CREATE TRIGGER DecrementPlacePlayerCount
AFTER UPDATE ON GameServerPlayers
FOR EACH ROW
WHEN NEW.LeftAt IS NOT NULL
BEGIN
    UPDATE Places SET CurrentPlayers = GREATEST(CurrentPlayers - 1, 0) WHERE Id = OLD.ServerId;
END //
DELIMITER ;

-- ============================================
-- DEFAULT ADMIN USER
-- Note: Password is 'Admin123!' - BCrypt hash
-- ============================================
-- INSERT INTO Users (Username, Email, PasswordHash, DisplayName, Description, CreatedAt, UpdatedAt, EmailVerified, Reputation)
-- VALUES ('Admin', 'admin@revival.local', '$2a$12$...', 'Administrator', 'Roblox Revival Administrator', NOW(), NOW(), 1, 100);
-- ============================================