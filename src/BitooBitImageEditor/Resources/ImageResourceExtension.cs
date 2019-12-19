using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace BitooBitImageEditor.Resources
{
    [ContentProperty("Source")]
    internal class ImageResourceExtension : IMarkupExtension
    {
        public const string resource = "BitooBitImageEditor.Resources.";
        public string Source { get; set; }

        public object ProvideValue(IServiceProvider serviceProvider)
        {
            if (Source == null)
            {
                return null;
            }

            var imageSource = ImageSource.FromResource($"{resource}{Source}.png");

            return imageSource;
        }
    }
}
