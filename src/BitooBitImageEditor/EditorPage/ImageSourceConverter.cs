using BitooBitImageEditor.Resources;
using SkiaSharp;
using System;
using System.Globalization;
using System.IO;
using Xamarin.Forms;

namespace BitooBitImageEditor.EditorPage
{
    internal class ImageSourceConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string name)
                return string.IsNullOrWhiteSpace(name) ? null : ImageSource.FromResource($"{ImageResourceExtension.resource}{name}.png");
            else if (value is SKBitmap bitmap)
            {
                SKData data = SKImage.FromBitmap(bitmap).Encode();
                using (Stream stream = data.AsStream())
                {
                    byte[] imageData = new byte[stream.Length];
                    stream.Read(imageData, 0, System.Convert.ToInt32(stream.Length));
                    return ImageSource.FromStream(() => new MemoryStream(imageData));
                }
            }
            else
                return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }
    }
}
