using BitooBitImageEditor.Croping;
using BitooBitImageEditor.ManipulationBitmap;
using BitooBitImageEditor.TouchTracking;
using SkiaSharp;
using SkiaSharp.Views.Forms;
using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using Xamarin.Forms;

namespace BitooBitImageEditor.EditorPage
{
    internal class ImageEditorViewModel : BaseNotifier
    {       
        private bool buttonsVisible = true;
        public ImageCropperCanvasView cropperCanvas;
        public TouchManipulationCanvasView mainCanvas;

        public ImageEditorViewModel(SKBitmap bitmap, ImageEditorConfig config)
        {
            Config = config;
            cropperCanvas = new ImageCropperCanvasView(bitmap, config.CropAspectRatio);
            mainCanvas = new TouchManipulationCanvasView(config);
            mainCanvas.AddBitmapToCanvas(bitmap, BitmapType.Main);
        }

        public bool CropVisible => CurrentEditType == ImageEditType.CropRotate;
        public bool MainVisible => !CropVisible;
        public bool TextVisible => CurrentEditType == ImageEditType.Text;
        public bool StickersVisible => CurrentEditType == ImageEditType.Stickers;
        public bool ButtonsVisible
        {
            get => CurrentEditType == ImageEditType.SelectType && buttonsVisible;
            set => buttonsVisible = value;
        }
            

        public ImageEditorConfig Config { get; set; }
        public ImageEditType CurrentEditType { private set; get; } = ImageEditType.SelectType;
        public Color CurrentColor { get; set; } = Color.Black;
        public string CurrentText { set; get; } = "";
        public ObservableCollection<Color> ColorCollect { get; } = SkiaHelper.GetColors();
        public ObservableCollection<CropItem> CropCollect { get; set; } = CropItem.GetCropItems();


        public ICommand ApplyChangesCommand => new Command<string>((value) =>
        {
            if (!string.IsNullOrWhiteSpace(value) && value.ToLower() == "apply")
            {
                switch (CurrentEditType)
                {
                    case ImageEditType.Text:
                        mainCanvas.AddBitmapToCanvas(CurrentText, CurrentColor.ToSKColor());
                        CurrentText = "";
                        break;
                    case ImageEditType.CropRotate:
                        mainCanvas.AddBitmapToCanvas(cropperCanvas.CroppedBitmap, BitmapType.Main);
                        break;
                }
            }

            CurrentEditType = ImageEditType.SelectType;
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
                case SKBitmap value:
                    mainCanvas.AddBitmapToCanvas(value, BitmapType.Stickers);
                    CurrentEditType = ImageEditType.SelectType;
                    break;
                default:
                    CurrentEditType = ImageEditType.SelectType;
                    break;
            }
        });

        public ICommand EditFinishCommand => new Command<string>((value) =>
        {
            SKBitmap bitmap = null;
            if (!string.IsNullOrWhiteSpace(value) && value.ToLower() == "save")
                bitmap = mainCanvas.EditedBitmap;

            ImageEditor.Instance.SetImage(bitmap);
        });


        internal void OnTouchEffectTouchAction(object sender, TouchActionEventArgs args)
        {
            ButtonsVisible = Device.RuntimePlatform == Device.UWP || (args.Type != TouchActionType.Pressed && args.Type != TouchActionType.Entered && args.Type != TouchActionType.Moved);

            if (CurrentEditType != ImageEditType.CropRotate)
                mainCanvas?.OnTouchEffectTouchAction(sender, args);
            else
                cropperCanvas?.OnTouchEffectTouchAction(sender, args);
        }
    
    }
}
