-- Script to create Contacts table for storing contact messages
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Contacts' and xtype='U')
BEGIN
    CREATE TABLE Contacts (
        ContactID INT IDENTITY(1,1) PRIMARY KEY,
        FullName NVARCHAR(200) NOT NULL,
        Email NVARCHAR(100) NOT NULL,
        Phone NVARCHAR(20) NULL,
        Content NVARCHAR(2000) NOT NULL,
        CreatedDate DATETIME DEFAULT GETDATE()
    );
    
    PRINT 'Table Contacts created successfully.';
END
ELSE
BEGIN
    PRINT 'Table Contacts already exists.';
END

