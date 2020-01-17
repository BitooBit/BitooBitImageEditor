using BitooBitImageEditor.Croping;
using BitooBitImageEditor.Helper;
using BitooBitImageEditor.ManipulationBitmap;
using BitooBitImageEditor.TouchTracking;
using SkiaSharp;
using SkiaSharp.Views.Forms;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows.Input;
using Xamarin.Forms;

namespace BitooBitImageEditor.EditorPage
{
    internal class ImageEditorViewModel : BaseNotifier, IDisposable
    {
        private TouchManipulationBitmap currentTextBitmap = null;
        private bool buttonsVisible = true;
        internal ImageCropperCanvasView cropperCanvas;
        internal TouchManipulationCanvasView mainCanvas;

        internal ImageEditorViewModel(SKBitmap bitmap, ImageEditorConfig config)
        {
            Config = config;
            cropperCanvas = new ImageCropperCanvasView(bitmap, config.CropAspectRatio);
            mainCanvas = new TouchManipulationCanvasView(config);
            mainCanvas.AddBitmapToCanvas(bitmap, BitmapType.Main);
            mainCanvas.TextBitmapClicked += MainCanvas_TextBitmapClicked;
            ColorCollect = SkiaHelper.GetColors();
            CropCollect = CropItem.GetCropItems(config.CanChangeCropAspectRatio);

            //if (config?.Stickers != null)
            //    foreach (var a in config?.Stickers)
            //    {
            //        SKData data = SKImage.FromBitmap(a).Encode();
            //        using (Stream stream = data.AsStream())
            //        {
            //            byte[] imageData = new byte[stream.Length];
            //            stream.Read(imageData, 0, System.Convert.ToInt32(stream.Length));
            //            Sources.Add(new SKBitmapImageSource() {Bitmap = a });

            //            //Sources.Add(ImageSource.FromStream(() => new MemoryStream(imageData)));
            //        }
            //    }

            GC.Collect();
        }

        public bool CropVisible => CurrentEditType == ImageEditType.CropRotate;
        public bool MainVisible => !CropVisible;
        public bool TextVisible => CurrentEditType == ImageEditType.Text;
        public bool StickersVisible => CurrentEditType == ImageEditType.Stickers;
        public bool PaintVisible => CurrentEditType == ImageEditType.Paint;
        public bool ButtonsVisible
        {
            get => CurrentEditType == ImageEditType.SelectType && buttonsVisible;
            private set => buttonsVisible = value;
        }


        public ImageEditorConfig Config { get; private set; }
        public ImageEditType CurrentEditType { private set; get; } = ImageEditType.SelectType;
        public Color CurrentColor { get; set; } = Color.White;
        public string CurrentText { set; get; } = "";
        public ObservableCollection<Color> ColorCollect { get; private set; } 
        public ObservableCollection<CropItem> CropCollect { get; private set; }


        public ICommand ApplyChangesCommand => new Command<string>((value) =>
        {
            if (!string.IsNullOrWhiteSpace(value) && value.ToLower() == "apply")
            {
                switch (CurrentEditType)
                {
                    case ImageEditType.Text:
                        {
                            if (currentTextBitmap == null)
                                mainCanvas.AddBitmapToCanvas(CurrentText, CurrentColor.ToSKColor());
                            else
                            {
                                currentTextBitmap.Bitmap = SKBitmapBuilder.FromText(CurrentText, CurrentColor.ToSKColor());
                                currentTextBitmap.Text = CurrentText;
                                currentTextBitmap.IsHide = false;
                                mainCanvas?.InvalidateSurface();
                            }

                            currentTextBitmap = null;
                            CurrentText = "";
                        }
                        break;
                    case ImageEditType.CropRotate:
                        mainCanvas.AddBitmapToCanvas(cropperCanvas.CroppedBitmap, BitmapType.Main);
                        break;
                }
            }

            CurrentEditType = ImageEditType.SelectType;
        });

        public ICommand CancelCommand => new Command(() =>
        {
            if (CurrentEditType == ImageEditType.Paint)
                mainCanvas.DeleteEndPath();          
        });

        public ICommand SelectItemCommand => new Command<object>((valueObj) =>
        {
            switch (valueObj)
            {
                case ImageEditType value:
                    CurrentEditType = value;
                    break;
                case Color value:
                    CurrentColor = value;
                    break;
                case CropItem value:
                    cropperCanvas.SetAspectRatio(value);
                    break;
                case SKBitmapImageSource value:
                    mainCanvas.AddBitmapToCanvas(value, BitmapType.Stickers);
                    CurrentEditType = ImageEditType.SelectType;
                    break;
                default:
                    CurrentEditType = ImageEditType.SelectType;
                    break;
            }
        });


        private bool lockFinish = false;
        public ICommand EditFinishCommand => new Command<string>((value) =>
        {
            if (!lockFinish)
            {
                lockFinish = true;
                SKBitmap bitmap = null;
                if (!string.IsNullOrWhiteSpace(value) && value.ToLower() == "save")
                    bitmap = mainCanvas.EditedBitmap;

                ImageEditor.Instance.SetImage(bitmap);
            }
        });

        internal void OnTouchEffectTouchAction(object sender, TouchActionEventArgs args)
        {
            ButtonsVisible = Device.RuntimePlatform == Device.UWP || (args.Type != TouchActionType.Moved);

            if (CurrentEditType != ImageEditType.CropRotate)
                mainCanvas?.OnTouchEffectTouchAction(args, CurrentEditType, CurrentColor.ToSKColor());
            else
                cropperCanvas?.OnTouchEffectTouchAction(args);
        }

        private void MainCanvas_TextBitmapClicked(TouchManipulationBitmap value)
        {
            CurrentColor = value?.Color.ToFormsColor() ?? Color.Black;
            CurrentText = value?.Text ?? "";
            CurrentEditType = ImageEditType.Text;
            currentTextBitmap = value;
        }

        #region IDisposable Support
        private bool disposedValue = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    Config = null;
                    ColorCollect = null;
                    CropCollect = null;
                    CurrentText = null;
                }

                ((IDisposable)cropperCanvas).Dispose();
                ((IDisposable)mainCanvas).Dispose();
                currentTextBitmap?.Bitmap?.Dispose();
                currentTextBitmap = null;
                disposedValue = true;
            }
        }

        ~ImageEditorViewModel()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion

    }
}
