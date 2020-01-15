using BitooBitImageEditor;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Windows.Input;
using Xamarin.Forms;

namespace SampleImageEditor
{
    public partial class MainPage : ContentPage
    {
        private byte[] data;
        Assembly assembly;
        List<SKBitmap> stickers;
        public MainPage()
        {
            InitializeComponent();
            assembly = GetType().GetTypeInfo().Assembly;
            this.BindingContext = this;
            GetBitmaps();
        }

        public bool ConfigVisible { get; set; }
        public ImageEditorConfig Config { get; set; } = new ImageEditorConfig();
        public bool CanAddStickers { get; set; } = true;
        public int? OutImageHeight { get; set; } = null;
        public int? OutImageWidht { get; set; } = null;
        public bool UseSampleImage { get; set; } = true;

        public List<Aspect> Aspects { get; } = new List<Aspect> { Aspect.AspectFill, Aspect.AspectFit, Aspect.Fill };
        public List<BackgroundType> BackgroundTypes { get; } = new List<BackgroundType> { BackgroundType.Transparent, BackgroundType.StretchedImage, BackgroundType.Color };
        public List<SKColor> Colors { get; } = new List<SKColor> { SKColors.Red, SKColors.Green, SKColors.Blue };

        private async void GetEditedImage_Clicked(object sender, EventArgs e)
        {
            try
            {
                Config.Stickers = CanAddStickers ? stickers : null;
                Config.SetOutImageSize(OutImageHeight, OutImageWidht);

                SKBitmap bitmap = null;
                if (UseSampleImage)
                    using (Stream stream = assembly.GetManifestResourceStream("SampleImageEditor.Resources.sample.png"))
                        bitmap = SKBitmap.Decode(stream);

                byte[] data = await ImageEditor.Instance.GetEditedImage(bitmap, Config);
                this.data = data;
                if (data != null)
                {
                    MyImage.Source = null;
                    MyImage.Source = ImageSource.FromStream(() => new MemoryStream(data));
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("", ex.Message, "fewf");
            }
        }


        private async void SaveImage_Clicked(object sender, EventArgs e)
        {
            string message;
            if (data != null)
            {
                if (await ImageEditor.Instance.SaveImage(data, $"img{DateTime.Now.ToString("dd.MM.yyyy HH-mm-ss")}.png"))
                    message = "Successfully!!!";
                else
                    message = "Unsuccessfully!!!";
            }
            else
                message = "You should edit the image";
            await DisplayAlert("", message, "Ok");
        }

        private void SetConfig_Clicked(object sender, EventArgs e)
        {
            ConfigVisible = !ConfigVisible;
        }

        private void GetBitmaps()
        {
            List<SKBitmap> _stickers = null;

            string[] resourceIDs = assembly.GetManifestResourceNames();

            foreach (string resourceID in resourceIDs)
            {
                if (resourceID.Contains("sticker") && resourceID.EndsWith(".png"))
                {
                    if (_stickers == null)
                        _stickers = new List<SKBitmap>();

                    using (Stream stream = assembly.GetManifestResourceStream(resourceID))
                    {
                        _stickers.Add(SKBitmap.Decode(stream));
                    }
                }
            }
            stickers = _stickers;
        }
    }
}
