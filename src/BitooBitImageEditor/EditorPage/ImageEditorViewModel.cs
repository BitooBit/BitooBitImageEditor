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
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;

namespace BitooBitImageEditor.EditorPage
{
    internal class ImageEditorViewModel : BaseNotifier, IDisposable
    {
        private TouchManipulationBitmap currentTextBitmap = null;
        internal ImageCropperCanvasView cropperCanvas;
        internal TouchManipulationCanvasView mainCanvas;

        internal ImageEditorViewModel(SKBitmap bitmap, ImageEditorConfig config)
        {
            Config = config;
            cropperCanvas = new ImageCropperCanvasView(bitmap, config.CropAspectRatio);
            mainCanvas = new TouchManipulationCanvasView(config);
            mainCanvas.AddBitmapToCanvas(bitmap.Copy(), BitmapType.Main);
            mainCanvas.TextBitmapClicked += MainCanvas_TextBitmapClicked;
            ColorCollect = SkiaHelper.GetColors();
            CropCollect = CropItem.GetCropItems(config.CanChangeCropAspectRatio);
            Message = config?.LoadingText;
        }

        public bool CropVisible => CurrentEditType == ImageEditType.CropRotate;
        public bool MainVisible => !CropVisible;
        public bool TextVisible => CurrentEditType == ImageEditType.Text;
        public bool StickersVisible => CurrentEditType == ImageEditType.Stickers;
        public bool PaintVisible => CurrentEditType == ImageEditType.Paint && !IsMoved;
        public bool InfoVisible => CurrentEditType == ImageEditType.Info;
        public bool ButtonsVisible => CurrentEditType == ImageEditType.SelectType && !IsMoved;
        public bool IsMoved { get; set; }


        public ImageEditorConfig Config { get; private set; }
        public ImageEditType CurrentEditType { private set; get; } = ImageEditType.SelectType;
        public Color CurrentColor { get; set; } = Color.White;
        public string CurrentText { set; get; } = "";
        public string Message { private set; get; } = "";
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


        public ICommand SaveCommand => new Command<string>(async (value) =>
        {
            CurrentEditType = ImageEditType.Info;

            var bitmap = mainCanvas.EditedBitmap;
            
                if (await ImageEditor.Instance.SaveImage(SkiaHelper.SKBitmapToBytes(bitmap), $"img{DateTime.Now.ToString("dd.MM.yyyy HH-mm-ss")}.png"))
                    Message = Config?.SuccessSaveText;
                else
                    Message = Config?.ErrorSaveText;
            bitmap.Dispose();
            bitmap = null;
            GC.Collect();

            int time = (int)(Message?.Length * 75);
            await Task.Delay(time >= 1500 ? time : 1500);
            Message = Config?.LoadingText;
            CurrentEditType = ImageEditType.SelectType;
        });


        internal void OnTouchEffectTouchAction(object sender, TouchActionEventArgs args)
        {
            IsMoved = Device.RuntimePlatform != Device.UWP && (args.Type == TouchActionType.Moved);

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
