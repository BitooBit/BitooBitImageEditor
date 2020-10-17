using Android.App;
using Android.Content;
using Android.Media;
using Android.OS;
using BitooBitImageEditor.Droid;
using Java.IO;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Essentials;
using System;
using Environment = Android.OS.Environment;

[assembly: Dependency(typeof(ImageHelper))]
namespace BitooBitImageEditor.Droid
{
    internal class ImageHelper : IImageHelper
    {
        public async Task<bool> SaveImageAsync(byte[] data, string filename, string folder = null)
        {
            try
            {
                File picturesDirectory = Environment.GetExternalStoragePublicDirectory(Environment.DirectoryPictures);
                File folderDirectory;

                if (await Permissions.RequestAsync<Permissions.StorageWrite>() != PermissionStatus.Granted)
                    return false;

                if (!string.IsNullOrEmpty(folder))
                {
                    folderDirectory = new File(picturesDirectory, folder);
                    folderDirectory.Mkdirs();
                }
                else
                    folderDirectory = picturesDirectory;

                using (File bitmapFile = new File(folderDirectory, filename))
                {
                    bitmapFile.CreateNewFile();

                    using (FileOutputStream outputStream = new FileOutputStream(bitmapFile))
                        await outputStream.WriteAsync(data);

                    // Make sure it shows up in the Photos gallery promptly.
                    MediaScannerConnection.ScanFile(Platform.CurrentActivity,
                                                    new string[] { bitmapFile.Path },
                                                    new string[] { "image/png", "image/jpeg" }, null);
                }
            }
            catch(Exception ex)
            {
                return false;
            }
            return true;
        }
    }
}