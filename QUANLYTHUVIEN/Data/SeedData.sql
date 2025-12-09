-- =============================================
-- Script thêm dữ liệu mẫu cho Database QLTHUVIEN
-- =============================================

USE QLTHUVIEN;
GO

-- =============================================
-- 1. Thêm dữ liệu vào bảng Categories (Thể loại)
-- =============================================
IF NOT EXISTS (SELECT 1 FROM Categories WHERE CategoryName = N'Tiểu thuyết')
BEGIN
    INSERT INTO Categories (CategoryName, Description)
    VALUES 
        (N'Tiểu thuyết', N'Các tác phẩm văn học dài, có cốt truyện phức tạp'),
        (N'Truyện ngắn', N'Các tác phẩm văn học ngắn gọn, súc tích'),
        (N'Khoa học viễn tưởng', N'Thể loại văn học về tương lai và công nghệ'),
        (N'Trinh thám', N'Thể loại truyện về điều tra và giải quyết tội phạm'),
        (N'Lịch sử', N'Sách về các sự kiện và nhân vật lịch sử'),
        (N'Khoa học', N'Sách về khoa học tự nhiên và xã hội'),
        (N'Kinh tế', N'Sách về kinh tế học và tài chính'),
        (N'Văn học cổ điển', N'Các tác phẩm văn học kinh điển'),
        (N'Thiếu nhi', N'Sách dành cho trẻ em'),
        (N'Giáo dục', N'Sách giáo khoa và tài liệu học tập');
END
GO

-- =============================================
-- 2. Thêm dữ liệu vào bảng Languages (Ngôn ngữ)
-- =============================================
IF NOT EXISTS (SELECT 1 FROM Languages WHERE LanguageName = N'Tiếng Việt')
BEGIN
    INSERT INTO Languages (LanguageName)
    VALUES 
        (N'Tiếng Việt'),
        (N'Tiếng Anh'),
        (N'Tiếng Pháp'),
        (N'Tiếng Trung'),
        (N'Tiếng Nhật'),
        (N'Tiếng Hàn'),
        (N'Tiếng Đức'),
        (N'Tiếng Tây Ban Nha');
END
GO

-- =============================================
-- 3. Thêm dữ liệu vào bảng Publishers (Nhà xuất bản)
-- =============================================
IF NOT EXISTS (SELECT 1 FROM Publishers WHERE PublisherName = N'Nhà xuất bản Trẻ')
BEGIN
    INSERT INTO Publishers (PublisherName, Address, Phone, Email)
    VALUES 
        (N'Nhà xuất bản Trẻ', N'161B Lý Chính Thắng, P.7, Q.3, TP.HCM', '02839316289', 'nxbtre@nxbtre.com.vn'),
        (N'Nhà xuất bản Kim Đồng', N'55 Quang Trung, Hai Bà Trưng, Hà Nội', '02439434730', 'info@nxbkimdong.com.vn'),
        (N'Nhà xuất bản Giáo dục Việt Nam', N'81 Trần Hưng Đạo, Hoàn Kiếm, Hà Nội', '02438220851', 'info@vnep.edu.vn'),
        (N'Nhà xuất bản Văn học', N'18 Nguyễn Trường Tộ, Ba Đình, Hà Nội', '02437161533', 'nxbvanhoc@vnn.vn'),
        (N'Nhà xuất bản Hội Nhà văn', N'65 Nguyễn Du, Hai Bà Trưng, Hà Nội', '02438221315', 'nxbhoinhavan@hnv.vn'),
        (N'Nhà xuất bản Tổng hợp TP.HCM', N'62 Nguyễn Thị Minh Khai, Q.3, TP.HCM', '02839300388', 'nxbtonghop@nxbtphcm.vn'),
        (N'Nhà xuất bản Thế giới', N'46 Trần Hưng Đạo, Hoàn Kiếm, Hà Nội', '02438253351', 'nxbthegioi@nxbthegioi.vn'),
        (N'Nhà xuất bản Phụ nữ', N'39 Hàng Chuối, Hai Bà Trưng, Hà Nội', '02439719131', 'nxbphunu@nxbphunu.com.vn');
END
GO

