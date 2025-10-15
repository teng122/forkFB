using Supabase;

namespace foodbook.Services
{
    public class StorageService
    {
        private readonly SupabaseService _supabaseService;
        private readonly ILogger<StorageService> _logger;

        public StorageService(SupabaseService supabaseService, ILogger<StorageService> logger)
        {
            _supabaseService = supabaseService;
            _logger = logger;
        }

        /// <summary>
        /// Upload file lên Supabase Storage
        /// </summary>
        /// <param name="file">File cần upload</param>
        /// <param name="isVideo">True nếu là video, False nếu là ảnh</param>
        /// <param name="folderPath">Thư mục con trong bucket (optional)</param>
        /// <returns>URL public của file đã upload</returns>
        public async Task<string> UploadFileAsync(IFormFile file, bool isVideo = false, string folderPath = "")
        {
            try
            {
                _logger.LogInformation("UploadFileAsync called: {FileName}, isVideo={IsVideo}, folder={Folder}", 
                    file?.FileName, isVideo, folderPath);
                
                if (file == null || file.Length == 0)
                {
                    _logger.LogError("File is null or empty");
                    throw new ArgumentException("File không hợp lệ");
                }

                // Chọn bucket dựa vào loại file
                var bucketName = isVideo ? "videos" : "img";
                _logger.LogInformation("Using bucket: {Bucket}", bucketName);

                // Tạo tên file unique
                var fileExtension = Path.GetExtension(file.FileName);
                var uniqueFileName = $"{Guid.NewGuid()}{fileExtension}";
                
                // Tạo đường dẫn đầy đủ
                var filePath = string.IsNullOrEmpty(folderPath) 
                    ? uniqueFileName 
                    : $"{folderPath}/{uniqueFileName}";
                
                _logger.LogInformation("File path: {FilePath}", filePath);

                // Đọc file content
                byte[] fileBytes;
                using (var memoryStream = new MemoryStream())
                {
                    await file.CopyToAsync(memoryStream);
                    fileBytes = memoryStream.ToArray();
                }
                
                _logger.LogInformation("File read: {Size} bytes", fileBytes.Length);

                // Upload lên Supabase Storage
                _logger.LogInformation("Uploading to Supabase Storage...");
                
                var uploadResult = await _supabaseService.Client.Storage
                    .From(bucketName)
                    .Upload(fileBytes, filePath, new Supabase.Storage.FileOptions
                    {
                        ContentType = file.ContentType,
                        Upsert = false
                    });
                
                _logger.LogInformation("Upload successful!");

                // Lấy public URL
                var publicUrl = _supabaseService.Client.Storage
                    .From(bucketName)
                    .GetPublicUrl(filePath);
                
                _logger.LogInformation("Public URL: {Url}", publicUrl);

                return publicUrl;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Upload failed: {Message}", ex.Message);
                _logger.LogError("Exception Type: {Type}", ex.GetType().Name);
                if (ex.InnerException != null)
                {
                    _logger.LogError("Inner Exception: {InnerMessage}", ex.InnerException.Message);
                }
                throw new Exception($"Không thể upload file '{file?.FileName}': {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Upload nhiều file cùng lúc
        /// </summary>
        public async Task<List<string>> UploadMultipleFilesAsync(List<IFormFile> files, bool isVideo = false, string folderPath = "")
        {
            var urls = new List<string>();

            foreach (var file in files)
            {
                if (file != null && file.Length > 0)
                {
                    var url = await UploadFileAsync(file, isVideo, folderPath);
                    urls.Add(url);
                }
            }

            return urls;
        }

        /// <summary>
        /// Xóa file khỏi Storage (optional - dùng khi cần cleanup)
        /// </summary>
        public async Task<bool> DeleteFileAsync(string fileUrl, bool isVideo = false)
        {
            try
            {
                var bucketName = isVideo ? "videos" : "img";
                
                // Extract file path từ URL
                var uri = new Uri(fileUrl);
                var pathSegments = uri.AbsolutePath.Split('/');
                var filePath = string.Join("/", pathSegments.Skip(pathSegments.Length - 1));

                await _supabaseService.Client.Storage
                    .From(bucketName)
                    .Remove(filePath);

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Không thể xóa file: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Kiểm tra xem file có phải là video không
        /// </summary>
        public bool IsVideoFile(IFormFile file)
        {
            if (file == null) return false;

            var videoExtensions = new[] { ".mp4", ".avi", ".mov", ".wmv", ".flv", ".webm", ".mkv" };
            var extension = Path.GetExtension(file.FileName).ToLower();
            
            return videoExtensions.Contains(extension) || 
                   file.ContentType.StartsWith("video/", StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Kiểm tra xem file có phải là ảnh không
        /// </summary>
        public bool IsImageFile(IFormFile file)
        {
            if (file == null) return false;

            var imageExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".webp", ".svg" };
            var extension = Path.GetExtension(file.FileName).ToLower();
            
            return imageExtensions.Contains(extension) || 
                   file.ContentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase);
        }
    }
}

