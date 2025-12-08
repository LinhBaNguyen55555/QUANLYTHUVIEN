# Hướng dẫn sử dụng hệ thống Quản lý Menu

## Tổng quan
Hệ thống quản lý menu cho phép bạn tạo và quản lý menu đa cấp cho website thư viện. Hỗ trợ menu 3 cấp với đầy đủ các chức năng CRUD.

## Cấu trúc Menu
- **Cấp 1**: Menu chính (không có menu cha)
- **Cấp 2**: Menu con (có menu cha là menu cấp 1)
- **Cấp 3**: Menu cháu (có menu cha là menu cấp 2)

## Cách sử dụng

### 1. Truy cập Admin Panel
- URL: `/Admin/Menu`
- Hoặc từ sidebar Admin: **Quản lý Menu**

### 2. Các chức năng chính

#### a) Xem danh sách menu
- Hiển thị tất cả menu theo thứ tự vị trí
- Hiển thị thông tin: ID, Tiêu đề, Alias, URL, Cấp, Menu cha, Vị trí, Trạng thái
- Các thao tác: Chỉnh sửa, Chi tiết, Xóa

#### b) Thêm menu mới
- Click "Thêm menu mới"
- Điền thông tin:
  - **Tiêu đề**: Tên hiển thị của menu (bắt buộc)
  - **Alias**: URL thân thiện (tự động tạo từ tiêu đề nếu để trống)
  - **URL**: Đường dẫn đích (có thể để trống)
  - **Mô tả**: Mô tả ngắn về menu
  - **Cấp menu**: 1, 2, hoặc 3 (bắt buộc)
  - **Menu cha**: Chọn menu cha (chỉ hiển thị khi tạo menu cấp 2, 3)
  - **Vị trí**: Số thứ tự sắp xếp (bắt buộc)
  - **Kích hoạt**: Checkbox để bật/tắt menu

#### c) Chỉnh sửa menu
- Click nút "Sửa" trên hàng tương ứng
- Chỉnh sửa thông tin như khi tạo mới
- Không thể chọn chính nó làm menu cha

#### d) Xem chi tiết menu
- Click nút "Chi tiết" để xem đầy đủ thông tin
- Hiển thị menu con (nếu có)

#### e) Xóa menu
- Click nút "Xóa" để xóa menu
- Hệ thống sẽ cảnh báo nếu menu có menu con

### 3. Quy tắc tạo menu

#### Thứ tự tạo menu:
1. Tạo menu cấp 1 trước
2. Tạo menu cấp 2 (chọn menu cha là menu cấp 1)
3. Tạo menu cấp 3 (chọn menu cha là menu cấp 2)

#### Vị trí sắp xếp:
- Số nhỏ hơn hiển thị trước
- Có thể trùng vị trí nhưng không khuyến khích

#### Alias tự động:
- Tự động tạo từ tiêu đề
- Loại bỏ dấu tiếng Việt
- Thay khoảng trắng bằng dấu gạch ngang
- Chỉ chứa chữ cái, số và dấu gạch ngang

### 4. Dữ liệu mẫu

Chạy file `menu_sample_data.sql` trong SQL Server để tạo dữ liệu mẫu:
```sql
-- Chạy trong SQL Server Management Studio
USE QLTHUVIEN;
-- Copy nội dung từ file menu_sample_data.sql và chạy
```

Dữ liệu mẫu bao gồm:
- 5 menu cấp 1: Trang chủ, Sách, Tác giả, Thể loại, Liên hệ
- 6 menu cấp 2: Các danh mục sách và tác giả
- 2 menu cấp 3: Các tiểu loại tiểu thuyết

### 5. Frontend Integration

Menu sẽ tự động hiển thị trên website thông qua `MenuTopViewComponent`:
- Hỗ trợ menu đa cấp
- Tự động sắp xếp theo vị trí
- Chỉ hiển thị menu đang kích hoạt

### 6. Lưu ý kỹ thuật

#### Validation:
- Tiêu đề: bắt buộc, tối đa 100 ký tự
- Alias: tối đa 100 ký tự
- URL: phải là URL hợp lệ (nếu có)
- Mô tả: tối đa 500 ký tự
- Cấp: 1-3
- Vị trí: số dương

#### Audit Trail:
- Tự động ghi CreatedDate, CreatedBy khi tạo
- Tự động ghi ModifiedDate, ModifiedBy khi cập nhật

#### Quan hệ dữ liệu:
- Menu có thể có nhiều menu con
- Menu chỉ có một menu cha
- Xóa menu cha sẽ không xóa menu con (chỉ mất quan hệ)

### 7. Troubleshooting

#### Lỗi thường gặp:
1. **Không thể chọn menu cha**: Đảm bảo đã tạo menu cấp trên trước
2. **Menu không hiển thị**: Kiểm tra trạng thái "Kích hoạt"
3. **Thứ tự sai**: Kiểm tra trường "Vị trí"

#### Debug:
- Kiểm tra Console Browser (F12) để xem lỗi JavaScript
- Kiểm tra SQL Server connection string trong appsettings.json

### 8. Mở rộng tính năng

#### Có thể thêm:
- Drag & drop để sắp xếp menu
- Import/Export menu từ Excel
- Preview menu trước khi lưu
- Phân quyền theo role
- Multi-language support

## Hỗ trợ

Nếu gặp vấn đề, kiểm tra:
1. File log trong thư mục `logs/`
2. Database connection
3. Permissions của SQL Server user
4. Browser console errors







