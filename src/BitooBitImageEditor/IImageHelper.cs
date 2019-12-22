using System.IO;
using System.Threading.Tasks;

namespace BitooBitImageEditor
{
    public interface IImageHelper
    {
        Task<Stream> GetImageAsync();
        Task<bool> SaveImageAsync(byte[] data, string filename, string folder = null);
    }
}
