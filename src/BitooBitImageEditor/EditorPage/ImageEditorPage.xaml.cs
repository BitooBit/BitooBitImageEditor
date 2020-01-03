using BitooBitImageEditor.TouchTracking;
using SkiaSharp;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace BitooBitImageEditor.EditorPage
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public sealed partial class ImageEditorPage : ContentPage
    {

        TouchEffect touchEffect = new TouchEffect();

        public ImageEditorPage(SKBitmap bitmap, float? aspectRatio = null)
        {
            InitializeComponent();
            var viewModel = new ImageEditorViewModel(bitmap, aspectRatio);
            this.BindingContext = viewModel;
            canvasCropViewHost.Children.Add(viewModel.imageCropperCanvas, 0, 0);
            canvasMainViewHost.Children.Add(viewModel.mainCanvas, 0,0);
            //canvasPaintViewHost.Children.Add(viewModel.paintCanvasView, 0, 0);
            //canvasPaintViewHost.Children.Add(viewModel.paintCanvasView, 0, 0);
            canvasTextViewHost.Children.Add(viewModel.textCanvasView, 0, 0);
            touchEffect.TouchAction += viewModel.OnTouchEffectTouchAction;
            canvasTop.Effects.Add(touchEffect);
        }


        protected override void OnAppearing()
        {
            base.OnAppearing();
            
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            ImageEditor.Instance.SetImage();
        }

    }
}