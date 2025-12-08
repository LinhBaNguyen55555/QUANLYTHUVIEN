# Hướng dẫn thêm dữ liệu mẫu vào Database QLTHUVIEN

## Mô tả
File `SeedData.sql` chứa các câu lệnh SQL để thêm dữ liệu mẫu vào các bảng trong database QLTHUVIEN.

## Các bảng sẽ được thêm dữ liệu:

1. **Categories** (Thể loại sách) - 10 thể loại
2. **Languages** (Ngôn ngữ) - 8 ngôn ngữ
3. **Publishers** (Nhà xuất bản) - 8 nhà xuất bản
4. **Suppliers** (Nhà cung cấp) - 5 nhà cung cấp
5. **Authors** (Tác giả) - 10 tác giả
6. **tb_Roles** (Vai trò) - 5 vai trò (Admin, Librarian, Staff, Member, Guest)
7. **tb_Menu** (Menu) - Menu chính và menu con
8. **Users** (Người dùng) - 4 tài khoản mẫu
9. **Customers** (Khách hàng) - 5 khách hàng mẫu
10. **Books** (Sách) - 6 cuốn sách mẫu với đầy đủ thông tin
11. **BookAuthors** (Liên kết sách-tác giả)
12. **RentalPrices** (Giá thuê sách)

## Cách chạy script:

### Cách 1: Sử dụng SQL Server Management Studio (SSMS)
1. Mở SQL Server Management Studio
2. Kết nối đến server SQL Server của bạn
3. Chọn database `QLTHUVIEN`
4. Mở file `SeedData.sql`
5. Nhấn F5 hoặc click nút Execute để chạy script

### Cách 2: Sử dụng Azure Data Studio
1. Mở Azure Data Studio
2. Kết nối đến SQL Server
3. Chọn database `QLTHUVIEN`
4. Mở file `SeedData.sql`
5. Nhấn Ctrl+Shift+E để chạy script

### Cách 3: Sử dụng Command Line (sqlcmd)
```bash
sqlcmd -S DESKTOP-B3RC6GD\SQLEXPRESS -d QLTHUVIEN -i "Data\SeedData.sql"
```

## Thông tin đăng nhập mặc định:

Sau khi chạy script, bạn có thể đăng nhập với các tài khoản sau:

| Username | Password | Vai trò | Mô tả |
|----------|----------|---------|-------|
| admin | 123456 | Admin | Quản trị viên hệ thống |
| thuthu1 | 123456 | Librarian | Thủ thư |
| nhanvien1 | 123456 | Staff | Nhân viên |
| thanhvien1 | 123456 | Member | Thành viên |

**Lưu ý:** Mật khẩu mặc định là `123456` (đã được hash bằng SHA256)

## Lưu ý quan trọng:

1. **Kiểm tra dữ liệu trước khi chạy:** Script sử dụng `IF NOT EXISTS` để tránh thêm dữ liệu trùng lặp, nhưng bạn nên kiểm tra database trước khi chạy.

2. **Backup database:** Nên backup database trước khi chạy script để có thể khôi phục nếu cần.

3. **Thứ tự thực thi:** Script được thiết kế để chạy theo thứ tự, các bảng phụ thuộc sẽ được thêm sau các bảng cha.

4. **Dữ liệu mẫu:** Đây là dữ liệu mẫu để test, bạn có thể chỉnh sửa theo nhu cầu thực tế.

## Cấu trúc dữ liệu:

### Categories (Thể loại)
- Tiểu thuyết
- Truyện ngắn
- Khoa học viễn tưởng
- Trinh thám
- Lịch sử
- Khoa học
- Kinh tế
- Văn học cổ điển
- Thiếu nhi
- Giáo dục

### Books (Sách mẫu)
1. Truyện Kiều - Nguyễn Du
2. Chí Phèo - Nam Cao
3. Số đỏ - Vũ Trọng Phụng
4. Tôi thấy hoa vàng trên cỏ xanh - Nguyễn Nhật Ánh
5. The Great Gatsby - F. Scott Fitzgerald
6. Harry Potter và Hòn đá Phù thủy - J.K. Rowling

## Troubleshooting:

Nếu gặp lỗi khi chạy script:

1. **Lỗi "Cannot insert duplicate key":** 
   - Dữ liệu đã tồn tại, script sẽ bỏ qua các dòng này
   - Không ảnh hưởng đến các dữ liệu khác

2. **Lỗi "Foreign key constraint":**
   - Kiểm tra xem các bảng cha đã có dữ liệu chưa
   - Chạy lại script từ đầu

3. **Lỗi "Invalid object name":**
   - Kiểm tra tên database có đúng là `QLTHUVIEN` không
   - Kiểm tra các bảng đã được tạo chưa

## Liên hệ:
Nếu có vấn đề, vui lòng liên hệ đội phát triển.

