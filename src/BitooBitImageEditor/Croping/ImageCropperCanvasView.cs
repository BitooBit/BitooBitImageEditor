using BitooBitImageEditor.TouchTracking;
using SkiaSharp;
using SkiaSharp.Views.Forms;
using System;
using System.Collections.Generic;
using Xamarin.Forms;

namespace BitooBitImageEditor.Croping
{
    internal class ImageCropperCanvasView : SKCanvasView
    {
        SKBitmap bitmap;
        //double angleR;
        float? aspectRatio;
        CroppingRectangle croppingRect;
        SKMatrix inverseBitmapMatrix;

        // Touch tracking  
        TouchEffect touchEffect = new TouchEffect();
        struct TouchPoint
        {
            public int CornerIndex { set; get; }
            public SKPoint Offset { set; get; }
        }

        Dictionary<long, TouchPoint> touchPoints = new Dictionary<long, TouchPoint>();
        Dictionary<long, SKPoint> touchPointsInside = new Dictionary<long, SKPoint>();

        SKPoint bitmapLocationfirst = new SKPoint();
        SKPoint bitmapLocationlast = new SKPoint();

        
        

        internal ImageCropperCanvasView(SKBitmap bitmap, float? aspectRatio = null)
        {
            this.aspectRatio = aspectRatio;
            this.bitmap = bitmap;
            SKRect bitmapRect = new SKRect(0, 0, bitmap.Width, bitmap.Height);
            croppingRect = new CroppingRectangle(bitmapRect, aspectRatio);

            touchEffect.TouchAction += OnTouchEffectTouchAction;
        }

        internal SKBitmap CroppedBitmap
        {
            get
            {
                SKRect cropRect = croppingRect.Rect;
                SKBitmap croppedBitmap = new SKBitmap((int)cropRect.Width,
                                                      (int)cropRect.Height);
                SKRect dest = new SKRect(0, 0, cropRect.Width, cropRect.Height);
                SKRect source = new SKRect(cropRect.Left, cropRect.Top,
                                           cropRect.Right, cropRect.Bottom);
                using (SKCanvas canvas = new SKCanvas(croppedBitmap))
                {
                    canvas.DrawBitmap(bitmap, source, dest);
                }
                return croppedBitmap;
            }
        }

        protected override void OnParentSet()
        {
            base.OnParentSet();

            // Attach TouchEffect to parent view
            Parent.Effects.Add(touchEffect);
        }

        internal void SetAspectRatio(float? aspectRatio = null, bool isFullRect = false)
        {
            this.aspectRatio = aspectRatio;
            SKRect bitmapRect = new SKRect(0, 0, bitmap.Width, bitmap.Height);
            croppingRect.SetRect(bitmapRect, aspectRatio, isFullRect);

            InvalidateSurface();
        }

        internal void Rotate()
        {
            var rotatedBitmap = new SKBitmap(bitmap.Height, bitmap.Width);

            using (var surface = new SKCanvas(rotatedBitmap))
            {
                surface.Translate(rotatedBitmap.Width, 0);
                surface.RotateDegrees(90);
                surface.DrawBitmap(bitmap, 0, 0);
            }

            bitmap = rotatedBitmap;

            SetAspectRatio(aspectRatio);
        }


