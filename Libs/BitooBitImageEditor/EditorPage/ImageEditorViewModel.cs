using BitooBitImageEditor.Croping;
using BitooBitImageEditor.Text;
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
            //,new MenuItem("gesture", ImageEditType.Paint, ActionEditType.SetCurrentType)
        };

        private readonly ObservableCollection<MenuItem> cropCollect = new ObservableCollection<MenuItem>
        {
             new MenuItem("rotate_right", ImageEditType.CropRotate, ActionEditType.CropRotate)
            ,new MenuItem("crop_full", ImageEditType.CropRotate, ActionEditType.CropFull)
            ,new MenuItem("crop_free", ImageEditType.CropRotate, ActionEditType.CropFree)
            ,new MenuItem("crop_square", ImageEditType.CropRotate, ActionEditType.CropSquare)
        };

        
        public ImageCropperCanvasView imageCropperCanvas;
        public PaintCanvasView paintCanvasView;
        public SKCanvasView mainCanvas;
        public SKBitmap originalBitmap;
        private SKBitmap editedBitmap;
        public TextCanvasView textCanvasView;


        public SKBitmap EditedBitmap
        {
            get => editedBitmap;
            set
            {
                editedBitmap = value;
                if(textCanvasView != null)
                    textCanvasView.SetBitmap(value);
            }
        }


        public ImageEditorViewModel(SKBitmap bitmap, float? aspectRatio = null)
        {
            originalBitmap = bitmap;
            EditedBitmap = bitmap;
            imageCropperCanvas = new ImageCropperCanvasView(bitmap, aspectRatio);
            imageCropperCanvas.Margin = 8;
            mainCanvas = new SKCanvasView();
            mainCanvas.PaintSurface += MainCanvas_PaintSurface;
            mainCanvas.Margin = 8;
            paintCanvasView = new PaintCanvasView(EditedBitmap);
            textCanvasView = new TextCanvasView(EditedBitmap);
            textCanvasView.Margin = 8;
        }

        

        public ImageEditType CurrentEditType { private set; get; } = ImageEditType.SelectType;
        public Color CurrentColor { private set; get; }
        public bool ColorsCollectVisible => CurrentEditType == ImageEditType.Paint || CurrentEditType == ImageEditType.Text;
        public bool MenuCollectVisible => !ColorsCollectVisible;
        public bool FinishVisible => CurrentEditType == ImageEditType.SelectType;
        public bool ApplyChangesVisible => !FinishVisible;
        public bool TextVisible => CurrentEditType == ImageEditType.Text;

        public bool CropVisible => CurrentEditType == ImageEditType.CropRotate;
        public bool PaintVisible => CurrentEditType == ImageEditType.Paint;

        public bool MainVisible => CurrentEditType == ImageEditType.Paint || CurrentEditType == ImageEditType.SelectType || CurrentEditType == ImageEditType.Text;

        private string text = "";
        public string Text
        {
            get => text;
            set
            {
                text = value;
                textCanvasView.Text = value;
            }
        }



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
            SKBitmap bitmap = originalBitmap;
            if(value == "Save")
                bitmap = EditedBitmap;

            ImageEditor.Instance.SetImage(bitmap);
        });

        public ICommand ApplyChangesCommand => new Command<string>((value) =>
        {
            if (value == "Cancel")
            {
                if(CurrentEditType == ImageEditType.Paint)
                {
                    paintCanvasView.completedPaths.Clear();
                    paintCanvasView.inProgressPaths.Clear();
                    paintCanvasView.InvalidateSurface();
                }
                if (CurrentEditType == ImageEditType.Text)
                {
                    textCanvasView.Text = "";
                }
            }
            else
            {                             
                if (CurrentEditType == ImageEditType.CropRotate)
                {
                    EditedBitmap = imageCropperCanvas.CroppedBitmap;
                }
                if (CurrentEditType == ImageEditType.Text)
                {
                    EditedBitmap = textCanvasView.BitmapWidthText;
                }

                mainCanvas.InvalidateSurface();
            }

            CurrentEditType = ImageEditType.SelectType;
        });

        public ICommand SelectColorCommand => new Command<Color>((value) =>
        {
            CurrentColor = value;
            paintCanvasView.CurrentColor = value.ToSKColor();
            textCanvasView.CurrentColor = value.ToSKColor();
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



    }
}
