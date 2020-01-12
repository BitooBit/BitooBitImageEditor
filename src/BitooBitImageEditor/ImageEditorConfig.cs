using BitooBitImageEditor.EditorPage;
using SkiaSharp;
using System.Collections.Generic;
using Xamarin.Forms;

namespace BitooBitImageEditor
{
    public enum BackgroundType
    {
        Transparent,
        Color,
        StretchedImage
    }

    public class ImageEditorConfig : BaseNotifier
    {

        private BackgroundType backgroundType = BackgroundType.Transparent;
        private Aspect aspect = Aspect.AspectFit;


        public ImageEditorConfig() { }

        public ImageEditorConfig(bool canAddText = true, bool canFingerPaint = true, bool canCropRotate = true, float? cropAspectRatio = null, 
                                 List<SKBitmap> stickers = null, int? outImageHeight = null, int? outImageWidht = null, Aspect aspect = Aspect.AspectFit, 
                                 BackgroundType backgroundType = BackgroundType.Transparent, SKColor backgroundColor = default)
        {
            CanAddText = canAddText;
            CanFingerPaint = canFingerPaint;
            CanCropRotate = canCropRotate;
            CropAspectRatio = cropAspectRatio;
            Stickers = stickers;
            Aspect = aspect;            
            BackgroundType = backgroundType;
            BackgroundColor = backgroundColor;
            SetOutWidthHeight(outImageHeight, outImageWidht);
        }


        public bool CanAddText { get; set; } = true;
        public bool CanFingerPaint { get; set; } = true;
        public bool CanCropRotate { get; set; } = true;
        public float? CropAspectRatio { get; set; } = null;
        public List<SKBitmap> Stickers { get; set; } = null;
        public int? OutImageHeight { get; private set; } = null;
        public int? OutImageWidht { get; private set; } = null;
        public SKColor BackgroundColor { get; private set; } = default;

        public BackgroundType BackgroundType
        {
            get => OutImageAutoSize ? BackgroundType.Transparent : backgroundType;
            set => backgroundType = value;
        }
        public Aspect Aspect
        {
            get => OutImageAutoSize ? Aspect.AspectFit : aspect;
            set => aspect = value;
        }

        public bool CanChangeCropAspectRatio => CropAspectRatio == null;
        public bool CanAddStickers => Stickers?.Count > 0;
        public bool OutImageAutoSize => OutImageHeight == null || OutImageWidht == null;

        public void SetOutWidthHeight(int? outImageHeight = null, int? outImageWidht = null)
        {
            if(outImageHeight == null || outImageWidht == null)
            {
                OutImageHeight = null;
                OutImageWidht = null;
            }
            else
            {
                OutImageHeight = outImageHeight;
                OutImageWidht = outImageWidht;
            }
        }

        public ImageEditorConfig Clone()
        {
            return (ImageEditorConfig)this.MemberwiseClone();
        }

    }
}
