using BitooBitImageEditor.TouchTracking;
using SkiaSharp;
using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace BitooBitImageEditor.EditorPage
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public sealed partial class ImageEditorPage : ContentPage
    {
        private readonly ImageEditorViewModel viewModel;
        internal ImageEditorPage(SKBitmap bitmap, ImageEditorConfig config)
        {
            InitializeComponent();
            viewModel = new ImageEditorViewModel(bitmap, config);
            this.BindingContext = viewModel;
            canvasCropViewHost.Children.Add(viewModel.cropperCanvas, 0, 0);
            canvasMainViewHost.Children.Add(viewModel.mainCanvas, 0, 0);
            viewModel.PropertyChanged += ViewModel_PropertyChanged;
        }

        private void ViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ImageEditorViewModel.TextVisible))
            {
                if (viewModel?.TextVisible ?? false)
                    textEditor.Focus();
            }
        }

        protected override void OnDisappearing()
        {
            ImageEditor.Instance.SetImage();
            base.OnDisappearing();
        }

        private void TouchEffect_TouchAction(object sender, TouchActionEventArgs args) => viewModel.OnTouchEffectTouchAction(sender, args);

        public void Dispose()
        {
            viewModel.Dispose();
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }
    }
}