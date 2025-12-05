-- Script tạo bảng TbMenus và dữ liệu mẫu
-- Chạy script này trong SQL Server Management Studio hoặc công cụ quản lý database

USE QLTHUVIEN;
GO

-- Tạo bảng TbMenus nếu chưa tồn tại
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='TbMenus' AND xtype='U')
BEGIN
    CREATE TABLE [dbo].[TbMenus](
        [MenuId] [int] IDENTITY(1,1) NOT NULL,
        [Title] [nvarchar](100) NOT NULL,
        [Alias] [nvarchar](100) NULL,
        [Url] [nvarchar](max) NULL,
        [Description] [nvarchar](500) NULL,
        [Levels] [int] NULL,
        [ParentId] [int] NULL,
        [Position] [int] NULL,
        [CreatedDate] [datetime2](7) NULL,
        [CreatedBy] [nvarchar](max) NULL,
        [ModifiedDate] [datetime2](7) NULL,
        [ModifiedBy] [nvarchar](max) NULL,
        [IsActive] [bit] NOT NULL,
     CONSTRAINT [PK_TbMenus] PRIMARY KEY CLUSTERED
    (
        [MenuId] ASC
    )WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
    ) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

    -- Tạo khóa ngoại
    ALTER TABLE [dbo].[TbMenus]  WITH CHECK ADD  CONSTRAINT [FK_TbMenus_TbMenus_ParentId] FOREIGN KEY([ParentId])
    REFERENCES [dbo].[TbMenus] ([MenuId])

    ALTER TABLE [dbo].[TbMenus] CHECK CONSTRAINT [FK_TbMenus_TbMenus_ParentId]

    PRINT 'Bảng TbMenus đã được tạo thành công!'
END
ELSE
BEGIN
    PRINT 'Bảng TbMenus đã tồn tại!'
END
GO

-- Xóa dữ liệu cũ nếu có
DELETE FROM TbMenus;
GO

-- Reset identity
DBCC CHECKIDENT ('TbMenus', RESEED, 0);
GO

-- Thêm dữ liệu mẫu cho bảng TbMenus
DECLARE @Now DATETIME2 = GETDATE()

-- Menu cấp 1
INSERT INTO TbMenus (Title, Alias, Url, Description, Levels, ParentId, Position, CreatedDate, CreatedBy, IsActive)
VALUES ('Trang chủ', 'trang-chu', '/', 'Trang chủ chính của website', 1, NULL, 1, @Now, 'Admin', 1)

INSERT INTO TbMenus (Title, Alias, Url, Description, Levels, ParentId, Position, CreatedDate, CreatedBy, IsActive)
VALUES ('Sách', 'sach', '/Books', 'Danh mục sách và tài liệu', 1, NULL, 2, @Now, 'Admin', 1)

INSERT INTO TbMenus (Title, Alias, Url, Description, Levels, ParentId, Position, CreatedDate, CreatedBy, IsActive)
VALUES ('Tác giả', 'tac-gia', '/Authors', 'Danh sách tác giả', 1, NULL, 3, @Now, 'Admin', 1)

INSERT INTO TbMenus (Title, Alias, Url, Description, Levels, ParentId, Position, CreatedDate, CreatedBy, IsActive)
VALUES ('Thể loại', 'the-loai', '/Categories', 'Danh mục thể loại sách', 1, NULL, 4, @Now, 'Admin', 1)

INSERT INTO TbMenus (Title, Alias, Url, Description, Levels, ParentId, Position, CreatedDate, CreatedBy, IsActive)
VALUES ('Liên hệ', 'lien-he', '/Contact', 'Thông tin liên hệ', 1, NULL, 5, @Now, 'Admin', 1)

-- Menu cấp 2 (con của Sách - ID = 2)
INSERT INTO TbMenus (Title, Alias, Url, Description, Levels, ParentId, Position, CreatedDate, CreatedBy, IsActive)
VALUES ('Sách giáo khoa', 'sach-giao-khoa', '/Books/Category/1', 'Sách giáo khoa các cấp', 2, 2, 1, @Now, 'Admin', 1)

INSERT INTO TbMenus (Title, Alias, Url, Description, Levels, ParentId, Position, CreatedDate, CreatedBy, IsActive)
VALUES ('Sách tham khảo', 'sach-tham-khao', '/Books/Category/2', 'Sách tham khảo học thuật', 2, 2, 2, @Now, 'Admin', 1)

INSERT INTO TbMenus (Title, Alias, Url, Description, Levels, ParentId, Position, CreatedDate, CreatedBy, IsActive)
VALUES ('Tiểu thuyết', 'tieu-thuyet', '/Books/Category/3', 'Tiểu thuyết và văn học', 2, 2, 3, @Now, 'Admin', 1)

INSERT INTO TbMenus (Title, Alias, Url, Description, Levels, ParentId, Position, CreatedDate, CreatedBy, IsActive)
VALUES ('Sách ngoại ngữ', 'sach-ngoai-ngu', '/Books/Category/4', 'Sách học ngoại ngữ', 2, 2, 4, @Now, 'Admin', 1)

-- Menu cấp 2 (con của Tác giả - ID = 3)
INSERT INTO TbMenus (Title, Alias, Url, Description, Levels, ParentId, Position, CreatedDate, CreatedBy, IsActive)
VALUES ('Tác giả Việt Nam', 'tac-gia-viet-nam', '/Authors/Country/1', 'Tác giả người Việt Nam', 2, 3, 1, @Now, 'Admin', 1)

INSERT INTO TbMenus (Title, Alias, Url, Description, Levels, ParentId, Position, CreatedDate, CreatedBy, IsActive)
VALUES ('Tác giả nước ngoài', 'tac-gia-nuoc-ngoai', '/Authors/Country/2', 'Tác giả quốc tế', 2, 3, 2, @Now, 'Admin', 1)

-- Menu cấp 3 (cháu của Tiểu thuyết - ID = 8 vì Tiểu thuyết là menu thứ 8)
INSERT INTO TbMenus (Title, Alias, Url, Description, Levels, ParentId, Position, CreatedDate, CreatedBy, IsActive)
VALUES ('Tiểu thuyết hiện đại', 'tieu-thuyet-hien-dai', '/Books/Category/3/Sub/1', 'Tiểu thuyết hiện đại', 3, 8, 1, @Now, 'Admin', 1)

INSERT INTO TbMenus (Title, Alias, Url, Description, Levels, ParentId, Position, CreatedDate, CreatedBy, IsActive)
VALUES ('Tiểu thuyết kinh điển', 'tieu-thuyet-kinh-dien', '/Books/Category/3/Sub/2', 'Tiểu thuyết kinh điển', 3, 8, 2, @Now, 'Admin', 1)

-- Hiển thị kết quả
SELECT * FROM TbMenus ORDER BY Position;

PRINT 'Đã tạo thành công dữ liệu mẫu cho menu!';
PRINT 'Tổng số menu: ' + CAST((SELECT COUNT(*) FROM TbMenus) AS NVARCHAR(10));
