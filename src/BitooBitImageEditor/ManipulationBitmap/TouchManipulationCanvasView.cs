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
        SKImageInfo info;
        SKRect rectInfo = new SKRect();

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





        internal SKBitmap EditedBitmap
        {
            get
            {
                SKRect dest = new SKRect(0, 0, width, height);
                SKBitmap croppedBitmap = new SKBitmap((int)width, (int)height);

                float scaleX = (float)rectInfo.Width / width;
                float scaleY = (float)rectInfo.Height / height;
                //float scale = 1 / Math.Max(scaleX, scaleY);


                var rectCanvas = SkiaHelper.CalculateRectangle(rectInfo, width, height).rect;


                var rectCanvasfefe = SkiaHelper.CalculateRectangle(rectInfo, width, height);
                float scale = 1 / rectCanvasfefe.scaleX;

                var rectCanvas1 = SkiaHelper.CalculateRectangle(rectInfo, width, height, Aspect.AspectFill);
                var rectCanvas2 = SkiaHelper.CalculateRectangle(rectInfo, width, height, Aspect.AspectFit).rect;

                //float left = 



                SKMatrix matrix = new SKMatrix(scale, 0, -rectCanvasfefe.rect.Left * rectCanvasfefe.scaleX, 0, scale, -rectCanvasfefe.rect.Top * rectCanvasfefe.scaleX, 0, 0, 1);

                //SKMatrix matrix = new SKMatrix(rectCanvas1.scaleX, 0, -rectCanvas2.Left * rectCanvas1.scaleX, 0, rectCanvas1.scaleY, rectCanvas2.Top * rectCanvas1.scaleY, 0, 0, 1);


                using (SKCanvas canvas = new SKCanvas(croppedBitmap))
                {
                    
                    canvas.DrawManipulationBitmaps(dest, config, backgroundBitmap, bitmapCollection, width, height, widthBitmap, heightBitmap, matrix, -rectCanvasfefe.rect.Left, -rectCanvasfefe.rect.Top, scale);
                }
                return croppedBitmap;
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
                if (bitmapCollection.Count == 0)
                    type = BitmapType.Main;

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
            info = args.Info;
            SKSurface surface = args.Surface;
            SKCanvas canvas = surface.Canvas;
            rectInfo = new SKRect(0, 0, info.Width, info.Height);

            canvas.DrawManipulationBitmaps(rectInfo, config, backgroundBitmap, bitmapCollection, width, height, widthBitmap, heightBitmap, SKMatrix.MakeIdentity(), 0, 0, 1);
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

                            int testResult = bitmap.HitTest(point, rectInfo);
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
