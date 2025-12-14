-- Tạo bảng BookReviews để lưu đánh giá sách
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[BookReviews]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[BookReviews] (
        [ReviewId] INT IDENTITY(1,1) PRIMARY KEY,
        [BookId] INT NOT NULL,
        [CustomerId] INT NOT NULL,
        [Rating] INT NOT NULL CHECK ([Rating] >= 1 AND [Rating] <= 5),
        [Comment] NVARCHAR(1000) NULL,
        [CreatedDate] DATETIME NOT NULL DEFAULT GETDATE(),
        CONSTRAINT [FK_BookReviews_Books] FOREIGN KEY ([BookId]) REFERENCES [dbo].[Books] ([BookID]) ON DELETE CASCADE,
        CONSTRAINT [FK_BookReviews_Customers] FOREIGN KEY ([CustomerId]) REFERENCES [dbo].[Customers] ([CustomerID]) ON DELETE CASCADE,
        CONSTRAINT [UQ_BookReviews_Book_Customer] UNIQUE ([BookId], [CustomerId]) -- Mỗi customer chỉ đánh giá 1 lần cho mỗi sách
    );

    -- Tạo index để tối ưu truy vấn
    CREATE INDEX [IX_BookReviews_BookId] ON [dbo].[BookReviews] ([BookId]);
    CREATE INDEX [IX_BookReviews_CustomerId] ON [dbo].[BookReviews] ([CustomerId]);
    CREATE INDEX [IX_BookReviews_CreatedDate] ON [dbo].[BookReviews] ([CreatedDate] DESC);

    PRINT 'Bảng BookReviews đã được tạo thành công!';
END
ELSE
BEGIN
    PRINT 'Bảng BookReviews đã tồn tại.';
END






