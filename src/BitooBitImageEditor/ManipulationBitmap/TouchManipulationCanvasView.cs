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
    internal class TouchManipulationCanvasView : SKCanvasView/*SKGLView*/, IDisposable
    {
        private ImageEditorConfig config;
        private float outImageWidht;
        private float outImageHeight;
        private float widthBitmap;
        private float heightBitmap;
        private SKBitmap backgroundBitmap = new SKBitmap();
        private SKBitmap mainBitmap = new SKBitmap();
        private SKBitmap tempBitmap = new SKBitmap();
        //private SKImageInfo info;
        private SKRect rectInfo = new SKRect();
        private readonly float sizeTrash;
        private SKRect rectTrash = new SKRect();
        private SKRect rectTrashBig = new SKRect();
        private bool trashVisible = false;
        private bool trashBigVisible = false;
        private readonly SKBitmap trashBitmap;
        private readonly SKBitmap trashOpenBitmap;


        internal event Action<TouchManipulationBitmap> TextBitmapClicked;

        private List<TouchManipulationBitmap> bitmapCollection = new List<TouchManipulationBitmap>();
        private Dictionary<long, TouchManipulationBitmap> bitmapDictionary = new Dictionary<long, TouchManipulationBitmap>();
        private Dictionary<long, PaintedPath> inProgressPaths = new Dictionary<long, PaintedPath>();
        private List<PaintedPath> completedPaths = new List<PaintedPath>();


        public TouchManipulationCanvasView(ImageEditorConfig config)
        {
            this.config = config;
            if (!config?.IsOutImageAutoSize ?? false)
            {
                outImageWidht = config?.OutImageWidht ?? 0;
                outImageHeight = config?.OutImageHeight ?? 0;
            }
            DisplayInfo displayInfo = DeviceDisplay.MainDisplayInfo;
            sizeTrash = (float)Math.Max(displayInfo.Height, displayInfo.Width) * 0.06f;

            using (Stream streamOpenTrash = GetType().GetTypeInfo().Assembly.GetManifestResourceStream($"{ImageResourceExtension.resource}trash_open.png"))
            using (Stream streamTrash = GetType().GetTypeInfo().Assembly.GetManifestResourceStream($"{ImageResourceExtension.resource}trash.png"))
            {
                trashBitmap = SKBitmap.Decode(streamTrash);
                trashOpenBitmap = SKBitmap.Decode(streamOpenTrash);
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
                    canvas.Save();
                    SKMatrix bitmapMatrix = new SKMatrix(scale, 0, -rectTranslate.rect.Left * scale, 0, scale, -rectTranslate.rect.Top * scale, 0, 0, 1);    
                    canvas.SetMatrix(bitmapMatrix);
                    canvas.DrawPath(completedPaths, null);
                    canvas.Restore();
                    canvas.DrawBitmap(bitmapCollection, -rectTranslate.rect.Left, -rectTranslate.rect.Top, scale);
                }
                return outBitmap;
            }
        }

        internal void OnTouchEffectTouchAction(TouchActionEventArgs args, ImageEditType editType, SKColor color)
        {
            Point pt = args.Location;
            SKPoint point = new SKPoint((float)(CanvasSize.Width * pt.X / Width), (float)(CanvasSize.Height * pt.Y / Height));
            switch (editType)
            {
                case ImageEditType.Paint:
                    OnTouchPathEffectAction(args, point, color);
                    break;
                default:
                    OnTouchBitmapEffectAction(args, point);
                    break;
            }
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

        //protected override void OnPaintSurface(SKPaintGLSurfaceEventArgs e)
        //{
        //    base.OnPaintSurface(e);
        //    SKSize info = CanvasSize;
        //    SKCanvas canvas = e.Surface.Canvas;
        //    if (rectInfo.Width != info.Width || rectInfo.Height != info.Height)
        //    {
        //        rectInfo = new SKRect(0, 0, info.Width, info.Height);
        //        SetTempBitmap();
        //        SetTrashRects(info);
        //    }
        //    var rectImage = SkiaHelper.CalculateRectangle(rectInfo, outImageWidht, outImageHeight).rect;

        //    canvas.Clear();
        //    canvas.DrawBitmap(tempBitmap, rectImage);

        //    canvas.DrawPath(completedPaths, inProgressPaths);

        //    canvas.DrawBitmap(bitmapCollection);
        //    canvas.DrawSurrounding(rectInfo, rectImage, 0xBD020207);
        //    if (trashVisible)
        //        canvas.DrawBitmap(trashBitmap, rectTrash);
        //    if (trashBigVisible)
        //        canvas.DrawBitmap(trashOpenBitmap, rectTrashBig);
        //}



        protected override void OnPaintSurface(SKPaintSurfaceEventArgs args)
        {
            base.OnPaintSurface(args);
            SKImageInfo info = args.Info;
            SKCanvas canvas = args.Surface.Canvas;
            if (rectInfo.Width != info.Width || rectInfo.Height != info.Height)
            {
                rectInfo = new SKRect(0, 0, info.Width, info.Height);
                SetTempBitmap();
                SetTrashRects(info);
            }
            var rectImage = SkiaHelper.CalculateRectangle(rectInfo, outImageWidht, outImageHeight).rect;

            canvas.Clear();
            canvas.DrawBitmap(tempBitmap, rectImage);

            canvas.DrawPath(completedPaths, inProgressPaths);

            canvas.DrawBitmap(bitmapCollection);
            canvas.DrawSurrounding(rectInfo, rectImage, 0xBD020207);
            if (trashVisible)
                canvas.DrawBitmap(trashBitmap, rectTrash);
            if (trashBigVisible)
                canvas.DrawBitmap(trashOpenBitmap, rectTrashBig);
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
                bitmap.ScalePixels(backgroundBitmap, SKFilterQuality.Low);
            }

            mainBitmap = bitmap;
            SetTempBitmap();
        }

        private void SetTempBitmap()
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
            tempBitmap?.Dispose();
            tempBitmap = outBitmap;
            GC.Collect();
        }

        private void SetTrashRects(SKImageInfo info)
        {
            float sizeTrashBig = sizeTrash * 1.5f;
            float midX = info.Width / 2;
            rectTrash = new SKRect(midX - sizeTrash / 2, info.Height - sizeTrash - 20, midX + sizeTrash / 2, info.Height - 20);
            rectTrashBig = new SKRect(midX - sizeTrashBig / 2, info.Height - sizeTrashBig - 10, midX + sizeTrashBig / 2, info.Height - 10);
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
                trashVisible = trashBigVisible = false;

            if(bitmapDictionary == null)
                bitmapDictionary = new Dictionary<long, TouchManipulationBitmap>();
            if (bitmapCollection == null)
                bitmapCollection = new List<TouchManipulationBitmap>();

            switch (args.Type)
            {
                case TouchActionType.Pressed:
                    if (!bitmapDictionary.ContainsKey(args.Id) && bitmapCollection?.Count > 0)
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
                                InvalidateSurface();
                                break;
                            }
                        }

                    break;

                case TouchActionType.Moved:
                    if (bitmapDictionary.ContainsKey(args.Id))
                    {
                        TouchManipulationBitmap bitmap = bitmapDictionary[args.Id];
                        bitmap.TouchAction = TouchActionType.Moved;

                        bitmap.ProcessTouchEvent(args.Id, args.Type, point);

                        if (rectTrash.Contains(point))
                        {
                            trashVisible = false;
                            trashBigVisible = true;
                        }
                        else
                        {
                            trashBigVisible = false;
                            trashVisible = true;
                        }

                        InvalidateSurface();
                    }
                    else
                        trashVisible = trashBigVisible = false;
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
                    inProgressPaths = null;
                }

                ((IDisposable)backgroundBitmap).Dispose();
                ((IDisposable)mainBitmap).Dispose();
                ((IDisposable)tempBitmap).Dispose();
                ((IDisposable)tempBitmap).Dispose();

                trashBitmap?.Dispose();
                trashOpenBitmap?.Dispose();

                foreach (var a in completedPaths)
                    a.Dispose();
                completedPaths = null;

                foreach (var a in bitmapCollection)
                {
                    if (a.Type == BitmapType.Text)
                        a.Bitmap.Dispose();
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
