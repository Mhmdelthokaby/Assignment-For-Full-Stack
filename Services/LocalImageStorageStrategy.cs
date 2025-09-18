using Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace Services
{
    using Domain.Interfaces;

    namespace Services
    {
        public class LocalImageStorageStrategy : IImageStorageStrategy
        {
            private readonly string _uploadPath;

            public LocalImageStorageStrategy(string rootPath)
            {
                _uploadPath = Path.Combine(rootPath, "uploads");
                if (!Directory.Exists(_uploadPath))
                    Directory.CreateDirectory(_uploadPath);
            }

            public string SaveImage(byte[] imageData, string extension = ".jpg")
            {
                if (imageData == null || imageData.Length == 0) return null;

                var fileName = $"{Guid.NewGuid()}{extension}";
                var filePath = Path.Combine(_uploadPath, fileName);

                File.WriteAllBytes(filePath, imageData);

                return $"/uploads/{fileName}";
            }

            public void DeleteImage(string imagePath)
            {
                if (string.IsNullOrEmpty(imagePath)) return;

                var fullPath = Path.Combine(_uploadPath, Path.GetFileName(imagePath));
                if (File.Exists(fullPath))
                    File.Delete(fullPath);
            }
        }
    }


}
