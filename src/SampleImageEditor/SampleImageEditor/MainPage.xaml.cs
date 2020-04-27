using BitooBitImageEditor;
using SkiaSharp;
using SkiaSharp.Views.Forms;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace SampleImageEditor
{
    public partial class MainPage : ContentPage
    {
        private readonly Assembly assembly;
        private List<SKBitmapImageSource> stickers;
        private int stickersCount = 15;
        public MainPage()
        {
            InitializeComponent();
            assembly = GetType().GetTypeInfo().Assembly;
            this.BindingContext = this;
            var display = DeviceDisplay.MainDisplayInfo;
            Config = new ImageEditorConfig(backgroundType: BackgroundType.StretchedImage, outImageHeight: (int)display.Height, outImageWidht: (int)display.Width, aspect: BBAspect.Auto);
        }

        public bool ConfigVisible { get; set; }
        public ImageEditorConfig Config { get; set; } = new ImageEditorConfig();
        public bool CanAddStickers { get; set; } = false;
        public int? OutImageHeight { get; set; } = null;
        public int? OutImageWidht { get; set; } = null;
        public bool UseSampleImage { get; set; } = true;

        public List<BBAspect> Aspects { get; } = new List<BBAspect> { BBAspect.Auto, BBAspect.AspectFill, BBAspect.AspectFit, BBAspect.Fill };
        public List<BackgroundType> BackgroundTypes { get; } = new List<BackgroundType> { BackgroundType.Transparent, BackgroundType.StretchedImage, BackgroundType.Color };
        public List<SKColor> Colors { get; } = new List<SKColor> { SKColors.Red, SKColors.Green, SKColors.Blue };

        private async void GetEditedImage_Clicked(object sender, EventArgs e)
        {
            if (!(Config?.Stickers?.Count > 0) && CanAddStickers)
                GetBitmaps(stickersCount);

            try
            {
                Config.Stickers = CanAddStickers ? stickers : null;
                Config.SetOutImageSize(OutImageHeight, OutImageWidht);

                SKBitmap bitmap = null;
                if (UseSampleImage)
                    using (Stream stream = assembly.GetManifestResourceStream("SampleImageEditor.Resources.sample.png"))
                        bitmap = SKBitmap.Decode(stream);

                byte[] data = await ImageEditor.Instance.GetEditedImage(bitmap, Config);             
                if (data != null)
                {
                    MyImage.Source = null;
                    MyImage.Source = ImageSource.FromStream(() => new MemoryStream(data));
                }
                data = null;
            }
            catch (Exception ex)
            {
                await DisplayAlert("", ex.Message, "fewf");
            }
        }


        private void SetConfig_Clicked(object sender, EventArgs e)
        {
            ConfigVisible = !ConfigVisible;
        }

        private void Clean_Clicked(object sender, EventArgs e)
        {
            Config.DisposeStickers();
            MyImage.Source = null;
            GC.Collect();
        }

        private void GetBitmaps(int maxCount)
        {
            List<SKBitmapImageSource> _stickers = null;

            string[] resourceIDs = assembly.GetManifestResourceNames();
            int i = 0;
            foreach (string resourceID in resourceIDs)
            {
                if (resourceID.Contains("sticker") && resourceID.EndsWith(".png"))
                {
                    if (_stickers == null)
                        _stickers = new List<SKBitmapImageSource>();

                    using (Stream stream = assembly.GetManifestResourceStream(resourceID))
                    {
                        _stickers.Add(SKBitmap.Decode(stream));
                    }
                }
                i++;
                if (i > maxCount)
                    break;
            }
            stickers = _stickers;
        }
   
    }
}
