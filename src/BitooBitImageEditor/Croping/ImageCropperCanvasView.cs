using BitooBitImageEditor.Helper;
using BitooBitImageEditor.TouchTracking;
using SkiaSharp;
using SkiaSharp.Views.Forms;
using System.Collections.Generic;

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

        struct TouchPoint
        {
            public int CornerIndex { set; get; }
            public SKPoint Offset { set; get; }
        }

        Dictionary<long, TouchPoint> touchPoints = new Dictionary<long, TouchPoint>();
        Dictionary<long, SKPoint> touchPointsInside = new Dictionary<long, SKPoint>();

        SKPoint bitmapLocationfirst = new SKPoint();
        SKPoint bitmapLocationlast = new SKPoint();

        internal bool IsActive { get; set; }



        internal ImageCropperCanvasView(SKBitmap bitmap, float? aspectRatio = null)
        {
            this.bitmap = bitmap;
            SKRect bitmapRect = new SKRect(0, 0, bitmap.Width, bitmap.Height);
            croppingRect = new CroppingRectangle(bitmapRect, aspectRatio);
            SetAspectRatio(aspectRatio);
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

        internal void SetAspectRatio(float? aspectRatio = null, bool isFullRect = false)
        {
            this.aspectRatio = aspectRatio;
            croppingRect.SetRect(new SKRect(0, 0, bitmap.Width, bitmap.Height), aspectRatio, isFullRect);
            InvalidateSurface();
        }


        internal void SetAspectRatio(CropItem crop)
        {
            switch (crop?.Action)
            {
                case CropRotateType.CropRotate:
                    Rotate();
                    break;
                case CropRotateType.CropFree:
                    SetAspectRatio(null, false);
                    break;
                case CropRotateType.CropFull:
                    SetAspectRatio(null, true);
                    break;
                case CropRotateType.CropSquare:
                    SetAspectRatio(1f);
                    break;
            }
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
            var rect = SkiaHelper.CalculateRectangle(new SKRect(0, 0, info.Width, info.Height), bitmap);
            canvas.DrawBitmap(bitmap, rect.rect);

            // Calculate a matrix transform for displaying the cropping rectangle
            SKMatrix bitmapScaleMatrix = SKMatrix.MakeIdentity();
            bitmapScaleMatrix.SetScaleTranslate(rect.scaleX, rect.scaleX, rect.rect.Left, rect.rect.Top);

            // Display rectangle
            SKRect scaledCropRect = bitmapScaleMatrix.MapRect(croppingRect.Rect);
            canvas.DrawRect(scaledCropRect, SkiaHelper.edgeStroke);


            canvas.DrawSurrounding(rect.rect, scaledCropRect, SKColors.Gray.WithAlpha((byte)(0xFF * 0.5)));



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

        internal void OnTouchEffectTouchAction(object sender, TouchActionEventArgs args)
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
