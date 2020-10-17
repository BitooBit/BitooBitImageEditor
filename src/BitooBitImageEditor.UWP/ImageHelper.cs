using BitooBitImageEditor.UWP;
using System;
using System.IO;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Streams;
using Xamarin.Forms;

[assembly: Dependency(typeof(ImageHelper))]
namespace BitooBitImageEditor.UWP
{
    internal class ImageHelper : IImageHelper
    {
        public async Task<bool> SaveImageAsync(byte[] data, string filename, string folder = null)
        {
            if (folder == null)
                folder = ImageEditor.Instance.FolderName;

            StorageFolder picturesDirectory = KnownFolders.PicturesLibrary;
            StorageFolder folderDirectory = picturesDirectory;

            // Get the folder or create it if necessary
            if (!string.IsNullOrEmpty(folder))
            {
                try
                {
                    folderDirectory = await picturesDirectory.GetFolderAsync(folder);
                }
                catch
                { }

                if (folderDirectory == null)
                {
                    try
                    {
                        folderDirectory = await picturesDirectory.CreateFolderAsync(folder);
                    }
                    catch
                    {
                        return false;
                    }
                }
            }

            try
            {
                // Create the file.
                StorageFile storageFile = await folderDirectory.CreateFileAsync(filename,
                                                    CreationCollisionOption.GenerateUniqueName);

                // Convert byte[] to Windows buffer and write it out.
                IBuffer buffer = WindowsRuntimeBuffer.Create(data, 0, data.Length, data.Length);
                await FileIO.WriteBufferAsync(storageFile, buffer);
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }
    }
}
