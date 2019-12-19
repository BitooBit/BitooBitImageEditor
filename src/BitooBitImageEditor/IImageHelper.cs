using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace BitooBitImageEditor
{
    public interface IImageHelper
    {
        Task<Stream> GetImageAsync();
        Task<bool> SaveImageAsync(byte[] data, string filename, string folder = null);
    }
}
