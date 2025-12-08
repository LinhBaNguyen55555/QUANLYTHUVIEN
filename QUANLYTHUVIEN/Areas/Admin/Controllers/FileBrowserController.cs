using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.StaticFiles;
using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace QUANLYTHUVIEN.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class FileBrowserController : Controller
    {
        private readonly IWebHostEnvironment _environment;

        public FileBrowserController(IWebHostEnvironment environment)
        {
            _environment = environment;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequestSizeLimit(10 * 1024 * 1024)] // 10MB limit to avoid oversized uploads
        public async Task<IActionResult> UploadImage(IFormFile file, string folderPath = "images")
        {
            if (file == null || file.Length == 0)
            {
                return Json(new { success = false, message = "Vui lòng chọn một file ảnh." });
            }

            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".webp" };
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

            if (!allowedExtensions.Contains(extension))
            {
                return Json(new { success = false, message = "Định dạng file không hợp lệ. Vui lòng chọn ảnh (jpg, jpeg, png, gif, bmp, webp)." });
            }

            // Đảm bảo đường dẫn hợp lệ và không bắt đầu bằng / hoặc ~
            folderPath = string.IsNullOrWhiteSpace(folderPath)
                ? "images"
                : folderPath.TrimStart('/', '~', '\\');

            var webRootPath = _environment.WebRootPath;
            var targetFolder = Path.Combine(webRootPath, folderPath);

            if (!Directory.Exists(targetFolder))
            {
                Directory.CreateDirectory(targetFolder);
            }

            // Làm sạch tên file và thêm hậu tố để tránh trùng
            var sanitizedName = Regex.Replace(Path.GetFileNameWithoutExtension(file.FileName), @"[^a-zA-Z0-9-_]+", "-").Trim('-');
            if (string.IsNullOrWhiteSpace(sanitizedName))
            {
                sanitizedName = "image";
            }

            var uniqueFileName = $"{sanitizedName}-{DateTime.UtcNow:yyyyMMddHHmmssfff}{extension}";
            var savePath = Path.Combine(targetFolder, uniqueFileName);

            try
            {
                using (var stream = new FileStream(savePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                var normalizedFolder = folderPath.Replace('\\', '/');
                var relativePath = $"{normalizedFolder}/{uniqueFileName}";

                return Json(new
                {
                    success = true,
                    message = "Tải ảnh lên thành công.",
                    path = $"/{relativePath}",
                    relativePath,
                    fileName = uniqueFileName
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Lỗi khi lưu file: {ex.Message}" });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteImage(string relativePath)
        {
            if (string.IsNullOrWhiteSpace(relativePath))
            {
                return Json(new { success = false, message = "Thiếu đường dẫn ảnh cần xóa." });
            }

            try
            {
                // Chuẩn hóa path, tránh ký tự dẫn tới thư mục khác
                var cleaned = relativePath.TrimStart('/', '\\', '~');
                cleaned = cleaned.Replace('/', Path.DirectorySeparatorChar).Replace('\\', Path.DirectorySeparatorChar);

                var fullPath = Path.Combine(_environment.WebRootPath, cleaned);

                // Đảm bảo file nằm trong wwwroot
                var webRootFull = Path.GetFullPath(_environment.WebRootPath);
                var targetFull = Path.GetFullPath(fullPath);
                if (!targetFull.StartsWith(webRootFull, StringComparison.OrdinalIgnoreCase))
                {
                    return Json(new { success = false, message = "Đường dẫn không hợp lệ." });
                }

                if (!System.IO.File.Exists(targetFull))
                {
                    return Json(new { success = false, message = "Ảnh không tồn tại hoặc đã bị xóa." });
                }

                System.IO.File.Delete(targetFull);
                return Json(new { success = true, message = "Đã xóa ảnh." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Lỗi khi xóa ảnh: {ex.Message}" });
            }
        }

        [HttpGet]
        public IActionResult Index(string folder = "images")
        {
            var webRootPath = _environment.WebRootPath;
            var folderPath = Path.Combine(webRootPath, folder);
            
            var imageFiles = new List<object>();

            if (Directory.Exists(folderPath))
            {
                // Lấy tất cả các file ảnh
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".webp" };
                
                // Lấy ảnh từ thư mục gốc
                var files = Directory.GetFiles(folderPath)
                    .Where(f => allowedExtensions.Contains(Path.GetExtension(f).ToLower()))
                    .Select(f => new
                    {
                        name = Path.GetFileName(f),
                        path = $"/{folder}/{Path.GetFileName(f)}",
                        folder = folder
                    });

                imageFiles.AddRange(files);

                // Lấy ảnh từ các thư mục con
                var subDirectories = Directory.GetDirectories(folderPath);
                foreach (var subDir in subDirectories)
                {
                    var subDirName = Path.GetFileName(subDir);
                    var subFiles = Directory.GetFiles(subDir)
                        .Where(f => allowedExtensions.Contains(Path.GetExtension(f).ToLower()))
                        .Select(f => new
                        {
                            name = Path.GetFileName(f),
                            path = $"/{folder}/{subDirName}/{Path.GetFileName(f)}",
                            folder = $"{folder}/{subDirName}"
                        });

                    imageFiles.AddRange(subFiles);
                }
            }

            return Json(imageFiles.OrderBy(f => ((dynamic)f).folder).ThenBy(f => ((dynamic)f).name));
        }

        [HttpGet]
        public IActionResult GetImagesByFolder(string folderPath = "images")
        {
            try
            {
                var webRootPath = _environment.WebRootPath;
                
                // Đảm bảo folderPath không bắt đầu bằng / hoặc ~
                folderPath = folderPath.TrimStart('/', '~', '\\');
                
                var fullPath = Path.Combine(webRootPath, folderPath);
                
                var imageFiles = new List<object>();

                if (Directory.Exists(fullPath))
                {
                    var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".webp" };
                    
                    var files = Directory.GetFiles(fullPath)
                        .Where(f => allowedExtensions.Contains(Path.GetExtension(f).ToLower()))
                        .Select(f =>
                        {
                            var fileName = Path.GetFileName(f);
                            var normalizedFolderPath = folderPath.Replace('\\', '/');
                            return new
                            {
                                name = fileName,
                                path = $"/{normalizedFolderPath}/{fileName}",
                                relativePath = $"{normalizedFolderPath}/{fileName}"
                            };
                        });

                    imageFiles.AddRange(files);
                }
                else
                {
                    // Trả về lỗi nếu thư mục không tồn tại
                    return Json(new { error = $"Thư mục '{folderPath}' không tồn tại. Đường dẫn đầy đủ: {fullPath}" });
                }

                return Json(imageFiles.OrderBy(f => ((dynamic)f).name));
            }
            catch (Exception ex)
            {
                return Json(new { error = $"Lỗi khi đọc thư mục: {ex.Message}" });
            }
        }
    }
}

