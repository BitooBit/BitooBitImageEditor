using BitooBitImageEditor;
using System;
using System.IO;
using Xamarin.Forms;

namespace SampleImageEditor
{


    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
            this.BindingContext = this;
        }


        private byte[] data;

        private async void GetEditedImage_Clicked(object sender, EventArgs e)
        {
            byte[] data = await ImageEditor.Instance.GetEditedImage();
            this.data = data;
            if (data != null)
            {
                MyImage.Source = ImageSource.FromStream(() => new MemoryStream(data));
            }
        }


        private async void SaveImage_Clicked(object sender, EventArgs e)
        {
            string message;
            if (data != null)
            {
                if (await ImageEditor.Instance.ImageHelper.SaveImageAsync(data, $"img{DateTime.Now.ToString()}.png"))
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