-- =============================================
-- 4. Thêm dữ liệu vào bảng Suppliers (Nhà cung cấp)
-- =============================================
IF NOT EXISTS (SELECT 1 FROM Suppliers WHERE SupplierName = N'Công ty Sách Alpha')
BEGIN
    INSERT INTO Suppliers (SupplierName, Address, Phone, Email)
    VALUES 
        (N'Công ty Sách Alpha', N'4 Ngọc Hà, Ba Đình, Hà Nội', '02437711717', 'info@alphabooks.vn'),
        (N'Công ty Cổ phần Văn hóa và Truyền thông Nhã Nam', N'59 Đỗ Quang, Cầu Giấy, Hà Nội', '02435147826', 'contact@nhanam.vn'),
        (N'Công ty TNHH Thương mại và Dịch vụ Văn hóa Đinh Tị', N'27 Nguyễn Trung Trực, Ba Đình, Hà Nội', '02437151515', 'info@dinhtibooks.com.vn'),
        (N'Công ty TNHH MTV Thương mại Sách Fahasa', N'60-62 Lê Lợi, Q.1, TP.HCM', '02838225757', 'info@fahasa.com.vn'),
        (N'Công ty Cổ phần Sách và Thiết bị Giáo dục Miền Nam', N'231 Nguyễn Văn Cừ, Q.5, TP.HCM', '02838350421', 'info@sged.vn');
END
GO

-- =============================================
-- 5. Thêm dữ liệu vào bảng Authors (Tác giả)
-- =============================================
IF NOT EXISTS (SELECT 1 FROM Authors WHERE AuthorName = N'Nguyễn Du')
BEGIN
    INSERT INTO Authors (AuthorName, Biography, Image)
    VALUES 
        (N'Nguyễn Du', N'Đại thi hào dân tộc Việt Nam, tác giả của Truyện Kiều', 'nguyen-du.jpg'),
        (N'Nam Cao', N'Nhà văn hiện thực xuất sắc của văn học Việt Nam', 'nam-cao.jpg'),
        (N'Vũ Trọng Phụng', N'Nhà văn, nhà báo nổi tiếng với các tác phẩm hiện thực phê phán', 'vu-trong-phung.jpg'),
        (N'Nguyễn Nhật Ánh', N'Nhà văn chuyên viết cho thanh thiếu niên', 'nguyen-nhat-anh.jpg'),
        (N'F. Scott Fitzgerald', N'Nhà văn Mỹ nổi tiếng với tác phẩm The Great Gatsby', 'fitzgerald.jpg'),
        (N'Ernest Hemingway', N'Nhà văn Mỹ đoạt giải Nobel Văn học', 'hemingway.jpg'),
        (N'J.K. Rowling', N'Tác giả bộ truyện Harry Potter', 'rowling.jpg'),
        (N'George Orwell', N'Nhà văn Anh với các tác phẩm 1984 và Animal Farm', 'orwell.jpg'),
        (N'Haruki Murakami', N'Nhà văn Nhật Bản nổi tiếng thế giới', 'murakami.jpg'),
        (N'Paulo Coelho', N'Nhà văn Brazil với tác phẩm Nhà giả kim', 'coelho.jpg');
END
GO

-- =============================================
-- 6. Thêm dữ liệu vào bảng tb_Roles (Vai trò)
-- =============================================
IF NOT EXISTS (SELECT 1 FROM tb_Roles WHERE RoleName = N'Admin')
BEGIN
    INSERT INTO tb_Roles (RoleName, Description, IsActive, CreatedDate, CreatedBy)
    VALUES 
        (N'Admin', N'Quản trị viên hệ thống, có toàn quyền', 1, GETDATE(), N'System'),
        (N'Librarian', N'Thủ thư, quản lý sách và cho mượn', 1, GETDATE(), N'System'),
        (N'Staff', N'Nhân viên thư viện', 1, GETDATE(), N'System'),
        (N'Member', N'Thành viên thư viện', 1, GETDATE(), N'System'),
        (N'Guest', N'Khách tham quan', 1, GETDATE(), N'System');
END
GO

