-- Script to create Notifications table for storing user notifications
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Notifications' and xtype='U')
BEGIN
    CREATE TABLE Notifications (
        NotificationID INT IDENTITY(1,1) PRIMARY KEY,
        UserID INT NULL, -- NULL = thông báo cho tất cả users
        Title NVARCHAR(200) NOT NULL,
        Content NVARCHAR(2000) NOT NULL,
        Type NVARCHAR(50) NULL, -- 'info', 'success', 'warning', 'danger'
        IsRead BIT DEFAULT 0,
        CreatedDate DATETIME DEFAULT GETDATE(),
        ReadDate DATETIME NULL,
        FOREIGN KEY (UserID) REFERENCES Users(UserID) ON DELETE SET NULL
    );
    
    PRINT 'Table Notifications created successfully.';
END
ELSE
BEGIN
    PRINT 'Table Notifications already exists.';
END


