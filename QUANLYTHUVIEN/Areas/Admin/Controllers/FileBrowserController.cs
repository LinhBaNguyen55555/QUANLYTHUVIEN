using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.StaticFiles;
using System.IO;
using System.Linq;

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

