using System.IO;
using System.Threading.Tasks;

namespace BitooBitImageEditor
{
    /// <summary>for internal use by <see cref="BitooBitImageEditor"/></summary>
    public interface IImageHelper
    {
        /// <summary>for internal use by <see cref="BitooBitImageEditor"/></summary>
        Task<bool> SaveImageAsync(byte[] data, string filename, string folder = null);
    }
}
