using BitooBitImageEditor.Croping;
using BitooBitImageEditor.ManipulationBitmap;
using BitooBitImageEditor.TouchTracking;
using SkiaSharp;
using SkiaSharp.Views.Forms;
using System.Collections.ObjectModel;
using System.Windows.Input;
using Xamarin.Forms;

namespace BitooBitImageEditor.EditorPage
{
    internal class ImageEditorViewModel : BaseNotifier
    {
        private readonly ObservableCollection<MenuItem> typesCollect = new ObservableCollection<MenuItem>
        {
             new MenuItem("crop_rotate", ImageEditType.CropRotate, ActionEditType.SetCurrentType)
            ,new MenuItem("format_shapes", ImageEditType.Text, ActionEditType.SetCurrentType)
            ,new MenuItem("gesture", ImageEditType.Paint, ActionEditType.SetCurrentType)
        };

        private readonly ObservableCollection<MenuItem> cropCollect = new ObservableCollection<MenuItem>
        {
             new MenuItem("rotate_right", ImageEditType.CropRotate, ActionEditType.CropRotate)
            ,new MenuItem("crop_full", ImageEditType.CropRotate, ActionEditType.CropFull)
            ,new MenuItem("crop_free", ImageEditType.CropRotate, ActionEditType.CropFree)
            ,new MenuItem("crop_square", ImageEditType.CropRotate, ActionEditType.CropSquare)
        };


        public ImageCropperCanvasView imageCropperCanvas;
        public TouchManipulationCanvasView mainCanvas;
        public SKBitmap originalBitmap;



        public ImageEditorViewModel(SKBitmap bitmap, float? aspectRatio = null)
        {
            originalBitmap = bitmap;
            EditedBitmap = bitmap;
            imageCropperCanvas = new ImageCropperCanvasView(bitmap, aspectRatio);
            mainCanvas = new TouchManipulationCanvasView();
            //mainCanvas.PaintSurface += MainCanvas_PaintSurface;
            imageCropperCanvas.Margin = mainCanvas.Margin = 0;

        }


        public bool CropVisible => CurrentEditType == ImageEditType.CropRotate;
        public bool MainVisible => !CropVisible;
        public bool ColorsCollectVisible => CurrentEditType == ImageEditType.Paint || CurrentEditType == ImageEditType.Text;
        public bool MenuCollectVisible => !ColorsCollectVisible;
        public bool TextVisible => CurrentEditType == ImageEditType.Text;
        public bool FinishVisible => CurrentEditType == ImageEditType.SelectType;
        public bool ApplyChangesVisible => !FinishVisible;



        public ImageEditType CurrentEditType { private set; get; } = ImageEditType.SelectType;
        public Color CurrentColor { set; get; }
        public string CurrentText { set; get; }
        public SKBitmap EditedBitmap { get; set; }



        public ObservableCollection<MenuItem> ItemsCollect
        {
            get
            {
                switch (CurrentEditType)
                {
                    case ImageEditType.SelectType:
                        return typesCollect;
                    case ImageEditType.CropRotate:
                        return cropCollect;
                    default:
                        return null;
                }
            }
        }

        public ObservableCollection<Color> ColorCollect { get; } = new ObservableCollection<Color>
        {
             Color.White
            ,Color.Red
            ,Color.Orange
            ,Color.Yellow
            ,Color.Green
            ,Color.Cyan
            ,Color.Blue
            ,Color.Violet
            ,Color.Black
        };


        public ICommand EditFinishCommand => new Command<string>((value) =>
        {
            SKBitmap bitmap = null;
            if (value == "Save")
                bitmap = EditedBitmap;

            ImageEditor.Instance.SetImage(bitmap);
        });

        public ICommand ApplyChangesCommand => new Command(() =>
        {           
            if(CurrentEditType == ImageEditType.Text)
            {
                mainCanvas.AddTextToCanvas(CurrentText, CurrentColor.ToSKColor());
                CurrentColor = Color.Black;
                CurrentText = "";
            }


            CurrentEditType = ImageEditType.SelectType;
        });

        public ICommand SelectColorCommand => new Command<Color>((value) =>
        {
            CurrentColor = value;
        });

        public ICommand SelectItemCommand => new Command<MenuItem>((value) =>
        {
            if (CurrentEditType == value.Type || value.Action == ActionEditType.SetCurrentType)
            {
                switch (value.Action)
                {
                    case ActionEditType.SetCurrentType:
                        CurrentEditType = value.Type;
                        break;

                    case ActionEditType.CropRotate:
                        imageCropperCanvas.Rotate();
                        break;
                    case ActionEditType.CropFree:
                        imageCropperCanvas.SetAspectRatio(null);
                        break;
                    case ActionEditType.CropFull:
                        imageCropperCanvas.SetAspectRatio(null, true);
                        break;
                    case ActionEditType.CropSquare:
                        imageCropperCanvas.SetAspectRatio(1f);
                        break;
                }
            }
        });

        private void MainCanvas_PaintSurface(object sender, SKPaintSurfaceEventArgs args)
        {
            args.Surface.Canvas.Clear();
            var rect = SkiaHelper.CalculateRectangle(args.Info, EditedBitmap);
            args.Surface.Canvas.DrawBitmap(EditedBitmap, rect.rect);
        }


        internal void OnTouchEffectTouchAction(object sender, TouchActionEventArgs args)
        {
            if(CurrentEditType != ImageEditType.CropRotate)
                mainCanvas?.OnTouchEffectTouchAction(sender, args);
            else
                imageCropperCanvas?.OnTouchEffectTouchAction(sender, args);
        }





    }
}
