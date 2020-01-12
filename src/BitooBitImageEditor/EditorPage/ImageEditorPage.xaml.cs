using BitooBitImageEditor.TouchTracking;
using SkiaSharp;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace BitooBitImageEditor.EditorPage
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public sealed partial class ImageEditorPage : ContentPage
    {
        readonly ImageEditorViewModel viewModel;
        internal ImageEditorPage(SKBitmap bitmap, ImageEditorConfig config)
        {
            InitializeComponent();
            viewModel = new ImageEditorViewModel(bitmap, config);
            this.BindingContext = viewModel;
            canvasCropViewHost.Children.Add(viewModel.imageCropperCanvas, 0, 0);
            canvasMainViewHost.Children.Add(viewModel.mainCanvas, 0,0);
        }

        protected override void OnDisappearing()
        {
            ImageEditor.Instance.SetImage();
            base.OnDisappearing();
        }

        private void TouchEffect_TouchAction(object sender, TouchActionEventArgs args) =>  viewModel.OnTouchEffectTouchAction(sender, args);
        
    }
}