-- =============================================
-- 7. Thêm dữ liệu vào bảng tb_Menu (Menu)
-- =============================================
IF NOT EXISTS (SELECT 1 FROM tb_Menu WHERE Title = N'Trang chủ')
BEGIN
    -- Menu cha
    INSERT INTO tb_Menu (Title, Alias, Url, Description, Levels, ParentId, IsActive, CreatedDate, CreatedBy)
    VALUES 
        (N'Trang chủ', N'trang-chu', N'/', N'Trang chủ của website', 1, NULL, 1, GETDATE(), N'System'),
        (N'Sách & Truyện', N'sach-truyen', N'/Book', N'Danh sách sách và truyện', 1, NULL, 1, GETDATE(), N'System'),
        (N'Tin tức', N'tin-tuc', N'/Blog', N'Tin tức và sự kiện', 1, NULL, 1, GETDATE(), N'System'),
        (N'Dịch vụ', N'dich-vu', N'/Services', N'Các dịch vụ của thư viện', 1, NULL, 1, GETDATE(), N'System'),
        (N'Liên hệ', N'lien-he', N'/Contact', N'Thông tin liên hệ', 1, NULL, 1, GETDATE(), N'System');
    
    -- Menu con cho "Sách & Truyện"
    DECLARE @MenuSachId INT = (SELECT MenuId FROM tb_Menu WHERE Alias = N'sach-truyen');
    
    INSERT INTO tb_Menu (Title, Alias, Url, Description, Levels, ParentId, IsActive, CreatedDate, CreatedBy)
    VALUES 
        (N'Tiểu thuyết', N'tieu-thuyet', N'/Book?category=1', N'Sách tiểu thuyết', 2, @MenuSachId, 1, GETDATE(), N'System'),
        (N'Truyện ngắn', N'truyen-ngan', N'/Book?category=2', N'Truyện ngắn', 2, @MenuSachId, 1, GETDATE(), N'System'),
        (N'Khoa học viễn tưởng', N'khoa-hoc-vien-tuong', N'/Book?category=3', N'Sách khoa học viễn tưởng', 2, @MenuSachId, 1, GETDATE(), N'System'),
        (N'Trinh thám', N'trinh-tham', N'/Book?category=4', N'Sách trinh thám', 2, @MenuSachId, 1, GETDATE(), N'System');
END
GO

-- =============================================
-- 8. Thêm dữ liệu vào bảng Users (Người dùng)
-- =============================================
-- Lưu ý: Mật khẩu mặc định là "123456" (đã hash bằng SHA256)
-- Hash của "123456" = "e10adc3949ba59abbe56e057f20f883e"
IF NOT EXISTS (SELECT 1 FROM Users WHERE Username = N'admin')
BEGIN
    DECLARE @AdminRoleId INT = (SELECT RoleId FROM tb_Roles WHERE RoleName = N'Admin');
    DECLARE @LibrarianRoleId INT = (SELECT RoleId FROM tb_Roles WHERE RoleName = N'Librarian');
    DECLARE @MemberRoleId INT = (SELECT RoleId FROM tb_Roles WHERE RoleName = N'Member');
    
    INSERT INTO Users (Username, PasswordHash, FullName, Email, Phone, Role, RoleId, CreatedAt)
    VALUES 
        (N'admin', N'e10adc3949ba59abbe56e057f20f883e', N'Quản trị viên', N'admin@libraria.com', N'0123456789', N'Admin', @AdminRoleId, GETDATE()),
        (N'thuthu1', N'e10adc3949ba59abbe56e057f20f883e', N'Nguyễn Văn A', N'thuthu1@libraria.com', N'0987654321', N'Librarian', @LibrarianRoleId, GETDATE()),
        (N'nhanvien1', N'e10adc3949ba59abbe56e057f20f883e', N'Trần Thị B', N'nhanvien1@libraria.com', N'0912345678', N'Staff', @LibrarianRoleId, GETDATE()),
        (N'thanhvien1', N'e10adc3949ba59abbe56e057f20f883e', N'Lê Văn C', N'thanhvien1@libraria.com', N'0923456789', N'Member', @MemberRoleId, GETDATE());
END
GO

-- =============================================
-- 9. Thêm dữ liệu vào bảng Customers (Khách hàng)
-- =============================================
IF NOT EXISTS (SELECT 1 FROM Customers WHERE Email = N'customer1@example.com')
BEGIN
    INSERT INTO Customers (FullName, Email, Phone, Address, CreatedAt)
    VALUES 
        (N'Phạm Thị D', N'customer1@example.com', N'0934567890', N'123 Đường ABC, Quận 1, TP.HCM', GETDATE()),
        (N'Hoàng Văn E', N'customer2@example.com', N'0945678901', N'456 Đường XYZ, Quận 2, TP.HCM', GETDATE()),
        (N'Võ Thị F', N'customer3@example.com', N'0956789012', N'789 Đường DEF, Quận 3, TP.HCM', GETDATE()),
        (N'Đặng Văn G', N'customer4@example.com', N'0967890123', N'321 Đường GHI, Quận 4, TP.HCM', GETDATE()),
        (N'Bùi Thị H', N'customer5@example.com', N'0978901234', N'654 Đường JKL, Quận 5, TP.HCM', GETDATE());
END
GO

