-- =============================================
-- Script cập nhật status các đơn hàng cũ từ "Đã thanh toán" thành "Đang thuê"
-- Chỉ cập nhật các đơn chưa trả (ReturnDate IS NULL)
-- =============================================

UPDATE Rentals
SET Status = N'Đang thuê'
WHERE Status = N'Đã thanh toán' 
  AND ReturnDate IS NULL
  AND RentalDate IS NOT NULL;

PRINT N'Đã cập nhật ' + CAST(@@ROWCOUNT AS NVARCHAR(10)) + N' đơn hàng từ "Đã thanh toán" thành "Đang thuê"';
GO

