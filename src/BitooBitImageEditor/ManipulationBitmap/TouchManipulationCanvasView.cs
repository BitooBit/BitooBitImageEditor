using BitooBitImageEditor.Helper;
using BitooBitImageEditor.TouchTracking;
using SkiaSharp;
using SkiaSharp.Views.Forms;
using System;
using System.Collections.Generic;
using Xamarin.Forms;

namespace BitooBitImageEditor.ManipulationBitmap
{
    internal class TouchManipulationCanvasView : SKCanvasView, IDisposable
    {
        ImageEditorConfig config;
        private float outImageWidht;
        private float outImageHeight;
        private float widthBitmap;
        private float heightBitmap;
        SKBitmap backgroundBitmap = new SKBitmap();
        SKBitmap mainBitmap = new SKBitmap();
        SKBitmap temporaryBitmap = new SKBitmap();
        SKImageInfo info;
        SKRect rectInfo = new SKRect();

        internal List<TouchManipulationBitmap> bitmapCollection =
            new List<TouchManipulationBitmap>();

        internal Dictionary<long, TouchManipulationBitmap> bitmapDictionary =
            new Dictionary<long, TouchManipulationBitmap>();

        
        public TouchManipulationCanvasView(ImageEditorConfig config)
        {
            this.config = config;
            if (!config?.IsOutImageAutoSize ?? false)
            {
                outImageWidht = config?.OutImageWidht ?? 0;
                outImageHeight = config?.OutImageHeight ?? 0;
            }
        }


        internal SKBitmap EditedBitmap
        {
            get
            {
                SKRect outRect = new SKRect(0, 0, outImageWidht, outImageHeight);
                SKBitmap outBitmap = new SKBitmap((int)outImageWidht, (int)outImageHeight);
                var rectTranslate = SkiaHelper.CalculateRectangle(rectInfo, outImageWidht, outImageHeight);
                float scale = 1 / rectTranslate.scaleX;
                var rectMianBitmap = SkiaHelper.CalculateRectangle(outRect, widthBitmap, heightBitmap, config.Aspect).rect;

                using (SKCanvas canvas = new SKCanvas(outBitmap))
                {
                    canvas.Clear();
                    canvas.DrawBitmap(mainBitmap, backgroundBitmap, outRect, rectMianBitmap, config);
                    canvas.DrawBitmap(bitmapCollection, -rectTranslate.rect.Left, -rectTranslate.rect.Top, scale);
                }
                return outBitmap;
            }
        }

       
        internal void AddBitmapToCanvas(string text, SKColor color)
        {
            var bitmap = SKBitmapBuilder.FromText(text, color);
            if (bitmap != null)
                AddBitmapToCanvas(new TouchManipulationBitmap(bitmap, BitmapType.Text, text));

            InvalidateSurface();
        }

        internal void AddBitmapToCanvas(SKBitmap bitmap, BitmapType type)
        {
            if (bitmap != null)
            {
                if (type != BitmapType.Main)
                    AddBitmapToCanvas(new TouchManipulationBitmap(bitmap, type, null));
                else
                    SetMainBitmap(bitmap);
                
                InvalidateSurface();
            }
        }
        protected override void OnPaintSurface(SKPaintSurfaceEventArgs args)
        {
            base.OnPaintSurface(args);
            info = args.Info;
            SKCanvas canvas = args.Surface.Canvas;
            if (rectInfo.Width != info.Width || rectInfo.Height != info.Height)
            {
                rectInfo = new SKRect(0, 0, info.Width, info.Height);
                SetTemporaryBitmap();
            }
            var rectImage = SkiaHelper.CalculateRectangle(rectInfo, outImageWidht, outImageHeight).rect;

            canvas.Clear();
            canvas.DrawBitmap(temporaryBitmap, rectImage);
            canvas.DrawBitmap(bitmapCollection);
            canvas.DrawSurrounding(rectInfo, rectImage, SKColors.DarkGray.WithAlpha(190));
        }

        private void AddBitmapToCanvas(TouchManipulationBitmap bitmap)
        {
            var rectImage = SkiaHelper.CalculateRectangle(rectInfo, outImageWidht, outImageHeight).rect;
            var rectSticker = new SKRect(rectImage.Left + rectImage.Width * 0.25f, rectImage.Top + rectImage.Height * 0.25f, rectImage.Right - rectImage.Width * 0.25f, rectImage.Bottom - rectImage.Height * 0.25f);
            var rect = SkiaHelper.CalculateRectangle(rectSticker, bitmap.Bitmap.Width, bitmap.Bitmap.Height);
            bitmap.Matrix = new SKMatrix(rect.scaleX, 0, rectInfo.MidX - rect.rect.Width / 2, 0, rect.scaleY, rectInfo.MidY - rect.rect.Height / 2, 0, 0, 1);
            bitmapCollection.Add(bitmap);
        }

