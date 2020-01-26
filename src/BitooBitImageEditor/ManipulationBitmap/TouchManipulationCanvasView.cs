using BitooBitImageEditor.EditorPage;
using BitooBitImageEditor.Helper;
using BitooBitImageEditor.Resources;
using BitooBitImageEditor.TouchTracking;
using SkiaSharp;
using SkiaSharp.Views.Forms;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace BitooBitImageEditor.ManipulationBitmap
{
    internal class TouchManipulationCanvasView : /*SKCanvasView*/ SKGLView, IDisposable
    {
        private ImageEditorConfig config;
        private float outImageWidht;
        private float outImageHeight;
        private SKBitmap backgroundBitmap;
        private TouchManipulationBitmap mainBitmap;
        private SKPoint previousTouchPoint = new SKPoint(0, 0);
        private SKRect rectInfo = new SKRect();
        private SKRect rectTrash = new SKRect();

        private List<TouchManipulationBitmap> bitmapCollection = new List<TouchManipulationBitmap>();
        private Dictionary<long, TouchManipulationBitmap> bitmapDictionary = new Dictionary<long, TouchManipulationBitmap>();

        private Dictionary<long, PaintedPath> inProgressPaths = new Dictionary<long, PaintedPath>();
        private List<PaintedPath> completedPaths = new List<PaintedPath>();

        internal event Action<TouchManipulationBitmap> TextBitmapClicked;
        internal event Action<bool, bool, bool> TrashEnabled;

        public TouchManipulationCanvasView(ImageEditorConfig config)
        {
            this.config = config;
            if (!config?.IsOutImageAutoSize ?? false)
            {
                outImageWidht = config?.OutImageWidht ?? 0;
                outImageHeight = config?.OutImageHeight ?? 0;
            }
            rectInfo = new SKRect(0, 0, CanvasSize.Width, CanvasSize.Height);            
        }

        internal SKBitmap EditedBitmap
        {
            get
            {
                SKBitmap outBitmap = new SKBitmap((int)outImageWidht, (int)outImageHeight);
                var rectTranslate = SkiaHelper.CalculateRectangle(rectInfo, outImageWidht, outImageHeight);

                using (SKCanvas canvas = new SKCanvas(outBitmap))
                {
                    canvas.Clear();
                    OnPaintSurface(canvas, new SKRect(0, 0, outImageWidht, outImageHeight), false, -rectTranslate.rect.Left, -rectTranslate.rect.Top, 1 / rectTranslate.scaleX);
                }

                return outBitmap;
            }
        }
           
        internal void OnTouchEffectTouchAction(TouchActionEventArgs args, ImageEditType editType, SKColor color)
        {
            Point pt = args.Location;
            SKPoint point = new SKPoint((float)(CanvasSize.Width * pt.X / Width), (float)(CanvasSize.Height * pt.Y / Height));
            if(editType != ImageEditType.Paint)
                OnTouchBitmapEffectAction(args, point);
            else
                OnTouchPathEffectAction(args, point, color);

            previousTouchPoint = point;
        }

        internal void AddBitmapToCanvas(string text, SKColor color)
        {
            var bitmap = SKBitmapBuilder.FromText(text, color);
            if (bitmap != null)
                AddBitmapToCanvas(new TouchManipulationBitmap(bitmap, BitmapType.Text, text, color));
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

        internal void DeleteEndPath()
        {
            if (completedPaths?.Count > 0)
            {
                completedPaths.Remove(completedPaths.Last());
                InvalidateSurface();
            }
        }

        //protected override void OnPaintSurface(SKPaintSurfaceEventArgs args)
        protected override void OnPaintSurface(SKPaintGLSurfaceEventArgs args)
        {
            base.OnPaintSurface(args);
            SKSize info = CanvasSize;
            SKCanvas canvas = args.Surface.Canvas;
            if (rectInfo.Width != info.Width || rectInfo.Height != info.Height)
            {
                rectInfo = new SKRect(0, 0, info.Width, info.Height);
                SetMainBitmapMatrix();
                SetTrashRects();
            }
            var rectImage = SkiaHelper.CalculateRectangle(rectInfo, outImageWidht, outImageHeight).rect;

            canvas.Clear();
            OnPaintSurface(canvas, rectImage, false);
            canvas.DrawSurrounding(rectInfo, rectImage, SkiaHelper.backgroundColor);
            DrawTrasRect(canvas);
        }

        private void OnPaintSurface(SKCanvas canvas, SKRect rect, bool isDrawResult, float transX = 0, float transY = 0, float scale = 1)
        {
            if(backgroundBitmap != null)
                canvas.DrawBackground(backgroundBitmap, rect, config);
            canvas.Save();
            canvas.SetMatrix(new SKMatrix(scale, 0, transX * scale, 0, scale, transY * scale, 0, 0, 1));
            canvas.DrawBitmap(mainBitmap, transX, transY, scale);
            canvas.DrawPath(completedPaths, isDrawResult ? null : inProgressPaths);
            canvas.Restore();
            canvas.DrawBitmap(bitmapCollection, transX, transY, scale);
        }

        private void AddBitmapToCanvas(TouchManipulationBitmap bitmap)
        {
            var rectImage = SkiaHelper.CalculateRectangle(rectInfo, outImageWidht, outImageHeight).rect;
            float scale = 0.25f;
            var rectSticker = new SKRect(rectImage.Left + rectImage.Width * scale, rectImage.Top + rectImage.Height * scale, rectImage.Right - rectImage.Width * scale, rectImage.Bottom - rectImage.Height * scale);
            var rect = SkiaHelper.CalculateRectangle(rectSticker, bitmap.Bitmap.Width, bitmap.Bitmap.Height);
            bitmap.Matrix = new SKMatrix(rect.scaleX, 0, rectInfo.MidX - rect.rect.Width / 2, 0, rect.scaleY, rectInfo.MidY - rect.rect.Height / 2, 0, 0, 1);
            bitmapCollection.Add(bitmap);
        }

        private void SetMainBitmap(SKBitmap bitmap)
        {
            outImageWidht = config?.IsOutImageAutoSize ?? false ? bitmap?.Width ?? 1 : config?.OutImageWidht ?? 1;
            outImageHeight = config?.IsOutImageAutoSize ?? false ? bitmap?.Height ?? 1 : config?.OutImageHeight ?? 1;

            backgroundBitmap?.Dispose();
            backgroundBitmap = config.BackgroundType == BackgroundType.StretchedImage ? SKBitmapBuilder.GetBlurBitmap(bitmap, new SKRect(0,0, outImageWidht, outImageHeight)) : null;
            
            mainBitmap?.Bitmap?.Dispose();
            if (mainBitmap == null)
                mainBitmap = new TouchManipulationBitmap(bitmap, BitmapType.Main, null);
            else
                mainBitmap.Bitmap = bitmap;

            SetMainBitmapMatrix();
        }

        private void SetMainBitmapMatrix()
        {
            var rect = SkiaHelper.CalculateRectangle(SkiaHelper.CalculateRectangle(rectInfo, outImageWidht, outImageHeight).rect, mainBitmap.Bitmap.Width, mainBitmap.Bitmap.Height, config.Aspect);
            mainBitmap.Matrix = new SKMatrix(rect.scaleX, 0, rectInfo.MidX - rect.rect.Width / 2, 0, rect.scaleY, rectInfo.MidY - rect.rect.Height / 2, 0, 0, 1);
        }


        private void SetTrashRects()
        {
            float size = (float)SkiaHelper.trashSize * (float)DeviceDisplay.MainDisplayInfo.Density;
            float margin = (float)SkiaHelper.trashMargin.Bottom * (float)DeviceDisplay.MainDisplayInfo.Density;
            float midX = rectInfo.Width / 2;
            rectTrash  = new SKRect(midX - size / 2, rectInfo.Height - size - margin, midX + size / 2, rectInfo.Height - margin);
        }

        private void OnTouchPathEffectAction(TouchActionEventArgs args, SKPoint point, SKColor color)
        {
            switch (args.Type)
            {
                case TouchActionType.Pressed:
                    if (!inProgressPaths.ContainsKey(args.Id))
                    {
                        PaintedPath path = new PaintedPath(new SKPath(), color);
                        path.Path.MoveTo(point);
                        inProgressPaths.Add(args.Id, path);
                        InvalidateSurface();
                    }
                    break;

                case TouchActionType.Moved:
                    if (inProgressPaths.ContainsKey(args.Id))
                    {
                        PaintedPath path = inProgressPaths[args.Id];
                        path.Path.LineTo(point);
                        InvalidateSurface();
                    }
                    break;

                case TouchActionType.Released:
                    if (inProgressPaths.ContainsKey(args.Id))
                    {
                        completedPaths.Add(inProgressPaths[args.Id]);
                        inProgressPaths.Remove(args.Id);
                        InvalidateSurface();
                    }
                    break;

                case TouchActionType.Cancelled:
                    if (inProgressPaths.ContainsKey(args.Id))
                    {
                        inProgressPaths.Remove(args.Id);
                        InvalidateSurface();
                    }
                    break;
            }
        }


        private void OnTouchBitmapEffectAction(TouchActionEventArgs args, SKPoint point)
        {
            if (args.Type != TouchActionType.Moved)
                TrashEnabled?.Invoke(false, false, false);

            if (bitmapDictionary == null)
                bitmapDictionary = new Dictionary<long, TouchManipulationBitmap>();
            if (bitmapCollection == null)
                bitmapCollection = new List<TouchManipulationBitmap>();

            switch (args.Type)
            {
                case TouchActionType.Pressed:
                    if (!bitmapDictionary.ContainsKey(args.Id))
                    {
                        bool isFindBitmap = false;
                        if (bitmapCollection?.Count > 0)
                        for (int i = bitmapCollection.Count - 1; i >= 0; i--)
                        {
                            TouchManipulationBitmap bitmap = bitmapCollection[i];

                            int testResult = bitmap.HitTest(point, rectInfo);
                            if (testResult != -1)
                            {
                                if (Device.RuntimePlatform == Device.UWP)
                                    bitmap.TouchManager.Mode = testResult == 4 ? TouchManipulationMode.ScaleRotate : TouchManipulationMode.ScaleDualRotate;

                                bitmap.TouchAction = TouchActionType.Pressed;
                                bitmapCollection.Remove(bitmap);
                                bitmapCollection.Add(bitmap);

                                // Do the touch processing
                                bitmapDictionary.Add(args.Id, bitmap);
                                bitmap.ProcessTouchEvent(args.Id, args.Type, point);
                                isFindBitmap = true;
                                InvalidateSurface();
                                break;
                            }
                        }
                        if ((config?.CanTransformMainBitmap ?? false) && !isFindBitmap && mainBitmap.HitTest(point, rectInfo) != -1)
                        {
                            bitmapDictionary.Add(args.Id, mainBitmap);
                            mainBitmap.ProcessTouchEvent(args.Id, args.Type, point);
                            InvalidateSurface();
                        }
                    }

                    break;

                case TouchActionType.Moved:
                    if (bitmapDictionary.ContainsKey(args.Id))
                    {
                        TouchManipulationBitmap bitmap = bitmapDictionary[args.Id];
                        bitmap.TouchAction = TouchActionType.Moved;

                        bitmap.ProcessTouchEvent(args.Id, args.Type, point);
                        if (bitmap.Type != BitmapType.Main)
                        {
                            if (rectTrash.Contains(point))
                                TrashEnabled?.Invoke(false, true, !rectTrash.Contains(previousTouchPoint));
                            else
                                TrashEnabled?.Invoke(true, false, false);
                        }
                        else
                            TrashEnabled?.Invoke(false, false, false);

                        InvalidateSurface();
                    }
                    else
                        TrashEnabled?.Invoke(false, false, false);

                    break;

                case TouchActionType.Released:
                case TouchActionType.Cancelled:
                    if (bitmapDictionary.ContainsKey(args.Id))
                    {
                        TouchManipulationBitmap bitmap = bitmapDictionary[args.Id];
                        bitmap.ProcessTouchEvent(args.Id, args.Type, point);
                        bitmapDictionary.Remove(args.Id);

                        if (bitmap.TouchAction == TouchActionType.Pressed && bitmap.Type == BitmapType.Text)
                        {
                            bitmap.IsHide = true;
                            TextBitmapClicked?.Invoke(bitmap);
                        }
                        bitmap.TouchAction = null;

                        if (rectTrash.Contains(point))
                        {
                            bitmapCollection.Remove(bitmap);
                        }

                        InvalidateSurface();
                    }
                    break;
            }

        }

        private void DrawTrasRect(SKCanvas canvas)
        {
#if DEBUG
            using (SKPaint paint = new SKPaint())
            {
                paint.Style = SKPaintStyle.Stroke;
                paint.Color = SKColors.Red;
                paint.StrokeWidth = 3;
                paint.IsAntialias = true;
                canvas.DrawRect(rectTrash, paint);
            }
#endif
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
                    inProgressPaths?.Clear();
                    inProgressPaths = null;
                    TrashEnabled = null;
                    TextBitmapClicked = null;
                }

                backgroundBitmap?.Dispose();
                backgroundBitmap = null;
                mainBitmap?.Bitmap.Dispose();
                if (mainBitmap != null)
                    mainBitmap.Bitmap = null;
                backgroundBitmap = null;
                mainBitmap = null;

                foreach (var a in completedPaths)
                    a.Dispose();
                completedPaths?.Clear();
                completedPaths = null;

                foreach (var a in bitmapCollection)
                {
                    if (a.Type != BitmapType.Stickers)
                        a.Bitmap?.Dispose();
                    a.Bitmap = null;
                }
                bitmapCollection = null;

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





