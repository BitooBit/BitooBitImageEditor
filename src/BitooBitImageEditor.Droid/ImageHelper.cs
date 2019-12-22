using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Media;
using Android.OS;
using BitooBitImageEditor.Droid;
using Java.IO;
using Xamarin.Forms;

[assembly: Dependency(typeof(ImageHelper))]
namespace BitooBitImageEditor.Droid
{
    internal class ImageHelper : IImageHelper
    {
        internal static TaskCompletionSource<System.IO.Stream> PickImageTaskCompletionSource { set; get; }


        public Task<System.IO.Stream> GetImageAsync()
        {
            // Define the Intent for getting images
            Intent intent = new Intent();
            intent.SetType("image/*");
            intent.SetAction(Intent.ActionGetContent);

            // Start the picture-picker activity (resumes in MainActivity.cs)
            Platform.CurrentActivity.StartActivityForResult(
                Intent.CreateChooser(intent, "Select Picture"),
                Platform.PickImageId);

            // Save the TaskCompletionSource object as a MainActivity property
            PickImageTaskCompletionSource = new TaskCompletionSource<System.IO.Stream>();

            // Return Task object
            return PickImageTaskCompletionSource.Task;
        }

        // Saving photos requires android.permission.WRITE_EXTERNAL_STORAGE in AndroidManifest.xml

        public async Task<bool> SaveImageAsync(byte[] data, string filename, string folder = null)
        {
            if (folder == null)
                folder = ImageEditor.Instance.FolderName;
            try
            {
                File picturesDirectory = Environment.GetExternalStoragePublicDirectory(Environment.DirectoryPictures);
                File folderDirectory = picturesDirectory;

                if (!string.IsNullOrEmpty(folder))
                {
                    folderDirectory = new File(picturesDirectory, folder);
                    folderDirectory.Mkdirs();
                }

                using (File bitmapFile = new File(folderDirectory, filename))
                {
                    bitmapFile.CreateNewFile();

                    using (FileOutputStream outputStream = new FileOutputStream(bitmapFile))
                    {
                        await outputStream.WriteAsync(data);
                    }

                    // Make sure it shows up in the Photos gallery promptly.
                    MediaScannerConnection.ScanFile(Platform.CurrentActivity,
                                                    new string[] { bitmapFile.Path },
                                                    new string[] { "image/png", "image/jpeg" }, null);
                }
            }
            catch
            {
                return false;
            }

            return true;
        }


        public static void OnActivityResult(Result resultCode, Intent intent)
        {

            if ((resultCode == Result.Ok) && (intent != null))
            {
                Android.Net.Uri uri = intent.Data;
                System.IO.Stream stream = Platform.CurrentActivity.ContentResolver.OpenInputStream(uri);

                // Set the Stream as the completion of the Task
                PickImageTaskCompletionSource.SetResult(stream);
            }
            else
            {
                PickImageTaskCompletionSource.SetResult(null);
            }
        }

    }
}