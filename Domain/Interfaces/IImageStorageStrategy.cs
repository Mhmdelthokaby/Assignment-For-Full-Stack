using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Interfaces
{
    public interface IImageStorageStrategy
    {
        string SaveImage(byte[] imageData, string extension = ".jpg");
        void DeleteImage(string imagePath);
    }

}