        private void SetMainBitmap(SKBitmap bitmap)
        {
            if (config?.IsOutImageAutoSize ?? false)
            {
                outImageWidht = bitmap.Width;
                outImageHeight = bitmap.Height;
            }
            widthBitmap = bitmap.Width;
            heightBitmap = bitmap.Height;

            if (config.BackgroundType == BackgroundType.StretchedImage)
            {
                backgroundBitmap = new SKBitmap(CalcBackgraundBitmapsize(widthBitmap), CalcBackgraundBitmapsize(heightBitmap));
                bitmap.ScalePixels(backgroundBitmap, SKFilterQuality.High);
            }

            mainBitmap = bitmap;
            SetTemporaryBitmap();
        }

        private void SetTemporaryBitmap()
        {
            var rectImage = SkiaHelper.CalculateRectangle(rectInfo, outImageWidht, outImageHeight).rect;
            SKRect outRect = new SKRect(0, 0, rectImage.Width, rectImage.Height);
            var rectMianBitmap = SkiaHelper.CalculateRectangle(outRect, widthBitmap, heightBitmap, config.Aspect).rect;

            SKBitmap outBitmap = new SKBitmap((int)rectImage.Width, (int)rectImage.Height);
            using (SKCanvas canvas = new SKCanvas(outBitmap))
            {
                canvas.Clear();
                canvas.DrawBitmap(mainBitmap, backgroundBitmap, outRect, rectMianBitmap, config);
            }
            temporaryBitmap = outBitmap;
            GC.Collect(0);
        }


        internal void OnTouchEffectTouchAction(object sender, TouchActionEventArgs args)
        {
            Point pt = args.Location;
            SKPoint point = new SKPoint((float)(CanvasSize.Width * pt.X / Width),
                            (float)(CanvasSize.Height * pt.Y / Height));

            switch (args.Type)
            {
                case TouchActionType.Pressed:
                    if (!bitmapDictionary.ContainsKey(args.Id))
                        for (int i = bitmapCollection.Count - 1; i >= 0; i--)
                        {
                            TouchManipulationBitmap bitmap = bitmapCollection[i];

                            int testResult = bitmap.HitTest(point, rectInfo);
                            if (testResult != -1)
                            {
                                if (Device.RuntimePlatform == Device.UWP)
                                    bitmap.TouchManager.Mode = testResult == 4 ? TouchManipulationMode.ScaleRotate : TouchManipulationMode.ScaleDualRotate; 

                                bitmapCollection.Remove(bitmap);
                                bitmapCollection.Add(bitmap);

                                // Do the touch processing
                                bitmapDictionary.Add(args.Id, bitmap);
                                bitmap.ProcessTouchEvent(args.Id, args.Type, point);
                                InvalidateSurface();
                                break;
                            }
                        }

                    break;

                case TouchActionType.Moved:
                    if (bitmapDictionary.ContainsKey(args.Id))
                    {
                        TouchManipulationBitmap bitmap = bitmapDictionary[args.Id];
                        bitmap.ProcessTouchEvent(args.Id, args.Type, point);
                        InvalidateSurface();
                    }
                    break;

                case TouchActionType.Released:
                case TouchActionType.Cancelled:
                    if (bitmapDictionary.ContainsKey(args.Id))
                    {
                        TouchManipulationBitmap bitmap = bitmapDictionary[args.Id];
                        bitmap.ProcessTouchEvent(args.Id, args.Type, point);
                        bitmapDictionary.Remove(args.Id);
                        InvalidateSurface();
                    }
                    break;
            }

        }

        private int CalcBackgraundBitmapsize(float value)
        {
            int _value = (int)(value * 0.008f);
            return _value > 2 ? _value : 2;
        }


        #region IDisposable Support
        private bool disposedValue = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    config = null;
                    bitmapDictionary = null;
                    bitmapCollection = null;
                }

                ((IDisposable)backgroundBitmap).Dispose();
                ((IDisposable)mainBitmap).Dispose();
                ((IDisposable)temporaryBitmap).Dispose();
               
                disposedValue = true;
            }
        }

        ~TouchManipulationCanvasView()
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
