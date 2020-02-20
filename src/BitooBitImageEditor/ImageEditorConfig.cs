using BitooBitImageEditor.EditorPage;
using SkiaSharp;
using SkiaSharp.Views.Forms;
using System;
using System.Collections.Generic;
using Xamarin.Forms;

namespace BitooBitImageEditor
{
#pragma warning disable CS1591 // Отсутствует комментарий XML для открытого видимого типа или члена
    /// <summary>Defines the background type</summary>
    public enum BackgroundType
    {
        Transparent,
        Color,
        StretchedImage
    }

    /// <summary>Determines how the image is displayed. </summary>
    public enum BBAspect
    {
        AspectFit = 0,
        AspectFill = 1,
        Fill = 2,
        Auto = 3
    }

    /// <summary>сonfigurator image editor</summary>
    public sealed class ImageEditorConfig : BaseNotifier
    {
        private float? сropAspectRatio;
        public const int maxPixels = 3000;


        public const string _loadingText = "Wait";
        public const string _successSaveText = "Success";
        public const string _errorSaveText = "Error";


#pragma warning restore CS1591

        /// <summary>constructor with default values</summary>
        public ImageEditorConfig() { }

        /// <summary></summary>
        public ImageEditorConfig(bool canAddText = true, bool canFingerPaint = true, bool canTransformMainBitmap = true, float? cropAspectRatio = null,
                                 List<SKBitmapImageSource> stickers = null, int? outImageHeight = null, int? outImageWidht = null, BBAspect aspect = BBAspect.Auto,
                                 BackgroundType backgroundType = BackgroundType.Transparent, SKColor backgroundColor = default, 
                                 bool canSaveImage = true, string loadingText = _loadingText, string successSaveText = _successSaveText, string errorSaveText = _errorSaveText)
        {
            CanAddText = canAddText;
            CanFingerPaint = canFingerPaint;
            CanTransformMainBitmap = canTransformMainBitmap;
            Stickers = stickers;
            CropAspectRatio = cropAspectRatio;
            Aspect = aspect;
            BackgroundType = backgroundType;
            BackgroundColor = backgroundColor;
            CanSaveImage = canSaveImage;
            LoadingText = loadingText;
            SuccessSaveText = successSaveText;
            ErrorSaveText = errorSaveText;
            SetOutImageSize(outImageHeight, outImageWidht);
        }

        /// <summary>determines whether the user can add text to image</summary>
        public bool CanAddText { get; set; } = true;

        /// <summary>determines whether the user can draw the image with their finger.</summary>
        public bool CanFingerPaint { get; set; } = true;

        /// <summary></summary>
        public bool CanTransformMainBitmap { get; set; } = true;

        /// <summary>determines whether the user can save imge.</summary>
        public bool CanSaveImage { get; set; } = true;

        /// <summary> </summary>
        public string LoadingText { get; set; } = _loadingText;
        /// <summary> </summary>
        public string SuccessSaveText { get; set; } = _successSaveText;
        /// <summary> </summary>
        public string ErrorSaveText { get; set; } = _errorSaveText;

        /// <summary>sets and returns the aspect ratio for cropping the image </summary>
        public float? CropAspectRatio
        {
            get => сropAspectRatio;
            set => сropAspectRatio = value <= 0 ? null : value;
        }

        /// <summary>sets a set of stickers.
        /// <para>do not use a large number of stickers this will lead to a large consumption of RAM</para>
        /// <para>use the <see cref="ImageEditorConfig.DisposeStickers"/> method when stickers are no longer needed</para>
        /// </summary>
        public List<SKBitmapImageSource> Stickers { get; set; } = null;

        /// <summary> get a height out image</summary>
        public int? OutImageHeight { get; private set; }

        /// <summary> get a widht out image</summary>
        public int? OutImageWidht { get; private set; }

        /// <summary>sets and returns the background color </summary>
        public SKColor BackgroundColor { get; set; } = default;

        /// <summary>Defines the background type</summary>
        public BackgroundType BackgroundType { get; set; } = BackgroundType.StretchedImage;

        /// <summary>Determines how the image is displayed</summary>
        public BBAspect Aspect { get; set; } = BBAspect.Auto;

        /// <summary>determines whether the user can change the aspect ratio when cropping an image </summary>
        public bool CanChangeCropAspectRatio => CropAspectRatio == null;

        /// <summary>determines whether the user can add stickers to the image.</summary>
        public bool CanAddStickers => Stickers?.Count > 0;

        /// <summary>determines whether the size of the output image will match the size of the original cropped image </summary>
        public bool IsOutImageAutoSize => OutImageHeight == null || OutImageWidht == null;

        /// <summary>sets the size of the out image</summary> <param name="height"> out height </param> <param name="widht"> out widht </param>
        public void SetOutImageSize(int? height = null, int? widht = null)
        {
            if (height == null || widht == null || height < 1 || widht < 1)
            {
                OutImageHeight = null;
                OutImageWidht = null;
            }
            else if (height > maxPixels || widht > maxPixels)
            {
                double outAspect = (double)widht / (double)height;
                OutImageHeight = widht > height ? (int)(maxPixels / outAspect) : maxPixels;
                OutImageWidht = widht > height ? maxPixels : (int)(maxPixels * outAspect);
            }
            else
            {
                OutImageHeight = height;
                OutImageWidht = widht;
            }
        }

        /// <summary>returns a copy of the ImageEditorConfig instance</summary><returns></returns>
        public ImageEditorConfig Clone()
        {
            return (ImageEditorConfig)this.MemberwiseClone();
        }

        /// <summary>use this method when <see cref="ImageEditorConfig.Stickers"/> are no longer needed</summary>
        public void DisposeStickers()
        {
            if (Stickers?.Count > 0)
            {
                foreach (var a in Stickers)
                {
                    a.Bitmap?.Dispose();
                    a.Bitmap = null;
                }
                Stickers.Clear();
                Stickers = null;
            }
        }

    }
}
