using BitooBitImageEditor.Helper;
using BitooBitImageEditor.TouchTracking;
using SkiaSharp;
using SkiaSharp.Views.Forms;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Xamarin.Forms;

namespace BitooBitImageEditor.ManipulationBitmap
{
    internal class TouchManipulationCanvasView : SKCanvasView
    {
        private bool isUWP;
        ImageEditorConfig config;
        private float width;
        private float height;
        private float widthBitmap;
        private float heightBitmap;
        SKBitmap backgroundBitmap = new SKBitmap();

        internal List<TouchManipulationBitmap> bitmapCollection =
            new List<TouchManipulationBitmap>();

        internal Dictionary<long, TouchManipulationBitmap> bitmapDictionary =
            new Dictionary<long, TouchManipulationBitmap>();

        



        public TouchManipulationCanvasView(ImageEditorConfig config)
        {
            isUWP = Device.RuntimePlatform == Device.UWP;
            this.config = config;
            if (!config?.OutImageAutoSize ?? false)
            {
                width = config?.OutImageWidht ?? 0;
                height = config?.OutImageHeight ?? 0;
            }
        }


        float PointToDraw(float point1, float point2)
        {
            return point1 / 2 - point2;
        }


        internal void AddBitmapToCanvas(string text, SKColor color)
        {
            var bitmap= SKBitmapBuilder.FromText(text, color);
            if (bitmap != null)
                bitmapCollection.Add(new TouchManipulationBitmap(bitmap, SKMatrix.MakeTranslation(PointToDraw((float)Width, bitmap.Width), PointToDraw((float)Height, bitmap.Height)), BitmapType.Text, text));

            InvalidateSurface();
        }

        internal void AddBitmapToCanvas(SKBitmap bitmap, BitmapType type)
        {
            if (bitmap != null)
            {
                bool mainExists = false;
                if (type == BitmapType.Main)
                {
                    if (config?.OutImageAutoSize ?? false)
                    {
                        width = bitmap.Width;
                        height = bitmap.Height;
                    }
                    widthBitmap = bitmap.Width;
                    heightBitmap = bitmap.Height;


                    backgroundBitmap = new SKBitmap(15, 15);
                    bitmap.ScalePixels(backgroundBitmap, SKFilterQuality.High);


                    TouchManipulationBitmap mainBitmap = bitmapCollection.Where(a => a.Type == BitmapType.Main).FirstOrDefault();
                    if (mainBitmap != null)
                    {
                        mainBitmap.Bitmap = bitmap;
                        mainExists = true;
                    }
                }

                if (!mainExists)
                    bitmapCollection.Add(new TouchManipulationBitmap(bitmap, SKMatrix.MakeTranslation(0, 0), type, null));
            }



            InvalidateSurface();
        }




        protected override void OnPaintSurface(SKPaintSurfaceEventArgs args)
        {
            base.OnPaintSurface(args);
            SKImageInfo info = args.Info;
            SKSurface surface = args.Surface;
            SKCanvas canvas = surface.Canvas;



            var rectCanvas = SkiaHelper.CalculateRectangle(new SKRect(0, 0, info.Width, info.Height), width, height);
            var rect = SkiaHelper.CalculateRectangle(rectCanvas.rect, widthBitmap, heightBitmap, config.Aspect);

            canvas.Clear();


            switch (config?.BackgroundType)
            {
                case BackgroundType.Color:
                    using (SKPaint paint = new SKPaint())
                    {
                        paint.Color = config.BackgroundColor;
                        paint.IsAntialias = true;
                        paint.Style = SKPaintStyle.Fill;
                        canvas.DrawRect(rectCanvas.rect, paint);
                    }
                    break;
                case BackgroundType.StretchedImage:
                    using (SKPaint paint = new SKPaint())
                    {
                        paint.Color = SKColor.Parse("#1f1f1f");
                        canvas.DrawRect(rectCanvas.rect, paint);
                        paint.ImageFilter = SKImageFilter.CreateBlur(100, 100);
                        canvas.DrawBitmap(backgroundBitmap, rectCanvas.rect, paint);
                    }

                    break;
            }



     
            



            foreach (TouchManipulationBitmap bitmap in bitmapCollection)
            {
                bitmap.Paint(canvas, args.Info, rect.rect);
            }

            canvas.DrawSurrounding(new SKRect(0,0, info.Width, info.Height), rectCanvas.rect, SKColors.DarkGray.WithAlpha((byte)(0xFF * 0.5)));


        }

        internal void OnTouchEffectTouchAction(object sender, TouchActionEventArgs args)
        {
            Point pt = args.Location;
            SKPoint point =
                new SKPoint((float)(CanvasSize.Width * pt.X / Width),
                            (float)(CanvasSize.Height * pt.Y / Height));

            switch (args.Type)
            {
                case TouchActionType.Pressed:
                    if (!bitmapDictionary.ContainsKey(args.Id))
                        for (int i = bitmapCollection.Count - 1; i >= 0; i--)
                        {
                            TouchManipulationBitmap bitmap = bitmapCollection[i];

                            int testResult = bitmap.HitTest(point);
                            if (testResult != -1)
                            {
                                if(isUWP)
                                    bitmap.TouchManager.Mode = testResult == 4 ? TouchManipulationMode.ScaleRotate : TouchManipulationMode.ScaleDualRotate; 


                                // Move bitmap to end of collection
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
    }
}
