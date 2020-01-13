using BitooBitImageEditor;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Xamarin.Forms;

namespace SampleImageEditor
{


    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
            this.BindingContext = this;

            //BitooBitImageEditor.EditorPage.
        }


        private byte[] data;



        private List<SKBitmap> GetBitmaps()
        {
            List<SKBitmap> stickers = null;
            Assembly assembly = GetType().GetTypeInfo().Assembly;
            string[] resourceIDs = assembly.GetManifestResourceNames();

            foreach (string resourceID in resourceIDs)
            {
                if (resourceID.EndsWith(".png") || resourceID.EndsWith(".jpg"))
                {
                    if (stickers == null)
                        stickers = new List<SKBitmap>();

                    using (Stream stream = assembly.GetManifestResourceStream(resourceID))
                    {
                        stickers.Add(SKBitmap.Decode(stream));
                    }
                }
            }
            return stickers;
        }


        private async void GetEditedImage_Clicked(object sender, EventArgs e)
        {
            try
            {
                ImageEditorConfig config = new ImageEditorConfig(stickers: GetBitmaps(), canFingerPaint: false, backgroundType: BackgroundType.StretchedImage, backgroundColor: SKColors.Blue,
                    outImageHeight: 1000, outImageWidht: 700, aspect: Aspect.AspectFit);

                byte[] data = await ImageEditor.Instance.GetEditedImage(config: config);
                this.data = data;
                if (data != null)
                {
                    MyImage.Source = null;
                    MyImage.Source = ImageSource.FromStream(() => new MemoryStream(data));
                }
            }
            catch(Exception ex)
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

    }
}
