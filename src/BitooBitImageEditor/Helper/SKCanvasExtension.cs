using BitooBitImageEditor.ManipulationBitmap;
using SkiaSharp;
using System;
using System.Collections.Generic;

namespace BitooBitImageEditor.Helper
{
    internal static class SKCanvasExtension
    {
        internal static void DrawManipulationBitmaps(this SKCanvas canvas, SKRect rectInfo, ImageEditorConfig config, SKBitmap backgroundBitmap, List<TouchManipulationBitmap> bitmapCollection, float width, float height,
        float widthBitmap, float heightBitmap, SKMatrix matrix, float transX, float transY, float scale)
        {
            var rectCanvas = SkiaHelper.CalculateRectangle(rectInfo, width, height).rect;
            var rect = SkiaHelper.CalculateRectangle(rectCanvas, widthBitmap, heightBitmap, config.Aspect).rect;

            canvas.Clear();


            switch (config?.BackgroundType)
            {
                case BackgroundType.Color:
                    using (SKPaint paint = new SKPaint())
                    {
                        paint.Color = config.BackgroundColor;
                        paint.IsAntialias = true;
                        paint.Style = SKPaintStyle.Fill;
                        canvas.DrawRect(rectCanvas, paint);
                    }
                    break;
                case BackgroundType.StretchedImage:
                    using (SKPaint paint = new SKPaint())
                    {
                        float blur = 0.08f * Math.Max(rectCanvas.Width, rectCanvas.Height);
                        paint.Color = SKColor.Parse("#1f1f1f");
                        canvas.DrawRect(rectCanvas, paint);
                        paint.ImageFilter = SKImageFilter.CreateBlur(blur, blur);
                        canvas.DrawBitmap(backgroundBitmap, rectCanvas, paint);
                    }

                    break;
            }

            foreach (TouchManipulationBitmap bitmap in bitmapCollection)
            {
                bitmap.Paint(canvas, rectInfo, rectCanvas, rect, matrix, transX, transY, scale);
            }

            canvas.DrawSurrounding(rectInfo, rectCanvas, SKColors.DarkGray.WithAlpha((byte)(0xFF * 0.75)));

        }








        internal static void DrawSurrounding(this SKCanvas canvas, SKRect outerRect, SKRect innerRect, SKColor color)
        {
            using (SKPaint paint = new SKPaint())
            {
                paint.Color = color;
                paint.IsAntialias = true;
                paint.Style = SKPaintStyle.Fill;

                SKRect blackoutCropRectLeft = new SKRect(outerRect.Left, innerRect.Top, innerRect.Left, innerRect.Bottom);
                SKRect blackoutCropRectTop = new SKRect(outerRect.Left, outerRect.Top, outerRect.Right, innerRect.Top);
                SKRect blackoutCropRectRight = new SKRect(innerRect.Right, innerRect.Top, outerRect.Right, innerRect.Bottom);
                SKRect blackoutCropRectBottom = new SKRect(outerRect.Left, innerRect.Bottom, outerRect.Right, outerRect.Bottom);

                canvas.DrawRect(blackoutCropRectTop, paint);
                canvas.DrawRect(blackoutCropRectBottom, paint);
                canvas.DrawRect(blackoutCropRectLeft, paint);
                canvas.DrawRect(blackoutCropRectRight, paint);
            }
        }

    }
}