-- =============================================
-- 10. Thêm dữ liệu mẫu vào bảng Books (Sách)
-- =============================================
-- Lưu ý: Cần có dữ liệu trong Categories, Languages, Publishers trước
IF NOT EXISTS (SELECT 1 FROM Books WHERE Title = N'Truyện Kiều')
BEGIN
    DECLARE @CategoryTieuThuyet INT = (SELECT TOP 1 CategoryId FROM Categories WHERE CategoryName = N'Tiểu thuyết');
    DECLARE @CategoryTruyenNgan INT = (SELECT TOP 1 CategoryId FROM Categories WHERE CategoryName = N'Truyện ngắn');
    DECLARE @CategoryKhoaHoc INT = (SELECT TOP 1 CategoryId FROM Categories WHERE CategoryName = N'Khoa học viễn tưởng');
    DECLARE @LanguageViet INT = (SELECT TOP 1 LanguageId FROM Languages WHERE LanguageName = N'Tiếng Việt');
    DECLARE @LanguageAnh INT = (SELECT TOP 1 LanguageId FROM Languages WHERE LanguageName = N'Tiếng Anh');
    DECLARE @PublisherTre INT = (SELECT TOP 1 PublisherId FROM Publishers WHERE PublisherName = N'Nhà xuất bản Trẻ');
    DECLARE @PublisherKimDong INT = (SELECT TOP 1 PublisherId FROM Publishers WHERE PublisherName = N'Nhà xuất bản Kim Đồng');
    
    INSERT INTO Books (Title, ISBN, CategoryId, PublisherId, LanguageId, PublishedYear, Quantity, Description, CoverImage)
    VALUES 
        (N'Truyện Kiều', N'9786041234567', @CategoryTieuThuyet, @PublisherTre, @LanguageViet, 1820, 50, N'Tác phẩm kinh điển của Nguyễn Du', N'books-media/gird-view/truyen-kieu.jpg'),
        (N'Chí Phèo', N'9786041234568', @CategoryTruyenNgan, @PublisherKimDong, @LanguageViet, 1941, 30, N'Truyện ngắn nổi tiếng của Nam Cao', N'books-media/gird-view/chi-pheo.jpg'),
        (N'Số đỏ', N'9786041234569', @CategoryTieuThuyet, @PublisherTre, @LanguageViet, 1936, 40, N'Tác phẩm của Vũ Trọng Phụng', N'books-media/gird-view/so-do.jpg'),
        (N'Tôi thấy hoa vàng trên cỏ xanh', N'9786041234570', @CategoryTieuThuyet, @PublisherTre, @LanguageViet, 2010, 60, N'Tác phẩm của Nguyễn Nhật Ánh', N'books-media/gird-view/toi-thay-hoa-vang.jpg'),
        (N'The Great Gatsby', N'9786041234571', @CategoryTieuThuyet, @PublisherTre, @LanguageAnh, 1925, 25, N'Classic American novel by F. Scott Fitzgerald', N'books-media/gird-view/great-gatsby.jpg'),
        (N'Harry Potter và Hòn đá Phù thủy', N'9786041234572', @CategoryKhoaHoc, @PublisherKimDong, @LanguageViet, 1997, 100, N'Bộ truyện nổi tiếng của J.K. Rowling', N'books-media/gird-view/harry-potter.jpg');
    
    -- Thêm liên kết tác giả với sách (BookAuthors)
    DECLARE @AuthorNguyenDu INT = (SELECT TOP 1 AuthorId FROM Authors WHERE AuthorName = N'Nguyễn Du');
    DECLARE @AuthorNamCao INT = (SELECT TOP 1 AuthorId FROM Authors WHERE AuthorName = N'Nam Cao');
    DECLARE @AuthorVuTrongPhung INT = (SELECT TOP 1 AuthorId FROM Authors WHERE AuthorName = N'Vũ Trọng Phụng');
    DECLARE @AuthorNguyenNhatAnh INT = (SELECT TOP 1 AuthorId FROM Authors WHERE AuthorName = N'Nguyễn Nhật Ánh');
    DECLARE @AuthorFitzgerald INT = (SELECT TOP 1 AuthorId FROM Authors WHERE AuthorName = N'F. Scott Fitzgerald');
    DECLARE @AuthorRowling INT = (SELECT TOP 1 AuthorId FROM Authors WHERE AuthorName = N'J.K. Rowling');
    
    DECLARE @BookTruyenKieu INT = (SELECT TOP 1 BookId FROM Books WHERE Title = N'Truyện Kiều');
    DECLARE @BookChiPheo INT = (SELECT TOP 1 BookId FROM Books WHERE Title = N'Chí Phèo');
    DECLARE @BookSoDo INT = (SELECT TOP 1 BookId FROM Books WHERE Title = N'Số đỏ');
    DECLARE @BookHoaVang INT = (SELECT TOP 1 BookId FROM Books WHERE Title = N'Tôi thấy hoa vàng trên cỏ xanh');
    DECLARE @BookGatsby INT = (SELECT TOP 1 BookId FROM Books WHERE Title = N'The Great Gatsby');
    DECLARE @BookHarryPotter INT = (SELECT TOP 1 BookId FROM Books WHERE Title = N'Harry Potter và Hòn đá Phù thủy');
    
    IF NOT EXISTS (SELECT 1 FROM BookAuthors WHERE BookId = @BookTruyenKieu AND AuthorId = @AuthorNguyenDu)
        INSERT INTO BookAuthors (BookId, AuthorId) VALUES (@BookTruyenKieu, @AuthorNguyenDu);
    
    IF NOT EXISTS (SELECT 1 FROM BookAuthors WHERE BookId = @BookChiPheo AND AuthorId = @AuthorNamCao)
        INSERT INTO BookAuthors (BookId, AuthorId) VALUES (@BookChiPheo, @AuthorNamCao);
    
    IF NOT EXISTS (SELECT 1 FROM BookAuthors WHERE BookId = @BookSoDo AND AuthorId = @AuthorVuTrongPhung)
        INSERT INTO BookAuthors (BookId, AuthorId) VALUES (@BookSoDo, @AuthorVuTrongPhung);
    
    IF NOT EXISTS (SELECT 1 FROM BookAuthors WHERE BookId = @BookHoaVang AND AuthorId = @AuthorNguyenNhatAnh)
        INSERT INTO BookAuthors (BookId, AuthorId) VALUES (@BookHoaVang, @AuthorNguyenNhatAnh);
    
    IF NOT EXISTS (SELECT 1 FROM BookAuthors WHERE BookId = @BookGatsby AND AuthorId = @AuthorFitzgerald)
        INSERT INTO BookAuthors (BookId, AuthorId) VALUES (@BookGatsby, @AuthorFitzgerald);
    
    IF NOT EXISTS (SELECT 1 FROM BookAuthors WHERE BookId = @BookHarryPotter AND AuthorId = @AuthorRowling)
        INSERT INTO BookAuthors (BookId, AuthorId) VALUES (@BookHarryPotter, @AuthorRowling);
    
    -- Thêm giá thuê (RentalPrices)
    IF NOT EXISTS (SELECT 1 FROM RentalPrices WHERE BookId = @BookTruyenKieu)
        INSERT INTO RentalPrices (BookId, DailyRate, WeeklyRate, MonthlyRate, EffectiveDate)
        VALUES (@BookTruyenKieu, 10000, 60000, 200000, GETDATE());
    
    IF NOT EXISTS (SELECT 1 FROM RentalPrices WHERE BookId = @BookChiPheo)
        INSERT INTO RentalPrices (BookId, DailyRate, WeeklyRate, MonthlyRate, EffectiveDate)
        VALUES (@BookChiPheo, 8000, 50000, 180000, GETDATE());
    
    IF NOT EXISTS (SELECT 1 FROM RentalPrices WHERE BookId = @BookSoDo)
        INSERT INTO RentalPrices (BookId, DailyRate, WeeklyRate, MonthlyRate, EffectiveDate)
        VALUES (@BookSoDo, 10000, 60000, 200000, GETDATE());
    
    IF NOT EXISTS (SELECT 1 FROM RentalPrices WHERE BookId = @BookHoaVang)
        INSERT INTO RentalPrices (BookId, DailyRate, WeeklyRate, MonthlyRate, EffectiveDate)
        VALUES (@BookHoaVang, 12000, 70000, 250000, GETDATE());
    
    IF NOT EXISTS (SELECT 1 FROM RentalPrices WHERE BookId = @BookGatsby)
        INSERT INTO RentalPrices (BookId, DailyRate, WeeklyRate, MonthlyRate, EffectiveDate)
        VALUES (@BookGatsby, 15000, 80000, 300000, GETDATE());
    
    IF NOT EXISTS (SELECT 1 FROM RentalPrices WHERE BookId = @BookHarryPotter)
        INSERT INTO RentalPrices (BookId, DailyRate, WeeklyRate, MonthlyRate, EffectiveDate)
        VALUES (@BookHarryPotter, 15000, 80000, 300000, GETDATE());
END
GO

PRINT N'Đã thêm dữ liệu mẫu thành công!';
PRINT N'Thông tin đăng nhập mặc định:';
PRINT N'  - Username: admin';
PRINT N'  - Password: 123456';
PRINT N'  - Username: thuthu1';
PRINT N'  - Password: 123456';


