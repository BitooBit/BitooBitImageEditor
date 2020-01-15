using BitooBitImageEditor.EditorPage;
using SkiaSharp;
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
#pragma warning restore CS1591 // Отсутствует комментарий XML для открытого видимого типа или члена

    /// <summary>сonfigurator image editor</summary>
    public sealed class ImageEditorConfig : BaseNotifier, IDisposable
    {
        private BackgroundType backgroundType = BackgroundType.Transparent;
        private Aspect aspect = Aspect.AspectFit;

        /// <summary>constructor with default values</summary>
        public ImageEditorConfig() { }

        /// <summary></summary>
        public ImageEditorConfig(bool canAddText = true, bool canFingerPaint = true, float? cropAspectRatio = null,
                                 List<SKBitmap> stickers = null, int? outImageHeight = null, int? outImageWidht = null, Aspect aspect = Aspect.AspectFit,
                                 BackgroundType backgroundType = BackgroundType.Transparent, SKColor backgroundColor = default)
        {
            CanAddText = canAddText;
            CanFingerPaint = canFingerPaint;
            Stickers = stickers;
            CropAspectRatio = cropAspectRatio;
            Aspect = aspect;
            BackgroundType = backgroundType;
            BackgroundColor = backgroundColor;
            SetOutImageSize(outImageHeight, outImageWidht);
        }

        /// <summary>determines whether the user can add text to image</summary>
        public bool CanAddText { get; set; } = true;

        /// <summary>determines whether the user can draw the image with their finger.</summary>
        public bool CanFingerPaint { get; set; } = true;

        /// <summary>sets and returns the aspect ratio for cropping the image </summary>
        public float? CropAspectRatio { get; set; } = null;

        /// <summary>sets a set of stickers.</summary>
        public List<SKBitmap> Stickers { get; set; } = null;

        /// <summary> get a height out image</summary>
        public int? OutImageHeight { get; private set; }

        /// <summary> get a widht out image</summary>
        public int? OutImageWidht { get; private set; }

        /// <summary>sets and returns the background color </summary>
        public SKColor BackgroundColor { get; set; } = default;

        /// <summary>Defines the background type</summary>
        public BackgroundType BackgroundType
        {
            get => IsOutImageAutoSize ? BackgroundType.Transparent : backgroundType;
            set => backgroundType = value;
        }

        /// <summary>Determines how the image is displayed</summary>
        public Aspect Aspect
        {
            get => IsOutImageAutoSize ? Aspect.AspectFit : aspect;
            set => aspect = value;
        }

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
            else
            {
                OutImageHeight = height < 3000 ? height : 3000;
                OutImageWidht = widht < 3000 ? widht : 3000;
            }
        }

        /// <summary>returns a copy of the ImageEditorConfig instance</summary><returns></returns>
        public ImageEditorConfig Clone()
        {
            return (ImageEditorConfig)this.MemberwiseClone();
        }

        #region IDisposable Support
        private bool disposedValue = false;

        private void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                { }
                if (Stickers != null)
                    foreach (var a in Stickers)
                    {
                        a.Dispose();
                    }
                Stickers = null;
            }
        }

        #pragma warning disable CS1591 // Отсутствует комментарий XML для открытого видимого типа или члена
        ~ImageEditorConfig()
        #pragma warning restore CS1591 // Отсутствует комментарий XML для открытого видимого типа или члена
        {
            Dispose(false);
        }

        /// <summary>Releases the unmanaged resources</summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion

    }
}