        protected override void OnPaintSurface(SKPaintSurfaceEventArgs args)
        {
            base.OnPaintSurface(args);

            SKImageInfo info = args.Info;
            SKSurface surface = args.Surface;
            SKCanvas canvas = surface.Canvas;

            canvas.Clear(SkiaHelper.backgraundColor);

            // Calculate rectangle for displaying bitmap 
            var rect = SkiaHelper.CalculateRectangle(info, bitmap);
            canvas.DrawBitmap(bitmap, rect.rect);

            // Calculate a matrix transform for displaying the cropping rectangle
            SKMatrix bitmapScaleMatrix = SKMatrix.MakeIdentity();
            bitmapScaleMatrix.SetScaleTranslate(rect.scale, rect.scale, rect.left, rect.top);

            // Display rectangle
            SKRect scaledCropRect = bitmapScaleMatrix.MapRect(croppingRect.Rect);
            canvas.DrawRect(scaledCropRect, SkiaHelper.edgeStroke);

            SKRect blackoutCropRectTop = new SKRect(rect.left, rect.top, rect.right, scaledCropRect.Top);
            canvas.DrawRect(blackoutCropRectTop, SkiaHelper.blackoutFill);

            SKRect blackoutCropRectBottom = new SKRect(rect.left, scaledCropRect.Bottom, rect.right, rect.bottom);
            canvas.DrawRect(blackoutCropRectBottom, SkiaHelper.blackoutFill);

            SKRect blackoutCropRectLeft = new SKRect(rect.left, scaledCropRect.Top, scaledCropRect.Left, scaledCropRect.Bottom);
            canvas.DrawRect(blackoutCropRectLeft, SkiaHelper.blackoutFill);

            SKRect blackoutCropRectRight = new SKRect(scaledCropRect.Right, scaledCropRect.Top, rect.right, scaledCropRect.Bottom);
            canvas.DrawRect(blackoutCropRectRight, SkiaHelper.blackoutFill);

            // Display heavier corners
            using (SKPath path = new SKPath())
            {
                path.MoveTo(scaledCropRect.Left, scaledCropRect.Top + SkiaHelper.corner);
                path.LineTo(scaledCropRect.Left, scaledCropRect.Top);
                path.LineTo(scaledCropRect.Left + SkiaHelper.corner, scaledCropRect.Top);

                path.MoveTo(scaledCropRect.Right - SkiaHelper.corner, scaledCropRect.Top);
                path.LineTo(scaledCropRect.Right, scaledCropRect.Top);
                path.LineTo(scaledCropRect.Right, scaledCropRect.Top + SkiaHelper.corner);

                path.MoveTo(scaledCropRect.Right, scaledCropRect.Bottom - SkiaHelper.corner);
                path.LineTo(scaledCropRect.Right, scaledCropRect.Bottom);
                path.LineTo(scaledCropRect.Right - SkiaHelper.corner, scaledCropRect.Bottom);

                path.MoveTo(scaledCropRect.Left + SkiaHelper.corner, scaledCropRect.Bottom);
                path.LineTo(scaledCropRect.Left, scaledCropRect.Bottom);
                path.LineTo(scaledCropRect.Left, scaledCropRect.Bottom - SkiaHelper.corner);

                canvas.DrawPath(path, SkiaHelper.cornerStroke);
            }

            // Invert the transform for touch tracking
            bitmapScaleMatrix.TryInvert(out inverseBitmapMatrix);
        }

        void OnTouchEffectTouchAction(object sender, TouchActionEventArgs args)
        {
            SKPoint pixelLocation = SkiaHelper.ConvertToPixel(this, args.Location);
            SKPoint bitmapLocation = inverseBitmapMatrix.MapPoint(pixelLocation);

            switch (args.Type)
            {
                case TouchActionType.Pressed:
                    // Convert radius to bitmap/cropping scale
                    float radius = inverseBitmapMatrix.ScaleX * SkiaHelper.radius;

                    // Find corner that the finger is touching
                    int cornerIndex = croppingRect.HitTest(bitmapLocation, radius);

                    if (cornerIndex != -1 && !touchPoints.ContainsKey(args.Id))
                    {
                        TouchPoint touchPoint = new TouchPoint
                        {
                            CornerIndex = cornerIndex,
                            Offset = bitmapLocation - croppingRect.Corners[cornerIndex]
                        };

                        touchPoints.Add(args.Id, touchPoint);
                    }
                    else
                    {
                        //При получении нажатия в словарь пишем его ID и в переменную записываем коардинаты первой точки
                        if (croppingRect.TestPointInsideSquare(bitmapLocation) && !touchPointsInside.ContainsKey(args.Id))
                        {
                            touchPointsInside.Add(args.Id, bitmapLocation);
                            bitmapLocationfirst = bitmapLocation;
                        }
                    }
                    break;

                case TouchActionType.Moved:
                    if (touchPoints.ContainsKey(args.Id))
                    {
                        TouchPoint touchPoint = touchPoints[args.Id];
                        croppingRect.MoveCorner(touchPoint.CornerIndex,
                                                bitmapLocation - touchPoint.Offset);
                        InvalidateSurface();
                    }
                    if (touchPointsInside.ContainsKey(args.Id))
                    {
                        //Если перемещение соответсвует айдишнику от его кардинат вычитаем корадинаты начальной точки и передаем в метод перемещения
                        bitmapLocationlast = bitmapLocation;
                        SKPoint point = new SKPoint();
                        point.X = bitmapLocationlast.X - bitmapLocationfirst.X;
                        point.Y = bitmapLocationlast.Y - bitmapLocationfirst.Y;
                        croppingRect.MoveAllCorner(point);
                        bitmapLocationfirst = bitmapLocationlast;
                        InvalidateSurface();
                    }
                    break;

                case TouchActionType.Released:
                case TouchActionType.Cancelled:
                    if (touchPoints.ContainsKey(args.Id))
                    {
                        touchPoints.Remove(args.Id);
                    }
                    if (touchPointsInside.ContainsKey(args.Id))
                    {
                        touchPointsInside.Remove(args.Id);
                    }
                    break;
            }
        }



    }
}
