-- =============================================
-- Script cập nhật menu "Thuê sách" thành "Sách đang thuê"
-- =============================================

-- Kiểm tra và cập nhật menu con "Thuê sách" trong menu "Dịch vụ"
DECLARE @DichVuMenuId INT = (SELECT MenuId FROM tb_Menu WHERE Alias = N'dich-vu' AND Levels = 1);

IF @DichVuMenuId IS NOT NULL
BEGIN
    -- Kiểm tra xem menu "Thuê sách" đã tồn tại chưa
    DECLARE @ThueSachMenuId INT = (SELECT MenuId FROM tb_Menu WHERE Alias = N'thue-sach' AND ParentId = @DichVuMenuId);
    
    IF @ThueSachMenuId IS NOT NULL
    BEGIN
        -- Cập nhật menu "Thuê sách" thành "Sách đang thuê"
        UPDATE tb_Menu
        SET 
            Title = N'Sách đang thuê',
            Alias = N'sach-dang-thue',
            Url = N'/sach-dang-thue',
            Description = N'Xem danh sách sách đang thuê',
            UpdatedDate = GETDATE()
        WHERE MenuId = @ThueSachMenuId;
        
        PRINT N'Đã cập nhật menu "Thuê sách" thành "Sách đang thuê"';
    END
    ELSE
    BEGIN
        -- Tạo mới menu "Sách đang thuê" nếu chưa tồn tại
        INSERT INTO tb_Menu (Title, Alias, Url, Description, Levels, ParentId, IsActive, CreatedDate, CreatedBy, Position)
        VALUES 
            (N'Sách đang thuê', N'sach-dang-thue', N'/sach-dang-thue', N'Xem danh sách sách đang thuê', 2, @DichVuMenuId, 1, GETDATE(), N'System', 1);
        
        PRINT N'Đã tạo mới menu "Sách đang thuê"';
    END
END
ELSE
BEGIN
    PRINT N'Không tìm thấy menu "Dịch vụ". Vui lòng kiểm tra lại.';
END
GO



