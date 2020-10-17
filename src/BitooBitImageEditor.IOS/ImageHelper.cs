using BitooBitImageEditor.IOS;
using Foundation;
using System;
using System.IO;
using System.Threading.Tasks;
using UIKit;
using Xamarin.Forms;

[assembly: Dependency(typeof(ImageHelper))]
namespace BitooBitImageEditor.IOS
{
    internal class ImageHelper : IImageHelper
    {
        public Task<bool> SaveImageAsync(byte[] data, string filename, string folder = null)
        {
            NSData nsData = NSData.FromArray(data);
            UIImage image = new UIImage(nsData);
            TaskCompletionSource<bool> taskCompletionSource = new TaskCompletionSource<bool>();

            image.SaveToPhotosAlbum((UIImage img, NSError error) => taskCompletionSource.SetResult(error == null));

            return taskCompletionSource.Task;
        }
    }
}